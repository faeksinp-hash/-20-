import sys
sys.stdout.reconfigure(encoding='utf-8')
with open('Views/CustomerView.xaml', 'r', encoding='utf-8') as f:
    c_lines = f.readlines()

for i, l in enumerate(c_lines):
    if "CustomerColumnHeaderStyle" in l:
        print("DataGrid styles:")
        for k in range(i-2, min(len(c_lines), i+120)):
            print(f"  {k+1}: {c_lines[k]}", end="")
        break
