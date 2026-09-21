import sys
import re

def update_purchase_order():
    file_path = r"d:\ShopManagement_หลัก\MatchaShop\Views\PurchaseOrderView.xaml"
    with open(file_path, "r", encoding="utf-8") as f:
        content = f.read()

    # 1. Styles
    # RoseBtnStyle
    content = re.sub(
        r'(<Style x:Key="RoseBtnStyle"[^>]*>[\s\S]*?<Setter Property="FontSize"\s+Value=")14("[\s\S]*?<Setter Property="Height"\s+Value=")40(")',
        r'\g<1>16\g<2>44\3',
        content
    )
    # ViewDetailsButtonStyle
    content = re.sub(
        r'(<Style x:Key="ViewDetailsButtonStyle"[^>]*>[\s\S]*?<Setter Property="FontSize"\s+Value=")12("[\s\S]*?<Setter Property="Height"\s+Value=")28(")',
        r'\g<1>14\g<2>34\3',
        content
    )
    # LightDatePickerStyle
    content = re.sub(
        r'(<Style x:Key="LightDatePickerStyle"[^>]*>[\s\S]*?<Setter Property="Height"\s+Value=")40("[\s\S]*?<Setter Property="FontSize"\s+Value=")13(")',
        r'\g<1>44\g<2>16\3',
        content
    )
    # SeamlessComboBoxStyle
    content = re.sub(
        r'(<Style x:Key="SeamlessComboBoxStyle"[^>]*>[\s\S]*?<Setter Property="FontSize"\s+Value=")13(")',
        r'\g<1>15\2',
        content
    )
    # POColumnHeaderStyle
    content = re.sub(
        r'(<Style x:Key="POColumnHeaderStyle"[^>]*>[\s\S]*?<Setter Property="Height"\s+Value=")46(")',
        r'\g<1>48\2',
        content
    )
    # PORowStyle
    content = re.sub(
        r'(<Style x:Key="PORowStyle"[^>]*>[\s\S]*?<Setter Property="FontSize"\s+Value=")13(")',
        r'\g<1>15\2',
        content
    )

    # Column Alignment Helpers: ColCenter, ColCenterBold, ColLeftBold, ColRightBold
    content = content.replace(
        '''<Style x:Key="ColCenter" TargetType="TextBlock">
<Setter Property="HorizontalAlignment" Value="Center"/>
<Setter Property="VerticalAlignment"   Value="Center"/>
<Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
<Setter Property="Foreground"          Value="#000000"/>
</Style>''',
        '''<Style x:Key="ColCenter" TargetType="TextBlock">
<Setter Property="HorizontalAlignment" Value="Center"/>
<Setter Property="VerticalAlignment"   Value="Center"/>
<Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
<Setter Property="FontSize"            Value="15"/>
<Setter Property="Foreground"          Value="#000000"/>
</Style>'''
    )
    content = content.replace(
        '''<Style x:Key="ColLeftBold" TargetType="TextBlock">
<Setter Property="HorizontalAlignment" Value="Left"/>
<Setter Property="VerticalAlignment"   Value="Center"/>
<Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
<Setter Property="FontWeight"          Value="Bold"/>
<Setter Property="Foreground"          Value="#000000"/>
</Style>''',
        '''<Style x:Key="ColLeftBold" TargetType="TextBlock">
<Setter Property="HorizontalAlignment" Value="Left"/>
<Setter Property="VerticalAlignment"   Value="Center"/>
<Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
<Setter Property="FontWeight"          Value="Bold"/>
<Setter Property="FontSize"            Value="15"/>
<Setter Property="Foreground"          Value="#000000"/>
</Style>'''
    )
    content = content.replace(
        '''<Style x:Key="ColRightBold" TargetType="TextBlock">
<Setter Property="HorizontalAlignment" Value="Right"/>
<Setter Property="VerticalAlignment"   Value="Center"/>
<Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
<Setter Property="FontWeight"          Value="Bold"/>
<Setter Property="Foreground"          Value="#000000"/>
</Style>''',
        '''<Style x:Key="ColRightBold" TargetType="TextBlock">
<Setter Property="HorizontalAlignment" Value="Right"/>
<Setter Property="VerticalAlignment"   Value="Center"/>
<Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
<Setter Property="FontWeight"          Value="Bold"/>
<Setter Property="FontSize"            Value="15"/>
<Setter Property="Foreground"          Value="#000000"/>
</Style>'''
    )

    # 2. Header subtitle
    content = content.replace(
        '''<TextBlock Text="จัดการสั่งซื้อสินค้าและเติมสต็อก — ระบบจัดการร้าน พอใจ"
FontSize="13"
Foreground="#718096"
Margin="0"/>''',
        '''<TextBlock Text="จัดการสั่งซื้อสินค้าและเติมสต็อก — ระบบจัดการร้าน พอใจ"
FontSize="15"
Foreground="#718096"
Margin="0"/>'''
    )

    # 3. KPI Cards: 1, 2, 3, 4
    # Card 1
    content = content.replace(
        '''<StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,10">
<Border Width="34" Height="34" CornerRadius="8" Background="#E0F2FE" Margin="0,0,8,0">
<TextBlock Text="📦" FontSize="16" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
</Border>
<TextBlock Text="ใบสั่งซื้อทั้งหมด" FontSize="12.5" FontWeight="SemiBold"
Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
</StackPanel>
<TextBlock Grid.Row="1"
Text="{Binding TotalPOCount}"
FontSize="26" FontWeight="Bold"
Foreground="#0B2545"
Margin="0,0,0,4"/>
<TextBlock Grid.Row="2"
Text="จำนวนใบสั่งซื้อที่ทำรายการ"
FontSize="11"
Foreground="#A0AEC0"
Margin="0"/>''',
        '''<StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,10">
<Border Width="40" Height="40" CornerRadius="8" Background="#E0F2FE" Margin="0,0,8,0">
<TextBlock Text="📦" FontSize="20" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
</Border>
<TextBlock Text="ใบสั่งซื้อทั้งหมด" FontSize="16" FontWeight="SemiBold"
Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
</StackPanel>
<TextBlock Grid.Row="1"
Text="{Binding TotalPOCount}"
FontSize="28" FontWeight="Bold"
Foreground="#0B2545"
Margin="0,0,0,4"/>
<TextBlock Grid.Row="2"
Text="จำนวนใบสั่งซื้อที่ทำรายการ"
FontSize="14"
Foreground="#A0AEC0"
Margin="0"/>'''
    )

    # Card 2
    content = content.replace(
        '''<StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,10">
<Border Width="34" Height="34" CornerRadius="8" Background="#FEF3C7" Margin="0,0,8,0">
<TextBlock Text="⏳" FontSize="16" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
</Border>
<TextBlock Text="รอดำเนินการ / ส่งมอบ" FontSize="12.5" FontWeight="SemiBold"
Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
</StackPanel>
<TextBlock Grid.Row="1"
Text="{Binding PendingPOCount}"
FontSize="26" FontWeight="Bold"
Foreground="#D69E2E"
Margin="0,0,0,4"/>
<TextBlock Grid.Row="2"
Text="อยู่ระหว่างรอซัพพลายเออร์ส่งของ"
FontSize="11"
Foreground="#A0AEC0"
Margin="0"/>''',
        '''<StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,10">
<Border Width="40" Height="40" CornerRadius="8" Background="#FEF3C7" Margin="0,0,8,0">
<TextBlock Text="⏳" FontSize="20" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
</Border>
<TextBlock Text="รอดำเนินการ / ส่งมอบ" FontSize="16" FontWeight="SemiBold"
Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
</StackPanel>
<TextBlock Grid.Row="1"
Text="{Binding PendingPOCount}"
FontSize="28" FontWeight="Bold"
Foreground="#D69E2E"
Margin="0,0,0,4"/>
<TextBlock Grid.Row="2"
Text="อยู่ระหว่างรอซัพพลายเออร์ส่งของ"
FontSize="14"
Foreground="#A0AEC0"
Margin="0"/>'''
    )

    # Card 3
    content = content.replace(
        '''<StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,10">
<Border Width="34" Height="34" CornerRadius="8" Background="#D1FAE5" Margin="0,0,8,0">
<TextBlock Text="✅" FontSize="16" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
</Border>
<TextBlock Text="ได้รับสินค้าแล้ว" FontSize="12.5" FontWeight="SemiBold"
Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
</StackPanel>
<TextBlock Grid.Row="1"
Text="{Binding CompletedPOCount}"
FontSize="26" FontWeight="Bold"
Foreground="#059669"
Margin="0,0,0,4"/>
<TextBlock Grid.Row="2"
Text="ตรวจรับและเข้าสต็อกสำเร็จ"
FontSize="11"
Foreground="#A0AEC0"
Margin="0"/>''',
        '''<StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,10">
<Border Width="40" Height="40" CornerRadius="8" Background="#D1FAE5" Margin="0,0,8,0">
<TextBlock Text="✅" FontSize="20" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
</Border>
<TextBlock Text="ได้รับสินค้าแล้ว" FontSize="16" FontWeight="SemiBold"
Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
</StackPanel>
<TextBlock Grid.Row="1"
Text="{Binding CompletedPOCount}"
FontSize="28" FontWeight="Bold"
Foreground="#059669"
Margin="0,0,0,4"/>
<TextBlock Grid.Row="2"
Text="ตรวจรับและเข้าสต็อกสำเร็จ"
FontSize="14"
Foreground="#A0AEC0"
Margin="0"/>'''
    )

    # Card 4
    content = content.replace(
        '''<StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,10">
<Border Width="34" Height="34" CornerRadius="8" Background="#F3E8FF" Margin="0,0,8,0">
<TextBlock Text="💵" FontSize="16" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
</Border>
<TextBlock Text="ยอดสั่งซื้อรวม" FontSize="12.5" FontWeight="SemiBold"
Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
</StackPanel>
<TextBlock Grid.Row="1"
Text="{Binding TotalPOAmount, StringFormat={}{0:N2}}"
FontSize="24" FontWeight="Bold"
Foreground="#6B21A8"
Margin="0,0,0,4"/>
<TextBlock Grid.Row="2"
Text="มูลค่าคำสั่งซื้อสินค้าทั้งหมด"
FontSize="11"
Foreground="#A0AEC0"
Margin="0"/>''',
        '''<StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,10">
<Border Width="40" Height="40" CornerRadius="8" Background="#F3E8FF" Margin="0,0,8,0">
<TextBlock Text="💵" FontSize="20" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
</Border>
<TextBlock Text="ยอดสั่งซื้อรวม" FontSize="16" FontWeight="SemiBold"
Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
</StackPanel>
<TextBlock Grid.Row="1"
Text="{Binding TotalPOAmount, StringFormat={}{0:N2}}"
FontSize="28" FontWeight="Bold"
Foreground="#6B21A8"
Margin="0,0,0,4"/>
<TextBlock Grid.Row="2"
Text="มูลค่าคำสั่งซื้อสินค้าทั้งหมด"
FontSize="14"
Foreground="#A0AEC0"
Margin="0"/>'''
    )

    # 4. Left Card (Purchase Orders):
    # Title
    content = content.replace(
        '''                <!-- Title & Icon -->
                <StackPanel Grid.Column="0" Orientation="Horizontal" VerticalAlignment="Center" Margin="0,0,12,0">
                    <Border Width="34" Height="34" CornerRadius="10" Background="#E0F2FE" Margin="0,0,8,0">
                        <TextBlock Text="📦" FontSize="17" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                    </Border>
                    <TextBlock Text="ใบสั่งซื้อ" FontSize="18" FontWeight="Bold" Foreground="#0F172A" VerticalAlignment="Center"/>
                </StackPanel>''',
        '''                <!-- Title & Icon -->
                <StackPanel Grid.Column="0" Orientation="Horizontal" VerticalAlignment="Center" Margin="0,0,12,0">
                    <Border Width="38" Height="38" CornerRadius="10" Background="#E0F2FE" Margin="0,0,8,0">
                        <TextBlock Text="📦" FontSize="18" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                    </Border>
                    <TextBlock Text="ใบสั่งซื้อ" FontSize="20" FontWeight="Bold" Foreground="#0F172A" VerticalAlignment="Center"/>
                </StackPanel>'''
    )

    # Search Box input container
    content = content.replace(
        '''                    <!-- Rounded Input Container -->
                    <Border x:Name="bdPOSearchInput"
                            Grid.Column="0"
                            Background="#FFFFFF"
                            BorderBrush="#BAE6FD"
                            BorderThickness="1.5"
                            CornerRadius="13"
                            Height="40"
                            Padding="14,0">
                        <Grid VerticalAlignment="Center">
                            <!-- TextBox -->
                            <TextBox x:Name="txtPOSearch"
                                     Text="{Binding POSearchKeyword, UpdateSourceTrigger=PropertyChanged}"
                                     Style="{StaticResource LightTextBoxStyle}"
                                     FontSize="13.5"
                                     FontFamily="Noto Sans Thai, Leelawadee UI, Segoe UI, Tahoma"
                                     Foreground="#0F172A"
                                     VerticalContentAlignment="Center"
                                     Padding="0,2"/>

                            <!-- Placeholder -->
                            <TextBlock Text="ค้นหาเลขที่ PO / ซัพพลายเออร์..."
                                       Foreground="#94A3B8"
                                       FontSize="13.5"
                                       FontFamily="Noto Sans Thai, Leelawadee UI, Segoe UI, Tahoma"
                                       VerticalAlignment="Center"
                                       Margin="2,0,0,0"
                                       IsHitTestVisible="False">''',
        '''                    <!-- Rounded Input Container -->
                    <Border x:Name="bdPOSearchInput"
                            Grid.Column="0"
                            Background="#FFFFFF"
                            BorderBrush="#BAE6FD"
                            BorderThickness="1.5"
                            CornerRadius="13"
                            Height="44"
                            Padding="14,0">
                        <Grid VerticalAlignment="Center">
                            <!-- TextBox -->
                            <TextBox x:Name="txtPOSearch"
                                     Text="{Binding POSearchKeyword, UpdateSourceTrigger=PropertyChanged}"
                                     Style="{StaticResource LightTextBoxStyle}"
                                     FontSize="16"
                                     FontFamily="Noto Sans Thai, Leelawadee UI, Segoe UI, Tahoma"
                                     Foreground="#0F172A"
                                     VerticalContentAlignment="Center"
                                     Padding="0,2"/>

                            <!-- Placeholder -->
                            <TextBlock Text="ค้นหาเลขที่ PO / ซัพพลายเออร์..."
                                       Foreground="#94A3B8"
                                       FontSize="15"
                                       FontFamily="Noto Sans Thai, Leelawadee UI, Segoe UI, Tahoma"
                                       VerticalAlignment="Center"
                                       Margin="2,0,0,0"
                                       IsHitTestVisible="False">'''
    )

    # Search Button
    content = content.replace(
        '''                    <!-- Search Button -->
                    <Button Grid.Column="1"
                            Command="{Binding SearchPOCommand}"
                            Margin="6,0,0,0"
                            Height="38"
                            Padding="16,0"
                            Cursor="Hand">
                        <Button.Style>
                            <Style TargetType="Button">
                                <Setter Property="Background" Value="#1E88E5"/>
                                <Setter Property="Foreground" Value="White"/>
                                <Setter Property="FontWeight" Value="ExtraBold"/>
                                <Setter Property="FontSize" Value="13.5"/>
                                <Setter Property="Template">
                                    <Setter.Value>
                                        <ControlTemplate TargetType="Button">
                                            <Border x:Name="border" 
                                                    Background="{TemplateBinding Background}" 
                                                    CornerRadius="13" 
                                                    Padding="{TemplateBinding Padding}">
                                                <Border.Effect>
                                                    <DropShadowEffect BlurRadius="8" ShadowDepth="2" Direction="270" Color="#1E88E5" Opacity="0.3"/>
                                                </Border.Effect>
                                                <TextBlock Text="ค้นหา" 
                                                           FontWeight="ExtraBold" 
                                                           Foreground="White" 
                                                           FontSize="13.5" 
                                                           HorizontalAlignment="Center" 
                                                           VerticalAlignment="Center"/>
                                            </Border>''',
        '''                    <!-- Search Button -->
                    <Button Grid.Column="1"
                            Command="{Binding SearchPOCommand}"
                            Margin="6,0,0,0"
                            Height="44"
                            Padding="20,0"
                            Cursor="Hand">
                        <Button.Style>
                            <Style TargetType="Button">
                                <Setter Property="Background" Value="#1E88E5"/>
                                <Setter Property="Foreground" Value="White"/>
                                <Setter Property="FontWeight" Value="Bold"/>
                                <Setter Property="FontSize" Value="16"/>
                                <Setter Property="Template">
                                    <Setter.Value>
                                        <ControlTemplate TargetType="Button">
                                            <Border x:Name="border" 
                                                    Background="{TemplateBinding Background}" 
                                                    CornerRadius="13" 
                                                    Padding="{TemplateBinding Padding}">
                                                <Border.Effect>
                                                    <DropShadowEffect BlurRadius="8" ShadowDepth="2" Direction="270" Color="#1E88E5" Opacity="0.3"/>
                                                </Border.Effect>
                                                <TextBlock Text="ค้นหา" 
                                                           FontWeight="Bold" 
                                                           Foreground="White" 
                                                           FontSize="16" 
                                                           HorizontalAlignment="Center" 
                                                           VerticalAlignment="Center"/>
                                            </Border>'''
    )

    # Status Filter Dropdown container
    content = content.replace(
        '''                <!-- Status Filter Dropdown (Unified Seamless Container) -->
                <Border Grid.Column="2" 
                        Background="#FFFFFF" 
                        BorderBrush="#BAE6FD" 
                        BorderThickness="1.5" 
                        CornerRadius="13" 
                        Padding="10,0,6,0" 
                        Height="40" 
                        VerticalAlignment="Center">
                    <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
                        <TextBlock Text="สถานะ:" 
                                   FontSize="13" 
                                   Foreground="#64748B" 
                                   FontWeight="SemiBold" 
                                   FontFamily="Noto Sans Thai, Leelawadee UI, Segoe UI, Tahoma"
                                   VerticalAlignment="Center" 
                                   Margin="0,0,4,0"/>
                        <ComboBox ItemsSource="{Binding POStatusFilterOptions}"
                                  SelectedItem="{Binding POStatusFilter}"
                                  Style="{StaticResource SeamlessComboBoxStyle}"
                                  Height="36"
                                  MinWidth="90"
                                  VerticalAlignment="Center"
                                  VerticalContentAlignment="Center"/>
                    </StackPanel>
                </Border>''',
        '''                <!-- Status Filter Dropdown (Unified Seamless Container) -->
                <Border Grid.Column="2" 
                        Background="#FFFFFF" 
                        BorderBrush="#BAE6FD" 
                        BorderThickness="1.5" 
                        CornerRadius="13" 
                        Padding="12,0,8,0" 
                        Height="44" 
                        VerticalAlignment="Center">
                    <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
                        <TextBlock Text="สถานะ:" 
                                   FontSize="15" 
                                   Foreground="#64748B" 
                                   FontWeight="SemiBold" 
                                   FontFamily="Noto Sans Thai, Leelawadee UI, Segoe UI, Tahoma"
                                   VerticalAlignment="Center" 
                                   Margin="0,0,4,0"/>
                        <ComboBox ItemsSource="{Binding POStatusFilterOptions}"
                                  SelectedItem="{Binding POStatusFilter}"
                                  Style="{StaticResource SeamlessComboBoxStyle}"
                                  Height="40"
                                  FontSize="15"
                                  MinWidth="100"
                                  VerticalAlignment="Center"
                                  VerticalContentAlignment="Center"/>
                    </StackPanel>
                </Border>'''
    )

    # PO Card Item fonts
    content = content.replace(
        '''                                        <TextBlock Grid.Column="0" Text="{Binding PONumber}" FontSize="15" FontWeight="Bold" Foreground="#0284C7" VerticalAlignment="Center"/>
                                        
                                        <!-- Status Badge -->
                                        <Border Grid.Column="1" CornerRadius="8" Padding="10,4">''',
        '''                                        <TextBlock Grid.Column="0" Text="{Binding PONumber}" FontSize="16" FontWeight="Bold" Foreground="#0284C7" VerticalAlignment="Center"/>
                                        
                                        <!-- Status Badge -->
                                        <Border Grid.Column="1" CornerRadius="8" Padding="10,4">'''
    )
    content = content.replace(
        '''                                            <TextBlock Text="{Binding Status}" FontSize="12" FontWeight="Bold">''',
        '''                                            <TextBlock Text="{Binding Status}" FontSize="14" FontWeight="Bold">'''
    )
    content = content.replace(
        '''                                    <!-- Row 2: Supplier Name -->
                                    <StackPanel Grid.Row="1" Orientation="Horizontal" Margin="0,0,0,6">
                                        <TextBlock Text="🏢 " FontSize="12.5"/>
                                        <TextBlock Text="{Binding SupplierName}" FontSize="13.5" FontWeight="SemiBold" Foreground="#334155" TextTrimming="CharacterEllipsis"/>
                                    </StackPanel>''',
        '''                                    <!-- Row 2: Supplier Name -->
                                    <StackPanel Grid.Row="1" Orientation="Horizontal" Margin="0,0,0,6">
                                        <TextBlock Text="🏢 " FontSize="14"/>
                                        <TextBlock Text="{Binding SupplierName}" FontSize="15" FontWeight="SemiBold" Foreground="#334155" TextTrimming="CharacterEllipsis"/>
                                    </StackPanel>'''
    )
    content = content.replace(
        '''                                        <!-- Dates Stack (Order Date & Expected Date) -->
                                        <StackPanel Grid.Column="0" VerticalAlignment="Center">
                                            <StackPanel Orientation="Horizontal" Margin="0,0,0,3">
                                                <TextBlock Text="📝 วันที่ทำรายการ: " FontSize="12" Foreground="#64748B"/>
                                                <TextBlock Text="{Binding OrderDate, StringFormat=dd/MM/yyyy HH:mm}" FontSize="12" FontWeight="SemiBold" Foreground="#475569"/>
                                            </StackPanel>
                                            <StackPanel Orientation="Horizontal">
                                                <TextBlock Text="📅 กำหนดส่งมอบ: " FontSize="12" Foreground="#64748B"/>
                                                <TextBlock Text="{Binding ExpectedDate, StringFormat=dd/MM/yyyy}" FontSize="12" FontWeight="SemiBold" Foreground="#475569"/>
                                            </StackPanel>
                                        </StackPanel>

                                        <!-- Total Amount -->
                                        <TextBlock Grid.Column="1" Text="{Binding TotalAmount, StringFormat={}{0:N2}}" FontSize="17" FontWeight="ExtraBold" Foreground="#0F172A" VerticalAlignment="Center"/>''',
        '''                                        <!-- Dates Stack (Order Date & Expected Date) -->
                                        <StackPanel Grid.Column="0" VerticalAlignment="Center">
                                            <StackPanel Orientation="Horizontal" Margin="0,0,0,3">
                                                <TextBlock Text="📝 วันที่ทำรายการ: " FontSize="13.5" Foreground="#64748B"/>
                                                <TextBlock Text="{Binding OrderDate, StringFormat=dd/MM/yyyy HH:mm}" FontSize="14" FontWeight="SemiBold" Foreground="#475569"/>
                                            </StackPanel>
                                            <StackPanel Orientation="Horizontal">
                                                <TextBlock Text="📅 กำหนดส่งมอบ: " FontSize="13.5" Foreground="#64748B"/>
                                                <TextBlock Text="{Binding ExpectedDate, StringFormat=dd/MM/yyyy}" FontSize="14" FontWeight="SemiBold" Foreground="#475569"/>
                                            </StackPanel>
                                        </StackPanel>

                                        <!-- Total Amount -->
                                        <TextBlock Grid.Column="1" Text="{Binding TotalAmount, StringFormat={}{0:N2}}" FontSize="18" FontWeight="ExtraBold" Foreground="#0F172A" VerticalAlignment="Center"/>'''
    )

    # Left card bottom summary
    content = content.replace(
        '''            <!-- 3. Bottom Count Summary -->
            <Border Grid.Row="2" Background="#F1F5F9" CornerRadius="8" Padding="12,8" Margin="0,4,0,0">
                <TextBlock Text="{Binding PurchaseOrders.Count, StringFormat='แสดงทั้งหมด {0} รายการใบสั่งซื้อ'}"
                           FontSize="12.5"
                           FontWeight="SemiBold"
                           Foreground="#64748B"
                           HorizontalAlignment="Center"/>
            </Border>''',
        '''            <!-- 3. Bottom Count Summary -->
            <Border Grid.Row="2" Background="#F1F5F9" CornerRadius="8" Padding="12,8" Margin="0,4,0,0">
                <TextBlock Text="{Binding PurchaseOrders.Count, StringFormat='แสดงทั้งหมด {0} รายการใบสั่งซื้อ'}"
                           FontSize="14"
                           FontWeight="SemiBold"
                           Foreground="#64748B"
                           HorizontalAlignment="Center"/>
            </Border>'''
    )

    # 5. Right Card (Low Stock Items)
    content = content.replace(
        '''                <!-- Title & Warning Icon -->
                <StackPanel Grid.Column="0" Orientation="Horizontal" VerticalAlignment="Center">
                    <Border Width="34" Height="34" CornerRadius="10" Background="#FEF3C7" Margin="0,0,10,0">
                        <TextBlock Text="⚠️" FontSize="17" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                    </Border>
                    <TextBlock Text="สินค้าใกล้หมด" FontSize="18" FontWeight="Bold" Foreground="#0F172A" VerticalAlignment="Center"/>
                </StackPanel>

                <!-- Selected Count Badge -->
                <Border Grid.Column="1" Background="#E0F2FE" BorderBrush="#BAE6FD" BorderThickness="1" CornerRadius="10" Padding="10,4" VerticalAlignment="Center">
                    <TextBlock Text="{Binding SelectedLowStockCount, StringFormat='เลือกแล้ว {0}'}" FontSize="12.5" FontWeight="Bold" Foreground="#0284C7"/>
                </Border>''',
        '''                <!-- Title & Warning Icon -->
                <StackPanel Grid.Column="0" Orientation="Horizontal" VerticalAlignment="Center">
                    <Border Width="38" Height="38" CornerRadius="10" Background="#FEF3C7" Margin="0,0,10,0">
                        <TextBlock Text="⚠️" FontSize="18" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                    </Border>
                    <TextBlock Text="สินค้าใกล้หมด" FontSize="20" FontWeight="Bold" Foreground="#0F172A" VerticalAlignment="Center"/>
                </StackPanel>

                <!-- Selected Count Badge -->
                <Border Grid.Column="1" Background="#E0F2FE" BorderBrush="#BAE6FD" BorderThickness="1" CornerRadius="10" Padding="10,4" VerticalAlignment="Center">
                    <TextBlock Text="{Binding SelectedLowStockCount, StringFormat='เลือกแล้ว {0}'}" FontSize="14" FontWeight="Bold" Foreground="#0284C7"/>
                </Border>'''
    )

    # Checkbox item product name & code
    content = content.replace(
        '''                                    <!-- Product Name & Code -->
                                    <StackPanel Grid.Column="1" VerticalAlignment="Center">
                                        <TextBlock Text="{Binding Name}" FontSize="14.5" FontWeight="Bold" Foreground="#0F172A" TextTrimming="CharacterEllipsis"/>
                                        <TextBlock Text="{Binding Code, StringFormat='รหัส: {0}'}" FontSize="12" Foreground="#64748B" Margin="0,2,0,0"/>
                                    </StackPanel>

                                    <!-- Stock Quantity Badge -->
                                    <Border Grid.Column="2" Background="#FEE2E2" BorderBrush="#FECACA" BorderThickness="1" CornerRadius="8" Padding="10,5" VerticalAlignment="Center">
                                        <TextBlock Text="{Binding Stock, StringFormat='เหลือ {0} ชิ้น'}" FontSize="12.5" FontWeight="Bold" Foreground="#DC2626"/>
                                    </Border>''',
        '''                                    <!-- Product Name & Code -->
                                    <StackPanel Grid.Column="1" VerticalAlignment="Center">
                                        <TextBlock Text="{Binding Name}" FontSize="16" FontWeight="Bold" Foreground="#0F172A" TextTrimming="CharacterEllipsis"/>
                                        <TextBlock Text="{Binding Code, StringFormat='รหัส: {0}'}" FontSize="13.5" Foreground="#64748B" Margin="0,2,0,0"/>
                                    </StackPanel>

                                    <!-- Stock Quantity Badge -->
                                    <Border Grid.Column="2" Background="#FEE2E2" BorderBrush="#FECACA" BorderThickness="1" CornerRadius="8" Padding="10,5" VerticalAlignment="Center">
                                        <TextBlock Text="{Binding Stock, StringFormat='เหลือ {0} ชิ้น'}" FontSize="14" FontWeight="Bold" Foreground="#DC2626"/>
                                    </Border>'''
    )

    # Bottom action button
    content = content.replace(
        '''            <!-- 3. Bottom Action Button (Pinned to Bottom) -->
            <Button Grid.Row="2"
                    Content="➕ สร้างใบสั่งซื้อจากรายการที่เลือก"
                    Command="{Binding CreatePOFromSelectedLowStockCommand}"
                    Height="46"
                    FontSize="14.5"
                    FontWeight="Bold"''',
        '''            <!-- 3. Bottom Action Button (Pinned to Bottom) -->
            <Button Grid.Row="2"
                    Content="➕ สร้างใบสั่งซื้อจากรายการที่เลือก"
                    Command="{Binding CreatePOFromSelectedLowStockCommand}"
                    Height="46"
                    FontSize="16"
                    FontWeight="Bold"'''
    )

    # 6. Modal Overlay (Add / Edit PO)
    content = content.replace(
        '''                                <TextBlock Text="{Binding POModalTitle}" FontSize="19" FontWeight="Bold" Foreground="#0F172A"/>
                                <TextBlock Text="กรอกรายละเอียดผู้จัดจำหน่ายและเพิ่มรายการสินค้าที่ต้องการสั่งซื้อ" FontSize="12" Foreground="#64748B" Margin="0,2,0,0"/>''',
        '''                                <TextBlock Text="{Binding POModalTitle}" FontSize="20" FontWeight="Bold" Foreground="#0F172A"/>
                                <TextBlock Text="กรอกรายละเอียดผู้จัดจำหน่ายและเพิ่มรายการสินค้าที่ต้องการสั่งซื้อ" FontSize="14" Foreground="#64748B" Margin="0,2,0,0"/>'''
    )
    content = content.replace(
        '''                        <!-- Auto Fill Reorder Items Button moved to top right corner (replacing circular '✕' button) -->
                        <Button DockPanel.Dock="Right"
                                Command="{Binding AutoFillReorderItemsCommand}"
                                Content="⚡ ดึงสินค้าถึงจุดสั่งซื้อ"
                                Height="36" Padding="14,0" FontSize="13" FontWeight="Bold"''',
        '''                        <!-- Auto Fill Reorder Items Button moved to top right corner (replacing circular '✕' button) -->
                        <Button DockPanel.Dock="Right"
                                Command="{Binding AutoFillReorderItemsCommand}"
                                Content="⚡ ดึงสินค้าถึงจุดสั่งซื้อ"
                                Height="40" Padding="16,0" FontSize="14" FontWeight="Bold"'''
    )

    # Modal Card 1
    content = content.replace(
        '''                            <!-- Supplier Name (Searchable ComboBox) -->
                            <StackPanel Grid.Column="0">
                                <TextBlock Text="ผู้จัดจำหน่าย / ซัพพลายเออร์ *" FontSize="12.5" FontWeight="SemiBold" Foreground="#475569" Margin="0,0,0,5"/>
                                <Border Background="#FFFFFF" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="6">
                                    <ComboBox ItemsSource="{Binding SuppliersList}"
                                              DisplayMemberPath="SupplierName"
                                              SelectedValuePath="SupplierID"
                                              TextSearch.TextPath="SupplierName"
                                              SelectedItem="{Binding SelectedSupplier, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                              SelectedValue="{Binding POSupplierId, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                              Text="{Binding POSupplierName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                              IsEditable="True"
                                              IsTextSearchEnabled="True"
                                              StaysOpenOnEdit="True"
                                              Style="{StaticResource SupplierSearchComboBoxStyle}"
                                              Height="36" Padding="10,0" Background="Transparent" BorderThickness="0"/>
                                </Border>
                            </StackPanel>

                            <!-- Expected Delivery Date -->
                            <StackPanel Grid.Column="2">
                                <TextBlock Text="วันที่คาดว่าจะได้รับ" FontSize="12.5" FontWeight="SemiBold" Foreground="#475569" Margin="0,0,0,5"/>
                                <Border Background="#FFFFFF" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="6">
                                    <DatePicker SelectedDate="{Binding POExpectedDate, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                                Height="36" Padding="6,0" FontSize="13" VerticalAlignment="Center" Background="Transparent" BorderThickness="0"/>
                                </Border>
                            </StackPanel>

                            <!-- Status (Editable for manage mode, locked when creating new) -->
                            <StackPanel Grid.Column="4">
                                <TextBlock Text="สถานะใบสั่งซื้อ" FontSize="12.5" FontWeight="SemiBold" Foreground="#475569" Margin="0,0,0,5"/>
                                <Border Background="#FFFFFF" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="6">
                                    <ComboBox SelectedValue="{Binding POStatus, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                              SelectedValuePath="Content"
                                              IsEnabled="{Binding IsNewPOMode, Converter={StaticResource InverseBoolConverter}}"
                                              Style="{StaticResource LightComboBoxStyle}"
                                              Height="36" Padding="10,0" FontSize="13" FontWeight="SemiBold"
                                              Foreground="#0369A1" Background="Transparent" BorderThickness="0">''',
        '''                            <!-- Supplier Name (Searchable ComboBox) -->
                            <StackPanel Grid.Column="0">
                                <TextBlock Text="ผู้จัดจำหน่าย / ซัพพลายเออร์ *" FontSize="14.5" FontWeight="SemiBold" Foreground="#475569" Margin="0,0,0,5"/>
                                <Border Background="#FFFFFF" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="6" Height="42">
                                    <ComboBox ItemsSource="{Binding SuppliersList}"
                                              DisplayMemberPath="SupplierName"
                                              SelectedValuePath="SupplierID"
                                              TextSearch.TextPath="SupplierName"
                                              SelectedItem="{Binding SelectedSupplier, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                              SelectedValue="{Binding POSupplierId, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                              Text="{Binding POSupplierName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                              IsEditable="True"
                                              IsTextSearchEnabled="True"
                                              StaysOpenOnEdit="True"
                                              Style="{StaticResource SupplierSearchComboBoxStyle}"
                                              Height="42" Padding="10,0" FontSize="15" Background="Transparent" BorderThickness="0"/>
                                </Border>
                            </StackPanel>

                            <!-- Expected Delivery Date -->
                            <StackPanel Grid.Column="2">
                                <TextBlock Text="วันที่คาดว่าจะได้รับ" FontSize="14.5" FontWeight="SemiBold" Foreground="#475569" Margin="0,0,0,5"/>
                                <Border Background="#FFFFFF" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="6" Height="42">
                                    <DatePicker SelectedDate="{Binding POExpectedDate, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                                Height="42" Padding="6,0" FontSize="15" VerticalAlignment="Center" Background="Transparent" BorderThickness="0"/>
                                </Border>
                            </StackPanel>

                            <!-- Status (Editable for manage mode, locked when creating new) -->
                            <StackPanel Grid.Column="4">
                                <TextBlock Text="สถานะใบสั่งซื้อ" FontSize="14.5" FontWeight="SemiBold" Foreground="#475569" Margin="0,0,0,5"/>
                                <Border Background="#FFFFFF" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="6" Height="42">
                                    <ComboBox SelectedValue="{Binding POStatus, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                              SelectedValuePath="Content"
                                              IsEnabled="{Binding IsNewPOMode, Converter={StaticResource InverseBoolConverter}}"
                                              Style="{StaticResource LightComboBoxStyle}"
                                              Height="42" Padding="10,0" FontSize="15" FontWeight="SemiBold"
                                              Foreground="#0369A1" Background="Transparent" BorderThickness="0">'''
    )

    # Modal Card 2 (Quick Add Item Bar)
    content = content.replace(
        '''                            <!-- Barcode / Product Name Search (Auto-complete Search ComboBox with Card ItemTemplate) -->
                            <StackPanel Grid.Column="0">
                                <TextBlock Text="สแกนบาร์โค้ด / รหัส / ค้นหาชื่อสินค้า 🔍" FontSize="12.5" FontWeight="SemiBold" Foreground="#475569" Margin="0,0,0,5"/>
                                <Border Background="#FFFFFF" BorderBrush="#38BDF8" BorderThickness="1.5" CornerRadius="6">
                                    <ComboBox ItemsSource="{Binding Products}"
                                              SelectedItem="{Binding SelectedPOProduct, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                              Text="{Binding POItemProductName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                              IsEditable="True"
                                              IsTextSearchEnabled="True"
                                              StaysOpenOnEdit="True"
                                              GotFocus="POProductComboBox_GotFocus"
                                              PreviewKeyUp="POProductComboBox_PreviewKeyUp"
                                              Style="{StaticResource ProductSearchComboBoxStyle}"
                                              Height="36" Padding="10,0" FontSize="13" Background="Transparent" BorderThickness="0">''',
        '''                            <!-- Barcode / Product Name Search (Auto-complete Search ComboBox with Card ItemTemplate) -->
                            <StackPanel Grid.Column="0">
                                <TextBlock Text="สแกนบาร์โค้ด / รหัส / ค้นหาชื่อสินค้า 🔍" FontSize="14.5" FontWeight="SemiBold" Foreground="#475569" Margin="0,0,0,5"/>
                                <Border Background="#FFFFFF" BorderBrush="#38BDF8" BorderThickness="1.5" CornerRadius="6" Height="42">
                                    <ComboBox ItemsSource="{Binding Products}"
                                              SelectedItem="{Binding SelectedPOProduct, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                              Text="{Binding POItemProductName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                              IsEditable="True"
                                              IsTextSearchEnabled="True"
                                              StaysOpenOnEdit="True"
                                              GotFocus="POProductComboBox_GotFocus"
                                              PreviewKeyUp="POProductComboBox_PreviewKeyUp"
                                              Style="{StaticResource ProductSearchComboBoxStyle}"
                                              Height="42" Padding="10,0" FontSize="15" Background="Transparent" BorderThickness="0">'''
    )

    content = content.replace(
        '''                            <!-- Quantity -->
                            <StackPanel Grid.Column="2">
                                <TextBlock Text="จำนวน" FontSize="12.5" FontWeight="SemiBold" Foreground="#475569" Margin="0,0,0,5" HorizontalAlignment="Center"/>
                                <Border Background="#FFFFFF" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="6">
                                    <TextBox Text="{Binding POItemQuantity, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                             Style="{StaticResource LightTextBoxStyle}"
                                             Height="36" Padding="6,0" FontSize="13" HorizontalContentAlignment="Center" VerticalAlignment="Center"/>
                                </Border>
                            </StackPanel>

                            <!-- Cost Price -->
                            <StackPanel Grid.Column="4">
                                <TextBlock Text="ราคาต้นทุนต่อหน่วย" FontSize="12.5" FontWeight="SemiBold" Foreground="#475569" Margin="0,0,0,5" HorizontalAlignment="Center"/>
                                <Border Background="#FFFFFF" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="6">
                                    <TextBox Text="{Binding POItemCost, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, StringFormat={}{0:G29}}"
                                             Style="{StaticResource LightTextBoxStyle}"
                                             Height="36" Padding="6,0" FontSize="13" HorizontalContentAlignment="Center" VerticalAlignment="Center"/>
                                </Border>
                            </StackPanel>

                            <!-- Add Item Button -->
                            <StackPanel Grid.Column="6" VerticalAlignment="Bottom">
                                <Button Command="{Binding AddPOItemCommand}"
                                        Content="➕ เพิ่มรายการ"
                                        Height="36" Padding="16,0" FontSize="13" FontWeight="Bold"
                                        Foreground="White" Background="#0284C7" BorderThickness="0" Cursor="Hand">''',
        '''                            <!-- Quantity -->
                            <StackPanel Grid.Column="2">
                                <TextBlock Text="จำนวน" FontSize="14.5" FontWeight="SemiBold" Foreground="#475569" Margin="0,0,0,5" HorizontalAlignment="Center"/>
                                <Border Background="#FFFFFF" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="6" Height="42">
                                    <TextBox Text="{Binding POItemQuantity, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                             Style="{StaticResource LightTextBoxStyle}"
                                             Height="42" Padding="6,0" FontSize="16" HorizontalContentAlignment="Center" VerticalAlignment="Center"/>
                                </Border>
                            </StackPanel>

                            <!-- Cost Price -->
                            <StackPanel Grid.Column="4">
                                <TextBlock Text="ราคาต้นทุนต่อหน่วย" FontSize="14.5" FontWeight="SemiBold" Foreground="#475569" Margin="0,0,0,5" HorizontalAlignment="Center"/>
                                <Border Background="#FFFFFF" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="6" Height="42">
                                    <TextBox Text="{Binding POItemCost, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, StringFormat={}{0:G29}}"
                                             Style="{StaticResource LightTextBoxStyle}"
                                             Height="42" Padding="6,0" FontSize="16" HorizontalContentAlignment="Center" VerticalAlignment="Center"/>
                                </Border>
                            </StackPanel>

                            <!-- Add Item Button -->
                            <StackPanel Grid.Column="6" VerticalAlignment="Bottom">
                                <Button Command="{Binding AddPOItemCommand}"
                                        Content="➕ เพิ่มรายการ"
                                        Height="42" Padding="18,0" FontSize="15" FontWeight="Bold"
                                        Foreground="White" Background="#0284C7" BorderThickness="0" Cursor="Hand">'''
    )

    # Modal Items DataGrid RowHeight
    content = content.replace(
        '''                    <!-- Card 3: Items Table Container (ตารางรายการสินค้าที่เพิ่มแล้ว) -->
                    <Border Background="#FFFFFF" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="12" Padding="4" MinHeight="170" MaxHeight="230" Margin="0,0,0,16">
                        <DataGrid ItemsSource="{Binding CurrentPOItems}"
                                  AutoGenerateColumns="False"
                                  IsReadOnly="False"
                                  CanUserAddRows="False"
                                  HeadersVisibility="Column"
                                  RowHeight="40"''',
        '''                    <!-- Card 3: Items Table Container (ตารางรายการสินค้าที่เพิ่มแล้ว) -->
                    <Border Background="#FFFFFF" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="12" Padding="4" MinHeight="180" MaxHeight="240" Margin="0,0,0,16">
                        <DataGrid ItemsSource="{Binding CurrentPOItems}"
                                  AutoGenerateColumns="False"
                                  IsReadOnly="False"
                                  CanUserAddRows="False"
                                  HeadersVisibility="Column"
                                  RowHeight="46"'''
    )

    # Validation message
    content = content.replace(
        '''                    <TextBlock Text="{Binding ProductValidationMessage}"
                               Visibility="{Binding HasProductValidationMessage, Converter={StaticResource BoolToVis}}"
                               Foreground="#DC2626" FontSize="13" FontWeight="SemiBold" Margin="0,0,0,10"/>''',
        '''                    <TextBlock Text="{Binding ProductValidationMessage}"
                               Visibility="{Binding HasProductValidationMessage, Converter={StaticResource BoolToVis}}"
                               Foreground="#DC2626" FontSize="14" FontWeight="SemiBold" Margin="0,0,0,10"/>'''
    )

    # Modal Footer
    content = content.replace(
        '''                    <!-- Footer Bar -->
                    <DockPanel LastChildFill="False" Margin="0,4,0,0">
                        <!-- Total Amount -->
                        <StackPanel Orientation="Horizontal" DockPanel.Dock="Left" VerticalAlignment="Center">
                            <TextBlock Text="ยอดรวมค่าสั่งซื้อสุทธิ: " FontSize="16" FontWeight="Bold" Foreground="#0F172A"/>
                            <TextBlock Text="{Binding POTotalAmount, StringFormat={}{0:N2}}" FontSize="20" FontWeight="Bold" Foreground="#0F172A"/>
                        </StackPanel>

                        <!-- Footer Action Buttons (Cancel & Save) -->
                        <StackPanel Orientation="Horizontal" DockPanel.Dock="Right" VerticalAlignment="Center">
                            <!-- Cancel Button (Light Blue) -->
                            <Button Command="{Binding ClosePOModalCommand}"
                                    Content="ยกเลิก"
                                    Height="40" Padding="20,0" FontSize="14" FontWeight="Bold"
                                    Foreground="#0284C7" Background="#F0F9FF" BorderBrush="#7DD3FC" BorderThickness="1.5"
                                    Cursor="Hand" Margin="0,0,10,0">
                                <Button.Template>
                                    <ControlTemplate TargetType="Button">
                                        <Border x:Name="bd" Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}" BorderThickness="{TemplateBinding BorderThickness}" CornerRadius="10" Padding="{TemplateBinding Padding}">
                                            <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                        </Border>
                                        <ControlTemplate.Triggers>
                                            <Trigger Property="IsMouseOver" Value="True">
                                                <Setter TargetName="bd" Property="Background" Value="#BAE6FD"/>
                                            </Trigger>
                                        </ControlTemplate.Triggers>
                                    </ControlTemplate>
                                </Button.Template>
                            </Button>

                            <!-- Save Button (Vibrant Blue with Save Icon) -->
                            <Button Command="{Binding SavePOCommand}"
                                    Height="40" Padding="20,0" FontSize="14" FontWeight="Bold"
                                    Foreground="White" Background="#00A3E0" BorderThickness="0" Cursor="Hand">
                                <Button.Template>
                                    <ControlTemplate TargetType="Button">
                                        <Border x:Name="bd" Background="#00A3E0" CornerRadius="10" Padding="{TemplateBinding Padding}">
                                            <StackPanel Orientation="Horizontal" HorizontalAlignment="Center" VerticalAlignment="Center">
                                                <TextBlock Text="💾 " FontSize="14" VerticalAlignment="Center"/>
                                                <TextBlock Text="บันทึกใบสั่งซื้อ" FontSize="14" FontWeight="Bold" VerticalAlignment="Center"/>
                                            </StackPanel>
                                        </Border>''',
        '''                    <!-- Footer Bar -->
                    <DockPanel LastChildFill="False" Margin="0,4,0,0">
                        <!-- Total Amount -->
                        <StackPanel Orientation="Horizontal" DockPanel.Dock="Left" VerticalAlignment="Center">
                            <TextBlock Text="ยอดรวมค่าสั่งซื้อสุทธิ: " FontSize="17" FontWeight="Bold" Foreground="#0F172A"/>
                            <TextBlock Text="{Binding POTotalAmount, StringFormat={}{0:N2}}" FontSize="26" FontWeight="Bold" Foreground="#0F172A"/>
                        </StackPanel>

                        <!-- Footer Action Buttons (Cancel & Save) -->
                        <StackPanel Orientation="Horizontal" DockPanel.Dock="Right" VerticalAlignment="Center">
                            <!-- Cancel Button (Light Blue) -->
                            <Button Command="{Binding ClosePOModalCommand}"
                                    Content="ยกเลิก"
                                    Height="46" Padding="22,0" FontSize="16" FontWeight="Bold"
                                    Foreground="#0284C7" Background="#F0F9FF" BorderBrush="#7DD3FC" BorderThickness="1.5"
                                    Cursor="Hand" Margin="0,0,10,0">
                                <Button.Template>
                                    <ControlTemplate TargetType="Button">
                                        <Border x:Name="bd" Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}" BorderThickness="{TemplateBinding BorderThickness}" CornerRadius="10" Padding="{TemplateBinding Padding}">
                                            <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                        </Border>
                                        <ControlTemplate.Triggers>
                                            <Trigger Property="IsMouseOver" Value="True">
                                                <Setter TargetName="bd" Property="Background" Value="#BAE6FD"/>
                                            </Trigger>
                                        </ControlTemplate.Triggers>
                                    </ControlTemplate>
                                </Button.Template>
                            </Button>

                            <!-- Save Button (Vibrant Blue with Save Icon) -->
                            <Button Command="{Binding SavePOCommand}"
                                    Height="46" Padding="22,0" FontSize="16" FontWeight="Bold"
                                    Foreground="White" Background="#00A3E0" BorderThickness="0" Cursor="Hand">
                                <Button.Template>
                                    <ControlTemplate TargetType="Button">
                                        <Border x:Name="bd" Background="#00A3E0" CornerRadius="10" Padding="{TemplateBinding Padding}">
                                            <StackPanel Orientation="Horizontal" HorizontalAlignment="Center" VerticalAlignment="Center">
                                                <TextBlock Text="💾 " FontSize="16" VerticalAlignment="Center"/>
                                                <TextBlock Text="บันทึกใบสั่งซื้อ" FontSize="16" FontWeight="Bold" VerticalAlignment="Center"/>
                                            </StackPanel>
                                        </Border>'''
    )

    with open(file_path, "w", encoding="utf-8") as f:
        f.write(content)

    print("Successfully updated PurchaseOrderView.xaml")

if __name__ == "__main__":
    update_purchase_order()
