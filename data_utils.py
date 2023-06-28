import json
import sys
import requests

from datasets import load_dataset
from transformers import AutoConfig, AutoModelForCausalLM, AutoTokenizer


def load_code_and_crop(dataset_path, dataset_type='json', test_split_size=0.1, input_column='input', crop_length=20, output="./code_for_generation.json"):
    dataset = load_dataset(dataset_type, data_files=dataset_path, split='train', streaming=False)
    
    checkpoint = "/root/pretrain/starcoder/starcoderbase/"
    tokenizer = AutoTokenizer.from_pretrained(checkpoint, trust_remote_code=True)
        
    for i, sample in enumerate(dataset):
        inputs = sample[input_column]
        token_ids = tokenizer.encode(inputs)
        if len(token_ids) < 1000 or len(token_ids) > 8000:
            continue
        code_length = len(token_ids)
        cropped_inputs = token_ids[:int(code_length/3)] + token_ids[int(code_length/3)+crop_length:]
        
        try:
            _, FIM_PREFIX, FIM_MIDDLE, FIM_SUFFIX, FIM_PAD = tokenizer.special_tokens_map[
                "additional_special_tokens"
                ][:5]
            suffix_tok_id, prefix_tok_id, middle_tok_id, pad_tok_id = (
                tokenizer.vocab[tok] for tok in [FIM_SUFFIX, FIM_PREFIX, FIM_MIDDLE, FIM_PAD]
            )
        except KeyError:
            suffix_tok_id, prefix_tok_id, middle_tok_id, pad_tok_id = None, None, None, None
        
        starcoder_inputs = []
        starcoder_inputs.extend([prefix_tok_id])
        starcoder_inputs.extend(token_ids[:int(code_length/3)])
        starcoder_inputs.extend([suffix_tok_id])
        starcoder_inputs.extend(token_ids[int(code_length/3)+crop_length:])
        starcoder_inputs.extend([middle_tok_id])
        # starcoder_inputs.extend(prefix_tok_id + token_ids[:int(code_length/3)] + suffix_tok_id + token_ids[int(code_length/3)+crop_length:] + middle_tok_id)
        
        with open(output, 'a') as f:
            json.dump({"inputs": inputs, "cropped_inputs": tokenizer.decode(cropped_inputs), "starcoder_inputs": tokenizer.decode(starcoder_inputs)}, f)
            f.write('\n')
            

# 测试inference的结果, inference的耗时
def load_for_inference(dataset_path, dataset_type='json', input_column='starcoder_inputs'):
    dataset = load_dataset(dataset_type, data_files=dataset_path, split='train', streaming=False)
    for sample in dataset:
        inputs = sample[input_column]
        yield inputs
        

if __name__ == '__main__':
    function_name = sys.argv[1]
    function_dict = {
        'load_for_inference': load_for_inference,
        'load_code_and_crop': load_code_and_crop,
    }
    
    function = function_dict[function_name]
    
    if function_name == 'load_code_and_crop':
        dataset_path = sys.argv[2]
        function(dataset_path)
    elif function_name == 'load_for_inference':
        dataset_path = sys.argv[2]
        code_iterator = function_dict[function_name](dataset_path)
        api_url = "http://192.168.1.73:8192/star/inference"

        for i, inputs in enumerate(code_iterator):
            data = {"inputs": inputs, "parameters": {}}
            response = requests.post(api_url, json=data)
            with open(f'./output/{i}.cs', 'w') as f:
                f.write(json.loads(response.content.decode('utf-8'))['generated_text'])
            print(json.loads(response.content.decode('utf-8'))['generated_text'])