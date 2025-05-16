from qcloud_cos import CosConfig
from qcloud_cos import CosS3Client
import sys
if len(sys.argv) < 2:
    print("No file need to upload, quit.")
    exit()
import logging

from datetime import datetime
now = datetime.now()
dt_string = now.strftime("%Y-%m-%d-%H-%M-%S")
name = sys.argv[1].replace(".", f"_{dt_string}.")

import time

logging.basicConfig(level=logging.ERROR, stream=sys.stdout)

secret_id = 'None'
secret_key = 'None'
region = 'ap-shanghai'
token = None
scheme = 'https'

config = CosConfig(Region=region, SecretId=secret_id, SecretKey=secret_key, Token=token, Scheme=scheme)
client = CosS3Client(config)

start_time = time.time()
print("Upload started. Take a cup of coffee.")
response = client.upload_file(
    Bucket='test-1255433865',
    LocalFilePath=sys.argv[1],
    Key=name,
    PartSize=4,
    MAXThread=6,
    EnableMD5=False
)
print(f"Upload finished, cost {(time.time() - start_time):.03f} seconds.")

url = client.get_object_url(
    Bucket='test-1255433865',
    Key=name
)
print(f"Download URL for {name}: {url}")
