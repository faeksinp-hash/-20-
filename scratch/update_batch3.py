import re
import sys
sys.stdout.reconfigure(encoding='utf-8')

# -------------------------------------------------------------
# 1. Update CustomerView.xaml
# -------------------------------------------------------------
def update_customer_view():
    path = "Views/CustomerView.xaml"
    with open(path, "r", encoding="utf-8") as f:
        content = f.read()

    # Style updates
    content = content.replace(
        '<Setter Property="FontSize"        Value="13"/>\n            <Setter Property="FontWeight"      Value="SemiBold"/>\n            <Setter Property="Cursor"          Value="Hand"/>\n            <Setter Property="Height"          Value="40"/>',
        '<Setter Property="FontSize"        Value="16"/>\n            <Setter Property="FontWeight"      Value="SemiBold"/>\n            <Setter Property="Cursor"          Value="Hand"/>\n            <Setter Property="Height"          Value="44"/>'
    )
    
    # PrimaryBtnStyle Height/FontSize (if different formatting)
    content = re.sub(
        r'(<Style x:Key="PrimaryBtnStyle"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"13"',
        r'\g<1>"16"',
        content, flags=re.DOTALL
    )
    content = re.sub(
        r'(<Style x:Key="PrimaryBtnStyle"[^>]*>.*?<Setter Property="Height"\s+Value=)"40"',
        r'\g<1>"44"',
        content, flags=re.DOTALL
    )
    # OutlineBtnStyle
    content = re.sub(
        r'(<Style x:Key="OutlineBtnStyle"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"12"',
        r'\g<1>"14"',
        content, flags=re.DOTALL
    )
    content = re.sub(
        r'(<Style x:Key="OutlineBtnStyle"[^>]*>.*?<Setter Property="Height"\s+Value=)"30"',
        r'\g<1>"36"',
        content, flags=re.DOTALL
    )

    # CustomerColumnHeaderStyle: Height="48"
    content = re.sub(
        r'(<Style x:Key="CustomerColumnHeaderStyle"[^>]*>.*?<Setter Property="Height"\s+Value=)"46"',
        r'\g<1>"48"',
        content, flags=re.DOTALL
    )

    # CustomerRowStyle: FontSize="15"
    content = re.sub(
        r'(<Style x:Key="CustomerRowStyle"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"13"',
        r'\g<1>"15"',
        content, flags=re.DOTALL
    )

    # Header Subtitle
    content = content.replace(
        '<TextBlock Text="จัดการข้อมูลลูกค้า — ระบบจัดการร้าน พอใจ"\n                                   FontSize="13"',
        '<TextBlock Text="จัดการข้อมูลลูกค้า — ระบบจัดการร้าน พอใจ"\n                                   FontSize="15"'
    )

    # KPI Cards: Border 36->40, Text 18->20, Label 13->16, Subtitle 11->14
    content = content.replace('<Border Width="36" Height="36" CornerRadius="10" Background="#E0F2FE" Margin="0,0,10,0">\n                                <TextBlock Text="👥" FontSize="18"',
                              '<Border Width="40" Height="40" CornerRadius="10" Background="#E0F2FE" Margin="0,0,10,0">\n                                <TextBlock Text="👥" FontSize="20"')
    content = content.replace('<TextBlock Text="ลูกค้าทั้งหมด" FontSize="13" FontWeight="SemiBold"',
                              '<TextBlock Text="ลูกค้าทั้งหมด" FontSize="16" FontWeight="SemiBold"')
    content = content.replace('<TextBlock Grid.Row="2"\n                                   Text="จำนวนสมาชิกทั้งหมดในระบบ"\n                                   FontSize="11"',
                              '<TextBlock Grid.Row="2"\n                                   Text="จำนวนสมาชิกทั้งหมดในระบบ"\n                                   FontSize="14"')

    content = content.replace('<Border Width="36" Height="36" CornerRadius="10" Background="#DCFCE7" Margin="0,0,10,0">\n                                <TextBlock Text="✨" FontSize="18"',
                              '<Border Width="40" Height="40" CornerRadius="10" Background="#DCFCE7" Margin="0,0,10,0">\n                                <TextBlock Text="✨" FontSize="20"')
    content = content.replace('<TextBlock Text="ลูกค้าใหม่เดือนนี้" FontSize="13" FontWeight="SemiBold"',
                              '<TextBlock Text="ลูกค้าใหม่เดือนนี้" FontSize="16" FontWeight="SemiBold"')
    content = content.replace('<TextBlock Grid.Row="2"\n                                   Text="สมัครสมาชิกใหม่ในเดือนปัจจุบัน"\n                                   FontSize="11"',
                              '<TextBlock Grid.Row="2"\n                                   Text="สมัครสมาชิกใหม่ในเดือนปัจจุบัน"\n                                   FontSize="14"')

    content = content.replace('<Border Width="36" Height="36" CornerRadius="10" Background="#FEF9C3" Margin="0,0,10,0">\n                                <TextBlock Text="👑" FontSize="18"',
                              '<Border Width="40" Height="40" CornerRadius="10" Background="#FEF9C3" Margin="0,0,10,0">\n                                <TextBlock Text="👑" FontSize="20"')
    content = content.replace('<TextBlock Text="ยอดซื้อสูงสุดเดือนนี้" FontSize="13" FontWeight="SemiBold"',
                              '<TextBlock Text="ยอดซื้อสูงสุดเดือนนี้" FontSize="16" FontWeight="SemiBold"')
    content = content.replace('<TextBlock Grid.Row="2"\n                                   Text="สมาชิกระดับ VIP / มียอดซื้อสูงสุด"\n                                   FontSize="11"',
                              '<TextBlock Grid.Row="2"\n                                   Text="สมาชิกระดับ VIP / มียอดซื้อสูงสุด"\n                                   FontSize="14"')

    # Search Bar
    content = content.replace(
        '<TextBox x:Name="CustomerSearchBox"\n                                     Text="{Binding CustomerSearchKeyword, UpdateSourceTrigger=PropertyChanged, Mode=TwoWay}" \n                                     Style="{StaticResource LightTextBoxStyle}"\n                                     Height="40"\n                                     Padding="38,0,12,0"\n                                     VerticalContentAlignment="Center"\n                                     FontSize="13"/>',
        '<TextBox x:Name="CustomerSearchBox"\n                                     Text="{Binding CustomerSearchKeyword, UpdateSourceTrigger=PropertyChanged, Mode=TwoWay}" \n                                     Style="{StaticResource LightTextBoxStyle}"\n                                     Height="44"\n                                     Padding="38,0,12,0"\n                                     VerticalContentAlignment="Center"\n                                     FontSize="16"/>'
    )
    content = content.replace(
        '<TextBlock Text="ค้นหาด้วยรหัสลูกค้า หรือ ชื่อลูกค้า..."\n                                       Foreground="#A0AEC0"\n                                       FontSize="13"',
        '<TextBlock Text="ค้นหาด้วยรหัสลูกค้า หรือ ชื่อลูกค้า..."\n                                       Foreground="#A0AEC0"\n                                       FontSize="15"'
    )

    # Search button
    content = re.sub(
        r'(<Button Grid\.Column="2"\s+Content="ค้นหา"[^>]*?Height=)"40"',
        r'\g<1>"44" FontSize="16"',
        content
    )
    # Refresh button
    content = re.sub(
        r'(<Button Grid\.Column="4"\s+Content="ล้างการค้นหา"[^>]*?Height=)"40"',
        r'\g<1>"44" FontSize="16"',
        content
    )
    # Add button
    content = re.sub(
        r'(<Button Grid\.Column="6"\s+Content="เพิ่มลูกค้าใหม่"[^>]*?Height=)"40"',
        r'\g<1>"44" FontSize="16"',
        content
    )
    # Manage button
    content = re.sub(
        r'(<Button Grid\.Column="8"\s+Content="จัดการข้อมูล"[^>]*?Height=)"40"',
        r'\g<1>"44" FontSize="16"',
        content
    )

    # DataGrid RowHeight 48 -> 50
    content = content.replace('RowHeight="48"', 'RowHeight="50"')

    # Status badge padding and font size in Col 6
    content = content.replace(
        '<Border CornerRadius="12" Padding="8,3" HorizontalAlignment="Center">',
        '<Border CornerRadius="12" Padding="10,4" HorizontalAlignment="Center">'
    )
    content = re.sub(
        r'(<Border CornerRadius="12" Padding="10,4"[^>]*>.*?<TextBlock[^>]*?FontSize=)"12"',
        r'\g<1>"14"',
        content, flags=re.DOTALL
    )

    with open(path, "w", encoding="utf-8") as f:
        f.write(content)
    print("CustomerView.xaml updated.")

