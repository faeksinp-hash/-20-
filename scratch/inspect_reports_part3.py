import sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Views/ReportsView.xaml', 'r', encoding='utf-8') as f:
    r_lines = f.readlines()

print("=== Left Card / Right Card / Modal Overview ===")
for i in range(748, len(r_lines)):
    line = r_lines[i]
    if any(k in line for k in ["<!--  LEFT CARD", "<!--  RIGHT CARD", "<!--  OVERLAY", "<!-- Circular Modal", "<DataGrid", "FontSize=", "Height="]):
        if any(h in line for h in ["FontSize=\"1", "FontSize=\"2", "Height=\"3", "Height=\"4", "Height=\"5", "DataGrid", "LEFT CARD", "RIGHT CARD", "OVERLAY"]):
            print(f"  {i+1}: {line.strip()[:100]}")
