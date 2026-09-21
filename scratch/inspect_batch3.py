import re

files = ['CustomerView.xaml', 'StaffView.xaml', 'PartnerView.xaml', 'ExpenseView.xaml']

for fname in files:
    path = f'Views/{fname}'
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    keys = re.findall(r'x:Key="([^"]+)"', content)
    print(f"=== {fname} ===")
    print("Keys:", keys)
    # Check buttons
    btns = re.findall(r'<Button[^>]*Height="([^"]+)"[^>]*FontSize="([^"]+)"', content)
    print(f"Button samples (Height, FontSize):", btns[:5])
    # Check KPI cards
    kpis = re.findall(r'(\d+pt)?[^\n]*<TextBlock[^>]*Text="([^"]+)"[^>]*FontSize="([^"]+)"', content)
    # Check DataGrid row height, header height
    dg_rows = re.findall(r'<DataGrid[^>]*RowHeight="([^"]+)"', content)
    print("DataGrid RowHeight:", dg_rows)
    # Check Header
    hdr = re.findall(r'<TextBlock[^>]*FontSize="(\d+)"[^>]*FontWeight="(?:Bold|SemiBold)"[^>]*Text="([^"]+)"', content)
    print("Headers:", hdr[:3])
