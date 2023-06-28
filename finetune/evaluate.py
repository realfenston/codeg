from typing import List, Dict
import torch
import logging
from torch.utils.data import DataLoader
from tqdm import tqdm

logging.basicConfig(
    format=logging.BASIC_FORMAT,
    datefmt='%Y-%m-%d %H:%M:%S',
    level=logging.INFO
)
logger = logging.getLogger(__name__)

class SingleCLMEvaluator():
    def __init__(self, dataloader: DataLoader = None,
                 data_tag: str = "dev",
                 device: int = None, tokenizer=None, early_stop_on: str = "perplexity"):

        if data_tag not in ["dev", "train", "test"]:
            raise ValueError("data_tag has to be one of dev, train or test")
        assert early_stop_on in ["loss", "perplexity"]
        self.early_stop_on = early_stop_on
        self.dataloader = dataloader
        self.data_tag = data_tag
        self.tokenizer = tokenizer

        self.n_gpu = torch.cuda.device_count()
        self.device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
        if device == -1:
            self.n_gpu = 0
            self.device = torch.device("cpu")

    def reset_dataloader(self, dataloader: DataLoader):
        self.dataloader = dataloader

    def get_metrics(self):
        raise NotImplementedError

    def get_loss(self):
        raise NotImplementedError

    def reset_logger(self, output_path):
        pass