import sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Views/ReportsView.xaml', 'r', encoding='utf-8') as f:
    r_lines = f.readlines()

print(f"ReportsView total lines: {len(r_lines)}")

# 1. Styles
print("\n--- Styles defined in ReportsView ---")
for i, l in enumerate(r_lines[:450]):
    if "<Style " in l:
        print(f"  {i+1}: {l.strip()}")

# 2. Header
print("\n--- Header in ReportsView ---")
for i, l in enumerate(r_lines):
    if "Row 0" in l or "Page Header" in l:
        for k in range(i, min(len(r_lines), i+30)):
            print(f"  {k+1}: {r_lines[k]}", end="")
        break

# 3. KPI Cards
print("\n--- KPI Cards in ReportsView ---")
for i, l in enumerate(r_lines):
    if "Row 1" in l or "KPI Summary" in l:
        for k in range(i, min(len(r_lines), i+60)):
            print(f"  {k+1}: {r_lines[k]}", end="")
        break

# 4. Filter Bar
print("\n--- Filter Bar in ReportsView ---")
for i, l in enumerate(r_lines):
    if "Filter Bar" in l or "Row 2" in l or "Filter Section" in l:
        for k in range(i, min(len(r_lines), i+70)):
            print(f"  {k+1}: {r_lines[k]}", end="")
        break
