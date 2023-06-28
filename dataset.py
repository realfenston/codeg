from torch.utils.data import Dataset
import os, pickle, json
import logging

logger = logging.getLogger(__name__)
import torch
from tqdm import tqdm
from transformers import AutoTokenizer
from torch.utils.data import IterableDataset
from datasets import load_dataset

import fim

import numpy as np
import random


class CodeIterableDataset(IterableDataset):
    def __init__(self, dataset, tokenizer, max_seq_length=8192, content_field="input", fim_rate=0.5, fim_spm_rate=0.5, seed=2023):
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
        self.content_field = content_field
        
        self.seed = seed
        
        self.fim_rate = fim_rate
        self.fim_spm_rate = fim_spm_rate
        (
            self.suffix_tok_id,
            self.prefix_tok_id,
            self.middle_tok_id,
            self.pad_tok_id,
        ) = fim.get_fim_token_ids(self.tokenizer)
        if not self.suffix_tok_id and self.fim_rate > 0:
            print("FIM is not supported by tokenizer, disabling FIM")
            self.fim_rate = 0
        
    def prepare_sample_text(self, example):
        return example[self.content_field]
                
    def __iter__(self):
        iterator = iter(self.dataset)
        more_examples = True
        while more_examples:
            buffer = []
            while True:
                try:
                    buffer.append(self.prepare_sample_text(next(iterator)))
                    break
                except StopIteration:
                    if self.infinite:
                        iterator = iter(self.dataset)
                    else:
                        more_examples = False
                        break
            tokenized_inputs = self.tokenizer(buffer, truncation=False)["input_ids"][0]
            
            np_rng = np.random.RandomState(seed=self.seed)
            if self.fim_rate > 0:
                        tokenized_inputs, np_rng = fim.permute(
                            tokenized_inputs,
                            np_rng,
                            self.suffix_tok_id,
                            self.prefix_tok_id,
                            self.middle_tok_id,
                            self.pad_tok_id,
                            fim_rate=self.fim_rate,
                            fim_spm_rate=self.fim_spm_rate,
                            truncate_or_pad=False,
                        )
                    
            all_token_ids = []
            all_token_ids.extend(tokenized_inputs + [self.concat_token_id])
            if len(all_token_ids) >= self.seq_length:
                all_token_ids = all_token_ids[:self.seq_length]
            
            yield {
                "input_ids": all_token_ids,
                "labels": all_token_ids
            }
            
            
class ConstantLengthDataset(IterableDataset):
    """
    Iterable dataset that returns constant length chunks of tokens from stream of text files.
        Args:
            tokenizer (Tokenizer): The processor used for proccessing the data.
            dataset (dataset.Dataset): Dataset with text files.
            infinite (bool): If True the iterator is reset after dataset reaches end else stops.
            seq_length (int): Length of token sequences to return.
            num_of_sequences (int): Number of token sequences to keep in buffer.
            chars_per_token (int): Number of characters per token used to estimate number of tokens in text buffer.
            fim_rate (float): Rate (0.0 to 1.0) that sample will be permuted with FIM.
            fim_spm_rate (float): Rate (0.0 to 1.0) of FIM permuations that will use SPM.
            seed (int): Seed for random number generator.
    """

    def __init__(
        self,
        tokenizer,
        dataset,
        infinite=False,
        seq_length=1024,
        num_of_sequences=1024,
        chars_per_token=3.6,
        content_field="content",
        fim_rate=0.5,
        fim_spm_rate=0.5,
        seed=0,
        eos_token_id=49152,
    ):
        self.tokenizer = tokenizer
        self.concat_token_id = self.tokenizer.eos_token_id
        self.dataset = dataset
        self.seq_length = seq_length
        self.infinite = infinite
        self.current_size = 0
        self.max_buffer_size = seq_length * chars_per_token * num_of_sequences
        self.content_field = content_field
        self.fim_rate = fim_rate
        self.fim_spm_rate = fim_spm_rate
        self.seed = seed

        (
            self.suffix_tok_id,
            self.prefix_tok_id,
            self.middle_tok_id,
            self.pad_tok_id,
        ) = fim.get_fim_token_ids(self.tokenizer)
        if not self.suffix_tok_id and self.fim_rate > 0:
            print("FIM is not supported by tokenizer, disabling FIM")
            self.fim_rate = 0

    def __iter__(self):
        iterator = iter(self.dataset)
        more_examples = True
        while more_examples:
            buffer, buffer_len = [], 0
            while True:
                if buffer_len >= self.max_buffer_size:
                    break
                try:
                    buffer.append(next(iterator)[self.content_field])
                    buffer_len += len(buffer[-1])
                except StopIteration:
                    if self.infinite:
                        iterator = iter(self.dataset)
                    else:
                        more_examples = False
                        break
            tokenized_inputs = self.tokenizer(buffer, truncation=False)["input_ids"]
            all_token_ids = []
            
            np_rng = np.random.RandomState(seed=self.seed)
            for tokenized_input in tokenized_inputs:
                # optionally do FIM permutations
                if self.fim_rate > 0:
                    tokenized_input, np_rng = fim.permute(
                        tokenized_input,
                        np_rng,
                        self.suffix_tok_id,
                        self.prefix_tok_id,
                        self.middle_tok_id,
                        self.pad_tok_id,
                        fim_rate=self.fim_rate,
                        fim_spm_rate=self.fim_spm_rate,
                        truncate_or_pad=False,
                    )

                all_token_ids.extend(tokenized_input + [self.concat_token_id])
            examples = []
            
            for i in range(0, len(all_token_ids), self.seq_length):
                input_ids = all_token_ids[i : i + self.seq_length]
                if len(input_ids) == self.seq_length:
                    examples.append(input_ids)
            random.shuffle(examples)
            
            for example in examples:
                self.current_size += 1
                yield {
                        "input_ids": example,
                        "labels": example,
                    }
                
                
def test_iterable():
    model_path = '/root/pretrain/starcoder/starcoderbase'
    tokenizer = AutoTokenizer.from_pretrained(model_path, use_auth_token=True)
    
    dataset = load_dataset('json', data_files="test_code.json", streaming=True, split='train')
    dataset = dataset.shuffle(buffer_size=500, seed=2023)
    dataset = CodeIterableDataset(dataset, tokenizer)
    for batch in dataset:
        print(batch)
        break
    
def test_constant_length():
    model_path = '/root/pretrain/starcoder/starcoderbase'
    tokenizer = AutoTokenizer.from_pretrained(model_path, use_auth_token=True)
    dataset = load_dataset('json', data_files="test_code.json", split='train', streaming=False)
    dataset = ConstantLengthDataset(tokenizer, dataset, infinite=True, content_field='input', seq_length=20)
    
    for batch in dataset:
        print(batch)
        break


if __name__ == "__main__":
    test_iterable()
    test_constant_length()
