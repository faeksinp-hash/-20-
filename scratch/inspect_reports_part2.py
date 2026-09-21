import sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Views/ReportsView.xaml', 'r', encoding='utf-8') as f:
    r_lines = f.readlines()

# Filter Bar (lines 560 to 700)
print("=== Filter Bar ===")
for i in range(560, min(len(r_lines), 670)):
    print(f"  {i+1}: {r_lines[i]}", end="")

# Charts & Content (lines 670 to 1015)
print("\n=== Row 3 / Content Area ===")
for i in range(670, min(len(r_lines), 750)):
    print(f"  {i+1}: {r_lines[i]}", end="")
