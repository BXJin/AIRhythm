import argparse
import json
import os
import sys

def main():
    p = argparse.ArgumentParser()
    p.add_argument("--in", dest="wav_path", required=True)
    p.add_argument("--out", dest="notes_json_path", required=True)
    p.add_argument("--durationMs", dest="duration_ms", type=int, required=True)
    p.add_argument("--intervalMs", dest="interval_ms", type=int, default=500)
    p.add_argument("--leadInMs", dest="lead_in_ms", type=int, default=500) 
    args = p.parse_args()

    if not os.path.exists(args.wav_path):
        print(f"input wav not found: {args.wav_path}", file=sys.stderr)
        return 2

    duration_ms = max(1, args.duration_ms)
    interval = max(50, args.interval_ms)
    lead_in = max(0, args.lead_in_ms)

    notes = [{"t_ms": t} for t in range(lead_in, duration_ms, interval)]  

    payload = { "notes": notes }

    os.makedirs(os.path.dirname(os.path.abspath(args.notes_json_path)), exist_ok=True)
    with open(args.notes_json_path, "w", encoding="utf-8") as f:
        json.dump(payload, f, ensure_ascii=False, indent=2)

    return 0

if __name__ == "__main__":
    raise SystemExit(main())
