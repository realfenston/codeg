from accelerate import load_checkpoint_and_dispatch
from accelerate import init_empty_weights
from transformers import AutoConfig, AutoModelForCausalLM
from transformers import AutoTokenizer

checkpoint = "/data/starcoder_70g"
config = AutoConfig.from_pretrained(checkpoint)

with init_empty_weights():
    model = AutoModelForCausalLM.from_config(config)

model = load_checkpoint_and_dispatch(model, checkpoint=checkpoint, device_map="auto", no_split_module_classes=["GPTBigCodeBlock"])
model.hf_device_map

tokenizer = AutoTokenizer.from_pretrained(checkpoint)

device = "cuda"
input = "def print_hello_world():"
inputs = tokenizer.encode(input, return_tensors="pt").to(device)
outputs = model.generate(inputs)
print(input)
print(outputs[0])
print(tokenizer.decode(outputs[0]))