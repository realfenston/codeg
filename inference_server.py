# -*- coding: UTF-8 -*-
"""
pip install flask==2.2.3
"""

from flask import Flask
from flask import request
from flask import make_response, jsonify
import threading
from datetime import datetime
import time

from CGModel import SantaCoderModel, StarCoderModel, StarCoderBaseModel, LoraFinetunedModel, LoraFinetunedModelInt8


################################
app = Flask(__name__)
bind_ip = "0.0.0.0"
port = 8192
endpoint = "/star/inference"
model = LoraFinetunedModelInt8()

block_ips = {}
cached_output = {}
lock = threading.Lock()


def block_repeated_requests(ip_address) -> bool:
    with lock:
        if ip_address not in block_ips:
            block_ips[ip_address] = datetime.now()
            return False
        else:
            time_in_seconds = (datetime.now() - block_ips[ip_address]).total_seconds()
            print('*' * 30, 'time elapsed since last request:', time_in_seconds)
            if time_in_seconds < 5:
                return True
            else:
                block_ips[ip_address] = datetime.now()
                return False
                

@app.route('/debugger', methods=['POST'])
def debugger():
    if block_repeated_requests(request.remote_addr):
        return make_response("OverLoaded", 500)


@app.route(endpoint, methods=['POST'])
def route_chat():
    """
    request.json example: {"inputs": "", "parameters": {max_new_tokens:256}}
    @return: response
    """
    
    if block_repeated_requests(request.remote_addr):
        time.sleep(1)
        # return make_response(jsonify({"generated_text": cached_output[request.remote_addr]}), 500)
        return make_response('Overloaded', 500)
    
    data = request.json

    if not data or set(data.keys()) != {"inputs", "parameters"}:
        return make_response("invalid request body", 400)
    
    try:
        new_code = code_generate(inputs=data['inputs'], **data['parameters'])
        response = {"generated_text": new_code}
        with lock:
            cached_output[request.remote_addr] = new_code
            block_ips[request.remote_addr] = datetime.now()
        return make_response(jsonify(response), 200)
    except Exception as e:
        return make_response(f"System error: {e}", 500)


def code_generate(inputs, **kwargs):
    # print(str(inputs))
    new_code = model.generate(inputs=inputs)
    return new_code


if __name__ == '__main__':
    app.run(host=bind_ip, port=port)

