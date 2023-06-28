import os
import json

def find_files(directory, extension):
    for dirpath, dirnames, filenames in os.walk(directory):
        for filename in filenames:
            if filename.endswith(extension):
                yield os.path.join(dirpath, filename)

# define directory: code repository
# define extension: files that need to be pre-processed
directory = "./totalfootballproduction_2022_3/"
extension = ".cs"

with open('gala_code_full.json', 'a') as json_f:
    for file_path in find_files(directory, extension):
        print(file_path)
        try:
            with open(file_path, 'r') as csharp_f:
                code_string = csha
                json_f.write(json.dumps({"input": code_string, "label": "csharp"}))
                json_f.write('\n')
        except:
            print(30 * '*', f'exception in file {file_path}')