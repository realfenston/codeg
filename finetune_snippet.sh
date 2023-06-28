#!/bin/bash

WORLD_SIZE=1 # size of GPUs to use

DISTRIBUTED_ARGS="--nproc_per_node $WORLD_SIZE \
                  --nnodes 1 \
                  --node_rank 0 \
                  --master_addr localhost \
                  --master_port 6000"

TRAIN_DATA="data/glue_data/MNLI/train.tsv"
VALID_DATA="data/glue_data/MNLI/dev_matched.tsv \
            data/glue_data/MNLI/dev_mismatched.tsv"
PRETRAINED_CHECKPOINT=checkpoints/bert_345m
VOCAB_FILE=/data/starcoder_70g/vocab.json
CHECKPOINT_PATH=/data/starcoder_70g/
TOKENIZER_PATH=/data/starcoder_70g/

torchrun ./finetune/finetune_snippet.py \
               --seed 1234 \
               --tokenizer-path $TOKENIZER_PATH \
               --vocab-file $VOCAB_FILE \
               --epochs 5 \
               --pretrained-checkpoint $PRETRAINED_CHECKPOINT \
               --tensor-model-parallel-size 1 \
               --num-layers 24 \
               --hidden-size 1024 \
               --num-attention-heads 16 \
               --micro-batch-size 8 \
               --activations-checkpoint-method uniform \
               --lr 5.0e-5 \
               --lr-decay-style linear \
               --lr-warmup-fraction 0.065 \
               --seq-length 512 \
               --max-position-embeddings 512 \
               --save-interval 500000 \
               --save 1000 \
               --log-interval 10 \
               --eval-interval 100 \
               --eval-iters 50 \
               --weight-decay 1.0e-1 \
               --fp16
