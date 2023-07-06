# -*- coding: UTF-8 -*-
from datetime import datetime

import torch
from accelerate import load_checkpoint_and_dispatch, init_empty_weights
from transformers import AutoConfig, AutoModelForCausalLM, AutoTokenizer
from peft import PeftModel, PeftConfig


class SantaCoderModel():
    def __init__(self):
        checkpoint = "/root/shihang/starcoder/pretrained/"
        self.tokenizer = AutoTokenizer.from_pretrained(checkpoint, trust_remote_code=True)
        self.device = "cuda"
        self.model = AutoModelForCausalLM.from_pretrained(checkpoint, trust_remote_code=True).to(self.device)

    def generate(self, inputs, max_new_tokens=250):
        inputs = self.tokenizer.encode(inputs, return_tensors="pt").to(self.device)
        outputs = self.model.generate(inputs, max_new_tokens=max_new_tokens, temperature=0.2, do_sample=True, top_p=0.95, stop=['<|endoftext|>'])
        print(self.tokenizer.decode(outputs[0]))
        return self.tokenizer.decode(outputs[0])
    

class StarCoderModel():
    def __init__(self):
        print("Loading the model")
        # disable caching mechanism when using gradient checkpointing
        
        checkpoint = '/root/shihang/starcoder/pretrained/'
        self.tokenizer = AutoTokenizer.from_pretrained(checkpoint, use_auth_token=True)
        config = AutoConfig.from_pretrained(checkpoint)
        self.device = 'cuda'

        with init_empty_weights():
            self.model = AutoModelForCausalLM.from_config(config)
        
        self.model = load_checkpoint_and_dispatch(self.model, checkpoint=checkpoint, device_map="auto", no_split_module_classes=["GPTBigCodeBlock"])
        self.model.eval()
        
    def generate(self, inputs):
        print('=' * 30, len(inputs))
        inputs = self.tokenizer.encode(inputs, return_tensors="pt").to(self.device)
        #outputs = self.model.generate(inputs, max_new_tokens=50, temperature=0.5, do_sample=True, top_p=0.95, pad_token_id=self.tokenizer.pad_token_id, bos_token_id=self.tokenizer.bos_token_id, eos_token_id=self.tokenizer.eos_token_id)
        
        t1 = datetime.now()
        outputs = self.model.generate(inputs, max_new_tokens=50, temperature=0.2, do_sample=True, top_p=0.95)
        t2 = datetime.now()
        print("time elapsed = ", t2 - t1)
        return self.tokenizer.decode(outputs[0], clean_up_tokenization_spaces=False)
    
    
class StarCoderBaseModel():
    def __init__(self):
        print("Loading the model")
        # disable caching mechanism when using gradient checkpointing
        
        checkpoint = '/root/pretrain/starcoder/checkpoints/checkpoint-8000/'
        self.tokenizer = AutoTokenizer.from_pretrained(checkpoint, use_auth_token=True)
        config = AutoConfig.from_pretrained(checkpoint)
        self.device = 'cuda'

        with init_empty_weights():
            self.model = AutoModelForCausalLM.from_config(config)
        
        self.model = load_checkpoint_and_dispatch(self.model, checkpoint=checkpoint, device_map="auto", no_split_module_classes=["GPTBigCodeBlock"])
        self.model = torch.compile(self.model)
        self.model.half() # to save memory
        self.model.eval()
        
    def generate(self, inputs):
        print('=' * 30, len(inputs))
        inputs = self.tokenizer.encode(inputs, return_tensors="pt").to(self.device)
        #outputs = self.model.generate(inputs, max_new_tokens=50, temperature=0.5, do_sample=True, top_p=0.95, pad_token_id=self.tokenizer.pad_token_id, bos_token_id=self.tokenizer.bos_token_id, eos_token_id=self.tokenizer.eos_token_id)
        
        t1 = datetime.now()
        outputs = self.model.generate(inputs, max_new_tokens=50, temperature=0.2, do_sample=True, top_p=0.95, pad_token_id=0)
        t2 = datetime.now()
        print("time elapsed = ", t2 - t1)
        
        return self.tokenizer.decode(outputs[0], clean_up_tokenization_spaces=False)


