import sys
sys.stdout.reconfigure(encoding='utf-8')

files = ['CustomerHistoryWindow.xaml', 'DeliverySlipWindow.xaml', 'POReceiptWindow.xaml', 'ReceiptWindow.xaml', 'CustomMessageBox.xaml']

for fname in files:
    print(f"\n==================== {fname} ====================")
    with open(f"Views/{fname}", "r", encoding="utf-8") as f:
        lines = f.readlines()
    print(f"Total lines: {len(lines)}")
    for i, l in enumerate(lines):
        if any(w in l for w in ["FontSize=", "Height=", "Width=", "<Button", "<DataGrid", "Title="]):
            if any(k in l for k in ["FontSize=\"1", "FontSize=\"2", "Height=\"3", "Height=\"4", "Height=\"5", "DataGrid", "Title="]):
                print(f"  L{i+1}: {l.strip()[:100]}")
