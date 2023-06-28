import argparse
import os

import torch
from accelerate import Accelerator
from peft import LoraConfig, get_peft_model, prepare_model_for_int8_training, set_peft_model_state_dict
from torch.utils.data import IterableDataset
from tqdm import tqdm
from transformers import AutoConfig, AutoModelForCausalLM, AutoTokenizer, Trainer, TrainingArguments, logging, set_seed
from transformers import TrainerCallback, TrainingArguments, TrainerState, TrainerControl
from transformers.trainer_utils import PREFIX_CHECKPOINT_DIR
from accelerate import load_checkpoint_and_dispatch
from accelerate import init_empty_weights
from transformers import AutoConfig, AutoModelForCausalLM
from transformers import AutoTokenizer
import os, pickle, json
from trainer import ModelTrainer
from evaluate import SingleCLMEvaluator
from data import SrcCodeIterableDataset
from model import GPTSingleHead
from datasets import load_dataset


class SavePeftModelCallback(TrainerCallback):
    def on_save(
        self,
        args: TrainingArguments,
        state: TrainerState,
        control: TrainerControl,
        **kwargs,
    ):
        checkpoint_folder = os.path.join(args.output_dir, f"{PREFIX_CHECKPOINT_DIR}-{state.global_step}")

        kwargs["model"].save_pretrained(checkpoint_folder)

        pytorch_model_path = os.path.join(checkpoint_folder, "pytorch_model.bin")
        torch.save({}, pytorch_model_path)
        return control
    
class LoadBestPeftModelCallback(TrainerCallback):
    def on_train_end(
        self,
        args: TrainingArguments,
        state: TrainerState,
        control: TrainerControl,
        **kwargs,
    ):
        print(f"Loading best peft model from {state.best_model_checkpoint} (score: {state.best_metric}).")
        best_model_path = os.path.join(state.best_model_checkpoint, "adapter_model.bin")
        adapters_weights = torch.load(best_model_path)
        model = kwargs["model"]
        set_peft_model_state_dict(model, adapters_weights)
        return control


def get_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("--model_path", type=str, default="/data/shihang/starcoderbase")
    parser.add_argument("--subset", type=str)
    parser.add_argument("--split", type=str)
    parser.add_argument("--size_valid_set", type=int, default=10000)
    parser.add_argument("--streaming", action="store_true")
    parser.add_argument("--shuffle_buffer", type=int, default=5000)
    parser.add_argument("--model_select", type=str, default="gpt2")
    parser.add_argument("--dataset_name", type=str, default="gala")
    parser.add_argument("--n_gpus", type=int, default=1)
    parser.add_argument("--visible_devices", type=str, default="0,1")
    parser.add_argument("--patch_size", type=int, default=500)

    parser.add_argument("--input_column_name", type=str, default="input_ids")
    parser.add_argument("--output_column_name", type=str, default="input_ids")
    parser.add_argument("--max_seq_length", type=int, default=2048)

    parser.add_argument("--seq_length", type=int, default=2048)
    parser.add_argument("--max_steps", type=int, default=10000)
    parser.add_argument("--batch_size", type=int, default=1)
    parser.add_argument("--gradient_accumulation_steps", type=int, default=1)
    parser.add_argument("--eos_token_id", type=int, default=49152)
    parser.add_argument("--epochs", type=int, default=100)

    parser.add_argument("--lora_r", type=int, default=8)
    parser.add_argument("--lora_alpha", type=int, default=32)
    parser.add_argument("--lora_dropout", type=float, default=0.05)

    parser.add_argument("--learning_rate", type=float, default=5e-6)
    parser.add_argument("--lr_scheduler_type", type=str, default="warmuplinear")
    parser.add_argument("--num_warmup_steps", type=int, default=100)
    parser.add_argument("--weight_decay", type=float, default=0.05)

    parser.add_argument("--local_rank", type=int, default=0)
    parser.add_argument("--no_fp16", action="store_false")
    parser.add_argument("--bf16", action="store_true", default=True)
    parser.add_argument("--no_gradient_checkpointing", action="store_false", default=False)
    parser.add_argument("--seed", type=int, default=2023)
    parser.add_argument("--num_workers", type=int, default=None)
    parser.add_argument("--output_dir", type=str, default="./ckpt")
    parser.add_argument("--log_freq", default=100, type=int)
    parser.add_argument("--eval_freq", default=10, type=int)
    parser.add_argument("--save_freq", default=1000, type=int)

    return parser.parse_args()


