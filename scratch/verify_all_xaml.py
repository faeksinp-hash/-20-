import os
import re
import sys
import xml.etree.ElementTree as ET
sys.stdout.reconfigure(encoding='utf-8')

xaml_files = []
for root, dirs, files in os.walk('.'):
    if any(ignore in root for ignore in ['bin', 'obj', '.git', '.vs', 'scratch']):
        continue
    for f in files:
        if f.endswith('.xaml'):
            xaml_files.append(os.path.join(root, f))

print(f"Total XAML files to verify: {len(xaml_files)}")
errors = 0

for path in sorted(xaml_files):
    with open(path, 'r', encoding='utf-8') as f:
        text = f.read()

    # 1. Baht symbol check
    if '฿' in text:
        print(f"[ERROR] Found ฿ symbol in {path}")
        errors += 1

    # 2. Check duplicate keys in UserControl.Resources or Window.Resources
    keys = re.findall(r'x:Key="([^"]+)"', text)
    seen = {}
    for k in keys:
        if 'SystemColors' in k or 'Static' in k:
            continue
        seen[k] = seen.get(k, 0) + 1
    dupes = [k for k, count in seen.items() if count > 1]
    if dupes:
        print(f"[ERROR] Duplicate x:Key in {path}: {dupes}")
        errors += 1

    # 3. Check for any tiny buttons (< 32px height) in non-close buttons
    tiny_btns = re.findall(r'<Button[^>]*?Height="([12]\d)"[^>]*?>', text)
    if tiny_btns:
        print(f"[WARN] Small buttons in {path}: {tiny_btns}")

if errors == 0:
    print("\n>>> ALL XAML VALIDATIONS PASSED PERFECTLY! 0 ERRORS! <<<")
else:
    print(f"\n[FAIL] Found {errors} validation errors.")
