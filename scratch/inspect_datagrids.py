import re
import sys
sys.stdout.reconfigure(encoding='utf-8')

for fname in ['StockInView.xaml', 'ManagementView.xaml']:
    print(f"==================== {fname} ====================")
    with open(f"Views/{fname}", "r", encoding="utf-8") as f:
        lines = f.readlines()
    for i, l in enumerate(lines):
        if "<DataGrid" in l:
            print(f"DataGrid at L{i+1}:")
            for j in range(i, min(len(lines), i+30)):
                print(f"  L{j+1}: {lines[j]}", end="")
                if ">" in lines[j]:
                    break
        if "<ScrollViewer" in l:
            print(f"ScrollViewer at L{i+1}: {l.strip()}")
