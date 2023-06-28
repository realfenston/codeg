import argparse
import os
import random

import numpy as np
import torch
from datasets import load_dataset
from torch.utils.data import IterableDataset
from torch.utils.data.dataloader import DataLoader
from tqdm import tqdm
from transformers import (
    AutoModelForCausalLM,
    AutoTokenizer,
    Trainer,
    TrainingArguments,
    logging,
    set_seed,
)

import dataset as ds
from accelerate import load_checkpoint_and_dispatch
from accelerate import init_empty_weights
from transformers import AutoConfig, AutoModelForCausalLM
from transformers import AutoTokenizer
from peft import LoraConfig, get_peft_model, prepare_model_for_int8_training, set_peft_model_state_dict

from trainer import ModelTrainer
from evaluate import SingleCLMEvaluator
from model import GPTSingleHead

import fim


def get_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("--model_path", type=str, default="bigcode/santacoder")
    parser.add_argument("--dataset_name", type=str, default="bigcode/the-stack-dedup")
    parser.add_argument("--subset", type=str, default="data")
    parser.add_argument("--split", type=str, default="train")
    parser.add_argument("--size_valid_set", type=int, default=4000)
    parser.add_argument("--streaming", action="store_true")
    parser.add_argument("--shuffle_buffer", type=int, default=5000)
    parser.add_argument("--data_column", type=str, default="content")
    parser.add_argument("--test_split_size", type=float, default=0.1)
    parser.add_argument("--visible_devices", type=str, default="0,1,2,3")
    parser.add_argument("--n_gpus", type=int, default=1)

    parser.add_argument("--seq_length", type=int, default=1024)
    parser.add_argument("--max_steps", type=int, default=10000)
    parser.add_argument("--batch_size", type=int, default=1)
    parser.add_argument("--gradient_accumulation_steps", type=int, default=8)
    parser.add_argument("--eos_token_id", type=int, default=49152)
    parser.add_argument("--epochs", type=int, default=10)

    parser.add_argument("--lora_r", type=int, default=8)
    parser.add_argument("--lora_alpha", type=int, default=32)
    parser.add_argument("--lora_dropout", type=float, default=0.05)

    parser.add_argument("--learning_rate", type=float, default=5e-5)
    parser.add_argument("--lr_scheduler_type", type=str, default="warmupcosine")
    parser.add_argument("--num_warmup_steps", type=int, default=100)
    parser.add_argument("--weight_decay", type=float, default=0.05)

    parser.add_argument("--local_rank", type=int, default=0)
    parser.add_argument("--no_fp16", action="store_false")
    parser.add_argument("--bf16", action="store_true")
    parser.add_argument("--no_gradient_checkpointing", action="store_false")
    parser.add_argument("--seed", type=int, default=0)
    parser.add_argument("--num_workers", type=int, default=None)
    parser.add_argument("--output_dir", type=str, default="./checkpoints")
    parser.add_argument("--log_freq", default=1, type=int)
    parser.add_argument("--eval_freq", default=1000, type=int)
    parser.add_argument("--save_freq", default=1000, type=int)

    parser.add_argument("--fim_rate", type=float, default=0.0)
    parser.add_argument("--fim_spm_rate", type=float, default=0.0)
    return parser.parse_args()


def chars_token_ratio(dataset, tokenizer, data_column, nb_examples=400):
    """
    Estimate the average number of characters per token in the dataset.
    """
    total_characters, total_tokens = 0, 0
    for _, example in tqdm(zip(range(nb_examples), iter(dataset)), total=nb_examples):
        total_characters += len(example[data_column])
        total_tokens += len(tokenizer(example[data_column]).tokens())

    return total_characters / total_tokens


def create_datasets(tokenizer, args):
    '''
    dataset = load_dataset(
        args.dataset_name,
        data_dir=args.subset,
        split=args.split,
        use_auth_token=True,
        num_proc=args.num_workers if not args.streaming else None,
        streaming=args.streaming,
    )
    '''
    dataset = load_dataset('json', data_files=args.dataset_name, split='train', streaming=args.streaming)
    if args.streaming:
        print("Loading the dataset in streaming mode")
        valid_data = dataset.take(args.size_valid_set)
        train_data = dataset.skip(args.size_valid_set)
        train_data = train_data.shuffle(buffer_size=args.shuffle_buffer, seed=args.seed)
    else:
        dataset = dataset.train_test_split(test_size=args.test_split_size, seed=args.seed)
        train_data = dataset["train"]
        valid_data = dataset["test"]
        print(
            f"Size of the train set: {len(train_data)}. Size of the validation set: {len(valid_data)}"
        )
    chars_per_token = chars_token_ratio(train_data, tokenizer, args.data_column)
    print(f"The character to token ratio of the dataset is: {chars_per_token:.2f}")
    
    train_dataset = ds.ConstantLengthDataset(
        tokenizer,
        train_data,
        infinite=True,
        seq_length=args.seq_length,
        chars_per_token=chars_per_token,
        content_field=args.data_column,
        fim_rate=args.fim_rate,
        fim_spm_rate=args.fim_spm_rate,
        seed=args.seed,
    )
    valid_dataset = ds.ConstantLengthDataset(
        tokenizer,
        valid_data,
        infinite=False,
        seq_length=args.seq_length,
        chars_per_token=chars_per_token,
        content_field=args.data_column,
        fim_rate=args.fim_rate,
        fim_spm_rate=args.fim_spm_rate,
        seed=args.seed,
    )
    '''
    train_dataset = ds.CodeIterableDataset(train_data, tokenizer, args.seq_length, content_field=args.data_column)
    
    # valid_data = valid_data.shuffle(buffer_size=args.patch_size, seed=2023)
    valid_dataset = ds.CodeIterableDataset(valid_data, tokenizer, args.seq_length, content_field=args.data_column)
    '''

    return train_dataset, valid_dataset


def run_training(args, train_data, val_data):
    print("Loading the model")
    # disable caching mechanism when using gradient checkpointing
    '''
    model = AutoModelForCausalLM.from_pretrained(
        args.model_path,
        trust_remote_code=True,
        use_cache=not args.no_gradient_checkpointing,
    )
    '''
    train_data.start_iteration = 0
    model = GPTSingleHead(model_name_or_path=args.model_path, max_seq_length=args.seq_length, args=args)

    print(f"Starting main loop")

    train_data.start_iteration = 0
    output_path = f"starcoder_finetuned"
    dev_evaluator = SingleCLMEvaluator()
    model_trainer = ModelTrainer(model,
                                 train_dataset=train_data,
                                 dev_dataset=val_data,
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
