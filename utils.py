import json
import os

# 模块配置， 默认启用lora
enable_deepspeed = False
enable_ptv2 = False
enable_lora = True
enable_int8 = False # qlora int8
enable_int4 = False # qlora int4


def get_deepspeed_config(train_info_args):
    '''
        lora prompt finetuning 使用 deepspeed_offload.json
        普通finetuning 使用deepspeed.json
    '''
    # 是否开启deepspeed
    if not enable_deepspeed:
        return None

    # 选择 deepspeed 配置文件
    is_need_update_config = False
    if enable_lora:
        is_need_update_config = True
        filename = os.path.join(os.path.dirname(__file__), 'deepspeed_offload.json')
    else:
        filename = os.path.join(os.path.dirname(__file__), 'deepspeed.json')


    with open(filename, mode='r', encoding='utf-8') as f:
        deepspeed_config = json.loads(f.read())

    #lora offload 同步优化器配置
    if is_need_update_config:
        optimizer = deepspeed_config.get('optimizer', None)
        if optimizer:
            optimizer['params']['betas'] = train_info_args.get('optimizer_betas', (0.9, 0.999))
            optimizer['params']['lr'] = train_info_args.get('learning_rate', 2e-5)
            optimizer['params']['eps'] = train_info_args.get('adam_epsilon', 1e-8)
    return deepspeed_config