# -------------------------------------------------------------
# 2. Update StaffView.xaml
# -------------------------------------------------------------
def update_staff_view():
    path = "Views/StaffView.xaml"
    with open(path, "r", encoding="utf-8") as f:
        content = f.read()

    # LightComboBoxStyle Height 40->44, FontSize 13->15
    content = re.sub(
        r'(<Style x:Key="LightComboBoxStyle"[^>]*>.*?<Setter Property="Height"\s+Value=)"40"',
        r'\g<1>"44"',
        content, flags=re.DOTALL
    )
    content = re.sub(
        r'(<Style x:Key="LightComboBoxStyle"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"13"',
        r'\g<1>"15"',
        content, flags=re.DOTALL
    )

    # Buttons in styles
    content = content.replace(
        '<Setter Property="FontSize"        Value="13"/>\n            <Setter Property="FontWeight"      Value="SemiBold"/>\n            <Setter Property="Cursor"          Value="Hand"/>\n            <Setter Property="Height"          Value="40"/>',
        '<Setter Property="FontSize"        Value="16"/>\n            <Setter Property="FontWeight"      Value="SemiBold"/>\n            <Setter Property="Cursor"          Value="Hand"/>\n            <Setter Property="Height"          Value="44"/>'
    )
    content = re.sub(
        r'(<Style x:Key="PrimaryBtnStyle"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"13"',
        r'\g<1>"16"',
        content, flags=re.DOTALL
    )
    content = re.sub(
        r'(<Style x:Key="PrimaryBtnStyle"[^>]*>.*?<Setter Property="Height"\s+Value=)"40"',
        r'\g<1>"44"',
        content, flags=re.DOTALL
    )
    content = re.sub(
        r'(<Style x:Key="OutlineBtnStyle"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"12"',
        r'\g<1>"14"',
        content, flags=re.DOTALL
    )
    content = re.sub(
        r'(<Style x:Key="OutlineBtnStyle"[^>]*>.*?<Setter Property="Height"\s+Value=)"30"',
        r'\g<1>"36"',
        content, flags=re.DOTALL
    )

    # StaffColumnHeaderStyle: Height="48"
    content = re.sub(
        r'(<Style x:Key="StaffColumnHeaderStyle"[^>]*>.*?<Setter Property="Height"\s+Value=)"46"',
        r'\g<1>"48"',
        content, flags=re.DOTALL
    )

    # StaffRowStyle: FontSize="15"
    content = re.sub(
        r'(<Style x:Key="StaffRowStyle"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"13"',
        r'\g<1>"15"',
        content, flags=re.DOTALL
    )

    # Header Subtitle
    content = content.replace(
        '<TextBlock Text="จัดการข้อมูลพนักงาน — ระบบจัดการร้าน พอใจ"\n                                   FontSize="13"',
        '<TextBlock Text="จัดการข้อมูลพนักงาน — ระบบจัดการร้าน พอใจ"\n                                   FontSize="15"'
    )

    # KPI Cards: Border 36->40, Text 18->20, Label 13->16, Subtitle 11->14
    content = content.replace('<Border Width="36" Height="36" CornerRadius="10" Background="#E0F2FE" Margin="0,0,10,0">\n                                <TextBlock Text="👤" FontSize="18"',
                              '<Border Width="40" Height="40" CornerRadius="10" Background="#E0F2FE" Margin="0,0,10,0">\n                                <TextBlock Text="👤" FontSize="20"')
    content = content.replace('<TextBlock Text="พนักงานทั้งหมด" FontSize="13" FontWeight="SemiBold"',
                              '<TextBlock Text="พนักงานทั้งหมด" FontSize="16" FontWeight="SemiBold"')
    content = content.replace('<TextBlock Grid.Row="2"\n                                   Text="จำนวนพนักงานทั้งหมดในระบบ"\n                                   FontSize="11"',
                              '<TextBlock Grid.Row="2"\n                                   Text="จำนวนพนักงานทั้งหมดในระบบ"\n                                   FontSize="14"')

    content = content.replace('<Border Width="36" Height="36" CornerRadius="10" Background="#DCFCE7" Margin="0,0,10,0">\n                                <TextBlock Text="✅" FontSize="18"',
                              '<Border Width="40" Height="40" CornerRadius="10" Background="#DCFCE7" Margin="0,0,10,0">\n                                <TextBlock Text="✅" FontSize="20"')
    content = content.replace('<TextBlock Text="เข้าปฏิบัติงานวันนี้" FontSize="13" FontWeight="SemiBold"',
                              '<TextBlock Text="เข้าปฏิบัติงานวันนี้" FontSize="16" FontWeight="SemiBold"')
    content = content.replace('<TextBlock Grid.Row="2"\n                                   Text="พนักงานที่มีสถานะพร้อมปฏิบัติงาน"\n                                   FontSize="11"',
                              '<TextBlock Grid.Row="2"\n                                   Text="พนักงานที่มีสถานะพร้อมปฏิบัติงาน"\n                                   FontSize="14"')

    content = content.replace('<Border Width="36" Height="36" CornerRadius="10" Background="#FEF9C3" Margin="0,0,10,0">\n                                <TextBlock Text="👑" FontSize="18"',
                              '<Border Width="40" Height="40" CornerRadius="10" Background="#FEF9C3" Margin="0,0,10,0">\n                                <TextBlock Text="👑" FontSize="20"')
    content = content.replace('<TextBlock Text="ยอดขายสูงสุดเดือนนี้" FontSize="13" FontWeight="SemiBold"',
                              '<TextBlock Text="ยอดขายสูงสุดเดือนนี้" FontSize="16" FontWeight="SemiBold"')
    content = content.replace('<TextBlock Grid.Row="2"\n                                   Text="พนักงานที่สร้างยอดขายสูงสุด"\n                                   FontSize="11"',
                              '<TextBlock Grid.Row="2"\n                                   Text="พนักงานที่สร้างยอดขายสูงสุด"\n                                   FontSize="14"')

    # Search Bar
    content = content.replace(
        '<TextBox x:Name="StaffSearchBox"\n                                     Text="{Binding StaffSearchKeyword, UpdateSourceTrigger=PropertyChanged, Mode=TwoWay}" \n                                     Style="{StaticResource LightTextBoxStyle}"\n                                     Height="40"\n                                     Padding="38,0,12,0"\n                                     VerticalContentAlignment="Center"\n                                     FontSize="13"/>',
        '<TextBox x:Name="StaffSearchBox"\n                                     Text="{Binding StaffSearchKeyword, UpdateSourceTrigger=PropertyChanged, Mode=TwoWay}" \n                                     Style="{StaticResource LightTextBoxStyle}"\n                                     Height="44"\n                                     Padding="38,0,12,0"\n                                     VerticalContentAlignment="Center"\n                                     FontSize="16"/>'
    )
    content = content.replace(
        '<TextBlock Text="ค้นหาด้วยรหัสพนักงาน หรือ ชื่อพนักงาน..."\n                                       Foreground="#A0AEC0"\n                                       FontSize="13"',
        '<TextBlock Text="ค้นหาด้วยรหัสพนักงาน หรือ ชื่อพนักงาน..."\n                                       Foreground="#A0AEC0"\n                                       FontSize="15"'
    )

    # Search button
    content = re.sub(
        r'(<Button Grid\.Column="2"\s+Content="ค้นหา"[^>]*?Height=)"40"',
        r'\g<1>"44" FontSize="16"',
        content
    )
    # Refresh button
    content = re.sub(
        r'(<Button Grid\.Column="4"\s+Content="ล้างการค้นหา"[^>]*?Height=)"40"',
        r'\g<1>"44" FontSize="16"',
        content
    )
    # Add button
    content = re.sub(
        r'(<Button Grid\.Column="6"\s+Content="เพิ่มพนักงานใหม่"[^>]*?Height=)"40"',
        r'\g<1>"44" FontSize="16"',
        content
    )
    # Manage button
    content = re.sub(
        r'(<Button Grid\.Column="8"\s+Content="จัดการข้อมูล"[^>]*?Height=)"40"',
        r'\g<1>"44" FontSize="16"',
        content
    )

    # DataGrid RowHeight 48 -> 50
    content = content.replace('RowHeight="48"', 'RowHeight="50"')

    # Role and Status badges
    content = content.replace(
        '<Border CornerRadius="12" Padding="8,3" HorizontalAlignment="Center">',
        '<Border CornerRadius="12" Padding="10,4" HorizontalAlignment="Center">'
    )
    content = re.sub(
        r'(<Border CornerRadius="12" Padding="10,4"[^>]*>.*?<TextBlock[^>]*?FontSize=)"12"',
        r'\g<1>"14"',
        content, flags=re.DOTALL
    )

    with open(path, "w", encoding="utf-8") as f:
        f.write(content)
    print("StaffView.xaml updated.")

