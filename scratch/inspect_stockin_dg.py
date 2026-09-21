import sys
sys.stdout.reconfigure(encoding='utf-8')

with open("Views/StockInView.xaml", "r", encoding="utf-8") as f:
    lines = f.readlines()

for i, l in enumerate(lines):
    if "<DataGrid " in l:
        print(f"DataGrid at L{i+1}:")
        for j in range(i, min(len(lines), i+25)):
            print(f"  {lines[j]}", end="")
            if ">" in lines[j]:
                break
