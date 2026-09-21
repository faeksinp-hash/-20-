import sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Views/MainWindow.xaml', 'r', encoding='utf-8') as f:
    lines = f.readlines()

overlays = []
current = None
for i, l in enumerate(lines[1160:], start=1161):
    if "MODAL OVERLAY" in l or "IN-PAGE" in l:
        if current:
            overlays.append(current)
        current = {"header": l.strip(), "start": i, "lines": []}
    if current:
        current["lines"].append((i, l))
if current:
    overlays.append(current)

print(f"Found {len(overlays)} modal overlays:")
for idx, o in enumerate(overlays):
    print(f"\n[{idx+1}] {o['header']} (from L{o['start']})")
    for line_num, l in o["lines"][:25]:
        if any(w in l for w in ["TextBlock", "TextBox", "ComboBox", "Button", "DataGrid", "Title", "Text="]):
            print(f"   L{line_num}: {l.strip()[:90]}")
