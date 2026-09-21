import re
import sys
sys.stdout.reconfigure(encoding='utf-8')

def update_mainwindow_modals():
    path = "Views/MainWindow.xaml"
    with open(path, "r", encoding="utf-8") as f:
        content = f.read()

    # Split before modal overlays:
    split_marker = "<!--  ROOT LEVEL FULL-SCREEN MODAL OVERLAY 1: ADD/EDIT CUSTOMER -->"
    if split_marker not in content:
        print("Split marker not found!")
        return

    top_content, modal_content = content.split(split_marker, 1)
    modal_content = split_marker + modal_content

    # 1. Form Labels: FontSize="13" FontWeight="SemiBold" Foreground="#475569" -> FontSize="15"
    modal_content = re.sub(
        r'(<TextBlock[^>]*?Text="[^"]+"[^>]*?FontSize=)"13"([^>]*?Foreground="#475569")',
        r'\g<1>"15"\g<2>',
        modal_content
    )
    modal_content = re.sub(
        r'(<TextBlock[^>]*?FontSize=)"13"([^>]*?Foreground="#475569"[^>]*?Text="[^"]+")',
        r'\g<1>"15"\g<2>',
        modal_content
    )
    # Also without Foreground check if it has FontWeight="SemiBold" or "Bold" and text has * or label
    modal_content = re.sub(
        r'(<TextBlock\s+Text="[^"]+\*"[^>]*?FontSize=)"13"',
        r'\g<1>"15"',
        modal_content
    )

    # 2. Input Heights & FontSizes in modals:
    # Single-line TextBox / ComboBox: Height="38" -> Height="44"
    modal_content = re.sub(
        r'(<(?:TextBox|ComboBox)[^>]*?Height=)"38"',
        r'\g<1>"44"',
        modal_content
    )
    modal_content = re.sub(
        r'(<(?:TextBox|ComboBox)[^>]*?Height=)"40"',
        r'\g<1>"44"',
        modal_content
    )
    # Also DatePicker: Height="38" -> Height="44"
    modal_content = re.sub(
        r'(<DatePicker[^>]*?Height=)"38"',
        r'\g<1>"44"',
        modal_content
    )
    # Inputs FontSize: FontSize="14" or "13" -> FontSize="16"
    modal_content = re.sub(
        r'(<TextBox[^>]*?Padding="12,0"[^>]*?FontSize=)"14"',
        r'\g<1>"16"',
        modal_content
    )
    modal_content = re.sub(
        r'(<TextBox[^>]*?Padding="12,0"[^>]*?FontSize=)"13"',
        r'\g<1>"16"',
        modal_content
    )
    modal_content = re.sub(
        r'(<DatePicker[^>]*?FontSize=)"13"',
        r'\g<1>"15"',
        modal_content
    )

    # Multi-line TextBoxes: Height="70" -> Height="76", FontSize="13" -> FontSize="15"
    modal_content = re.sub(
        r'(<TextBox[^>]*?Height=)"70"(\s+Padding="12,8"\s+FontSize=)"13"',
        r'\g<1>"76"\g<2>"15"',
        modal_content
    )
    # Category description: Height="60" -> Height="76", FontSize="13" -> FontSize="15"
    modal_content = re.sub(
        r'(<TextBox[^>]*?Height=)"60"(\s+Padding="12,8"\s+FontSize=)"13"',
        r'\g<1>"76"\g<2>"15"',
        modal_content
    )

    # 3. Modal Action Buttons: Height="42" FontSize="15" -> Height="46" FontSize="16"
    modal_content = re.sub(
        r'(<Button[^>]*?Height=)"42"(\s+FontSize=)"15"',
        r'\g<1>"46"\g<2>"16"',
        modal_content
    )
    modal_content = re.sub(
        r'(<Button[^>]*?Height=)"42"(\s+Padding="[^"]*"\s+FontSize=)"15"',
        r'\g<1>"46"\g<2>"16"',
        modal_content
    )

    # 4. Confirmation & Alert Modal Buttons & Text:
    # Buttons with Height="42"
    modal_content = re.sub(
        r'(<Button\s+Grid\.Column="[02]"\s+Command="\{Binding [^\}]+\}"\s+Content="[^"]+"\s+Background="[^"]+"\s+Cursor="Hand"\s+Padding="[^"]*"\s+Height=)"42"',
        r'\g<1>"46" FontSize="16" FontWeight="SemiBold"',
        modal_content
    )
    modal_content = re.sub(
        r'(<Button\s+Content="ตกลง"\s+Command="\{Binding CloseMainAlertModalCommand\}"[^>]*?Height=)"42"',
        r'\g<1>"46" FontSize="16" FontWeight="SemiBold"',
        modal_content
    )
    # Confirmation & Alert Text: FontSize="14" -> FontSize="15"
    modal_content = re.sub(
        r'(<TextBlock\s+Text="\{Binding MainConfirmMessage[^\}]*\}"\s+FontSize=)"14"',
        r'\g<1>"15"',
        modal_content
    )
    modal_content = re.sub(
        r'(<TextBlock\s+Text="\{Binding MainAlertMessage[^\}]*\}"\s+FontSize=)"14"',
        r'\g<1>"15"',
        modal_content
    )

    # 5. Customer History Modal (Overlay 2):
    # Header Icon 20 -> 22, Title 18 -> 20, Subtitle 12.5 -> 14
    modal_content = modal_content.replace(
        '<TextBlock Text="📜 " FontSize="20"/>\n                            <StackPanel Margin="10,0,0,0">\n                                <TextBlock Text="{Binding CustomerHistoryTitle, FallbackValue=\'📜 ประวัติการซื้อสินค้า\'}"\n                                           FontSize="18"',
        '<TextBlock Text="📜 " FontSize="22"/>\n                            <StackPanel Margin="10,0,0,0">\n                                <TextBlock Text="{Binding CustomerHistoryTitle, FallbackValue=\'📜 ประวัติการซื้อสินค้า\'}"\n                                           FontSize="20"'
    )
    modal_content = modal_content.replace(
        'Text="รายการคำสั่งซื้อย้อนหลังทั้งหมดของสมาชิกท่านนี้"\n                                           FontSize="12.5"',
        'Text="รายการคำสั่งซื้อย้อนหลังทั้งหมดของสมาชิกท่านนี้"\n                                           FontSize="14"'
    )
    # Inset customer details
    modal_content = modal_content.replace(
        '<TextBlock Text="👤 ชื่อลูกค้า:" FontSize="13" FontWeight="SemiBold" Foreground="#718096"/>\n                                        <TextBlock Text="{Binding SelectedCustomerForHistory.Name, FallbackValue=\'-\'}"\n                                                   FontSize="14"',
        '<TextBlock Text="👤 ชื่อลูกค้า:" FontSize="14.5" FontWeight="SemiBold" Foreground="#718096"/>\n                                        <TextBlock Text="{Binding SelectedCustomerForHistory.Name, FallbackValue=\'-\'}"\n                                                   FontSize="15.5"'
    )
    modal_content = modal_content.replace(
        '<TextBlock Text="📞 เบอร์โทรศัพท์:" FontSize="13" FontWeight="SemiBold" Foreground="#718096"/>\n                                        <TextBlock Text="{Binding SelectedCustomerForHistory.Phone, FallbackValue=\'-\'}"\n                                                   FontSize="14"',
        '<TextBlock Text="📞 เบอร์โทรศัพท์:" FontSize="14.5" FontWeight="SemiBold" Foreground="#718096"/>\n                                        <TextBlock Text="{Binding SelectedCustomerForHistory.Phone, FallbackValue=\'-\'}"\n                                                   FontSize="15.5"'
    )
    # DataGrid RowHeight 44 -> 48
    modal_content = modal_content.replace(
        '<DataGrid ItemsSource="{Binding CustomerHistoryOrders}"\n                                  AutoGenerateColumns="False"\n                                  IsReadOnly="True"\n                                  HeadersVisibility="Column"\n                                  SelectionMode="Single"\n                                  GridLinesVisibility="None"\n                                  BorderThickness="0"\n                                  Background="Transparent"\n                                  RowHeight="44"',
        '<DataGrid ItemsSource="{Binding CustomerHistoryOrders}"\n                                  AutoGenerateColumns="False"\n                                  IsReadOnly="True"\n                                  HeadersVisibility="Column"\n                                  SelectionMode="Single"\n                                  GridLinesVisibility="None"\n                                  BorderThickness="0"\n                                  Background="Transparent"\n                                  RowHeight="48"'
    )
    # History Footer: Total Amount FontSize 18 -> 20, Close button Height 42 -> 46, FontSize 16
    modal_content = re.sub(
        r'(<TextBlock Text="\{Binding CustomerHistoryTotalAmount, FallbackValue=\'0\.00\'\}"\s+FontSize=)"18"',
        r'\g<1>"20"',
        modal_content
    )
    modal_content = modal_content.replace(
        '<Button Grid.Column="1"\n                                Command="{Binding CloseCustomerHistoryModalCommand}"\n                                Style="{StaticResource PrimaryBtnStyle}"\n                                Padding="20,0" Height="42"',
        '<Button Grid.Column="1"\n                                Command="{Binding CloseCustomerHistoryModalCommand}"\n                                Style="{StaticResource PrimaryBtnStyle}"\n                                Padding="20,0" Height="46" FontSize="16"'
    )

    # 6. Product Modal Image Buttons (Browse / Remove): Height 36 -> 38, FontSize 13 -> 14
    modal_content = modal_content.replace(
        '<Button Command="{Binding BrowseImageCommand}"\n                                                        Content="เลือกรูป"\n                                                        Style="{StaticResource PrimaryBtnStyle}"\n                                                        Height="36" Padding="14,0" FontSize="13"',
        '<Button Command="{Binding BrowseImageCommand}"\n                                                        Content="เลือกรูป"\n                                                        Style="{StaticResource PrimaryBtnStyle}"\n                                                        Height="38" Padding="14,0" FontSize="14"'
    )
    modal_content = modal_content.replace(
        '<Button Command="{Binding RemoveImageCommand}"\n                                                        Content="ลบ"\n                                                        Style="{StaticResource DangerBtnStyle}"\n                                                        Height="36" Padding="12,0" FontSize="13"',
        '<Button Command="{Binding RemoveImageCommand}"\n                                                        Content="ลบ"\n                                                        Style="{StaticResource DangerBtnStyle}"\n                                                        Height="38" Padding="12,0" FontSize="14"'
    )

    # Write combined content back
    with open(path, "w", encoding="utf-8") as f:
        f.write(top_content + modal_content)

    print("MainWindow.xaml modals updated successfully!")

if __name__ == "__main__":
    update_mainwindow_modals()
