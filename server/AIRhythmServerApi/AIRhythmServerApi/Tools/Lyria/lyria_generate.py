import argparse
import base64
import json
import os
import urllib.request
import urllib.error
import subprocess
import subprocess, shutil
import google.auth
import google.auth.transport.requests

# def get_access_token():
#     gcloud = shutil.which("gcloud.cmd") or shutil.which("gcloud.exe") or shutil.which("gcloud")
#     if not gcloud:
#         raise RuntimeError("gcloud not found in PATH.")
#     cmd = [gcloud, "auth", "application-default", "print-access-token"]
#     if gcloud.lower().endswith((".cmd", ".bat")):
#         cmd = ["cmd", "/c"] + cmd

#     r = subprocess.run(cmd, capture_output=True, text=True)
#     if r.returncode != 0:
#         raise RuntimeError(f"Failed to get ADC access token: {r.stderr.strip()}")
#     return r.stdout.strip()

def get_access_token():
    # 로컬/서버 어디서든 자동으로 인증 정보를 찾습니다 (ADC 활용)
    credentials, project = google.auth.default()
    
    # 토큰이 없거나 만료되었다면 새로 고침
    auth_req = google.auth.transport.requests.Request()
    credentials.refresh(auth_req)
    
    return credentials.token

def main():
    p = argparse.ArgumentParser()
    p.add_argument("--project", required=True)
    p.add_argument("--location", required=True)
    p.add_argument("--prompt", required=True)
    p.add_argument("--out", required=True)
    p.add_argument("--sampleCount", type=int, default=1)
    p.add_argument("--durationSec", type=int, default=0)
    args = p.parse_args()

    token = get_access_token()

    url = (
        f"https://{args.location}-aiplatform.googleapis.com/v1/"
        f"projects/{args.project}/locations/{args.location}/publishers/google/models/lyria-002:predict"
    )

    body = {
        "instances": [{"prompt": args.prompt}],
        "parameters": {"sample_count": args.sampleCount}
    }

    req = urllib.request.Request(
        url=url,
        data=json.dumps(body).encode("utf-8"),
        method="POST",
        headers={
            "Authorization": f"Bearer {token}",
            "Content-Type": "application/json; charset=utf-8",
        },
    )

    try:
        with urllib.request.urlopen(req, timeout=180) as resp:
            raw = resp.read().decode("utf-8")
    except urllib.error.HTTPError as e:
        err = e.read().decode("utf-8", errors="ignore")
        raise RuntimeError(f"HTTPError {e.code}: {err}")

    obj = json.loads(raw)
    preds = obj.get("predictions") or []
    if not preds:
        raise RuntimeError(f"No predictions in response: {obj}")

    b64 = preds[0].get("bytesBase64Encoded")
    if not b64:
        raise RuntimeError(f"Missing bytesBase64Encoded: {preds[0]}")

    wav_bytes = base64.b64decode(b64)

    out_path = os.path.abspath(args.out)
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    with open(out_path, "wb") as f:
        f.write(wav_bytes)

    print(out_path)

if __name__ == "__main__":
    main()
