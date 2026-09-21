import re
import sys
sys.stdout.reconfigure(encoding='utf-8')

def analyze_view(filename):
    print(f"================== {filename} ==================")
    with open(f"Views/{filename}", "r", encoding="utf-8") as f:
        content = f.read()

    # Find styles
    styles = re.findall(r'<Style\s+(?:x:Key="([^"]+)")?[^>]*TargetType="([^"]+)"(?:[^>]*BasedOn="([^"]+)")?', content)
    print("Styles:")
    for s in styles:
        print("  Key:", s[0], "TargetType:", s[1], "BasedOn:", s[2])
        
    # Check headers (Title / Subtitle)
    print("\nPage Header:")
    # look around line with Page Header or top Grid
    lines = content.splitlines()
    for i, line in enumerate(lines[:600]):
        if any(w in line for w in ["จัดการข้อมูลลูกค้า", "จัดการข้อมูลพนักงาน", "จัดการข้อมูลคู่ค้า", "จัดการค่าใช้จ่าย", "ระบบรายงานและวิเคราะห์"]):
            for j in range(max(0, i-5), min(len(lines), i+15)):
                print(f"  L{j+1}: {lines[j]}")
            break

    # Look for KPI cards
    print("\nKPI Cards:")
    for i, line in enumerate(lines):
        if "KpiCardStyle" in line:
            for j in range(i, min(len(lines), i+40)):
                print(f"  L{j+1}: {lines[j]}")
            break

    # Look for Search / Action Bar
    print("\nSearch / Action Bar:")
    for i, line in enumerate(lines):
        if "ค้นหา" in line:
            for j in range(max(0, i-10), min(len(lines), i+30)):
                print(f"  L{j+1}: {lines[j]}")
            break

    # Look for DataGrid
    print("\nDataGrid:")
    for i, line in enumerate(lines):
        if "<DataGrid" in line and "x:Name" in line:
            for j in range(i, min(len(lines), i+25)):
                print(f"  L{j+1}: {lines[j]}")
            break

for f in ['CustomerView.xaml', 'StaffView.xaml', 'PartnerView.xaml', 'ExpenseView.xaml']:
    analyze_view(f)
