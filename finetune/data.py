from torch.utils.data import Dataset
import os, pickle, json
import logging

logger = logging.getLogger(__name__)
import torch
from tqdm import tqdm
from transformers import AutoTokenizer
from torch.utils.data import IterableDataset
from datasets import load_dataset


class SrcCodeIterableDataset(IterableDataset):
    def __init__(self, dataset, tokenizer, max_seq_length=8192):
        """
        this dataset class is used to load source code dataset in batch for fine-tuning with GPT2LMModel
        :param model: the model that the dataset will be fed to
        """
        self.inputs = []
        self.dataset = dataset
        self.seq_length = max_seq_length
        self.chars_per_token = 1
        self.num_of_sequences = 1
        self.max_buffer_size = self.seq_length * self.chars_per_token * self.num_of_sequences
        self.current_size = 0
        self.infinite = True
        
        self.tokenizer = tokenizer
        self.concat_token_id = tokenizer.eos_token_id
        
    def prepare_sample_text(self, example):
        return example['token_ids']
                
    def __iter__(self):
        iterator = iter(self.dataset)
        more_examples = True
        while more_examples:
            buffer, buffer_len = [], 0
            while True:
                '''
                if buffer_len >= self.max_buffer_size:
                    break
                '''
                try:
                    buffer.append(self.prepare_sample_text(next(iterator)))
                    # buffer_len += len(buffer[-1])
                    break
                except StopIteration:
                    if self.infinite:
                        iterator = iter(self.dataset)
                    else:
                        more_examples = False
                        break
            # tokenized_inputs = self.tokenizer(buffer, truncation=False)["input_ids"]
            all_token_ids = []
            for tokenized_input in buffer:
                all_token_ids.extend(tokenized_input + [self.concat_token_id])
                if len(all_token_ids) >= self.seq_length:
                    all_token_ids = all_token_ids[:self.seq_length]
            '''
            for i in range(0, len(all_token_ids), self.seq_length):
                input_ids = all_token_ids[i : i + self.seq_length]
                if len(input_ids) == self.seq_length:
                    self.current_size += 1
                    yield {
                        "input_ids": input_ids,
                    }
            '''
            self.current_size += 1
            yield {
                "input_ids": all_token_ids,
                "labels": all_token_ids
            }
                
                
def test_iterable():
    model_path = '/root/pretrain/starcoder/starcoderbase'
    tokenizer = AutoTokenizer.from_pretrained(model_path, use_auth_token=True)
    
    dataset_folder = f"dataset/gala/json/"
    dataset = load_dataset('json', data_files=f"{dataset_folder}/train.jsonl", streaming=True, split='train')
    dataset = dataset.shuffle(buffer_size=500, seed=2023)
    dataset = SrcCodeIterableDataset(dataset, tokenizer)
    for batch in dataset:
        print(batch['input_ids'])
        print(batch['labels'])
        break

if __name__ == "__main__":
    test_iterable()
