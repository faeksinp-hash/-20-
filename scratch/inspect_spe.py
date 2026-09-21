import sys
sys.stdout.reconfigure(encoding='utf-8')

for name in ['StaffView', 'PartnerView', 'ExpenseView']:
    print(f"=== {name} ===")
    with open(f'Views/{name}.xaml', 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    # Check headers
    for i, l in enumerate(lines):
        if "Row 0" in l:
            for k in range(i, min(len(lines), i+30)):
                if "TextBlock Text=" in lines[k]:
                    print(f"  Header: {lines[k].strip()}")
            break
            
    # Check KPI labels
    for i, l in enumerate(lines):
        if "KPI Card 1" in l:
            for k in range(i, min(len(lines), i+80)):
                if "FontSize=" in lines[k] and "TextBlock" in lines[k]:
                    print(f"  KPI Text: {lines[k].strip()}")
            break

    # Check Buttons & Search
    for i, l in enumerate(lines):
        if "Search Box" in l or "Search Input" in l or "Row 2" in l:
            for k in range(i, min(len(lines), i+50)):
                if "Height=" in lines[k] and ("TextBox" in lines[k] or "Button" in lines[k]):
                    print(f"  Search/Action: {lines[k].strip()}")
            break