def create_datasets(tokenizer, args):
    # dataset = load_dataset("parquet", data_files={'train': 'data-00000-of-00144.parquet'})

    '''
    if args.streaming:
        print("Loading the dataset in streaming mode")
        valid_data = dataset.take(args.size_valid_set)
        train_data = dataset.skip(args.size_valid_set)
        train_data = train_data.shuffle(buffer_size=args.shuffle_buffer, seed=args.seed)
    else:
        train_data = dataset["train"]
        valid_data = dataset["test"]
        print(f"Size of the train set: {len(train_data)}. Size of the validation set: {len(valid_data)}")
    '''

    dataset_folder = f"dataset/{args.dataset_name}/json/"
    
    ''' 
    output_path = f"model/{args.model_select}_fine_tuned"
    file_path = dataset_folder + "train.jsonl"
    train_dataset = SrcCodeDataset(file_path, tokenizer, cache_path=os.path.join(".cache", output_path, "train"))
    #load developlemt dataset
    file_path = dataset_folder + "dev.jsonl"
    valid_dataset = SrcCodeDataset(file_path, tokenizer, cache_path=os.path.join(".cache", output_path, "dev"))
    '''
    
    
    train_data = load_dataset('json', data_files=f"{dataset_folder}/train.jsonl", streaming=True, split='train')
    valid_data = load_dataset('json', data_files=f"{dataset_folder}/dev.jsonl", streaming=True, split='train') # do not correctly categorize training and validation data
    train_data = train_data.shuffle(buffer_size=args.patch_size, seed=2023)
    train_dataset = SrcCodeIterableDataset(train_data, tokenizer, args.max_seq_length)
    
    # valid_data = valid_data.shuffle(buffer_size=args.patch_size, seed=2023)
    valid_dataset = SrcCodeIterableDataset(valid_data, tokenizer, args.max_seq_length)
    
    
    return train_dataset, valid_dataset


def run_training(args, train_dataset, val_dataset):
    model = GPTSingleHead(model_name_or_path=args.model_path, max_seq_length=args.max_seq_length, args=args)
    train_dataset.start_iteration = 0
    output_path = f"model/{args.model_select}_fine_tuned"
    dev_evaluator = SingleCLMEvaluator()
    model_trainer = ModelTrainer(model,
                                 train_dataset=train_dataset,
                                 dev_dataset=val_dataset,
                                 dev_evaluator=dev_evaluator,
                                 scheduler=args.lr_scheduler_type,
                                 epochs=args.epochs,
                                 per_gpu_train_batch_size=args.batch_size,
                                 output_path=output_path,
                                 optimizer_params={'lr': args.learning_rate, 'eps': 1e-6, 'correct_bias': False},
                                 evaluation_steps=args.eval_freq,
                                 early_stop=20,
                                 dev_batch_size=args.batch_size,
                                 restore_training=False,
                                 accumulation_steps=args.gradient_accumulation_steps,
                                 n_gpu=args.n_gpus,
                                 visible_devices=args.visible_devices,
                                 warmup_ratio=0.2,
                                 seed=args.seed,
                                 data_loader_shuffle=True,
                                 wandb_config=None,
                                 fp16=False)
    #start training
    model_trainer.train()
    
    print("Saving last checkpoint of the model")
    torch.save(model.gpt.state_dict(), os.path.join(args.output_dir, "final_checkpoint.pt"))


def main(args):
    tokenizer = AutoTokenizer.from_pretrained(args.model_path, use_auth_token=True)
    train_dataset, eval_dataset = create_datasets(tokenizer, args)
    run_training(args, train_dataset, eval_dataset)


if __name__ == "__main__":
    args = get_args()
    set_seed(args.seed)
    os.makedirs(args.output_dir, exist_ok=True)
    logging.set_verbosity_error()
    main(args)