# -------------------------------------------------------------
# 3. Update PartnerView.xaml
# -------------------------------------------------------------
def update_partner_view():
    path = "Views/PartnerView.xaml"
    with open(path, "r", encoding="utf-8") as f:
        content = f.read()

    # CyanInputBorderStyle: Height="44"
    content = re.sub(
        r'(<Style x:Key="CyanInputBorderStyle"[^>]*>.*?<Setter Property="Height"\s+Value=)"40"',
        r'\g<1>"44"',
        content, flags=re.DOTALL
    )

    # Buttons in styles
    content = content.replace(
        '<Setter Property="FontSize"        Value="13"/>\n            <Setter Property="FontWeight"      Value="SemiBold"/>\n            <Setter Property="Cursor"          Value="Hand"/>\n            <Setter Property="Height"          Value="40"/>',
        '<Setter Property="FontSize"        Value="16"/>\n            <Setter Property="FontWeight"      Value="SemiBold"/>\n            <Setter Property="Cursor"          Value="Hand"/>\n            <Setter Property="Height"          Value="44"/>'
    )
    content = re.sub(
        r'(<Style x:Key="PrimaryBtnStyle"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"13"',
        r'\g<1>"16"',
        content, flags=re.DOTALL
    )
    content = re.sub(
        r'(<Style x:Key="PrimaryBtnStyle"[^>]*>.*?<Setter Property="Height"\s+Value=)"40"',
        r'\g<1>"44"',
        content, flags=re.DOTALL
    )

    # PartnerColumnHeaderStyle: Height="48"
    content = re.sub(
        r'(<Style x:Key="PartnerColumnHeaderStyle"[^>]*>.*?<Setter Property="Height"\s+Value=)"46"',
        r'\g<1>"48"',
        content, flags=re.DOTALL
    )

    # PartnerRowStyle: FontSize="15"
    content = re.sub(
        r'(<Style x:Key="PartnerRowStyle"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"13"',
        r'\g<1>"15"',
        content, flags=re.DOTALL
    )

    # Header Subtitle
    content = content.replace(
        '<TextBlock Text="ระบบบริหารจัดการข้อมูลซัพพลายเออร์และผู้จัดจำหน่าย — ระบบจัดการร้าน พอใจ"\n                                   FontSize="13"',
        '<TextBlock Text="ระบบบริหารจัดการข้อมูลซัพพลายเออร์และผู้จัดจำหน่าย — ระบบจัดการร้าน พอใจ"\n                                   FontSize="15"'
    )

    # KPI Cards: Border 36->40, Text 18->20, Label 13->16, Subtitle 11->14
    content = content.replace('<Border Width="36" Height="36" CornerRadius="10" Background="#E0F2FE" Margin="0,0,10,0">\n                                <TextBlock Text="🏢" FontSize="18"',
                              '<Border Width="40" Height="40" CornerRadius="10" Background="#E0F2FE" Margin="0,0,10,0">\n                                <TextBlock Text="🏢" FontSize="20"')
    content = content.replace('<TextBlock Text="บริษัทคู่ค้าทั้งหมด" FontSize="13" FontWeight="SemiBold"',
                              '<TextBlock Text="บริษัทคู่ค้าทั้งหมด" FontSize="16" FontWeight="SemiBold"')
    content = content.replace('<TextBlock Grid.Row="2"\n                                   Text="จำนวนซัพพลายเออร์ในระบบ"\n                                   FontSize="11"',
                              '<TextBlock Grid.Row="2"\n                                   Text="จำนวนซัพพลายเออร์ในระบบ"\n                                   FontSize="14"')

    content = content.replace('<Border Width="36" Height="36" CornerRadius="10" Background="#DCFCE7" Margin="0,0,10,0">\n                                <TextBlock Text="📦" FontSize="18"',
                              '<Border Width="40" Height="40" CornerRadius="10" Background="#DCFCE7" Margin="0,0,10,0">\n                                <TextBlock Text="📦" FontSize="20"')
    content = content.replace('<TextBlock Text="การสั่งซื้อเดือนนี้" FontSize="13" FontWeight="SemiBold"',
                              '<TextBlock Text="การสั่งซื้อเดือนนี้" FontSize="16" FontWeight="SemiBold"')
    content = content.replace('<TextBlock Grid.Row="2"\n                                   Text="มูลค่าสั่งซื้อรวมในเดือนปัจจุบัน"\n                                   FontSize="11"',
                              '<TextBlock Grid.Row="2"\n                                   Text="มูลค่าสั่งซื้อรวมในเดือนปัจจุบัน"\n                                   FontSize="14"')

    content = content.replace('<Border Width="36" Height="36" CornerRadius="10" Background="#FEF9C3" Margin="0,0,10,0">\n                                <TextBlock Text="👑" FontSize="18"',
                              '<Border Width="40" Height="40" CornerRadius="10" Background="#FEF9C3" Margin="0,0,10,0">\n                                <TextBlock Text="👑" FontSize="20"')
    content = content.replace('<TextBlock Text="คู่ค้ายอดสั่งสูงสุด" FontSize="13" FontWeight="SemiBold"',
                              '<TextBlock Text="คู่ค้ายอดสั่งสูงสุด" FontSize="16" FontWeight="SemiBold"')
    content = content.replace('<TextBlock Grid.Row="2"\n                                   Text="ซัพพลายเออร์หลักของร้าน"\n                                   FontSize="11"',
                              '<TextBlock Grid.Row="2"\n                                   Text="ซัพพลายเออร์หลักของร้าน"\n                                   FontSize="14"')

    # Search Bar
    content = content.replace(
        '<TextBox x:Name="PartnerSearchBox"\n                                     Text="{Binding PartnerSearchKeyword, UpdateSourceTrigger=PropertyChanged, Mode=TwoWay}" \n                                     Style="{StaticResource LightTextBoxStyle}"\n                                     Height="40"\n                                     Padding="38,0,12,0"\n                                     VerticalContentAlignment="Center"\n                                     FontSize="13"/>',
        '<TextBox x:Name="PartnerSearchBox"\n                                     Text="{Binding PartnerSearchKeyword, UpdateSourceTrigger=PropertyChanged, Mode=TwoWay}" \n                                     Style="{StaticResource LightTextBoxStyle}"\n                                     Height="44"\n                                     Padding="38,0,12,0"\n                                     VerticalContentAlignment="Center"\n                                     FontSize="16"/>'
    )
    content = content.replace(
        '<TextBlock Text="ค้นหาด้วยรหัสคู่ค้า, ชื่อบริษัท หรือผู้ติดต่อ..."\n                                       Foreground="#A0AEC0"\n                                       FontSize="13"',
        '<TextBlock Text="ค้นหาด้วยรหัสคู่ค้า, ชื่อบริษัท หรือผู้ติดต่อ..."\n                                       Foreground="#A0AEC0"\n                                       FontSize="15"'
    )

    # Search button
    content = re.sub(
        r'(<Button Grid\.Column="2"\s+Content="ค้นหา"[^>]*?Height=)"40"',
        r'\g<1>"44" FontSize="16"',
        content
    )
    # Refresh button
    content = re.sub(
        r'(<Button Grid\.Column="4"\s+Content="ล้างการค้นหา"[^>]*?Height=)"40"',
        r'\g<1>"44" FontSize="16"',
        content
    )
    # Add button
    content = re.sub(
        r'(<Button Grid\.Column="6"\s+Content="เพิ่มคู่ค้าใหม่"[^>]*?Height=)"40"',
        r'\g<1>"44" FontSize="16"',
        content
    )
    # Manage button
    content = re.sub(
        r'(<Button Grid\.Column="8"\s+Content="จัดการข้อมูล"[^>]*?Height=)"40"',
        r'\g<1>"44" FontSize="16"',
        content
    )

    # DataGrid RowHeight 48 -> 50
    content = content.replace('RowHeight="48"', 'RowHeight="50"')

    # Status badge
    content = content.replace(
        '<Border CornerRadius="12" Padding="8,3" HorizontalAlignment="Center">',
        '<Border CornerRadius="12" Padding="10,4" HorizontalAlignment="Center">'
    )
    content = re.sub(
        r'(<Border CornerRadius="12" Padding="10,4"[^>]*>.*?<TextBlock[^>]*?FontSize=)"12"',
        r'\g<1>"14"',
        content, flags=re.DOTALL
    )

    with open(path, "w", encoding="utf-8") as f:
        f.write(content)
    print("PartnerView.xaml updated.")

