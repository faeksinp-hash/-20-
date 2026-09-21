import sys
import re
sys.stdout.reconfigure(encoding='utf-8')

with open('Views/MainWindow.xaml', 'r', encoding='utf-8') as f:
    text = f.read()

# Let's inspect overlay 1: Customer Modal (around L1166 - 1320)
def inspect_range(start_pat, end_pat, name):
    print(f"\n=================== {name} ===================")
    m1 = re.search(start_pat, text)
    m2 = re.search(end_pat, text)
    if not m1 or not m2:
        print("Pattern not found!")
        return
    chunk = text[m1.start():m2.start()]
    lines = chunk.splitlines()
    for l in lines:
        if any(keyword in l for keyword in ["FontSize=", "Height=", "<Button", "<TextBox", "<ComboBox", "DataGrid"]):
            print("  ", l.strip()[:100])

inspect_range(r'ROOT LEVEL FULL-SCREEN MODAL OVERLAY 1: ADD/EDIT CUSTOMER', r'ROOT LEVEL FULL-SCREEN MODAL OVERLAY 2: CUSTOMER HISTORY', '1. Customer Modal')
inspect_range(r'ROOT LEVEL FULL-SCREEN MODAL OVERLAY 2: CUSTOMER HISTORY', r'ROOT LEVEL FULL-SCREEN MODAL OVERLAY: ADD / EDIT STAFF', '2. Customer History Modal')
inspect_range(r'ROOT LEVEL FULL-SCREEN MODAL OVERLAY: ADD / EDIT STAFF', r'ROOT LEVEL FULL-SCREEN MODAL OVERLAY: ADD / EDIT PARTNER', '3. Staff Modal')
inspect_range(r'ROOT LEVEL FULL-SCREEN MODAL OVERLAY: ADD / EDIT PARTNER', r'ROOT LEVEL FULL-SCREEN MODAL OVERLAY: ADD / EDIT EXPENSE', '4. Partner Modal')
inspect_range(r'ROOT LEVEL FULL-SCREEN MODAL OVERLAY: ADD / EDIT EXPENSE', r'ROOT LEVEL FULL-SCREEN MODAL OVERLAY: ADD / EDIT PRODUCT', '5. Expense Modal')
inspect_range(r'ROOT LEVEL FULL-SCREEN MODAL OVERLAY: ADD / EDIT PRODUCT', r'ROOT LEVEL FULL-SCREEN MODAL OVERLAY: ADD / EDIT CATEGORY', '6. Product Modal')
inspect_range(r'ROOT LEVEL FULL-SCREEN MODAL OVERLAY: ADD / EDIT CATEGORY', r'UNIVERSAL TOP-LEVEL IN-PAGE CONFIRMATION MODAL OVERLAY', '7. Category Modal')
inspect_range(r'UNIVERSAL TOP-LEVEL IN-PAGE CONFIRMATION MODAL OVERLAY', r'UNIVERSAL TOP-LEVEL IN-PAGE ALERT MODAL OVERLAY', '8. Confirmation Modal')
inspect_range(r'UNIVERSAL TOP-LEVEL IN-PAGE ALERT MODAL OVERLAY', r'</Grid>\s*</Grid>\s*</Window>', '9. Alert Modal')