class LoraFinetunedModel():
    def __init__(self, model_path, peft_model_path):
        print("Loading the model")
        # disable caching mechanism when using gradient checkpointing
        
        checkpoint = model_path
        peft_model_checkpoint = peft_model_path
        
        self.tokenizer = AutoTokenizer.from_pretrained(checkpoint, use_auth_token=True)
        config = AutoConfig.from_pretrained(checkpoint)
        self.device = 'cuda'

        with init_empty_weights():
            self.model = AutoModelForCausalLM.from_config(config)
        self.model.tie_weights()
        
        self.model = load_checkpoint_and_dispatch(self.model, checkpoint=checkpoint, device_map="auto", no_split_module_classes=["GPTBigCodeBlock"])
        # model = AutoModelForCausalLM.from_pretrained(checkpoint, device_map="auto", offload_folder="offload", offload_state_dict=True, torch_dtype=torch.float32)
        
        # peft_config = PeftConfig.from_pretrained(peft_model_checkpoint)
        self.model = PeftModel.from_pretrained(self.model, peft_model_checkpoint)
        self.model = self.model.merge_and_unload()
        
        self.model = torch.compile(self.model)
        self.model.half().eval()
        
    def generate(self, inputs):
        print('=' * 30, len(inputs))
        inputs = self.tokenizer.encode(inputs, return_tensors="pt").to(self.device)
        # outputs = self.model.generate(inputs, max_new_tokens=50, temperature=0.5, do_sample=True, top_p=0.95, pad_token_id=self.tokenizer.pad_token_id, bos_token_id=self.tokenizer.bos_token_id, eos_token_id=self.tokenizer.eos_token_id)
        
        # outputs = self.model.generate({inputs, max_new_tokens=50, temperature=0.2, do_sample=True, top_p=0.95, pad_token_id=0})
        outputs = self.model.generate(**{
            'inputs': inputs,
            'max_new_tokens': 50,
            'top_p': 0.95,
            'temperature': 0.4,
            'do_sample': True,
            'pad_token_id': 0, 
        })
        # outputs = self.model.generate()
        print("time elapsed = ", t2 - t1)
        
        return self.tokenizer.decode(outputs[0], clean_up_tokenization_spaces=False)
    

class LoraFinetunedModelInt8():
    def __init__(self, model_path, peft_model_path):
        print("Loading the model", type(self).__name__)
        # disable caching mechanism when using gradient checkpointing
        
        checkpoint = model_path
        peft_model_checkpoint = peft_model_path
        
        self.tokenizer = AutoTokenizer.from_pretrained(checkpoint, use_auth_token=True)
        config = AutoConfig.from_pretrained(checkpoint)
        self.device = 'cuda'

        with init_empty_weights():
            self.model = AutoModelForCausalLM.from_config(config)
        self.model.tie_weights()
        
        self.model = load_checkpoint_and_dispatch(self.model, checkpoint=checkpoint, device_map="auto", no_split_module_classes=["GPTBigCodeBlock"], dtype=torch.float32)
        # model = AutoModelForCausalLM.from_pretrained(checkpoint, device_map="auto", offload_folder="offload", offload_state_dict=True, torch_dtype=torch.float32)
        
        peft_config = PeftConfig.from_pretrained(peft_model_checkpoint)
        self.model = PeftModel.from_pretrained(self.model, peft_model_checkpoint)
        
        self.model = torch.compile(self.model)
        self.model = torch.ao.quantization.quantize_dynamic(
            self.model,
            dtype = torch.qint8,
        )
        
    def generate(self, inputs):
        print('=' * 30, len(inputs))
        inputs = self.tokenizer.encode(inputs, return_tensors="pt").to(self.device)
        #outputs = self.model.generate(inputs, max_new_tokens=50, temperature=0.5, do_sample=True, top_p=0.95, pad_token_id=self.tokenizer.pad_token_id, bos_token_id=self.tokenizer.bos_token_id, eos_token_id=self.tokenizer.eos_token_id)
        
        t1 = datetime.now()
        # outputs = self.model.generate({inputs, max_new_tokens=50, temperature=0.2, do_sample=True, top_p=0.95, pad_token_id=0})
        outputs = self.model.generate(**{
            'inputs': inputs,
            'max_new_tokens': 50,
            'top_p': 0.95,
            'temperature': 0.5,
            'do_sample': True,
            'pad_token_id': 0, 
        })
        # outputs = self.model.generate()
        t2 = datetime.now()
        print("time elapsed = ", t2 - t1)
        
        return self.tokenizer.decode(outputs[0], clean_up_tokenization_spaces=False)