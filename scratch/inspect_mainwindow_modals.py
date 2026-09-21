import sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Views/MainWindow.xaml', 'r', encoding='utf-8') as f:
    m_lines = f.readlines()

print(f"MainWindow total lines: {len(m_lines)}")

# Look for modals starting around line 1160
for i in range(1160, len(m_lines)):
    line = m_lines[i]
    if any(k in line for k in ["<!-- Modal", "<!-- In-Page", "<!-- ==", "MODAL", "Overlay", "Visibility="]):
        if "Visibility=" in line and "Modal" in line:
            print(f"  {i+1}: {line.strip()[:100]}")
        elif "MODAL" in line or "<!-- Modal" in line:
            print(f"  {i+1}: {line.strip()[:100]}")
