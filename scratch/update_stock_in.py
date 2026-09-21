import sys
import re

def update_stock_in():
    file_path = r"d:\ShopManagement_หลัก\MatchaShop\Views\StockInView.xaml"
    with open(file_path, "r", encoding="utf-8") as f:
        content = f.read()

    # 1. Styles
    # RoseBtnStyle
    content = re.sub(
        r'(<Style x:Key="RoseBtnStyle"[^>]*>[\s\S]*?<Setter Property="FontSize"\s+Value=")14("[\s\S]*?<Setter Property="Height"\s+Value=")40(")',
        r'\g<1>16\g<2>44\3',
        content
    )
    # CancelBtnStyle
    content = re.sub(
        r'(<Style x:Key="CancelBtnStyle"[^>]*>[\s\S]*?<Setter Property="FontSize"\s+Value=")14("[\s\S]*?<Setter Property="Height"\s+Value=")40(")',
        r'\g<1>16\g<2>44\3',
        content
    )
    # ViewDetailsButtonStyle
    content = re.sub(
        r'(<Style x:Key="ViewDetailsButtonStyle"[^>]*>[\s\S]*?<Setter Property="FontSize"\s+Value=")12("[\s\S]*?<Setter Property="Height"\s+Value=")28(")',
        r'\g<1>14\g<2>34\3',
        content
    )
    # StockInColumnHeaderStyle
    content = re.sub(
        r'(<Style x:Key="StockInColumnHeaderStyle"[^>]*>[\s\S]*?<Setter Property="Height"\s+Value=")46(")',
        r'\g<1>48\2',
        content
    )
    # StockInRowStyle
    content = re.sub(
        r'(<Style x:Key="StockInRowStyle"[^>]*>[\s\S]*?<Setter Property="FontSize"\s+Value=")13(")',
        r'\g<1>15\2',
        content
    )

    # Column Alignment Helpers: ColCenter, ColCenterBold, ColLeftBold, ColRightBold
    content = content.replace(
        '''        <Style x:Key="ColCenter" TargetType="TextBlock">
            <Setter Property="HorizontalAlignment" Value="Center"/>
            <Setter Property="VerticalAlignment"   Value="Center"/>
            <Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="Foreground"          Value="#000000"/>
        </Style>''',
        '''        <Style x:Key="ColCenter" TargetType="TextBlock">
            <Setter Property="HorizontalAlignment" Value="Center"/>
            <Setter Property="VerticalAlignment"   Value="Center"/>
            <Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="FontSize"            Value="15"/>
            <Setter Property="Foreground"          Value="#000000"/>
        </Style>'''
    )
    content = content.replace(
        '''        <Style x:Key="ColLeftBold" TargetType="TextBlock">
            <Setter Property="HorizontalAlignment" Value="Left"/>
            <Setter Property="VerticalAlignment"   Value="Center"/>
            <Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="FontWeight"          Value="Bold"/>
            <Setter Property="Foreground"          Value="#000000"/>
        </Style>''',
        '''        <Style x:Key="ColLeftBold" TargetType="TextBlock">
            <Setter Property="HorizontalAlignment" Value="Left"/>
            <Setter Property="VerticalAlignment"   Value="Center"/>
            <Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="FontWeight"          Value="Bold"/>
            <Setter Property="FontSize"            Value="15"/>
            <Setter Property="Foreground"          Value="#000000"/>
        </Style>'''
    )
    content = content.replace(
        '''        <Style x:Key="ColRightBold" TargetType="TextBlock">
            <Setter Property="HorizontalAlignment" Value="Right"/>
            <Setter Property="VerticalAlignment"   Value="Center"/>
            <Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="FontWeight"          Value="Bold"/>
            <Setter Property="Foreground"          Value="#000000"/>
        </Style>''',
        '''        <Style x:Key="ColRightBold" TargetType="TextBlock">
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
        '''                        <TextBlock Text="บันทึกรับสินค้าเข้าคลัง — ระบบจัดการร้าน พอใจ"
                                   FontSize="13"
                                   Foreground="#718096"
                                   Margin="0"/>''',
        '''                        <TextBlock Text="บันทึกรับสินค้าเข้าคลัง — ระบบจัดการร้าน พอใจ"
                                   FontSize="15"
                                   Foreground="#718096"
                                   Margin="0"/>'''
    )

    # 3. KPI Cards
    # Card 1
    content = content.replace(
        '''                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="36" Height="36" CornerRadius="10" Background="#E0F2FE" Margin="0,0,10,0">
                                <TextBlock Text="📥" FontSize="18" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="รายการรับเข้าทั้งหมด" FontSize="13" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <TextBlock Grid.Row="1"
                                   Text="{Binding TotalStockInEntriesCount}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#0B2545"
                                   Margin="0,0,0,6"/>
                        <TextBlock Grid.Row="2"
                                   Text="จำนวนครั้งที่บันทึกการรับสินค้าเข้า"
                                   FontSize="11"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>''',
        '''                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="40" Height="40" CornerRadius="10" Background="#E0F2FE" Margin="0,0,10,0">
                                <TextBlock Text="📥" FontSize="20" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="รายการรับเข้าทั้งหมด" FontSize="16" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <TextBlock Grid.Row="1"
                                   Text="{Binding TotalStockInEntriesCount}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#0B2545"
                                   Margin="0,0,0,6"/>
                        <TextBlock Grid.Row="2"
                                   Text="จำนวนครั้งที่บันทึกการรับสินค้าเข้า"
                                   FontSize="14"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>'''
    )

    # Card 2
    content = content.replace(
        '''                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="36" Height="36" CornerRadius="10" Background="#D1FAE5" Margin="0,0,10,0">
                                <TextBlock Text="📦" FontSize="18" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="จำนวนชิ้นที่รับเข้ารวม" FontSize="13" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <TextBlock Grid.Row="1"
                                   Text="{Binding TotalStockInQtyCount, StringFormat='{}{0:N0} ชิ้น'}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#059669"
                                   Margin="0,0,0,6"/>
                        <TextBlock Grid.Row="2"
                                   Text="รวมจำนวนชิ้นสินค้าที่เพิ่มเข้าสต็อก"
                                   FontSize="11"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>''',
        '''                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="40" Height="40" CornerRadius="10" Background="#D1FAE5" Margin="0,0,10,0">
                                <TextBlock Text="📦" FontSize="20" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="จำนวนชิ้นที่รับเข้ารวม" FontSize="16" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <TextBlock Grid.Row="1"
                                   Text="{Binding TotalStockInQtyCount, StringFormat='{}{0:N0} ชิ้น'}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#059669"
                                   Margin="0,0,0,6"/>
                        <TextBlock Grid.Row="2"
                                   Text="รวมจำนวนชิ้นสินค้าที่เพิ่มเข้าสต็อก"
                                   FontSize="14"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>'''
    )

    # Card 3
    content = content.replace(
        '''                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="36" Height="36" CornerRadius="10" Background="#F3E8FF" Margin="0,0,10,0">
                                <TextBlock Text="💰" FontSize="18" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="มูลค่าทุนรับเข้ารวม" FontSize="13" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <TextBlock Grid.Row="1"
                                   Text="{Binding TotalStockInCostSum, StringFormat={}{0:N2}}"
                                   FontSize="24" FontWeight="Bold"
                                   Foreground="#6B21A8"
                                   Margin="0,0,0,6"/>
                        <TextBlock Grid.Row="2"
                                   Text="รวมมูลค่าเงินทุนที่ใช้ในการรับสินค้าเข้า"
                                   FontSize="11"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>''',
        '''                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="40" Height="40" CornerRadius="10" Background="#F3E8FF" Margin="0,0,10,0">
                                <TextBlock Text="💰" FontSize="20" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="มูลค่าทุนรับเข้ารวม" FontSize="16" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <TextBlock Grid.Row="1"
                                   Text="{Binding TotalStockInCostSum, StringFormat={}{0:N2}}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#6B21A8"
                                   Margin="0,0,0,6"/>
                        <TextBlock Grid.Row="2"
                                   Text="รวมมูลค่าเงินทุนที่ใช้ในการรับสินค้าเข้า"
                                   FontSize="14"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>'''
    )

    # 4. Filter / Search Bar
    content = content.replace(
        '''                            <!-- Search Icon -->
                            <Path Data="{StaticResource IconSearch}"
                                  Fill="#718096"
                                  Width="18" Height="18"
                                  HorizontalAlignment="Left"
                                  Margin="12,0,0,0"
                                  Stretch="Uniform"
                                  VerticalAlignment="Center"/>
                            <!-- TextBox -->
                            <TextBox x:Name="StockInSearchBox"
                                     Text="{Binding PendingPOSearchKeyword, UpdateSourceTrigger=PropertyChanged}"
                                     Style="{StaticResource LightTextBoxStyle}"
                                     Height="40"
                                     Padding="38,0,12,0"
                                     VerticalContentAlignment="Center"
                                     FontSize="13"/>
                            <!-- Placeholder -->
                            <TextBlock Text="ค้นหาตามเลขที่ใบสั่งซื้อ หรือ ชื่อซัพพลายเออร์..."
                                       Foreground="#A0AEC0"
                                       FontSize="13"
                                       VerticalAlignment="Center"
                                       Margin="38,0,12,0"
                                       IsHitTestVisible="False">''',
        '''                            <!-- Search Icon -->
                            <Path Data="{StaticResource IconSearch}"
                                  Fill="#718096"
                                  Width="20" Height="20"
                                  HorizontalAlignment="Left"
                                  Margin="14,0,0,0"
                                  Stretch="Uniform"
                                  VerticalAlignment="Center"/>
                            <!-- TextBox -->
                            <TextBox x:Name="StockInSearchBox"
                                     Text="{Binding PendingPOSearchKeyword, UpdateSourceTrigger=PropertyChanged}"
                                     Style="{StaticResource LightTextBoxStyle}"
                                     Height="44"
                                     Padding="42,0,12,0"
                                     VerticalContentAlignment="Center"
                                     FontSize="16"/>
                            <!-- Placeholder -->
                            <TextBlock Text="ค้นหาตามเลขที่ใบสั่งซื้อ หรือ ชื่อซัพพลายเออร์..."
                                       Foreground="#A0AEC0"
                                       FontSize="15"
                                       VerticalAlignment="Center"
                                       Margin="42,0,12,0"
                                       IsHitTestVisible="False">'''
    )

    content = content.replace(
        '''                    <!-- Search Button -->
                    <Button Grid.Column="2"
                            Content="ค้นหา"
                            Command="{Binding SearchPendingPOCommand}"
                            Background="#1E88E5"
                            Cursor="Hand"
                            Padding="22,0"
                            Height="40">
                        <Button.Style>
                            <Style TargetType="Button">
                                <Setter Property="Template">
                                    <Setter.Value>
                                        <ControlTemplate TargetType="Button">
                                            <Border x:Name="Bd"
                                                    Background="{TemplateBinding Background}"
                                                    CornerRadius="8"
                                                    Padding="{TemplateBinding Padding}">
                                                <TextBlock Text="ค้นหา"
                                                           Foreground="#FFFFFF"
                                                           FontWeight="Bold"
                                                           FontSize="14"
                                                           HorizontalAlignment="Center"
                                                           VerticalAlignment="Center"/>
                                            </Border>
                                            <ControlTemplate.Triggers>
                                                <Trigger Property="IsMouseOver" Value="True">
                                                    <Setter TargetName="Bd" Property="Background" Value="#42A5F5"/>
                                                </Trigger>
                                                <Trigger Property="IsPressed" Value="True">
                                                    <Setter TargetName="Bd" Property="Background" Value="#1565C0"/>
                                                </Trigger>
                                            </ControlTemplate.Triggers>
                                        </ControlTemplate>
                                    </Setter.Value>
                                </Setter>
                            </Style>
                        </Button.Style>
                        <Button.Effect>
                            <DropShadowEffect Color="#1E88E5" BlurRadius="10" ShadowDepth="2" Direction="270" Opacity="0.35"/>
                        </Button.Effect>
                    </Button>

                    <!-- Action Button: Open Goods Receipt Modal / View Details -->
                    <Button Grid.Column="4"
                            Content="{Binding SelectedPOReceiveButtonText}"
                            Command="{Binding OpenGoodsReceiptModalCommand}"
                            CommandParameter="{Binding SelectedReceiptPO}"
                            IsEnabled="{Binding HasSelectedReceiptPO}"
                            Foreground="White"
                            Padding="20,0"
                            Height="40">
                        <Button.Style>
                            <Style TargetType="Button">
                                <Setter Property="Background" Value="#0284C7"/>
                                <Setter Property="BorderBrush" Value="#0284C7"/>
                                <Setter Property="BorderThickness" Value="1"/>
                                <Setter Property="Cursor" Value="Hand"/>
                                <Setter Property="FontWeight" Value="Bold"/>
                                <Setter Property="FontSize" Value="13.5"/>''',
        '''                    <!-- Search Button -->
                    <Button Grid.Column="2"
                            Content="ค้นหา"
                            Command="{Binding SearchPendingPOCommand}"
                            Background="#1E88E5"
                            Cursor="Hand"
                            Padding="24,0"
                            Height="44">
                        <Button.Style>
                            <Style TargetType="Button">
                                <Setter Property="Template">
                                    <Setter.Value>
                                        <ControlTemplate TargetType="Button">
                                            <Border x:Name="Bd"
                                                    Background="{TemplateBinding Background}"
                                                    CornerRadius="8"
                                                    Padding="{TemplateBinding Padding}">
                                                <TextBlock Text="ค้นหา"
                                                           Foreground="#FFFFFF"
                                                           FontWeight="Bold"
                                                           FontSize="16"
                                                           HorizontalAlignment="Center"
                                                           VerticalAlignment="Center"/>
                                            </Border>
                                            <ControlTemplate.Triggers>
                                                <Trigger Property="IsMouseOver" Value="True">
                                                    <Setter TargetName="Bd" Property="Background" Value="#42A5F5"/>
                                                </Trigger>
                                                <Trigger Property="IsPressed" Value="True">
                                                    <Setter TargetName="Bd" Property="Background" Value="#1565C0"/>
                                                </Trigger>
                                            </ControlTemplate.Triggers>
                                        </ControlTemplate>
                                    </Setter.Value>
                                </Setter>
                            </Style>
                        </Button.Style>
                        <Button.Effect>
                            <DropShadowEffect Color="#1E88E5" BlurRadius="10" ShadowDepth="2" Direction="270" Opacity="0.35"/>
                        </Button.Effect>
                    </Button>

                    <!-- Action Button: Open Goods Receipt Modal / View Details -->
                    <Button Grid.Column="4"
                            Content="{Binding SelectedPOReceiveButtonText}"
                            Command="{Binding OpenGoodsReceiptModalCommand}"
                            CommandParameter="{Binding SelectedReceiptPO}"
                            IsEnabled="{Binding HasSelectedReceiptPO}"
                            Foreground="White"
                            Padding="22,0"
                            Height="44">
                        <Button.Style>
                            <Style TargetType="Button">
                                <Setter Property="Background" Value="#0284C7"/>
                                <Setter Property="BorderBrush" Value="#0284C7"/>
                                <Setter Property="BorderThickness" Value="1"/>
                                <Setter Property="Cursor" Value="Hand"/>
                                <Setter Property="FontWeight" Value="Bold"/>
                                <Setter Property="FontSize" Value="16"/>'''
    )

    # 5. Main DataGrid
    content = content.replace(
        '''                                        <TextBlock Text="{Binding TotalAmount, StringFormat={}{0:N2}}"
                                                   FontSize="14" FontWeight="Bold"
                                                   Foreground="#000000"
                                                   HorizontalAlignment="Right" VerticalAlignment="Center"
                                                   Margin="0,0,12,0"/>''',
        '''                                        <TextBlock Text="{Binding TotalAmount, StringFormat={}{0:N2}}"
                                                   FontSize="16" FontWeight="Bold"
                                                   Foreground="#000000"
                                                   HorizontalAlignment="Right" VerticalAlignment="Center"
                                                   Margin="0,0,12,0"/>'''
    )
    content = content.replace(
        '''                                            <TextBlock FontSize="12.5" FontWeight="Bold" HorizontalAlignment="Center" VerticalAlignment="Center">''',
        '''                                            <TextBlock FontSize="14" FontWeight="Bold" HorizontalAlignment="Center" VerticalAlignment="Center">'''
    )
    content = content.replace(
        '''                        <TextBlock Text="{Binding StockInSummaryText}"
                                   Foreground="#475569"
                                   FontSize="13"
                                   FontWeight="SemiBold"
                                   VerticalAlignment="Center"
                                   DockPanel.Dock="Left"
                                   Margin="0"/>''',
        '''                        <TextBlock Text="{Binding StockInSummaryText}"
                                   Foreground="#475569"
                                   FontSize="14"
                                   FontWeight="SemiBold"
                                   VerticalAlignment="Center"
                                   DockPanel.Dock="Left"
                                   Margin="0"/>'''
    )

    # 6. Goods Receipt Modal Overlay
    content = content.replace(
        '''                                <TextBlock Text="ตรวจรับสินค้าเข้าคลัง (Goods Receipt)"
                                           FontSize="18" FontWeight="Bold"
                                           Foreground="#2D3748"
                                           Margin="0"/>
                                <TextBlock Text="ตรวจสอบรายการสินค้าที่ได้รับจริง และกรอกเลขที่ใบส่งของ/ใบกำกับภาษี"
                                           FontSize="12.5"
                                           Foreground="#718096"
                                           Margin="0,2,0,0"/>''',
        '''                                <TextBlock Text="ตรวจรับสินค้าเข้าคลัง (Goods Receipt)"
                                           FontSize="20" FontWeight="Bold"
                                           Foreground="#2D3748"
                                           Margin="0"/>
                                <TextBlock Text="ตรวจสอบรายการสินค้าที่ได้รับจริง และกรอกเลขที่ใบส่งของ/ใบกำกับภาษี"
                                           FontSize="14"
                                           Foreground="#718096"
                                           Margin="0,2,0,0"/>'''
    )

    # PO Info Banner
    content = content.replace(
        '''                                <!-- Column 0: PO Number -->
                                <StackPanel Grid.Column="0" VerticalAlignment="Center" Margin="0,0,16,0">
                                    <TextBlock Text="เลขที่ใบสั่งซื้อ" FontSize="11.5" Foreground="#718096" FontWeight="SemiBold"/>
                                    <TextBlock Text="{Binding SelectedReceiptPO.PONumber}" FontSize="15" FontWeight="Bold" Foreground="#0B2545" Margin="0,2,0,0"/>
                                </StackPanel>

                                <!-- Column 1: Supplier Name (Expands in middle, trimmed with ellipsis if very long) -->
                                <StackPanel Grid.Column="1" VerticalAlignment="Center" Margin="0,0,24,0">
                                    <TextBlock Text="ผู้จัดจำหน่าย" FontSize="11.5" Foreground="#718096" FontWeight="SemiBold"/>
                                    <TextBlock Text="{Binding SelectedReceiptPO.SupplierName}" 
                                               FontSize="14" FontWeight="Bold" Foreground="#2D3748" 
                                               TextTrimming="CharacterEllipsis"
                                               ToolTip="{Binding SelectedReceiptPO.SupplierName}"
                                               Margin="0,2,0,0"/>
                                </StackPanel>

                                <!-- Column 2: Order Date (Right-aligned to right border edge) -->
                                <StackPanel Grid.Column="2" VerticalAlignment="Center" HorizontalAlignment="Right">
                                    <TextBlock Text="วันที่สั่งซื้อ" FontSize="11.5" Foreground="#718096" FontWeight="SemiBold" TextAlignment="Right" HorizontalAlignment="Right"/>
                                    <TextBlock Text="{Binding SelectedReceiptPO.OrderDate, StringFormat=dd/MM/yyyy}" FontSize="14" FontWeight="SemiBold" Foreground="#2D3748" TextAlignment="Right" HorizontalAlignment="Right" Margin="0,2,0,0"/>
                                </StackPanel>''',
        '''                                <!-- Column 0: PO Number -->
                                <StackPanel Grid.Column="0" VerticalAlignment="Center" Margin="0,0,16,0">
                                    <TextBlock Text="เลขที่ใบสั่งซื้อ" FontSize="13.5" Foreground="#718096" FontWeight="SemiBold"/>
                                    <TextBlock Text="{Binding SelectedReceiptPO.PONumber}" FontSize="16" FontWeight="Bold" Foreground="#0B2545" Margin="0,2,0,0"/>
                                </StackPanel>

                                <!-- Column 1: Supplier Name (Expands in middle, trimmed with ellipsis if very long) -->
                                <StackPanel Grid.Column="1" VerticalAlignment="Center" Margin="0,0,24,0">
                                    <TextBlock Text="ผู้จัดจำหน่าย" FontSize="13.5" Foreground="#718096" FontWeight="SemiBold"/>
                                    <TextBlock Text="{Binding SelectedReceiptPO.SupplierName}" 
                                               FontSize="15" FontWeight="Bold" Foreground="#2D3748" 
                                               TextTrimming="CharacterEllipsis"
                                               ToolTip="{Binding SelectedReceiptPO.SupplierName}"
                                               Margin="0,2,0,0"/>
                                </StackPanel>

                                <!-- Column 2: Order Date (Right-aligned to right border edge) -->
                                <StackPanel Grid.Column="2" VerticalAlignment="Center" HorizontalAlignment="Right">
                                    <TextBlock Text="วันที่สั่งซื้อ" FontSize="13.5" Foreground="#718096" FontWeight="SemiBold" TextAlignment="Right" HorizontalAlignment="Right"/>
                                    <TextBlock Text="{Binding SelectedReceiptPO.OrderDate, StringFormat=dd/MM/yyyy}" FontSize="15" FontWeight="SemiBold" Foreground="#2D3748" TextAlignment="Right" HorizontalAlignment="Right" Margin="0,2,0,0"/>
                                </StackPanel>'''
    )

    # Notice banner
    content = content.replace(
        '''                                    <TextBlock Text="รายการใบสั่งซื้อนี้ได้ทำการตรวจรับสินค้าเข้าคลังเรียบร้อยแล้ว" FontWeight="Bold" FontSize="13" Foreground="#15803D"/>
                                    <TextBlock Text="อยู่ในโหมดดูรายละเอียด (Read-Only) เพื่อป้องกันการตรวจรับซ้ำซ้อน" FontSize="11.5" Foreground="#166534"/>''',
        '''                                    <TextBlock Text="รายการใบสั่งซื้อนี้ได้ทำการตรวจรับสินค้าเข้าคลังเรียบร้อยแล้ว" FontWeight="Bold" FontSize="14" Foreground="#15803D"/>
                                    <TextBlock Text="อยู่ในโหมดดูรายละเอียด (Read-Only) เพื่อป้องกันการตรวจรับซ้ำซ้อน" FontSize="12.5" Foreground="#166534"/>'''
    )

    # Delivery Note Input Field
    content = content.replace(
        '''                            <!-- Left label and format guide -->
                            <StackPanel Grid.Column="0" VerticalAlignment="Center">
                                <TextBlock Text="เลขที่ใบส่งของ / ใบกำกับภาษี *" FontSize="13" FontWeight="Bold" Foreground="#0B2545"/>
                                <TextBlock Text="รูปแบบ: RC-XXXXX (ตัวอย่าง: RC-00001)" FontSize="11" Foreground="#64748B" Margin="0,3,0,0"/>
                            </StackPanel>''',
        '''                            <!-- Left label and format guide -->
                            <StackPanel Grid.Column="0" VerticalAlignment="Center">
                                <TextBlock Text="เลขที่ใบส่งของ / ใบกำกับภาษี *" FontSize="14.5" FontWeight="Bold" Foreground="#0B2545"/>
                                <TextBlock Text="รูปแบบ: RC-XXXXX (ตัวอย่าง: RC-00001)" FontSize="12" Foreground="#64748B" Margin="0,3,0,0"/>
                            </StackPanel>'''
    )

    content = content.replace(
        '''                                        <TextBox x:Name="txtInvoiceNo"
                                                 Text="{Binding ReceiptDeliveryNoteNo, UpdateSourceTrigger=PropertyChanged}" 
                                                 IsEnabled="{Binding CanEditReceiptModal}"
                                                 Style="{StaticResource LightTextBoxStyle}"
                                                 FontWeight="SemiBold"
                                                 Padding="10,6" Height="36" FontSize="13.5" VerticalContentAlignment="Center"/>
                                        <TextBlock Text="เช่น RC-00001 (บังคับรูปแบบ RC-XXXXX)"
                                                   Foreground="#94A3B8"
                                                   FontSize="12.5"
                                                   VerticalAlignment="Center"
                                                   Margin="12,0,10,0"
                                                   IsHitTestVisible="False">''',
        '''                                        <TextBox x:Name="txtInvoiceNo"
                                                 Text="{Binding ReceiptDeliveryNoteNo, UpdateSourceTrigger=PropertyChanged}" 
                                                 IsEnabled="{Binding CanEditReceiptModal}"
                                                 Style="{StaticResource LightTextBoxStyle}"
                                                 FontWeight="SemiBold"
                                                 Padding="10,6" Height="42" FontSize="16" VerticalContentAlignment="Center"/>
                                        <TextBlock Text="เช่น RC-00001 (บังคับรูปแบบ RC-XXXXX)"
                                                   Foreground="#94A3B8"
                                                   FontSize="14"
                                                   VerticalAlignment="Center"
                                                   Margin="12,0,10,0"
                                                   IsHitTestVisible="False">'''
    )

    # Receipt Items DataGrid RowHeight & Fonts
    content = content.replace(
        '''                        <DataGrid ItemsSource="{Binding ReceiptItems}"
                                  AutoGenerateColumns="False"
                                  IsReadOnly="False"
                                  CanUserAddRows="False"
                                  HeadersVisibility="Column"
                                  RowHeight="46"''',
        '''                        <DataGrid ItemsSource="{Binding ReceiptItems}"
                                  AutoGenerateColumns="False"
                                  IsReadOnly="False"
                                  CanUserAddRows="False"
                                  HeadersVisibility="Column"
                                  RowHeight="48"'''
    )
    content = content.replace(
        '''                                                    <TextBox Text="{Binding ReceivedQty, UpdateSourceTrigger=PropertyChanged, Mode=TwoWay}" IsEnabled="{Binding DataContext.CanEditReceiptModal, RelativeSource={RelativeSource AncestorType=UserControl}}"
                                                             Style="{StaticResource LightTextBoxStyle}"
                                                             FontWeight="Bold" FontSize="14" Foreground="#0B2545"
                                                             TextAlignment="Center" VerticalContentAlignment="Center"/>''',
        '''                                                    <TextBox Text="{Binding ReceivedQty, UpdateSourceTrigger=PropertyChanged, Mode=TwoWay}" IsEnabled="{Binding DataContext.CanEditReceiptModal, RelativeSource={RelativeSource AncestorType=UserControl}}"
                                                             Style="{StaticResource LightTextBoxStyle}"
                                                             FontWeight="Bold" FontSize="16" Foreground="#0B2545"
                                                             TextAlignment="Center" VerticalContentAlignment="Center"/>'''
    )
    content = content.replace(
        '''                                                <TextBlock Text="{Binding ReceivedSubtotal, StringFormat='{}{0:N2}'}"
                                                           FontSize="14" FontWeight="Bold" Foreground="#059669"
                                                           HorizontalAlignment="Right" VerticalAlignment="Center"/>''',
        '''                                                <TextBlock Text="{Binding ReceivedSubtotal, StringFormat='{}{0:N2}'}"
                                                           FontSize="16" FontWeight="Bold" Foreground="#059669"
                                                           HorizontalAlignment="Right" VerticalAlignment="Center"/>'''
    )

    # Totals & Actions Bar
    content = content.replace(
        '''                        <StackPanel Grid.Column="0" Orientation="Horizontal" VerticalAlignment="Center">
                            <TextBlock Text="ยอดรวมทุนการรับเข้าสุทธิ: " FontSize="15" Foreground="#718096" VerticalAlignment="Center"/>
                            <TextBlock Text="{Binding ReceiptTotalAmount, StringFormat={}{0:N2}}" FontSize="20" FontWeight="Bold" Foreground="#059669" VerticalAlignment="Center"/>
                        </StackPanel>

                        <Button Grid.Column="2"
                                Command="{Binding CloseGoodsReceiptModalCommand}"
                                Height="42">
                            <Button.Style>
                                <Style TargetType="Button" BasedOn="{StaticResource CancelBtnStyle}">
                                    <Setter Property="Content" Value="ยกเลิก"/>
                                    <Style.Triggers>
                                        <DataTrigger Binding="{Binding IsReceiptViewOnly}" Value="True">
                                            <Setter Property="Content" Value="ปิดหน้าต่าง"/>
                                        </DataTrigger>
                                    </Style.Triggers>
                                </Style>
                            </Button.Style>
                        </Button>

                        <Button Grid.Column="4"
                                x:Name="btnConfirmReceipt"
                                Content="📥 ยืนยันการรับสินค้าเข้าคลัง"
                                Command="{Binding ConfirmGoodsReceiptCommand}" 
                                IsEnabled="{Binding CanConfirmGoodsReceipt}"
                                Visibility="{Binding IsReceiptViewOnly, Converter={StaticResource InverseBoolToVis}}"
                                Style="{StaticResource RoseBtnStyle}"
                                Height="42"/>''',
        '''                        <StackPanel Grid.Column="0" Orientation="Horizontal" VerticalAlignment="Center">
                            <TextBlock Text="ยอดรวมทุนการรับเข้าสุทธิ: " FontSize="16" Foreground="#718096" VerticalAlignment="Center"/>
                            <TextBlock Text="{Binding ReceiptTotalAmount, StringFormat={}{0:N2}}" FontSize="24" FontWeight="Bold" Foreground="#059669" VerticalAlignment="Center"/>
                        </StackPanel>

                        <Button Grid.Column="2"
                                Command="{Binding CloseGoodsReceiptModalCommand}"
                                Height="46"
                                FontSize="16">
                            <Button.Style>
                                <Style TargetType="Button" BasedOn="{StaticResource CancelBtnStyle}">
                                    <Setter Property="Content" Value="ยกเลิก"/>
                                    <Style.Triggers>
                                        <DataTrigger Binding="{Binding IsReceiptViewOnly}" Value="True">
                                            <Setter Property="Content" Value="ปิดหน้าต่าง"/>
                                        </DataTrigger>
                                    </Style.Triggers>
                                </Style>
                            </Button.Style>
                        </Button>

                        <Button Grid.Column="4"
                                x:Name="btnConfirmReceipt"
                                Content="📥 ยืนยันการรับสินค้าเข้าคลัง"
                                Command="{Binding ConfirmGoodsReceiptCommand}" 
                                IsEnabled="{Binding CanConfirmGoodsReceipt}"
                                Visibility="{Binding IsReceiptViewOnly, Converter={StaticResource InverseBoolToVis}}"
                                Style="{StaticResource RoseBtnStyle}"
                                Height="46"
                                FontSize="16"/>'''
    )

    # 7. Stock-In Form Modal Overlay
    content = content.replace(
        '''                                <TextBlock Text="บันทึกรับสินค้าเข้าคลัง"
                                           FontSize="18" FontWeight="Bold"
                                           Foreground="#2D3748"
                                           Margin="0"/>
                                <TextBlock Text="เลือกรายการสินค้าและกรอกรายละเอียดการรับสินค้าเข้าสต็อก"
                                           FontSize="12.5"
                                           Foreground="#718096"
                                           Margin="0,2,0,0"/>''',
        '''                                <TextBlock Text="บันทึกรับสินค้าเข้าคลัง"
                                           FontSize="20" FontWeight="Bold"
                                           Foreground="#2D3748"
                                           Margin="0"/>
                                <TextBlock Text="เลือกรายการสินค้าและกรอกรายละเอียดการรับสินค้าเข้าสต็อก"
                                           FontSize="14"
                                           Foreground="#718096"
                                           Margin="0,2,0,0"/>'''
    )
    content = content.replace(
        '''                            <!-- Product Dropdown Picker -->
                            <TextBlock Text="เลือกสินค้าที่ต้องการรับเข้า *" FontSize="12.5" Foreground="#718096" FontWeight="SemiBold" Margin="0,0,0,4"/>
                            <Border Background="#FFFFFF" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="8" Margin="0,0,0,10">
                                <ComboBox ItemsSource="{Binding Products}"
                                          SelectedItem="{Binding SelectedProduct, Mode=TwoWay}" IsEnabled="{Binding CanEditStockIn}"
                                          Style="{StaticResource LightComboBoxStyle}"
                                          Height="38" BorderThickness="0">''',
        '''                            <!-- Product Dropdown Picker -->
                            <TextBlock Text="เลือกสินค้าที่ต้องการรับเข้า *" FontSize="14.5" Foreground="#718096" FontWeight="SemiBold" Margin="0,0,0,4"/>
                            <Border Background="#FFFFFF" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="8" Margin="0,0,0,10" Height="42">
                                <ComboBox ItemsSource="{Binding Products}"
                                          SelectedItem="{Binding SelectedProduct, Mode=TwoWay}" IsEnabled="{Binding CanEditStockIn}"
                                          Style="{StaticResource LightComboBoxStyle}"
                                          Height="42" FontSize="15" BorderThickness="0">'''
    )

    # Form Fields in Stock-In Form
    content = content.replace(
        '''                            <!-- Ref No -->
                            <StackPanel Grid.Column="0">
                                <TextBlock Text="เลขที่ใบส่งของ หรือ ใบกำกับภาษี *" FontSize="12.5" Foreground="#718096" FontWeight="SemiBold" Margin="0,0,0,4"/>
                                <Border Background="#E0F2FE" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="8">
                                    <TextBox Text="{Binding StockInRefNo, UpdateSourceTrigger=PropertyChanged}" IsEnabled="{Binding CanEditStockIn}"
                                             Style="{StaticResource LightTextBoxStyle}"
                                             Padding="10,8"/>
                                </Border>
                            </StackPanel>

                            <!-- Supplier -->
                            <StackPanel Grid.Column="2">
                                <TextBlock Text="ผู้จัดจำหน่าย หรือ ซัพพลายเออร์ *" FontSize="12.5" Foreground="#718096" FontWeight="SemiBold" Margin="0,0,0,4"/>
                                <Border Background="#E0F2FE" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="8">
                                    <TextBox Text="{Binding StockInSupplier, UpdateSourceTrigger=PropertyChanged}" IsEnabled="{Binding CanEditStockIn}"
                                             Style="{StaticResource LightTextBoxStyle}"
                                             Padding="10,8"/>
                                </Border>
                            </StackPanel>''',
        '''                            <!-- Ref No -->
                            <StackPanel Grid.Column="0">
                                <TextBlock Text="เลขที่ใบส่งของ หรือ ใบกำกับภาษี *" FontSize="14.5" Foreground="#718096" FontWeight="SemiBold" Margin="0,0,0,4"/>
                                <Border Background="#E0F2FE" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="8" Height="42">
                                    <TextBox Text="{Binding StockInRefNo, UpdateSourceTrigger=PropertyChanged}" IsEnabled="{Binding CanEditStockIn}"
                                             Style="{StaticResource LightTextBoxStyle}"
                                             Height="42" FontSize="16"
                                             Padding="10,0" VerticalContentAlignment="Center"/>
                                </Border>
                            </StackPanel>

                            <!-- Supplier -->
                            <StackPanel Grid.Column="2">
                                <TextBlock Text="ผู้จัดจำหน่าย หรือ ซัพพลายเออร์ *" FontSize="14.5" Foreground="#718096" FontWeight="SemiBold" Margin="0,0,0,4"/>
                                <Border Background="#E0F2FE" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="8" Height="42">
                                    <TextBox Text="{Binding StockInSupplier, UpdateSourceTrigger=PropertyChanged}" IsEnabled="{Binding CanEditStockIn}"
                                             Style="{StaticResource LightTextBoxStyle}"
                                             Height="42" FontSize="16"
                                             Padding="10,0" VerticalContentAlignment="Center"/>
                                </Border>
                            </StackPanel>'''
    )

    content = content.replace(
        '''                            <!-- Quantity -->
                            <StackPanel Grid.Column="0">
                                <TextBlock Text="จำนวนที่รับเข้า *" FontSize="12.5" Foreground="#718096" FontWeight="SemiBold" Margin="0,0,0,4"/>
                                <Border Background="#E0F2FE" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="8">
                                    <TextBox Text="{Binding StockInQuantity, UpdateSourceTrigger=PropertyChanged}" IsEnabled="{Binding CanEditStockIn}"
                                             Style="{StaticResource LightTextBoxStyle}"
                                             Padding="10,8" TextAlignment="Center"/>
                                </Border>
                            </StackPanel>

                            <!-- Unit Cost -->
                            <StackPanel Grid.Column="2">
                                <TextBlock Text="ต้นทุนต่อหน่วย (บาท) *" FontSize="12.5" Foreground="#718096" FontWeight="SemiBold" Margin="0,0,0,4"/>
                                <Border Background="#E0F2FE" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="8">
                                    <TextBox Text="{Binding StockInCost, UpdateSourceTrigger=PropertyChanged}" IsEnabled="{Binding CanEditStockIn}"
                                             Style="{StaticResource LightTextBoxStyle}"
                                             Padding="10,8"/>
                                </Border>
                            </StackPanel>''',
        '''                            <!-- Quantity -->
                            <StackPanel Grid.Column="0">
                                <TextBlock Text="จำนวนที่รับเข้า *" FontSize="14.5" Foreground="#718096" FontWeight="SemiBold" Margin="0,0,0,4"/>
                                <Border Background="#E0F2FE" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="8" Height="42">
                                    <TextBox Text="{Binding StockInQuantity, UpdateSourceTrigger=PropertyChanged}" IsEnabled="{Binding CanEditStockIn}"
                                             Style="{StaticResource LightTextBoxStyle}"
                                             Height="42" FontSize="16"
                                             Padding="10,0" TextAlignment="Center" VerticalContentAlignment="Center"/>
                                </Border>
                            </StackPanel>

                            <!-- Unit Cost -->
                            <StackPanel Grid.Column="2">
                                <TextBlock Text="ต้นทุนต่อหน่วย (บาท) *" FontSize="14.5" Foreground="#718096" FontWeight="SemiBold" Margin="0,0,0,4"/>
                                <Border Background="#E0F2FE" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="8" Height="42">
                                    <TextBox Text="{Binding StockInCost, UpdateSourceTrigger=PropertyChanged}" IsEnabled="{Binding CanEditStockIn}"
                                             Style="{StaticResource LightTextBoxStyle}"
                                             Height="42" FontSize="16"
                                             Padding="10,0" VerticalContentAlignment="Center"/>
                                </Border>
                            </StackPanel>'''
    )

    # Modal Actions Bar in Stock-In Form
    content = content.replace(
        '''                        <!-- Cancel -->
                        <Button Grid.Column="0"
                                Content="ยกเลิก"
                                Command="{Binding CloseStockInModalCommand}"
                                Style="{StaticResource RoseBtnStyle}"
                                Background="#E0F2FE"
                                Foreground="#718096"
                                BorderBrush="#BAE6FD"
                                BorderThickness="1"
                                Height="42"/>

                        <!-- Confirm -->
                        <Button Grid.Column="2"
                                Content="📥 ยืนยันการรับสินค้าเข้าคลัง"
                                Command="{Binding ConfirmStockInCommand}" IsEnabled="{Binding CanEditStockIn}"
                                Style="{StaticResource RoseBtnStyle}"
                                Background="#00A3E0"
                                Foreground="White"
                                Height="42"/>''',
        '''                        <!-- Cancel -->
                        <Button Grid.Column="0"
                                Content="ยกเลิก"
                                Command="{Binding CloseStockInModalCommand}"
                                Style="{StaticResource RoseBtnStyle}"
                                Background="#E0F2FE"
                                Foreground="#718096"
                                BorderBrush="#BAE6FD"
                                BorderThickness="1"
                                Height="46"
                                FontSize="16"/>

                        <!-- Confirm -->
                        <Button Grid.Column="2"
                                Content="📥 ยืนยันการรับสินค้าเข้าคลัง"
                                Command="{Binding ConfirmStockInCommand}" IsEnabled="{Binding CanEditStockIn}"
                                Style="{StaticResource RoseBtnStyle}"
                                Background="#00A3E0"
                                Foreground="White"
                                Height="46"
                                FontSize="16"/>'''
    )

    with open(file_path, "w", encoding="utf-8") as f:
        f.write(content)

    print("Successfully updated StockInView.xaml")

if __name__ == "__main__":
    update_stock_in()
