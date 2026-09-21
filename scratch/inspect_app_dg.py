import re
import sys
sys.stdout.reconfigure(encoding='utf-8')

with open('App.xaml', 'r', encoding='utf-8') as f:
    text = f.read()

styles = re.findall(r'<Style\b[^>]*TargetType="DataGrid"[^>]*>.*?</Style>', text, re.DOTALL)
print("DataGrid styles in App.xaml count:", len(styles))
for s in styles:
    print(s)
