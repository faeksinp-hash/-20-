import sys
sys.stdout.reconfigure(encoding='utf-8')

for fname in ['DeliverySlipWindow.xaml', 'POReceiptWindow.xaml', 'ReceiptWindow.xaml']:
    print(f"\n==================== {fname} ====================")
    with open(f"Views/{fname}", "r", encoding="utf-8") as f:
        print(f.read())
