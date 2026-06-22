#!/usr/bin/env python3
import json, sys
with open(sys.argv[1], encoding='utf-16') as f:
    data = json.load(f)
for i in data['items']:
    if i.get('status') != 'Done':
        c = i.get('content', {})
        print(f"{i.get('status')}|{c.get('number', '?')}|{i.get('title', '?')}")
