import sys
sys.stdout.reconfigure(encoding='utf-8')
with open('Views/CustomerView.xaml', 'r', encoding='utf-8') as f:
    c_lines = f.readlines()

print(f"CustomerView total lines: {len(c_lines)}")
# Print lines around header
for i, l in enumerate(c_lines):
    if "Page Header" in l or "Header" in l and "<Grid" in l:
        print(f"Customer Header around {i+1}")
        for k in range(i, min(len(c_lines), i+30)):
            print(f"  {k+1}: {c_lines[k]}", end="")
        break

# Print KPI cards
for i, l in enumerate(c_lines):
    if "KPI Cards" in l or "Cards Row" in l:
        print(f"\nCustomer KPI around {i+1}")
        for k in range(i, min(len(c_lines), i+80)):
            print(f"  {k+1}: {c_lines[k]}", end="")
        break