# -------------------------------------------------------------
# 4. Update ExpenseView.xaml
# -------------------------------------------------------------
def update_expense_view():
    path = "Views/ExpenseView.xaml"
    with open(path, "r", encoding="utf-8") as f:
        content = f.read()

    # CyanInputBorderStyle: Height="44"
    content = re.sub(
        r'(<Style x:Key="CyanInputBorderStyle"[^>]*>.*?<Setter Property="Height"\s+Value=)"40"',
        r'\g<1>"44"',
        content, flags=re.DOTALL
    )

    # LightComboBoxStyle: Height="44", FontSize="15"
    content = re.sub(
        r'(<Style x:Key="LightComboBoxStyle"[^>]*>.*?<Setter Property="Height"\s+Value=)"40"',
        r'\g<1>"44"',
        content, flags=re.DOTALL
    )
    content = re.sub(
        r'(<Style x:Key="LightComboBoxStyle"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"13"',
        r'\g<1>"15"',
        content, flags=re.DOTALL
    )

    # Buttons in styles
    content = content.replace(
        '<Setter Property="FontSize"        Value="13"/>\n            <Setter Property="FontWeight"      Value="SemiBold"/>\n            <Setter Property="Cursor"          Value="Hand"/>\n            <Setter Property="Height"          Value="40"/>',
        '<Setter Property="FontSize"        Value="16"/>\n            <Setter Property="FontWeight"      Value="SemiBold"/>\n            <Setter Property="Cursor"          Value="Hand"/>\n            <Setter Property="Height"          Value="44"/>'
    )
    content = re.sub(
        r'(<Style x:Key="PrimaryBtnStyle"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"13"',
        r'\g<1>"16"',
        content, flags=re.DOTALL
    )
    content = re.sub(
        r'(<Style x:Key="PrimaryBtnStyle"[^>]*>.*?<Setter Property="Height"\s+Value=)"40"',
        r'\g<1>"44"',
        content, flags=re.DOTALL
    )

    # ExpenseColumnHeaderStyle: Height="48"
    content = re.sub(
        r'(<Style x:Key="ExpenseColumnHeaderStyle"[^>]*>.*?<Setter Property="Height"\s+Value=)"46"',
        r'\g<1>"48"',
        content, flags=re.DOTALL
    )

    # ExpenseRowStyle: FontSize="15"
    content = re.sub(
        r'(<Style x:Key="ExpenseRowStyle"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"13"',
        r'\g<1>"15"',
        content, flags=re.DOTALL
    )

    # Header Subtitle
    content = content.replace(
        '<TextBlock Text="ระบบบริหารจัดการและบันทึกค่าใช้จ่ายของร้าน"\n                                   FontSize="13"',
        '<TextBlock Text="ระบบบริหารจัดการและบันทึกค่าใช้จ่ายของร้าน"\n                                   FontSize="15"'
    )

    # KPI Cards: Border 36->40, Text 18->20, Label 13->16, Subtitle 11->14
    content = content.replace('<Border Width="36" Height="36" CornerRadius="10" Background="#FEE2E2" Margin="0,0,10,0">\n                                <TextBlock Text="💸" FontSize="18"',
                              '<Border Width="40" Height="40" CornerRadius="10" Background="#FEE2E2" Margin="0,0,10,0">\n                                <TextBlock Text="💸" FontSize="20"')
    content = content.replace('<TextBlock Text="รายจ่ายรวมเดือนนี้" FontSize="13" FontWeight="SemiBold"',
                              '<TextBlock Text="รายจ่ายรวมเดือนนี้" FontSize="16" FontWeight="SemiBold"')
    content = content.replace('<TextBlock Grid.Row="2"\n                                   Text="ยอดรายจ่ายสะสมในเดือนปัจจุบัน"\n                                   FontSize="11"',
                              '<TextBlock Grid.Row="2"\n                                   Text="ยอดรายจ่ายสะสมในเดือนปัจจุบัน"\n                                   FontSize="14"')

    content = content.replace('<Border Width="36" Height="36" CornerRadius="10" Background="#E0F2FE" Margin="0,0,10,0">\n                                <TextBlock Text="📋" FontSize="18"',
                              '<Border Width="40" Height="40" CornerRadius="10" Background="#E0F2FE" Margin="0,0,10,0">\n                                <TextBlock Text="📋" FontSize="20"')
    content = content.replace('<TextBlock Text="จำนวนรายการรายจ่าย" FontSize="13" FontWeight="SemiBold"',
                              '<TextBlock Text="จำนวนรายการรายจ่าย" FontSize="16" FontWeight="SemiBold"')
    content = content.replace('<TextBlock Grid.Row="2"\n                                   Text="บันทึกรายจ่ายทั้งหมดในเดือนนี้"\n                                   FontSize="11"',
                              '<TextBlock Grid.Row="2"\n                                   Text="บันทึกรายจ่ายทั้งหมดในเดือนนี้"\n                                   FontSize="14"')

    content = content.replace('<Border Width="36" Height="36" CornerRadius="10" Background="#FEF9C3" Margin="0,0,10,0">\n                                <TextBlock Text="⚡" FontSize="18"',
                              '<Border Width="40" Height="40" CornerRadius="10" Background="#FEF9C3" Margin="0,0,10,0">\n                                <TextBlock Text="⚡" FontSize="20"')
    content = content.replace('<TextBlock Text="หมวดหมู่สูงสุด" FontSize="13" FontWeight="SemiBold"',
                              '<TextBlock Text="หมวดหมู่สูงสุด" FontSize="16" FontWeight="SemiBold"')
    content = content.replace('<TextBlock Grid.Row="2"\n                                   Text="กลุ่มค่าใช้จ่ายที่มีมูลค่าสูงสุด"\n                                   FontSize="11"',
                              '<TextBlock Grid.Row="2"\n                                   Text="กลุ่มค่าใช้จ่ายที่มีมูลค่าสูงสุด"\n                                   FontSize="14"')

    # Search Bar
    content = content.replace(
        '<TextBox x:Name="ExpenseSearchBox"\n                                     Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged, Mode=TwoWay}" \n                                     Style="{StaticResource LightTextBoxStyle}"\n                                     Height="40"\n                                     Padding="38,0,12,0"\n                                     VerticalContentAlignment="Center"\n                                     FontSize="13"/>',
        '<TextBox x:Name="ExpenseSearchBox"\n                                     Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged, Mode=TwoWay}" \n                                     Style="{StaticResource LightTextBoxStyle}"\n                                     Height="44"\n                                     Padding="38,0,12,0"\n                                     VerticalContentAlignment="Center"\n                                     FontSize="16"/>'
    )
    content = content.replace(
        '<TextBlock Text="ค้นหาด้วยรหัส หรือชื่อ..."\n                                       Foreground="#A0AEC0"\n                                       FontSize="13"',
        '<TextBlock Text="ค้นหาด้วยรหัส หรือชื่อ..."\n                                       Foreground="#A0AEC0"\n                                       FontSize="15"'
    )

    # Search button
    content = re.sub(
        r'(<Button Grid\.Column="2"\s+Content="ค้นหา"[^>]*?Height=)"40"',
        r'\g<1>"44" FontSize="16"',
        content
    )
    # Refresh button
    content = re.sub(
        r'(<Button Grid\.Column="4"\s+Content="ล้างการค้นหา"[^>]*?Height=)"40"',
        r'\g<1>"44" FontSize="16"',
        content
    )
    # Add button
    content = re.sub(
        r'(<Button Grid\.Column="6"\s+Content="เพิ่มรายการรายจ่าย"[^>]*?Height=)"40"',
        r'\g<1>"44" FontSize="16"',
        content
    )
    # Manage button
    content = re.sub(
        r'(<Button Grid\.Column="8"\s+Content="จัดการข้อมูล"[^>]*?Height=)"40"',
        r'\g<1>"44" FontSize="16"',
        content
    )

    # DataGrid RowHeight 48 -> 50
    content = content.replace('RowHeight="48"', 'RowHeight="50"')

    # Category and Status badges
    content = content.replace(
        '<Border CornerRadius="12" Padding="8,3" HorizontalAlignment="Center">',
        '<Border CornerRadius="12" Padding="10,4" HorizontalAlignment="Center">'
    )
    content = re.sub(
        r'(<Border CornerRadius="12" Padding="10,4"[^>]*>.*?<TextBlock[^>]*?FontSize=)"12"',
        r'\g<1>"14"',
        content, flags=re.DOTALL
    )

    with open(path, "w", encoding="utf-8") as f:
        f.write(content)
    print("ExpenseView.xaml updated.")

