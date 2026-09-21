import re
import sys
sys.stdout.reconfigure(encoding='utf-8')

# -------------------------------------------------------------
# 1. Update CustomerHistoryWindow.xaml
# -------------------------------------------------------------
def update_customer_history_window():
    path = "Views/CustomerHistoryWindow.xaml"
    with open(path, "r", encoding="utf-8") as f:
        content = f.read()

    # ModalColHeaderStyle: Height 44 -> 48, FontSize 15 -> 16
    content = re.sub(
        r'(<Style x:Key="ModalColHeaderStyle"[^>]*>.*?<Setter Property="Height"\s+Value=)"44"',
        r'\g<1>"48"',
        content, flags=re.DOTALL
    )
    content = re.sub(
        r'(<Style x:Key="ModalColHeaderStyle"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"15"',
        r'\g<1>"16"',
        content, flags=re.DOTALL
    )

    # ModalRowStyle: MinHeight 44 -> 48, FontSize 14 -> 15
    content = re.sub(
        r'(<Style x:Key="ModalRowStyle"[^>]*>.*?<Setter Property="MinHeight"\s+Value=)"44"',
        r'\g<1>"48"',
        content, flags=re.DOTALL
    )
    content = re.sub(
        r'(<Style x:Key="ModalRowStyle"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"14"',
        r'\g<1>"15"',
        content, flags=re.DOTALL
    )

    # Inset customer summary labels & values
    content = content.replace(
        '<TextBlock Text="👤 ชื่อลูกค้า:" FontSize="13" FontWeight="SemiBold" Foreground="#718096"/>\n                            <TextBlock x:Name="CustomerNameTextBlock" FontSize="14" FontWeight="Bold"',
        '<TextBlock Text="👤 ชื่อลูกค้า:" FontSize="14.5" FontWeight="SemiBold" Foreground="#718096"/>\n                            <TextBlock x:Name="CustomerNameTextBlock" FontSize="15.5" FontWeight="Bold"'
    )
    content = content.replace(
        '<TextBlock Text="📞 เบอร์โทรศัพท์:" FontSize="13" FontWeight="SemiBold" Foreground="#718096"/>\n                            <TextBlock x:Name="CustomerPhoneTextBlock" FontSize="14" FontWeight="Bold"',
        '<TextBlock Text="📞 เบอร์โทรศัพท์:" FontSize="14.5" FontWeight="SemiBold" Foreground="#718096"/>\n                            <TextBlock x:Name="CustomerPhoneTextBlock" FontSize="15.5" FontWeight="Bold"'
    )

    # DataGrid RowHeight 44 -> 48
    content = content.replace('RowHeight="44"', 'RowHeight="48"')

    # Total Spent value FontSize 18 -> 20
    content = content.replace(
        '<TextBlock x:Name="TotalSpentTextBlock" FontSize="18" FontWeight="Bold" Foreground="#0B2545"/>',
        '<TextBlock x:Name="TotalSpentTextBlock" FontSize="20" FontWeight="Bold" Foreground="#0B2545"/>'
    )

    # Close button Height 42 -> 46, FontSize 16
    content = content.replace(
        'Style="{StaticResource PrimaryBtnStyle}"\n                                Padding="20,0" Height="42"',
        'Style="{StaticResource PrimaryBtnStyle}"\n                                Padding="20,0" Height="46" FontSize="16"'
    )

    with open(path, "w", encoding="utf-8") as f:
        f.write(content)
    print("CustomerHistoryWindow.xaml updated.")

# -------------------------------------------------------------
# 2. Update DeliverySlipWindow.xaml
# -------------------------------------------------------------
def update_delivery_slip_window():
    path = "Views/DeliverySlipWindow.xaml"
    with open(path, "r", encoding="utf-8") as f:
        content = f.read()

    # Buttons
    content = content.replace('Height="36" Width="130" Background="#00A3E0"',
                              'Height="40" Width="140" Background="#00A3E0"')
    content = content.replace('<TextBlock Text="พิมพ์ใบส่งของ" FontWeight="Bold" FontSize="13.5"',
                              '<TextBlock Text="พิมพ์ใบส่งของ" FontWeight="Bold" FontSize="15"')
    content = content.replace('<Button x:Name="CloseBtn" Click="CloseBtn_Click" Height="36" Width="80"',
                              '<Button x:Name="CloseBtn" Click="CloseBtn_Click" Height="40" Width="90"')
    content = content.replace('<TextBlock Text="ปิด" FontWeight="SemiBold" FontSize="13"',
                              '<TextBlock Text="ปิด" FontWeight="SemiBold" FontSize="15"')

    # DataGrid RowHeight 34 -> 42
    content = content.replace('RowHeight="34"', 'RowHeight="42"')
    # DataGrid ColumnHeader Height 38 -> 42, FontSize 13.5 -> 15
    content = content.replace('<Setter Property="Height" Value="38"/>\n                            <Setter Property="FontSize" Value="13.5"/>',
                              '<Setter Property="Height" Value="42"/>\n                            <Setter Property="FontSize" Value="15"/>')

    # Grand total FontSize 22 -> 24
    content = content.replace('FontSize="22" FontWeight="Bold" DockPanel.Dock="Right"',
                              'FontSize="24" FontWeight="Bold" DockPanel.Dock="Right"')

    with open(path, "w", encoding="utf-8") as f:
        f.write(content)
    print("DeliverySlipWindow.xaml updated.")

