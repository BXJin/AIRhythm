import argparse
import base64
import json
import os
import urllib.request
import urllib.error
import subprocess

from google.auth import default
credentials, project = default()
import google.auth.transport.requests
auth_req = google.auth.transport.requests.Request()
credentials.refresh(auth_req)
print(f"1. 프로젝트 ID: {project}")
print(f"2. 서비스 계정/이메일: {credentials.service_account_email if hasattr(credentials, 'service_account_email') else '개인 계정'}")
print(f"3. 토큰 유효 여부: {credentials.valid}")
print(f"4. 권한 범위(Scopes): {credentials.scopes}")
print(f"4. 토큰: {credentials.token}") 