# -------------------------------------------------------------
# 5. Update ReportsView.xaml
# -------------------------------------------------------------
def update_reports_view():
    path = "Views/ReportsView.xaml"
    with open(path, "r", encoding="utf-8") as f:
        content = f.read()

    # PrimaryButtonStyle, SuccessButtonStyle, InfoButtonStyle, OutlineButtonStyle
    # Height="44" FontSize="16"
    for style_key in ["PrimaryButtonStyle", "SuccessButtonStyle", "InfoButtonStyle", "OutlineButtonStyle"]:
        content = re.sub(
            r'(<Style x:Key="' + style_key + r'"[^>]*>.*?<Setter Property="Height"\s+Value=)"40"',
            r'\g<1>"44"',
            content, flags=re.DOTALL
        )
        content = re.sub(
            r'(<Style x:Key="' + style_key + r'"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"13\.?5?"',
            r'\g<1>"16"',
            content, flags=re.DOTALL
        )

    # LightComboBoxStyle, LightDatePickerStyle
    for style_key in ["LightComboBoxStyle", "LightDatePickerStyle"]:
        content = re.sub(
            r'(<Style x:Key="' + style_key + r'"[^>]*>.*?<Setter Property="Height"\s+Value=)"40"',
            r'\g<1>"44"',
            content, flags=re.DOTALL
        )
        content = re.sub(
            r'(<Style x:Key="' + style_key + r'"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"13\.?5?"',
            r'\g<1>"15"',
            content, flags=re.DOTALL
        )

    # SeamlessComboBoxStyle
    content = re.sub(
        r'(<Style x:Key="SeamlessComboBoxStyle"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"13"',
        r'\g<1>"15"',
        content, flags=re.DOTALL
    )

    # ReportColumnHeaderStyle
    content = re.sub(
        r'(<Style x:Key="ReportColumnHeaderStyle"[^>]*>.*?<Setter Property="Height"\s+Value=)"46"',
        r'\g<1>"48"',
        content, flags=re.DOTALL
    )
    content = re.sub(
        r'(<Style x:Key="ReportColumnHeaderStyle"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"14"',
        r'\g<1>"16"',
        content, flags=re.DOTALL
    )

    # ReportRowStyle
    content = re.sub(
        r'(<Style x:Key="ReportRowStyle"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"13"',
        r'\g<1>"15"',
        content, flags=re.DOTALL
    )
    content = re.sub(
        r'(<Style x:Key="ReportRowStyle"[^>]*>.*?<Setter Property="MinHeight"\s+Value=)"42"',
        r'\g<1>"48"',
        content, flags=re.DOTALL
    )

    # ColCenter, ColCenterBold, ColLeftBold, ColAmount
    content = re.sub(
        r'(<Style x:Key="ColCenter"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"13"',
        r'\g<1>"15"',
        content, flags=re.DOTALL
    )
    content = re.sub(
        r'(<Style x:Key="ColLeftBold"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"13"',
        r'\g<1>"15"',
        content, flags=re.DOTALL
    )
    content = re.sub(
        r'(<Style x:Key="ColAmount"[^>]*>.*?<Setter Property="FontSize"\s+Value=)"13\.?5?"',
        r'\g<1>"16"',
        content, flags=re.DOTALL
    )

    # Page Header Subtitle
    content = content.replace(
        '<TextBlock Grid.Row="1" Text="รายงานสรุปผลและวิเคราะห์ข้อมูล — ระบบจัดการร้าน พอใจ"\n                                   FontSize="13"',
        '<TextBlock Grid.Row="1" Text="รายงานสรุปผลและวิเคราะห์ข้อมูล — ระบบจัดการร้าน พอใจ"\n                                   FontSize="15"'
    )

    # KPI Cards:
    # Card 1: 💰 ยอดขายรวม
    content = content.replace(
        '<Border Width="36" Height="36" CornerRadius="10" Background="#DCFCE7" Margin="0,0,10,0">\n                                    <TextBlock Text="💰" FontSize="18"',
        '<Border Width="40" Height="40" CornerRadius="10" Background="#DCFCE7" Margin="0,0,10,0">\n                                    <TextBlock Text="💰" FontSize="20"'
    )
    content = content.replace(
        '<TextBlock Text="ยอดขายรวม" FontSize="13" FontWeight="SemiBold"',
        '<TextBlock Text="ยอดขายรวม" FontSize="16" FontWeight="SemiBold"'
    )
    content = content.replace(
        '<TextBlock Grid.Row="2"\n                                   Text="ยอดขายสุทธิของช่วงเวลาที่เลือก"\n                                   FontSize="11"',
        '<TextBlock Grid.Row="2"\n                                   Text="ยอดขายสุทธิของช่วงเวลาที่เลือก"\n                                   FontSize="14"'
    )

    # Card 2: 🧾 จำนวนออเดอร์
    content = content.replace(
        '<Border Width="36" Height="36" CornerRadius="10" Background="#E0F2FE" Margin="0,0,10,0">\n                                    <TextBlock Text="🧾" FontSize="18"',
        '<Border Width="40" Height="40" CornerRadius="10" Background="#E0F2FE" Margin="0,0,10,0">\n                                    <TextBlock Text="🧾" FontSize="20"'
    )
    content = content.replace(
        '<TextBlock Text="จำนวนออเดอร์" FontSize="13" FontWeight="SemiBold"',
        '<TextBlock Text="จำนวนออเดอร์" FontSize="16" FontWeight="SemiBold"'
    )
    content = content.replace(
        '<TextBlock Grid.Row="2"\n                                   Text="จำนวนบิลขายทั้งหมดที่บันทึก"\n                                   FontSize="11"',
        '<TextBlock Grid.Row="2"\n                                   Text="จำนวนบิลขายทั้งหมดที่บันทึก"\n                                   FontSize="14"'
    )

    # Card 3: 💸 ค่าใช้จ่ายรวม
    content = content.replace(
        '<Border Width="36" Height="36" CornerRadius="10" Background="#FEE2E2" Margin="0,0,10,0">\n                                    <TextBlock Text="💸" FontSize="18"',
        '<Border Width="40" Height="40" CornerRadius="10" Background="#FEE2E2" Margin="0,0,10,0">\n                                    <TextBlock Text="💸" FontSize="20"'
    )
    content = content.replace(
        '<TextBlock Text="ค่าใช้จ่ายรวม" FontSize="13" FontWeight="SemiBold"',
        '<TextBlock Text="ค่าใช้จ่ายรวม" FontSize="16" FontWeight="SemiBold"'
    )
    content = content.replace(
        '<TextBlock Grid.Row="2"\n                                   Text="รายจ่ายดำเนินงานทั้งหมด"\n                                   FontSize="11"',
        '<TextBlock Grid.Row="2"\n                                   Text="รายจ่ายดำเนินงานทั้งหมด"\n                                   FontSize="14"'
    )

    # Filter Bar Labels: FontSize 12.5 -> 14.5
    content = content.replace('Text="ประเภทรายงาน:" FontWeight="SemiBold" FontSize="12.5"',
                              'Text="ประเภทรายงาน:" FontWeight="SemiBold" FontSize="14.5"')
    content = content.replace('Text="ช่วงเวลา:" FontWeight="SemiBold" FontSize="12.5"',
                              'Text="ช่วงเวลา:" FontWeight="SemiBold" FontSize="14.5"')
    content = content.replace('Text="ตั้งแต่วันที่:" FontWeight="SemiBold" FontSize="12.5"',
                              'Text="ตั้งแต่วันที่:" FontWeight="SemiBold" FontSize="14.5"')
    content = content.replace('Text="ถึงวันที่:" FontWeight="SemiBold" FontSize="12.5"',
                              'Text="ถึงวันที่:" FontWeight="SemiBold" FontSize="14.5"')

    # Filter Bar Action Buttons
    # ประมวลผล button
    content = re.sub(
        r'(<Button Command="\{Binding GenerateReportCommand\}"[^>]*?Height=)"40"',
        r'\g<1>"44"',
        content
    )
    content = content.replace(
        '<TextBlock Text="ประมวลผล"\n                                                               Foreground="#FFFFFF"\n                                                               FontWeight="Bold"\n                                                               FontSize="14"',
        '<TextBlock Text="ประมวลผล"\n                                                               Foreground="#FFFFFF"\n                                                               FontWeight="Bold"\n                                                               FontSize="16"'
    )
    # ล้าง button
    content = re.sub(
        r'(<Button Command="\{Binding ClearReportFilterCommand\}"\s+Height=)"40"',
        r'\g<1>"44"',
        content
    )
    content = content.replace(
        '<TextBlock Text="ล้าง" FontWeight="SemiBold" FontSize="13.5"',
        '<TextBlock Text="ล้าง" FontWeight="SemiBold" FontSize="16"'
    )

    # Left Card (Chart):
    # Header Icon 32->36, Title 16->18
    content = content.replace(
        '<Border Width="32" Height="32" CornerRadius="8" Background="#E0F2FE" Margin="0,0,10,0">\n                                <TextBlock Text="📈" FontSize="16"',
        '<Border Width="36" Height="36" CornerRadius="8" Background="#E0F2FE" Margin="0,0,10,0">\n                                <TextBlock Text="📈" FontSize="18"'
    )
    content = content.replace(
        '<TextBlock Text="แนวโน้มยอดขายและข้อมูลเชิงลึก"\n                                           FontSize="16"',
        '<TextBlock Text="แนวโน้มยอดขายและข้อมูลเชิงลึก"\n                                           FontSize="18"'
    )
    # Excel button
    content = re.sub(
        r'(<Button Command="\{Binding ExportReportCommand\}"[^>]*?Height=)"34"',
        r'\g<1>"38"',
        content
    )
    content = content.replace(
        '<TextBlock Text="📊" FontSize="12" Margin="0,0,4,0" VerticalAlignment="Center"/>\n                                        <TextBlock Text="Excel" FontWeight="Bold" FontSize="12.5"',
        '<TextBlock Text="📊" FontSize="14" Margin="0,0,4,0" VerticalAlignment="Center"/>\n                                        <TextBlock Text="Excel" FontWeight="Bold" FontSize="14"'
    )
    # Print button
    content = re.sub(
        r'(<Button Command="\{Binding PrintReportCommand\}"[^>]*?Height=)"34"',
        r'\g<1>"38"',
        content
    )
    content = content.replace(
        '<TextBlock Text="🖨️" FontSize="12" Margin="0,0,4,0" VerticalAlignment="Center"/>\n                                        <TextBlock Text="พิมพ์" FontWeight="Bold" FontSize="12.5"',
        '<TextBlock Text="🖨️" FontSize="14" Margin="0,0,4,0" VerticalAlignment="Center"/>\n                                        <TextBlock Text="พิมพ์" FontWeight="Bold" FontSize="14"'
    )

    # Right Card (Summary Ranking):
    # Header Icon 30->34, Title 15->18
    content = content.replace(
        '<Border Width="30" Height="30" CornerRadius="8" Background="#FEF3C7" Margin="0,0,8,0">\n                                <TextBlock Text="🏆" FontSize="15"',
        '<Border Width="34" Height="34" CornerRadius="8" Background="#FEF3C7" Margin="0,0,8,0">\n                                <TextBlock Text="🏆" FontSize="18"'
    )
    content = content.replace(
        '<TextBlock Text="{Binding SummaryListTitle, FallbackValue=\'สรุปอันดับสำคัญ\'}"\n                                           FontSize="15"',
        '<TextBlock Text="{Binding SummaryListTitle, FallbackValue=\'สรุปอันดับสำคัญ\'}"\n                                           FontSize="18"'
    )
    # Ranking items:
    content = content.replace('FontSize="12" FontWeight="Bold" Foreground="#0284C7"',
                              'FontSize="14" FontWeight="Bold" Foreground="#0284C7"')
    content = content.replace('FontSize="13" FontWeight="Bold" Foreground="#1E293B"',
                              'FontSize="15" FontWeight="Bold" Foreground="#1E293B"')
    content = content.replace('FontSize="11.5" Foreground="#64748B"',
                              'FontSize="13" Foreground="#64748B"')
    content = content.replace('FontSize="13.5" FontWeight="ExtraBold" Foreground="#0F172A"',
                              'FontSize="15.5" FontWeight="ExtraBold" Foreground="#0F172A"')
    content = content.replace('FontSize="10.5" FontWeight="Bold" Foreground="#0284C7"',
                              'FontSize="12" FontWeight="Bold" Foreground="#0284C7"')
    content = content.replace('Text="คลิกที่รายการเพื่อดูรายละเอียดแบบเจาะลึก" FontSize="12"',
                              'Text="คลิกที่รายการเพื่อดูรายละเอียดแบบเจาะลึก" FontSize="13.5"')

    # Report Item Details Modal Overlay:
    content = content.replace(
        '<TextBlock Grid.Column="0" Text="📜 " FontSize="20"/>\n                            <TextBlock Grid.Column="1"\n                                       Text="รายละเอียดรายการสรุป"\n                                       FontSize="18"',
        '<TextBlock Grid.Column="0" Text="📜 " FontSize="22"/>\n                            <TextBlock Grid.Column="1"\n                                       Text="รายละเอียดรายการสรุป"\n                                       FontSize="20"'
    )
    content = content.replace(
        'Text="ข้อมูลเชิงลึกและประวัติของรายการที่เลือก"\n                                       FontSize="12.5"',
        'Text="ข้อมูลเชิงลึกและประวัติของรายการที่เลือก"\n                                       FontSize="14"'
    )

    # Modal inset summary
    content = content.replace('Text="📌 รหัส/รายการ:" FontSize="13" FontWeight="SemiBold"',
                              'Text="📌 รหัส/รายการ:" FontSize="14.5" FontWeight="SemiBold"')
    content = content.replace('Text="{Binding SelectedReportRow.Col1}" FontSize="14" FontWeight="Bold"',
                              'Text="{Binding SelectedReportRow.Col1}" FontSize="15.5" FontWeight="Bold"')
    content = content.replace('Text="📅 วันที่:" FontSize="13" FontWeight="SemiBold"',
                              'Text="📅 วันที่:" FontSize="14.5" FontWeight="SemiBold"')
    content = content.replace('Text="{Binding SelectedReportRow.DateStr}" FontSize="14" FontWeight="Bold"',
                              'Text="{Binding SelectedReportRow.DateStr}" FontSize="15.5" FontWeight="Bold"')

    # Modal detail rows
    content = content.replace('Text="รายละเอียดสินค้า/รายการ:" FontSize="13" FontWeight="SemiBold"',
                              'Text="รายละเอียดสินค้า/รายการ:" FontSize="14.5" FontWeight="SemiBold"')
    content = content.replace('Text="{Binding SelectedReportRow.Col2}" FontSize="14" FontWeight="Bold"',
                              'Text="{Binding SelectedReportRow.Col2}" FontSize="15.5" FontWeight="Bold"')

    content = content.replace('Text="จำนวนเงิน/ปริมาณ:" FontSize="13" FontWeight="SemiBold"',
                              'Text="จำนวนเงิน/ปริมาณ:" FontSize="14.5" FontWeight="SemiBold"')
    content = content.replace('Text="{Binding SelectedReportRow.Col3}" FontSize="15" FontWeight="Bold"',
                              'Text="{Binding SelectedReportRow.Col3}" FontSize="16" FontWeight="Bold"')

    content = content.replace('Text="หมวดหมู่/วิธีการชำระ:" FontSize="13" FontWeight="SemiBold"',
                              'Text="หมวดหมู่/วิธีการชำระ:" FontSize="14.5" FontWeight="SemiBold"')
    content = content.replace('Text="{Binding SelectedReportRow.Col4}" FontSize="14" FontWeight="SemiBold"',
                              'Text="{Binding SelectedReportRow.Col4}" FontSize="15.5" FontWeight="SemiBold"')

    content = content.replace('Text="สถานะ/หมายเหตุ:" FontSize="13" FontWeight="SemiBold"',
                              'Text="สถานะ/หมายเหตุ:" FontSize="14.5" FontWeight="SemiBold"')
    content = content.replace('Text="ประมวลผลสำเร็จ เรียบร้อย" FontSize="13"',
                              'Text="ประมวลผลสำเร็จ เรียบร้อย" FontSize="14.5"')

    # Modal buttons (Height 38 -> 46, FontSize 13.5/13 -> 15.5)
    content = re.sub(
        r'(<Button Command="\{Binding PrintReportCommand\}"[^>]*?Height=)"38"(\s+Padding="20,0">.*?<TextBlock Text="🖨️ พิมพ์ใบรายงาน"\s+FontWeight="SemiBold"\s+FontSize=)"13\.5"',
        r'\g<1>"46"\g<2>"15.5"',
        content, flags=re.DOTALL
    )
    content = re.sub(
        r'(<Button Command="\{Binding CloseReportModalCommand\}"[^>]*?Height=)"38"(\s+Padding="20,0">.*?<TextBlock Text="ปิด"\s+FontWeight="SemiBold"\s+FontSize=)"13"',
        r'\g<1>"46"\g<2>"15.5"',
        content, flags=re.DOTALL
    )

    with open(path, "w", encoding="utf-8") as f:
        f.write(content)
    print("ReportsView.xaml updated.")

if __name__ == "__main__":
    update_customer_view()
    update_staff_view()
    update_partner_view()
    update_expense_view()
    update_reports_view()
    print("All Batch 3 views successfully updated!")
