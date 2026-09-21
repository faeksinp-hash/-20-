import sys
sys.stdout.reconfigure(encoding='utf-8')
with open('Views/CustomerView.xaml', 'r', encoding='utf-8') as f:
    c_lines = f.readlines()

for i, l in enumerate(c_lines):
    if "ROW 2" in l or "ROW 3" in l or "DataGrid Table" in l:
        print(f"Around {i+1}:")
        for k in range(i, min(len(c_lines), i+60)):
            print(f"  {k+1}: {c_lines[k]}", end="")
        print("\n" + "="*40 + "\n")
