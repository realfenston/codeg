import requests


api_url = "http://192.168.1.73:8192/debugger"


for i in range(10):
    requests.post(api_url, json={})