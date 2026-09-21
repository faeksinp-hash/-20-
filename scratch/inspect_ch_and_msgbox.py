import sys
sys.stdout.reconfigure(encoding='utf-8')

def inspect_file(fname):
    print(f"\n====================== {fname} ======================")
    with open(f"Views/{fname}", "r", encoding="utf-8") as f:
        lines = f.readlines()
    for i, l in enumerate(lines):
        if any(w in l for w in ["Title", "FontSize=", "Height=", "RowHeight=", "<Button", "ColumnHeaderStyle", "RowStyle", "CellStyle"]):
            print(f"  L{i+1}: {l.strip()[:100]}")

inspect_file("CustomerHistoryWindow.xaml")
inspect_file("CustomMessageBox.xaml")