# -------------------------------------------------------------
# 3. Update POReceiptWindow.xaml
# -------------------------------------------------------------
def update_po_receipt_window():
    path = "Views/POReceiptWindow.xaml"
    with open(path, "r", encoding="utf-8") as f:
        content = f.read()

    # ItemsGrid RowHeight 40 -> 44, FontSize="15"
    content = content.replace(
        '<DataGrid Grid.Row="2" x:Name="ItemsGrid" AutoGenerateColumns="False" IsReadOnly="True"\n                  GridLinesVisibility="Horizontal" HeadersVisibility="Column"\n                  RowHeight="40">',
        '<DataGrid Grid.Row="2" x:Name="ItemsGrid" AutoGenerateColumns="False" IsReadOnly="True"\n                  GridLinesVisibility="Horizontal" HeadersVisibility="Column"\n                  RowHeight="44" FontSize="15">'
    )

    # Grand total TextBlock
    content = content.replace(
        '<TextBlock Text="{Binding TotalAmount, StringFormat=N2}" Width="150" TextAlignment="Right" FontWeight="Bold" FontSize="18"/>',
        '<TextBlock Text="{Binding TotalAmount, StringFormat=N2}" Width="150" TextAlignment="Right" FontWeight="Bold" FontSize="20"/>'
    )

    with open(path, "w", encoding="utf-8") as f:
        f.write(content)
    print("POReceiptWindow.xaml updated.")

# -------------------------------------------------------------
# 4. Update ReceiptWindow.xaml
# -------------------------------------------------------------
def update_receipt_window():
    path = "Views/ReceiptWindow.xaml"
    with open(path, "r", encoding="utf-8") as f:
        content = f.read()

    # PrintBtn Height 36 -> 40, FontSize 14 -> 15
    content = content.replace(
        'Width="120" Height="36" Background="#00A3E0" Foreground="White" FontWeight="Bold" FontSize="14"',
        'Width="130" Height="40" Background="#00A3E0" Foreground="White" FontWeight="Bold" FontSize="15"'
    )

    # Grand Total: FontSize 14 -> 16
    content = content.replace(
        '<TextBlock Text="ยอดรวมสุทธิ:" FontSize="14" FontWeight="Bold"/>\n                    <TextBlock Text="{Binding TotalAmount, StringFormat=F2}" FontSize="14" FontWeight="Bold"',
        '<TextBlock Text="ยอดรวมสุทธิ:" FontSize="16" FontWeight="Bold"/>\n                    <TextBlock Text="{Binding TotalAmount, StringFormat=F2}" FontSize="16" FontWeight="Bold"'
    )

    with open(path, "w", encoding="utf-8") as f:
        f.write(content)
    print("ReceiptWindow.xaml updated.")

# -------------------------------------------------------------
# 5. Update CustomMessageBox.xaml
# -------------------------------------------------------------
def update_custom_message_box():
    path = "Views/CustomMessageBox.xaml"
    with open(path, "r", encoding="utf-8") as f:
        content = f.read()

    # DialogPrimaryBtnStyle, DialogDangerBtnStyle, DialogSecondaryBtnStyle: Height="44" MinHeight="44" FontSize="16"
    for style_key in ["DialogPrimaryBtnStyle", "DialogDangerBtnStyle", "DialogSecondaryBtnStyle"]:
        content = re.sub(
            r'(<Style x:Key="' + style_key + r'"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"14"',
            r'\g<1>"16"',
            content, flags=re.DOTALL
        )
        content = re.sub(
            r'(<Style x:Key="' + style_key + r'"[^>]*>.*?<Setter Property="Height"\s+Value=)"40"',
            r'\g<1>"44"',
            content, flags=re.DOTALL
        )
        content = re.sub(
            r'(<Style x:Key="' + style_key + r'"[^>]*>.*?<Setter Property="MinHeight"\s+Value=)"40"',
            r'\g<1>"44"',
            content, flags=re.DOTALL
        )

    # Padding="20,0" -> Padding="24,0" for dialog buttons
    content = content.replace('Padding="20,0"/>', 'Padding="24,0"/>')

    # Title: FontSize 18 -> 20
    content = content.replace('<TextBlock x:Name="TxtTitle"\n                               FontSize="18"',
                              '<TextBlock x:Name="TxtTitle"\n                               FontSize="20"')

    # Message: FontSize 14.5 -> 15.5, LineHeight 22 -> 24
    content = content.replace('FontSize="14.5"\n                               Foreground="#334155"\n                               LineHeight="22"',
                              'FontSize="15.5"\n                               Foreground="#334155"\n                               LineHeight="24"')

    with open(path, "w", encoding="utf-8") as f:
        f.write(content)
    print("CustomMessageBox.xaml updated.")

if __name__ == "__main__":
    update_customer_history_window()
    update_delivery_slip_window()
    update_po_receipt_window()
    update_receipt_window()
    update_custom_message_box()
    print("All Batch 5 auxiliary windows updated!")
