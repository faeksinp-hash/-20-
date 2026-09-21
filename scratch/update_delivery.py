import re

def update_delivery_view():
    path = r"d:\ShopManagement_หลัก\MatchaShop\Views\DeliveryView.xaml"
    with open(path, "r", encoding="utf-8") as f:
        content = f.read()

    # 1. Update LightDatePickerStyle
    old_datepicker = """        <Style x:Key="LightDatePickerStyle" TargetType="DatePicker">
            <Setter Property="Background"   Value="#E0F2FE"/>
            <Setter Property="Foreground"   Value="#2D3748"/>
            <Setter Property="BorderBrush"  Value="#BAE6FD"/>
            <Setter Property="BorderThickness" Value="1"/>
            <Setter Property="Height"       Value="40"/>
            <Setter Property="FontFamily"   Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="FontSize"     Value="13"/>
            <Setter Property="Padding"      Value="8,0"/>
        </Style>"""
    new_datepicker = """        <Style x:Key="LightDatePickerStyle" TargetType="DatePicker">
            <Setter Property="Background"   Value="#E0F2FE"/>
            <Setter Property="Foreground"   Value="#2D3748"/>
            <Setter Property="BorderBrush"  Value="#BAE6FD"/>
            <Setter Property="BorderThickness" Value="1"/>
            <Setter Property="Height"       Value="44"/>
            <Setter Property="FontFamily"   Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="FontSize"     Value="15"/>
            <Setter Property="Padding"      Value="8,0"/>
        </Style>"""
    assert old_datepicker in content, "old_datepicker not found"
    content = content.replace(old_datepicker, new_datepicker)

    # 2. Update LightComboBoxStyle
    old_combobox = """        <Style x:Key="LightComboBoxStyle" TargetType="ComboBox">
            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI, Tahoma"/>
            <Setter Property="FontSize" Value="14"/>
            <Setter Property="FontWeight" Value="SemiBold"/>
            <Setter Property="Foreground" Value="#0F172A"/>
            <Setter Property="Background" Value="#FFFFFF"/>
            <Setter Property="BorderBrush" Value="#BAE6FD"/>
            <Setter Property="BorderThickness" Value="1"/>
            <Setter Property="Height" Value="40"/>"""
    new_combobox = """        <Style x:Key="LightComboBoxStyle" TargetType="ComboBox">
            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI, Tahoma"/>
            <Setter Property="FontSize" Value="15"/>
            <Setter Property="FontWeight" Value="SemiBold"/>
            <Setter Property="Foreground" Value="#0F172A"/>
            <Setter Property="Background" Value="#FFFFFF"/>
            <Setter Property="BorderBrush" Value="#BAE6FD"/>
            <Setter Property="BorderThickness" Value="1"/>
            <Setter Property="Height" Value="44"/>"""
    assert old_combobox in content, "old_combobox not found"
    content = content.replace(old_combobox, new_combobox)

    # 3. Update Status Items in Dropdown
    content = content.replace("""        <Style x:Key="StatusItemPendingStyle" TargetType="ComboBoxItem">
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Foreground" Value="#0369A1"/>
            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="FontSize" Value="12.5"/>""",
            """        <Style x:Key="StatusItemPendingStyle" TargetType="ComboBoxItem">
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Foreground" Value="#0369A1"/>
            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="FontSize" Value="14"/>""")

    content = content.replace("""        <Style x:Key="StatusItemInTransitStyle" TargetType="ComboBoxItem">
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Foreground" Value="#B45309"/>
            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="FontSize" Value="12.5"/>""",
            """        <Style x:Key="StatusItemInTransitStyle" TargetType="ComboBoxItem">
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Foreground" Value="#B45309"/>
            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="FontSize" Value="14"/>""")

    content = content.replace("""        <Style x:Key="StatusItemSuccessStyle" TargetType="ComboBoxItem">
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Foreground" Value="#065F46"/>
            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="FontSize" Value="12.5"/>""",
            """        <Style x:Key="StatusItemSuccessStyle" TargetType="ComboBoxItem">
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Foreground" Value="#065F46"/>
            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="FontSize" Value="14"/>""")

    content = content.replace("""        <Style x:Key="StatusItemCancelledStyle" TargetType="ComboBoxItem">
            <Setter Property="Background" Value="#FFF1F2"/>
            <Setter Property="BorderBrush" Value="#FECDD3"/>
            <Setter Property="BorderThickness" Value="1"/>
            <Setter Property="Foreground" Value="#BE123C"/>
            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="FontSize" Value="12.5"/>""",
            """        <Style x:Key="StatusItemCancelledStyle" TargetType="ComboBoxItem">
            <Setter Property="Background" Value="#FFF1F2"/>
            <Setter Property="BorderBrush" Value="#FECDD3"/>
            <Setter Property="BorderThickness" Value="1"/>
            <Setter Property="Foreground" Value="#BE123C"/>
            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="FontSize" Value="14"/>""")

    # 4. Update DeliveryStatusModalComboBoxStyle
    old_modal_combo = """        <Style x:Key="DeliveryStatusModalComboBoxStyle" TargetType="ComboBox">
            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI, Tahoma"/>
            <Setter Property="FontSize" Value="12"/>
            <Setter Property="FontWeight" Value="Bold"/>
            <Setter Property="Height" Value="30"/>
            <Setter Property="MinWidth" Value="125"/>
            <Setter Property="Cursor" Value="Hand"/>
            <Setter Property="Padding" Value="10,0,24,0"/>
            <Setter Property="VerticalContentAlignment" Value="Center"/>
            <Setter Property="HorizontalContentAlignment" Value="Center"/>
            <Setter Property="BorderThickness" Value="1"/>
            <Setter Property="Background" Value="#FEF3C7"/>
            <Setter Property="BorderBrush" Value="#FDE68A"/>
            <Setter Property="Foreground" Value="#B45309"/>"""
    new_modal_combo = """        <Style x:Key="DeliveryStatusModalComboBoxStyle" TargetType="ComboBox">
            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI, Tahoma"/>
            <Setter Property="FontSize" Value="13.5"/>
            <Setter Property="FontWeight" Value="Bold"/>
            <Setter Property="Height" Value="36"/>
            <Setter Property="MinWidth" Value="135"/>
            <Setter Property="Cursor" Value="Hand"/>
            <Setter Property="Padding" Value="12,0,26,0"/>
            <Setter Property="VerticalContentAlignment" Value="Center"/>
            <Setter Property="HorizontalContentAlignment" Value="Center"/>
            <Setter Property="BorderThickness" Value="1"/>
            <Setter Property="Background" Value="#FEF3C7"/>
            <Setter Property="BorderBrush" Value="#FDE68A"/>
            <Setter Property="Foreground" Value="#B45309"/>"""
    assert old_modal_combo in content, "old_modal_combo not found"
    content = content.replace(old_modal_combo, new_modal_combo)

    content = content.replace("""                                    <Style TargetType="TextBlock">
                                        <Setter Property="Foreground" Value="{Binding Foreground, RelativeSource={RelativeSource AncestorType=ComboBox}}"/>
                                        <Setter Property="FontWeight" Value="Bold"/>
                                        <Setter Property="FontSize" Value="12"/>
                                    </Style>""",
                              """                                    <Style TargetType="TextBlock">
                                        <Setter Property="Foreground" Value="{Binding Foreground, RelativeSource={RelativeSource AncestorType=ComboBox}}"/>
                                        <Setter Property="FontWeight" Value="Bold"/>
                                        <Setter Property="FontSize" Value="13.5"/>
                                    </Style>""")

    # 5. Update RoseBtnStyle
    old_rose_btn = """        <!-- Primary Rose Button Style -->
        <Style x:Key="RoseBtnStyle" TargetType="Button">
            <Setter Property="FontFamily"      Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="Background"      Value="#00A3E0"/>
            <Setter Property="Foreground"      Value="White"/>
            <Setter Property="FontSize"        Value="14"/>
            <Setter Property="FontWeight"      Value="SemiBold"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Cursor"          Value="Hand"/>
            <Setter Property="Height"          Value="40"/>"""
    new_rose_btn = """        <!-- Primary Rose Button Style -->
        <Style x:Key="RoseBtnStyle" TargetType="Button">
            <Setter Property="FontFamily"      Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="Background"      Value="#00A3E0"/>
            <Setter Property="Foreground"      Value="White"/>
            <Setter Property="FontSize"        Value="16"/>
            <Setter Property="FontWeight"      Value="SemiBold"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Cursor"          Value="Hand"/>
            <Setter Property="Height"          Value="44"/>"""
    assert old_rose_btn in content, "old_rose_btn not found"
    content = content.replace(old_rose_btn, new_rose_btn)

    # 6. Update ViewDetailsButtonStyle
    old_view_btn = """        <!-- View Details Outline Button Style -->
        <Style x:Key="ViewDetailsButtonStyle" TargetType="Button">
            <Setter Property="FontFamily"      Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="Background"      Value="#E0F2FE"/>
            <Setter Property="Foreground"      Value="#0B2545"/>
            <Setter Property="BorderBrush"     Value="#00A3E0"/>
            <Setter Property="BorderThickness" Value="1"/>
            <Setter Property="FontSize"        Value="12"/>
            <Setter Property="FontWeight"      Value="SemiBold"/>
            <Setter Property="Cursor"          Value="Hand"/>
            <Setter Property="Height"          Value="28"/>
            <Setter Property="Padding"         Value="6,2"/>
            <Setter Property="MinWidth"        Value="95"/>"""
    new_view_btn = """        <!-- View Details Outline Button Style -->
        <Style x:Key="ViewDetailsButtonStyle" TargetType="Button">
            <Setter Property="FontFamily"      Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="Background"      Value="#E0F2FE"/>
            <Setter Property="Foreground"      Value="#0B2545"/>
            <Setter Property="BorderBrush"     Value="#00A3E0"/>
            <Setter Property="BorderThickness" Value="1"/>
            <Setter Property="FontSize"        Value="14"/>
            <Setter Property="FontWeight"      Value="SemiBold"/>
            <Setter Property="Cursor"          Value="Hand"/>
            <Setter Property="Height"          Value="36"/>
            <Setter Property="Padding"         Value="8,2"/>
            <Setter Property="MinWidth"        Value="105"/>"""
    assert old_view_btn in content, "old_view_btn not found"
    content = content.replace(old_view_btn, new_view_btn)

    # 7. DeliveryColumnHeaderStyle
    content = content.replace("""        <Style x:Key="DeliveryColumnHeaderStyle" TargetType="DataGridColumnHeader">
            <Setter Property="Background"              Value="#F1F5F9"/>
            <Setter Property="Foreground"              Value="#000000"/>
            <Setter Property="FontWeight"              Value="Bold"/>
            <Setter Property="FontSize"                Value="16"/>
            <Setter Property="Height"                  Value="46"/>""",
            """        <Style x:Key="DeliveryColumnHeaderStyle" TargetType="DataGridColumnHeader">
            <Setter Property="Background"              Value="#F1F5F9"/>
            <Setter Property="Foreground"              Value="#000000"/>
            <Setter Property="FontWeight"              Value="Bold"/>
            <Setter Property="FontSize"                Value="16"/>
            <Setter Property="Height"                  Value="48"/>""")

    # 8. DeliveryRowStyle
    content = content.replace("""        <Style x:Key="DeliveryRowStyle" TargetType="DataGridRow">
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Foreground" Value="#2D3748"/>
            <Setter Property="FontSize"   Value="13"/>""",
            """        <Style x:Key="DeliveryRowStyle" TargetType="DataGridRow">
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Foreground" Value="#2D3748"/>
            <Setter Property="FontSize"   Value="15"/>
            <Setter Property="MinHeight"  Value="48"/>""")

    # 9. ColCenter / ColLeft / ColAmountStyle
    content = content.replace("""        <!-- Column Text Helpers -->
        <Style x:Key="ColCenter" TargetType="TextBlock">
            <Setter Property="HorizontalAlignment" Value="Center"/>
            <Setter Property="VerticalAlignment"   Value="Center"/>
            <Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
        </Style>
        <Style x:Key="ColLeft" TargetType="TextBlock">
            <Setter Property="HorizontalAlignment" Value="Left"/>
            <Setter Property="VerticalAlignment"   Value="Center"/>
            <Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="Margin"              Value="4,0"/>
        </Style>
        <Style x:Key="ColCenterBold" TargetType="TextBlock" BasedOn="{StaticResource ColCenter}">
            <Setter Property="FontWeight" Value="Bold"/>
        </Style>
        <Style x:Key="ColAmountStyle" TargetType="TextBlock">
            <Setter Property="HorizontalAlignment" Value="Right"/>
            <Setter Property="VerticalAlignment"   Value="Center"/>
            <Setter Property="FontWeight"          Value="Bold"/>
            <Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="Margin"              Value="0,0,8,0"/>
            <Setter Property="Foreground"          Value="#0B2545"/>
        </Style>""",
        """        <!-- Column Text Helpers -->
        <Style x:Key="ColCenter" TargetType="TextBlock">
            <Setter Property="HorizontalAlignment" Value="Center"/>
            <Setter Property="VerticalAlignment"   Value="Center"/>
            <Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="FontSize"            Value="15"/>
        </Style>
        <Style x:Key="ColLeft" TargetType="TextBlock">
            <Setter Property="HorizontalAlignment" Value="Left"/>
            <Setter Property="VerticalAlignment"   Value="Center"/>
            <Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="FontSize"            Value="15"/>
            <Setter Property="Margin"              Value="4,0"/>
        </Style>
        <Style x:Key="ColCenterBold" TargetType="TextBlock" BasedOn="{StaticResource ColCenter}">
            <Setter Property="FontWeight" Value="Bold"/>
            <Setter Property="FontSize"   Value="15"/>
        </Style>
        <Style x:Key="ColAmountStyle" TargetType="TextBlock">
            <Setter Property="HorizontalAlignment" Value="Right"/>
            <Setter Property="VerticalAlignment"   Value="Center"/>
            <Setter Property="FontWeight"          Value="Bold"/>
            <Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="FontSize"            Value="16"/>
            <Setter Property="Margin"              Value="0,0,8,0"/>
            <Setter Property="Foreground"          Value="#0B2545"/>
        </Style>""")

    # 10. LightModalColHeaderStyle & LightModalRowStyle
    content = content.replace("""        <Style x:Key="LightModalColHeaderStyle" TargetType="DataGridColumnHeader">
            <Setter Property="Background"              Value="#F8FAFC"/>
            <Setter Property="Foreground"              Value="#475569"/>
            <Setter Property="FontWeight"              Value="SemiBold"/>
            <Setter Property="FontSize"                Value="12.5"/>
            <Setter Property="Height"                  Value="36"/>
            <Setter Property="Padding"                 Value="10,0"/>
            <Setter Property="BorderBrush"             Value="#E2E8F0"/>
            <Setter Property="BorderThickness"         Value="0,0,0,1"/>
            <Setter Property="VerticalContentAlignment" Value="Center"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="DataGridColumnHeader">
                        <Border Background="{TemplateBinding Background}"
                                BorderBrush="{TemplateBinding BorderBrush}"
                                BorderThickness="{TemplateBinding BorderThickness}"
                                Padding="{TemplateBinding Padding}">
                            <TextBlock Text="{TemplateBinding Content}"
                                       Foreground="#475569"
                                       FontWeight="SemiBold"
                                       FontSize="12.5"
                                       VerticalAlignment="Center"/>
                        </Border>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>

        <Style x:Key="LightModalRowStyle" TargetType="DataGridRow">
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Foreground" Value="#1E293B"/>
            <Setter Property="FontSize"   Value="13"/>
            <Setter Property="MinHeight"  Value="36"/>""",
            """        <Style x:Key="LightModalColHeaderStyle" TargetType="DataGridColumnHeader">
            <Setter Property="Background"              Value="#F8FAFC"/>
            <Setter Property="Foreground"              Value="#475569"/>
            <Setter Property="FontWeight"              Value="SemiBold"/>
            <Setter Property="FontSize"                Value="15"/>
            <Setter Property="Height"                  Value="44"/>
            <Setter Property="Padding"                 Value="10,0"/>
            <Setter Property="BorderBrush"             Value="#E2E8F0"/>
            <Setter Property="BorderThickness"         Value="0,0,0,1"/>
            <Setter Property="VerticalContentAlignment" Value="Center"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="DataGridColumnHeader">
                        <Border Background="{TemplateBinding Background}"
                                BorderBrush="{TemplateBinding BorderBrush}"
                                BorderThickness="{TemplateBinding BorderThickness}"
                                Padding="{TemplateBinding Padding}">
                            <TextBlock Text="{TemplateBinding Content}"
                                       Foreground="#475569"
                                       FontWeight="SemiBold"
                                       FontSize="15"
                                       VerticalAlignment="Center"/>
                        </Border>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>

        <Style x:Key="LightModalRowStyle" TargetType="DataGridRow">
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Foreground" Value="#1E293B"/>
            <Setter Property="FontSize"   Value="15"/>
            <Setter Property="MinHeight"  Value="44"/>""")

    # 11. Header Subtitle:
    content = content.replace("""                        <TextBlock Text="จัดการการจัดส่งสินค้า — ระบบจัดการร้าน พอใจ"
                                   FontSize="13"
                                   Foreground="#718096"
                                   Margin="0"/>""",
                              """                        <TextBlock Text="จัดการการจัดส่งสินค้า — ระบบจัดการร้าน พอใจ"
                                   FontSize="15"
                                   Foreground="#718096"
                                   Margin="0"/>""")

    # 12. KPI Summary Cards (3 cards):
    # Card 1: Pending Deliveries
    old_kpi1 = """                        <!-- Icon + Label -->
                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="36" Height="36" CornerRadius="10" Background="#E0F2FE" Margin="0,0,10,0">
                                <TextBlock Text="⏳" FontSize="18" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="ออเดอร์รอจัดส่ง" FontSize="13" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <!-- Value -->
                        <TextBlock Grid.Row="1"
                                   Text="{Binding PendingDeliveriesCount}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#0B2545"
                                   Margin="0,0,0,6"/>
                        <!-- Sub-label -->
                        <TextBlock Grid.Row="2"
                                   Text="รายการที่ต้องเตรียม/นำส่ง"
                                   FontSize="11"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>"""
    new_kpi1 = """                        <!-- Icon + Label -->
                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="40" Height="40" CornerRadius="10" Background="#E0F2FE" Margin="0,0,10,0">
                                <TextBlock Text="⏳" FontSize="20" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="ออเดอร์รอจัดส่ง" FontSize="16" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <!-- Value -->
                        <TextBlock Grid.Row="1"
                                   Text="{Binding PendingDeliveriesCount}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#0B2545"
                                   Margin="0,0,0,6"/>
                        <!-- Sub-label -->
                        <TextBlock Grid.Row="2"
                                   Text="รายการที่ต้องเตรียม/นำส่ง"
                                   FontSize="14"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>"""
    assert old_kpi1 in content, "old_kpi1 not found"
    content = content.replace(old_kpi1, new_kpi1)

    # Card 2: In Delivery
    old_kpi2 = """                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="36" Height="36" CornerRadius="10" Background="#EBF8FF" Margin="0,0,10,0">
                                <TextBlock Text="🛵" FontSize="18" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="อยู่ระหว่างจัดส่ง" FontSize="13" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <TextBlock Grid.Row="1"
                                   Text="{Binding InDeliveryCount}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#2B6CB0"
                                   Margin="0,0,0,6"/>
                        <TextBlock Grid.Row="2"
                                   Text="ไรเดอร์กำลังเดินทางส่งสินค้า"
                                   FontSize="11"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>"""
    new_kpi2 = """                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="40" Height="40" CornerRadius="10" Background="#EBF8FF" Margin="0,0,10,0">
                                <TextBlock Text="🛵" FontSize="20" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="อยู่ระหว่างจัดส่ง" FontSize="16" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <TextBlock Grid.Row="1"
                                   Text="{Binding InDeliveryCount}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#2B6CB0"
                                   Margin="0,0,0,6"/>
                        <TextBlock Grid.Row="2"
                                   Text="ไรเดอร์กำลังเดินทางส่งสินค้า"
                                   FontSize="14"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>"""
    assert old_kpi2 in content, "old_kpi2 not found"
    content = content.replace(old_kpi2, new_kpi2)

    # Card 3: Delivered Today
    old_kpi3 = """                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="36" Height="36" CornerRadius="10" Background="#D1FAE5" Margin="0,0,10,0">
                                <TextBlock Text="✅" FontSize="18" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="จัดส่งสำเร็จวันนี้" FontSize="13" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <TextBlock Grid.Row="1"
                                   Text="{Binding DeliveredTodayCount}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#059669"
                                   Margin="0,0,0,6"/>
                        <TextBlock Grid.Row="2"
                                   Text="ออเดอร์ส่งถึงมือลูกค้าเรียบร้อย"
                                   FontSize="11"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>"""
    new_kpi3 = """                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="40" Height="40" CornerRadius="10" Background="#D1FAE5" Margin="0,0,10,0">
                                <TextBlock Text="✅" FontSize="20" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="จัดส่งสำเร็จวันนี้" FontSize="16" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <TextBlock Grid.Row="1"
                                   Text="{Binding DeliveredTodayCount}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#059669"
                                   Margin="0,0,0,6"/>
                        <TextBlock Grid.Row="2"
                                   Text="ออเดอร์ส่งถึงมือลูกค้าเรียบร้อย"
                                   FontSize="14"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>"""
    assert old_kpi3 in content, "old_kpi3 not found"
    content = content.replace(old_kpi3, new_kpi3)

    # 13. Filter Bar
    old_filter = """                    <!-- Search TextBox -->
                    <Border Grid.Column="0"
                            Background="#E0F2FE"
                            BorderBrush="#BAE6FD"
                            BorderThickness="1.5"
                            CornerRadius="8">
                        <Grid>
                            <!-- Search Icon -->
                            <Path Data="{StaticResource IconSearch}"
                                  Fill="#718096"
                                  Width="18" Height="18"
                                  HorizontalAlignment="Left"
                                  Margin="12,0,0,0"
                                  Stretch="Uniform"
                                  VerticalAlignment="Center"/>
                            <!-- TextBox -->
                            <TextBox x:Name="DeliverySearchBox"
                                     Text="{Binding DeliverySearchKeyword, UpdateSourceTrigger=PropertyChanged}"
                                     Style="{StaticResource LightTextBoxStyle}"
                                     Height="40"
                                     Padding="38,0,12,0"
                                     VerticalContentAlignment="Center"
                                     FontSize="13"/>
                            <!-- Placeholder -->
                            <TextBlock Text="ค้นหา เลขที่บิล, ชื่อลูกค้า, เบอร์โทร..."
                                       Foreground="#A0AEC0"
                                       FontSize="13"
                                       VerticalAlignment="Center"
                                       Margin="38,0,12,0"
                                       IsHitTestVisible="False">
                                <TextBlock.Style>
                                    <Style TargetType="TextBlock">
                                        <Setter Property="Visibility" Value="Collapsed"/>
                                        <Style.Triggers>
                                            <DataTrigger Binding="{Binding Text, ElementName=DeliverySearchBox}" Value="">
                                                <Setter Property="Visibility" Value="Visible"/>
                                            </DataTrigger>
                                        </Style.Triggers>
                                    </Style>
                                </TextBlock.Style>
                            </TextBlock>
                        </Grid>
                    </Border>

                    <!-- Status Filter ComboBox -->
                    <Border Grid.Column="2"
                            Background="#E0F2FE"
                            BorderBrush="#BAE6FD"
                            BorderThickness="1.5"
                            CornerRadius="8">
                        <ComboBox ItemsSource="{Binding DeliveryStatusOptions}"
                                  SelectedItem="{Binding SelectedDeliveryStatusFilter}"
                                  Style="{StaticResource LightComboBoxStyle}"
                                  BorderThickness="0"
                                  Background="Transparent"/>
                    </Border>

                    <!-- Search Button -->
                    <Button Grid.Column="4"
                            Content="ค้นหา"
                            Command="{Binding SearchDeliveriesCommand}"
                            Background="#1E88E5"
                            Cursor="Hand"
                            Padding="24,0"
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

                    <!-- Clear / Reset Button -->
                    <Button Grid.Column="6"
                            Content="ล้าง"
                            Command="{Binding ClearDeliveryFilterCommand}"
                            Style="{StaticResource SecondaryBtnStyle}"
                            Padding="16,0"
                            Height="40"/>"""

    new_filter = """                    <!-- Search TextBox -->
                    <Border Grid.Column="0"
                            Background="#E0F2FE"
                            BorderBrush="#BAE6FD"
                            BorderThickness="1.5"
                            CornerRadius="8">
                        <Grid>
                            <!-- Search Icon -->
                            <Path Data="{StaticResource IconSearch}"
                                  Fill="#718096"
                                  Width="18" Height="18"
                                  HorizontalAlignment="Left"
                                  Margin="12,0,0,0"
                                  Stretch="Uniform"
                                  VerticalAlignment="Center"/>
                            <!-- TextBox -->
                            <TextBox x:Name="DeliverySearchBox"
                                     Text="{Binding DeliverySearchKeyword, UpdateSourceTrigger=PropertyChanged}"
                                     Style="{StaticResource LightTextBoxStyle}"
                                     Height="44"
                                     Padding="38,0,12,0"
                                     VerticalContentAlignment="Center"
                                     FontSize="16"/>
                            <!-- Placeholder -->
                            <TextBlock Text="ค้นหา เลขที่บิล, ชื่อลูกค้า, เบอร์โทร..."
                                       Foreground="#A0AEC0"
                                       FontSize="15"
                                       VerticalAlignment="Center"
                                       Margin="38,0,12,0"
                                       IsHitTestVisible="False">
                                <TextBlock.Style>
                                    <Style TargetType="TextBlock">
                                        <Setter Property="Visibility" Value="Collapsed"/>
                                        <Style.Triggers>
                                            <DataTrigger Binding="{Binding Text, ElementName=DeliverySearchBox}" Value="">
                                                <Setter Property="Visibility" Value="Visible"/>
                                            </DataTrigger>
                                        </Style.Triggers>
                                    </Style>
                                </TextBlock.Style>
                            </TextBlock>
                        </Grid>
                    </Border>

                    <!-- Status Filter ComboBox -->
                    <Border Grid.Column="2"
                            Background="#E0F2FE"
                            BorderBrush="#BAE6FD"
                            BorderThickness="1.5"
                            CornerRadius="8">
                        <ComboBox ItemsSource="{Binding DeliveryStatusOptions}"
                                  SelectedItem="{Binding SelectedDeliveryStatusFilter}"
                                  Style="{StaticResource LightComboBoxStyle}"
                                  Height="44"
                                  BorderThickness="0"
                                  Background="Transparent"/>
                    </Border>

                    <!-- Search Button -->
                    <Button Grid.Column="4"
                            Content="ค้นหา"
                            Command="{Binding SearchDeliveriesCommand}"
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

                    <!-- Clear / Reset Button -->
                    <Button Grid.Column="6"
                            Content="ล้าง"
                            Command="{Binding ClearDeliveryFilterCommand}"
                            Style="{StaticResource SecondaryBtnStyle}"
                            FontSize="16"
                            Padding="16,0"
                            Height="44"/>"""
    assert old_filter in content, "old_filter not found"
    content = content.replace(old_filter, new_filter)

    # 14. DataGrid cell templates
    # Col 4 Phone & Address
    old_col4 = """                                        <StackPanel VerticalAlignment="Center" Margin="6,0">
                                            <TextBlock Text="{Binding CustomerPhone, TargetNullValue='-', FallbackValue='-'}" FontWeight="Bold" FontSize="12.5" Foreground="#2D3748"/>
                                            <TextBlock Text="{Binding CustomerAddress, TargetNullValue='ไม่ระบุที่อยู่', FallbackValue='ไม่ระบุที่อยู่'}" FontSize="11" Foreground="#718096" TextTrimming="CharacterEllipsis"/>
                                        </StackPanel>"""
    new_col4 = """                                        <StackPanel VerticalAlignment="Center" Margin="6,0">
                                            <TextBlock Text="{Binding CustomerPhone, TargetNullValue='-', FallbackValue='-'}" FontWeight="Bold" FontSize="14" Foreground="#2D3748"/>
                                            <TextBlock Text="{Binding CustomerAddress, TargetNullValue='ไม่ระบุที่อยู่', FallbackValue='ไม่ระบุที่อยู่'}" FontSize="13" Foreground="#718096" TextTrimming="CharacterEllipsis"/>
                                        </StackPanel>"""
    assert old_col4 in content, "old_col4 not found"
    content = content.replace(old_col4, new_col4)

    # Col 5 StatusBadge
    old_badge = """                                        <Border x:Name="StatusBadge"
                                                CornerRadius="6" Padding="8,4"
                                                HorizontalAlignment="Center"
                                                Background="#E0F2FE"
                                                BorderBrush="#BAE6FD"
                                                BorderThickness="1">
                                            <StackPanel Orientation="Horizontal" HorizontalAlignment="Center" VerticalAlignment="Center">
                                                <TextBlock x:Name="StatusIcon" Text="⌛ " FontSize="11" Foreground="#0369A1" VerticalAlignment="Center"/>
                                                <TextBlock x:Name="StatusText"
                                                           Text="{Binding DeliveryStatus}"
                                                           FontSize="12" FontWeight="Bold"
                                                           Foreground="#0369A1"
                                                           VerticalAlignment="Center"/>
                                            </StackPanel>
                                        </Border>"""
    new_badge = """                                        <Border x:Name="StatusBadge"
                                                CornerRadius="6" Padding="10,4"
                                                HorizontalAlignment="Center"
                                                Background="#E0F2FE"
                                                BorderBrush="#BAE6FD"
                                                BorderThickness="1">
                                            <StackPanel Orientation="Horizontal" HorizontalAlignment="Center" VerticalAlignment="Center">
                                                <TextBlock x:Name="StatusIcon" Text="⌛ " FontSize="13" Foreground="#0369A1" VerticalAlignment="Center"/>
                                                <TextBlock x:Name="StatusText"
                                                           Text="{Binding DeliveryStatus}"
                                                           FontSize="14" FontWeight="Bold"
                                                           Foreground="#0369A1"
                                                           VerticalAlignment="Center"/>
                                            </StackPanel>
                                        </Border>"""
    assert old_badge in content, "old_badge not found"
    content = content.replace(old_badge, new_badge)

    # Bottom status bar
    content = content.replace("""                        <TextBlock Text="{Binding Deliveries.Count, StringFormat=แสดงทั้งหมด {0} รายการการจัดส่ง}"
                                   Foreground="#718096"
                                   FontSize="12"
                                   VerticalAlignment="Center"
                                   DockPanel.Dock="Left"
                                   Margin="0"/>""",
                              """                        <TextBlock Text="{Binding Deliveries.Count, StringFormat=แสดงทั้งหมด {0} รายการการจัดส่ง}"
                                   Foreground="#718096"
                                   FontSize="14"
                                   VerticalAlignment="Center"
                                   DockPanel.Dock="Left"
                                   Margin="0"/>""")

    # 15. Delivery Detail Modal Overlay
    old_modal_hdr = """                                <TextBlock Text="รายละเอียดคำสั่งซื้อเดลิเวอรี่"
                                           FontSize="18" FontWeight="Bold"
                                           Foreground="#0F172A"
                                           Margin="0,0,0,2"/>
                                <TextBlock Text="{Binding SelectedDeliveryOrder.RefNo, StringFormat='เลขที่บิล: {0}'}"
                                           FontSize="12.5" FontWeight="SemiBold"
                                           Foreground="#00A3E0"
                                           Margin="0"/>"""
    new_modal_hdr = """                                <TextBlock Text="รายละเอียดคำสั่งซื้อเดลิเวอรี่"
                                           FontSize="20" FontWeight="Bold"
                                           Foreground="#0F172A"
                                           Margin="0,0,0,2"/>
                                <TextBlock Text="{Binding SelectedDeliveryOrder.RefNo, StringFormat='เลขที่บิล: {0}'}"
                                           FontSize="14.5" FontWeight="SemiBold"
                                           Foreground="#00A3E0"
                                           Margin="0"/>"""
    assert old_modal_hdr in content, "old_modal_hdr not found"
    content = content.replace(old_modal_hdr, new_modal_hdr)

    # Inset card customer name & phone
    old_inset_cust = """                                        <StackPanel Grid.Column="1" VerticalAlignment="Center">
                                            <TextBlock Text="ชื่อลูกค้า" FontSize="10.5" Foreground="#64748B" FontWeight="SemiBold"/>
                                            <TextBlock Text="{Binding SelectedDeliveryOrder.CustomerName}" FontSize="13.5" FontWeight="Bold" Foreground="#0F172A" TextTrimming="CharacterEllipsis"/>
                                        </StackPanel>"""
    new_inset_cust = """                                        <StackPanel Grid.Column="1" VerticalAlignment="Center">
                                            <TextBlock Text="ชื่อลูกค้า" FontSize="13" Foreground="#64748B" FontWeight="SemiBold"/>
                                            <TextBlock Text="{Binding SelectedDeliveryOrder.CustomerName}" FontSize="15" FontWeight="Bold" Foreground="#0F172A" TextTrimming="CharacterEllipsis"/>
                                        </StackPanel>"""
    assert old_inset_cust in content, "old_inset_cust not found"
    content = content.replace(old_inset_cust, new_inset_cust)

    old_inset_phone = """                                        <StackPanel Grid.Column="1" VerticalAlignment="Center">
                                            <TextBlock Text="เบอร์โทรศัพท์" FontSize="10.5" Foreground="#64748B" FontWeight="SemiBold"/>
                                            <TextBlock Text="{Binding SelectedDeliveryOrder.CustomerPhone}" FontSize="13.5" FontWeight="Bold" Foreground="#00A3E0" TextTrimming="CharacterEllipsis"/>
                                        </StackPanel>"""
    new_inset_phone = """                                        <StackPanel Grid.Column="1" VerticalAlignment="Center">
                                            <TextBlock Text="เบอร์โทรศัพท์" FontSize="13" Foreground="#64748B" FontWeight="SemiBold"/>
                                            <TextBlock Text="{Binding SelectedDeliveryOrder.CustomerPhone}" FontSize="15" FontWeight="Bold" Foreground="#00A3E0" TextTrimming="CharacterEllipsis"/>
                                        </StackPanel>"""
    assert old_inset_phone in content, "old_inset_phone not found"
    content = content.replace(old_inset_phone, new_inset_phone)

    # Inset address & timestamp & status
    old_inset_addr = """                                <StackPanel Grid.Column="1" VerticalAlignment="Center">
                                    <TextBlock Text="ที่อยู่จัดส่ง:" FontSize="11" FontWeight="SemiBold" Foreground="#64748B" Margin="0,0,0,2"/>
                                    <TextBlock Text="{Binding SelectedDeliveryOrder.CustomerAddress, TargetNullValue='ไม่ระบุที่อยู่', FallbackValue='ไม่ระบุที่อยู่'}"
                                               FontSize="13" Foreground="#1E293B" FontWeight="Medium"
                                               TextWrapping="Wrap" LineHeight="18"/>
                                </StackPanel>"""
    new_inset_addr = """                                <StackPanel Grid.Column="1" VerticalAlignment="Center">
                                    <TextBlock Text="ที่อยู่จัดส่ง:" FontSize="13" FontWeight="SemiBold" Foreground="#64748B" Margin="0,0,0,2"/>
                                    <TextBlock Text="{Binding SelectedDeliveryOrder.CustomerAddress, TargetNullValue='ไม่ระบุที่อยู่', FallbackValue='ไม่ระบุที่อยู่'}"
                                               FontSize="14.5" Foreground="#1E293B" FontWeight="Medium"
                                               TextWrapping="Wrap" LineHeight="20"/>
                                </StackPanel>"""
    assert old_inset_addr in content, "old_inset_addr not found"
    content = content.replace(old_inset_addr, new_inset_addr)

    old_inset_time = """                                <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
                                    <TextBlock Text="🕒 วันเวลาสั่งซื้อ: " FontSize="12" Foreground="#64748B"/>
                                    <TextBlock Text="{Binding SelectedDeliveryOrder.Timestamp, StringFormat=dd/MM/yyyy HH:mm}"
                                               FontSize="12" Foreground="#1E293B" FontWeight="SemiBold"/>
                                </StackPanel>

                                <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" VerticalAlignment="Center">
                                    <TextBlock Text="สถานะจัดส่ง: " FontSize="12" Foreground="#64748B" VerticalAlignment="Center" Margin="0,0,6,0"/>"""
    new_inset_time = """                                <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
                                    <TextBlock Text="🕒 วันเวลาสั่งซื้อ: " FontSize="14" Foreground="#64748B"/>
                                    <TextBlock Text="{Binding SelectedDeliveryOrder.Timestamp, StringFormat=dd/MM/yyyy HH:mm}"
                                               FontSize="14" Foreground="#1E293B" FontWeight="SemiBold"/>
                                </StackPanel>

                                <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" VerticalAlignment="Center">
                                    <TextBlock Text="สถานะจัดส่ง: " FontSize="14" Foreground="#64748B" VerticalAlignment="Center" Margin="0,0,6,0"/>"""
    assert old_inset_time in content, "old_inset_time not found"
    content = content.replace(old_inset_time, new_inset_time)

    # 2: Items Header Label
    content = content.replace("""                    <!-- 2: Items Header Label -->
                    <TextBlock Grid.Row="2" Text="รายการสินค้าที่จัดส่ง"
                               FontSize="12.5" FontWeight="Bold"
                               Foreground="#475569" Margin="2,0,0,6"/>""",
                              """                    <!-- 2: Items Header Label -->
                    <TextBlock Grid.Row="2" Text="รายการสินค้าที่จัดส่ง"
                               FontSize="16" FontWeight="Bold"
                               Foreground="#475569" Margin="2,0,0,6"/>""")

    # 3: Items List Table inside Modal
    old_modal_items = """                                      RowHeight="36"
                                      GridLinesVisibility="Horizontal"
                                      HorizontalGridLinesBrush="#F1F5F9"
                                      BorderThickness="0"
                                      Background="Transparent"
                                      HorizontalScrollBarVisibility="Disabled"
                                      ColumnHeaderStyle="{StaticResource LightModalColHeaderStyle}"
                                      RowStyle="{StaticResource LightModalRowStyle}"
                                      CellStyle="{StaticResource LightModalCellStyle}">
                                <DataGrid.Columns>
                                    <DataGridTextColumn Header="รายการสินค้า"
                                                        Binding="{Binding ProductName}"
                                                        Width="*">
                                        <DataGridTextColumn.ElementStyle>
                                            <Style TargetType="TextBlock">
                                                <Setter Property="Foreground" Value="#1E293B"/>
                                                <Setter Property="VerticalAlignment" Value="Center"/>
                                                <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
                                                <Setter Property="FontSize" Value="13"/>
                                                <Setter Property="Margin" Value="10,0,0,0"/>
                                            </Style>
                                        </DataGridTextColumn.ElementStyle>
                                    </DataGridTextColumn>
                                    <DataGridTextColumn Header="จำนวน"
                                                        Binding="{Binding Quantity}"
                                                        Width="70">
                                        <DataGridTextColumn.ElementStyle>
                                            <Style TargetType="TextBlock">
                                                <Setter Property="Foreground" Value="#64748B"/>
                                                <Setter Property="HorizontalAlignment" Value="Center"/>
                                                <Setter Property="VerticalAlignment" Value="Center"/>
                                                <Setter Property="FontSize" Value="13"/>
                                            </Style>
                                        </DataGridTextColumn.ElementStyle>
                                    </DataGridTextColumn>
                                    <DataGridTextColumn Header="ราคา"
                                                        Binding="{Binding UnitPrice, StringFormat={}{0:N2}}"
                                                        Width="95">
                                        <DataGridTextColumn.ElementStyle>
                                            <Style TargetType="TextBlock">
                                                <Setter Property="Foreground" Value="#64748B"/>
                                                <Setter Property="HorizontalAlignment" Value="Right"/>
                                                <Setter Property="VerticalAlignment" Value="Center"/>
                                                <Setter Property="FontSize" Value="14"/>
                                                <Setter Property="Margin" Value="0,0,10,0"/>
                                            </Style>
                                        </DataGridTextColumn.ElementStyle>
                                    </DataGridTextColumn>
                                    <DataGridTextColumn Header="รวมเงิน"
                                                        Binding="{Binding Total, StringFormat={}{0:N2}}"
                                                        Width="105">
                                        <DataGridTextColumn.ElementStyle>
                                            <Style TargetType="TextBlock">
                                                <Setter Property="Foreground" Value="#0F172A"/>
                                                <Setter Property="FontWeight" Value="SemiBold"/>
                                                <Setter Property="HorizontalAlignment" Value="Right"/>
                                                <Setter Property="VerticalAlignment" Value="Center"/>
                                                <Setter Property="FontSize" Value="14"/>
                                                <Setter Property="Margin" Value="0,0,12,0"/>
                                            </Style>
                                        </DataGridTextColumn.ElementStyle>
                                    </DataGridTextColumn>
                                </DataGrid.Columns>"""

    new_modal_items = """                                      RowHeight="44"
                                      GridLinesVisibility="Horizontal"
                                      HorizontalGridLinesBrush="#F1F5F9"
                                      BorderThickness="0"
                                      Background="Transparent"
                                      HorizontalScrollBarVisibility="Disabled"
                                      ColumnHeaderStyle="{StaticResource LightModalColHeaderStyle}"
                                      RowStyle="{StaticResource LightModalRowStyle}"
                                      CellStyle="{StaticResource LightModalCellStyle}">
                                <DataGrid.Columns>
                                    <DataGridTextColumn Header="รายการสินค้า"
                                                        Binding="{Binding ProductName}"
                                                        Width="*">
                                        <DataGridTextColumn.ElementStyle>
                                            <Style TargetType="TextBlock">
                                                <Setter Property="Foreground" Value="#1E293B"/>
                                                <Setter Property="VerticalAlignment" Value="Center"/>
                                                <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
                                                <Setter Property="FontSize" Value="15"/>
                                                <Setter Property="Margin" Value="10,0,0,0"/>
                                            </Style>
                                        </DataGridTextColumn.ElementStyle>
                                    </DataGridTextColumn>
                                    <DataGridTextColumn Header="จำนวน"
                                                        Binding="{Binding Quantity}"
                                                        Width="80">
                                        <DataGridTextColumn.ElementStyle>
                                            <Style TargetType="TextBlock">
                                                <Setter Property="Foreground" Value="#64748B"/>
                                                <Setter Property="HorizontalAlignment" Value="Center"/>
                                                <Setter Property="VerticalAlignment" Value="Center"/>
                                                <Setter Property="FontSize" Value="15"/>
                                            </Style>
                                        </DataGridTextColumn.ElementStyle>
                                    </DataGridTextColumn>
                                    <DataGridTextColumn Header="ราคา"
                                                        Binding="{Binding UnitPrice, StringFormat={}{0:N2}}"
                                                        Width="100">
                                        <DataGridTextColumn.ElementStyle>
                                            <Style TargetType="TextBlock">
                                                <Setter Property="Foreground" Value="#64748B"/>
                                                <Setter Property="HorizontalAlignment" Value="Right"/>
                                                <Setter Property="VerticalAlignment" Value="Center"/>
                                                <Setter Property="FontSize" Value="15"/>
                                                <Setter Property="Margin" Value="0,0,10,0"/>
                                            </Style>
                                        </DataGridTextColumn.ElementStyle>
                                    </DataGridTextColumn>
                                    <DataGridTextColumn Header="รวมเงิน"
                                                        Binding="{Binding Total, StringFormat={}{0:N2}}"
                                                        Width="115">
                                        <DataGridTextColumn.ElementStyle>
                                            <Style TargetType="TextBlock">
                                                <Setter Property="Foreground" Value="#0F172A"/>
                                                <Setter Property="FontWeight" Value="SemiBold"/>
                                                <Setter Property="HorizontalAlignment" Value="Right"/>
                                                <Setter Property="VerticalAlignment" Value="Center"/>
                                                <Setter Property="FontSize" Value="15"/>
                                                <Setter Property="Margin" Value="0,0,12,0"/>
                                            </Style>
                                        </DataGridTextColumn.ElementStyle>
                                    </DataGridTextColumn>
                                </DataGrid.Columns>"""
    assert old_modal_items in content, "old_modal_items not found"
    content = content.replace(old_modal_items, new_modal_items)

    # 4: Totals Section
    old_totals = """                            <!-- 1. ยอดรวมสินค้า (Subtotal) -->
                            <DockPanel LastChildFill="False">
                                <TextBlock Text="ยอดรวมสินค้า"
                                           Foreground="#64748B"
                                           FontSize="15"
                                           FontWeight="Medium"
                                           DockPanel.Dock="Left"
                                           VerticalAlignment="Center"/>
                                <TextBlock Text="{Binding SelectedDeliverySubtotal, StringFormat={}{0:N2}}"
                                           Foreground="#1E293B"
                                           FontSize="15"
                                           FontWeight="SemiBold"
                                           DockPanel.Dock="Right"
                                           VerticalAlignment="Center"/>
                            </DockPanel>

                            <!-- 2. ค่าจัดส่ง (Shipping Fee) -->
                            <DockPanel LastChildFill="False" Margin="0,8,0,0">
                                <StackPanel Orientation="Horizontal" DockPanel.Dock="Left" VerticalAlignment="Center">
                                    <TextBlock Text="🚚 " FontSize="14" VerticalAlignment="Center"/>
                                    <TextBlock Text="ค่าจัดส่ง"
                                               Foreground="#64748B"
                                               FontSize="15"
                                               FontWeight="Medium"
                                               VerticalAlignment="Center"/>
                                </StackPanel>
                                <TextBlock Text="{Binding SelectedDeliveryShippingFee, StringFormat={}{0:N2}}"
                                           Foreground="#00A3E0"
                                           FontSize="15"
                                           FontWeight="SemiBold"
                                           DockPanel.Dock="Right"
                                           VerticalAlignment="Center"/>
                            </DockPanel>

                            <!-- 3. เส้นคั่น (Divider) -->
                            <Border Height="1" Background="#E2E8F0" Margin="0,12,0,12"/>

                            <!-- 4. ยอดรวมสุทธิทั้งสิ้น (Grand Total) -->
                            <DockPanel LastChildFill="False">
                                <TextBlock Text="ยอดรวมสุทธิทั้งสิ้น"
                                           Foreground="#0F172A"
                                           FontSize="16"
                                           FontWeight="Bold"
                                           DockPanel.Dock="Left"
                                           VerticalAlignment="Center"/>
                                <TextBlock Text="{Binding SelectedDeliveryGrandTotal, StringFormat={}{0:N2}}"
                                           FontSize="26"
                                           FontWeight="Bold"
                                           Foreground="#00A3E0"
                                           DockPanel.Dock="Right"
                                           VerticalAlignment="Center"/>
                            </DockPanel>"""

    new_totals = """                            <!-- 1. ยอดรวมสินค้า (Subtotal) -->
                            <DockPanel LastChildFill="False">
                                <TextBlock Text="ยอดรวมสินค้า"
                                           Foreground="#64748B"
                                           FontSize="15"
                                           FontWeight="Medium"
                                           DockPanel.Dock="Left"
                                           VerticalAlignment="Center"/>
                                <TextBlock Text="{Binding SelectedDeliverySubtotal, StringFormat={}{0:N2}}"
                                           Foreground="#1E293B"
                                           FontSize="16"
                                           FontWeight="SemiBold"
                                           DockPanel.Dock="Right"
                                           VerticalAlignment="Center"/>
                            </DockPanel>

                            <!-- 2. ค่าจัดส่ง (Shipping Fee) -->
                            <DockPanel LastChildFill="False" Margin="0,8,0,0">
                                <StackPanel Orientation="Horizontal" DockPanel.Dock="Left" VerticalAlignment="Center">
                                    <TextBlock Text="🚚 " FontSize="15" VerticalAlignment="Center"/>
                                    <TextBlock Text="ค่าจัดส่ง"
                                               Foreground="#64748B"
                                               FontSize="15"
                                               FontWeight="Medium"
                                               VerticalAlignment="Center"/>
                                </StackPanel>
                                <TextBlock Text="{Binding SelectedDeliveryShippingFee, StringFormat={}{0:N2}}"
                                           Foreground="#00A3E0"
                                           FontSize="16"
                                           FontWeight="SemiBold"
                                           DockPanel.Dock="Right"
                                           VerticalAlignment="Center"/>
                            </DockPanel>

                            <!-- 3. เส้นคั่น (Divider) -->
                            <Border Height="1" Background="#E2E8F0" Margin="0,12,0,12"/>

                            <!-- 4. ยอดรวมสุทธิทั้งสิ้น (Grand Total) -->
                            <DockPanel LastChildFill="False">
                                <TextBlock Text="ยอดรวมสุทธิทั้งสิ้น"
                                           Foreground="#0F172A"
                                           FontSize="17"
                                           FontWeight="Bold"
                                           DockPanel.Dock="Left"
                                           VerticalAlignment="Center"/>
                                <TextBlock Text="{Binding SelectedDeliveryGrandTotal, StringFormat={}{0:N2}}"
                                           FontSize="28"
                                           FontWeight="Bold"
                                           Foreground="#00A3E0"
                                           DockPanel.Dock="Right"
                                           VerticalAlignment="Center"/>
                            </DockPanel>"""
    assert old_totals in content, "old_totals not found"
    content = content.replace(old_totals, new_totals)

    # 5: Action Buttons inside Modal
    old_modal_actions = """                        <!-- 1. Leftmost: Update Delivery Status (Solid Blue with Soft Shadow) -->
                        <Button Grid.Column="0"
                                Content="🛵  อัปเดตสถานะจัดส่ง"
                                Command="{Binding UpdateDeliveryStatusCommand}"
                                Height="42"
                                Cursor="Hand">
                            <Button.Style>
                                <Style TargetType="Button">
                                    <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
                                    <Setter Property="FontSize" Value="13"/>
                                    <Setter Property="FontWeight" Value="Bold"/>
                                    <Setter Property="Foreground" Value="White"/>
                                    <Setter Property="Template">
                                        <Setter.Value>
                                            <ControlTemplate TargetType="Button">
                                                <Border x:Name="Bd" Background="#00A3E0" CornerRadius="10" Padding="8,0">
                                                    <Border.Effect>
                                                        <DropShadowEffect Color="#00A3E0" BlurRadius="8" ShadowDepth="2" Direction="270" Opacity="0.3"/>
                                                    </Border.Effect>
                                                    <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                                </Border>
                                                <ControlTemplate.Triggers>
                                                    <Trigger Property="IsMouseOver" Value="True">
                                                        <Setter TargetName="Bd" Property="Background" Value="#38BDF8"/>
                                                    </Trigger>
                                                    <Trigger Property="IsPressed" Value="True">
                                                        <Setter TargetName="Bd" Property="Background" Value="#0082B3"/>
                                                    </Trigger>
                                                </ControlTemplate.Triggers>
                                            </ControlTemplate>
                                        </Setter.Value>
                                    </Setter>
                                </Style>
                            </Button.Style>
                        </Button>

                        <!-- 2. Middle: Print Delivery Slip (White Background with Blue Outline) -->
                        <Button Grid.Column="2"
                                Content="🖨️  พิมพ์ใบจัดส่ง"
                                Command="{Binding PrintDeliverySlipCommand}"
                                CommandParameter="{Binding SelectedDeliveryOrder}"
                                Height="42"
                                Cursor="Hand">
                            <Button.Style>
                                <Style TargetType="Button">
                                    <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
                                    <Setter Property="FontSize" Value="13"/>
                                    <Setter Property="FontWeight" Value="SemiBold"/>
                                    <Setter Property="Foreground" Value="#00A3E0"/>
                                    <Setter Property="Template">
                                        <Setter.Value>
                                            <ControlTemplate TargetType="Button">
                                                <Border x:Name="Bd" Background="#FFFFFF" BorderBrush="#00A3E0" BorderThickness="1.5" CornerRadius="10" Padding="8,0">
                                                    <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                                </Border>
                                                <ControlTemplate.Triggers>
                                                    <Trigger Property="IsMouseOver" Value="True">
                                                        <Setter TargetName="Bd" Property="Background" Value="#E0F2FE"/>
                                                    </Trigger>
                                                    <Trigger Property="IsPressed" Value="True">
                                                        <Setter TargetName="Bd" Property="Background" Value="#BAE6FD"/>
                                                    </Trigger>
                                                </ControlTemplate.Triggers>
                                            </ControlTemplate>
                                        </Setter.Value>
                                    </Setter>
                                </Style>
                            </Button.Style>
                        </Button>

                        <!-- 3. Rightmost: Close Window (Soft Light Grey) -->
                        <Button Grid.Column="4"
                                Content="ปิด"
                                Command="{Binding CloseDeliveryDetailCommand}"
                                Height="42"
                                Cursor="Hand">
                            <Button.Style>
                                <Style TargetType="Button">
                                    <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
                                    <Setter Property="FontSize" Value="13"/>
                                    <Setter Property="FontWeight" Value="SemiBold"/>
                                    <Setter Property="Foreground" Value="#64748B"/>
                                    <Setter Property="Template">
                                        <Setter.Value>
                                            <ControlTemplate TargetType="Button">
                                                <Border x:Name="Bd" Background="#F1F5F9" BorderBrush="#CBD5E1" BorderThickness="1" CornerRadius="10" Padding="8,0">
                                                    <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                                </Border>
                                                <ControlTemplate.Triggers>
                                                    <Trigger Property="IsMouseOver" Value="True">
                                                        <Setter TargetName="Bd" Property="Background" Value="#E2E8F0"/>
                                                        <Setter Property="Foreground" Value="#334155"/>
                                                    </Trigger>
                                                    <Trigger Property="IsPressed" Value="True">
                                                        <Setter TargetName="Bd" Property="Background" Value="#CBD5E1"/>
                                                    </Trigger>
                                                </ControlTemplate.Triggers>
                                            </ControlTemplate>
                                        </Setter.Value>
                                    </Setter>
                                </Style>
                            </Button.Style>
                        </Button>"""

    new_modal_actions = """                        <!-- 1. Leftmost: Update Delivery Status (Solid Blue with Soft Shadow) -->
                        <Button Grid.Column="0"
                                Content="🛵  อัปเดตสถานะจัดส่ง"
                                Command="{Binding UpdateDeliveryStatusCommand}"
                                Height="46"
                                Cursor="Hand">
                            <Button.Style>
                                <Style TargetType="Button">
                                    <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
                                    <Setter Property="FontSize" Value="15"/>
                                    <Setter Property="FontWeight" Value="Bold"/>
                                    <Setter Property="Foreground" Value="White"/>
                                    <Setter Property="Template">
                                        <Setter.Value>
                                            <ControlTemplate TargetType="Button">
                                                <Border x:Name="Bd" Background="#00A3E0" CornerRadius="10" Padding="8,0">
                                                    <Border.Effect>
                                                        <DropShadowEffect Color="#00A3E0" BlurRadius="8" ShadowDepth="2" Direction="270" Opacity="0.3"/>
                                                    </Border.Effect>
                                                    <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                                </Border>
                                                <ControlTemplate.Triggers>
                                                    <Trigger Property="IsMouseOver" Value="True">
                                                        <Setter TargetName="Bd" Property="Background" Value="#38BDF8"/>
                                                    </Trigger>
                                                    <Trigger Property="IsPressed" Value="True">
                                                        <Setter TargetName="Bd" Property="Background" Value="#0082B3"/>
                                                    </Trigger>
                                                </ControlTemplate.Triggers>
                                            </ControlTemplate>
                                        </Setter.Value>
                                    </Setter>
                                </Style>
                            </Button.Style>
                        </Button>

                        <!-- 2. Middle: Print Delivery Slip (White Background with Blue Outline) -->
                        <Button Grid.Column="2"
                                Content="🖨️  พิมพ์ใบจัดส่ง"
                                Command="{Binding PrintDeliverySlipCommand}"
                                CommandParameter="{Binding SelectedDeliveryOrder}"
                                Height="46"
                                Cursor="Hand">
                            <Button.Style>
                                <Style TargetType="Button">
                                    <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
                                    <Setter Property="FontSize" Value="15"/>
                                    <Setter Property="FontWeight" Value="SemiBold"/>
                                    <Setter Property="Foreground" Value="#00A3E0"/>
                                    <Setter Property="Template">
                                        <Setter.Value>
                                            <ControlTemplate TargetType="Button">
                                                <Border x:Name="Bd" Background="#FFFFFF" BorderBrush="#00A3E0" BorderThickness="1.5" CornerRadius="10" Padding="8,0">
                                                    <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                                </Border>
                                                <ControlTemplate.Triggers>
                                                    <Trigger Property="IsMouseOver" Value="True">
                                                        <Setter TargetName="Bd" Property="Background" Value="#E0F2FE"/>
                                                    </Trigger>
                                                    <Trigger Property="IsPressed" Value="True">
                                                        <Setter TargetName="Bd" Property="Background" Value="#BAE6FD"/>
                                                    </Trigger>
                                                </ControlTemplate.Triggers>
                                            </ControlTemplate>
                                        </Setter.Value>
                                    </Setter>
                                </Style>
                            </Button.Style>
                        </Button>

                        <!-- 3. Rightmost: Close Window (Soft Light Grey) -->
                        <Button Grid.Column="4"
                                Content="ปิด"
                                Command="{Binding CloseDeliveryDetailCommand}"
                                Height="46"
                                Cursor="Hand">
                            <Button.Style>
                                <Style TargetType="Button">
                                    <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
                                    <Setter Property="FontSize" Value="15"/>
                                    <Setter Property="FontWeight" Value="SemiBold"/>
                                    <Setter Property="Foreground" Value="#64748B"/>
                                    <Setter Property="Template">
                                        <Setter.Value>
                                            <ControlTemplate TargetType="Button">
                                                <Border x:Name="Bd" Background="#F1F5F9" BorderBrush="#CBD5E1" BorderThickness="1" CornerRadius="10" Padding="8,0">
                                                    <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                                </Border>
                                                <ControlTemplate.Triggers>
                                                    <Trigger Property="IsMouseOver" Value="True">
                                                        <Setter TargetName="Bd" Property="Background" Value="#E2E8F0"/>
                                                        <Setter Property="Foreground" Value="#334155"/>
                                                    </Trigger>
                                                    <Trigger Property="IsPressed" Value="True">
                                                        <Setter TargetName="Bd" Property="Background" Value="#CBD5E1"/>
                                                    </Trigger>
                                                </ControlTemplate.Triggers>
                                            </ControlTemplate>
                                        </Setter.Value>
                                    </Setter>
                                </Style>
                            </Button.Style>
                        </Button>"""
    assert old_modal_actions in content, "old_modal_actions not found"
    content = content.replace(old_modal_actions, new_modal_actions)

    # 16. Status Update Modal Overlay
    old_status_modal = """                    <!-- Header -->
                    <Grid Margin="0,0,0,20">
                        <TextBlock Text="🚚 อัปเดตสถานะการจัดส่งสินค้า" FontSize="16" FontWeight="Bold" Foreground="#2D3748" VerticalAlignment="Center"/>
                        
                        <!-- Circular Soft Pink Close Button ('✕') -->
                        <Button HorizontalAlignment="Right"
                                Command="{Binding CloseManageModalCommand}"
                                Style="{StaticResource ModalCloseButtonStyle}"/>
                    </Grid>

                    <!-- Form Content -->
                    <Border Background="#E0F2FE" BorderBrush="#BAE6FD" BorderThickness="1" CornerRadius="12" Padding="16" Margin="0,0,0,20">
                        <Grid>
                            <Grid.RowDefinitions>
                                <RowDefinition Height="Auto"/>
                                <RowDefinition Height="Auto"/>
                                <RowDefinition Height="Auto"/>
                                <RowDefinition Height="Auto"/>
                                <RowDefinition Height="Auto"/>
                            </Grid.RowDefinitions>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="100"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>

                            <TextBlock Grid.Row="0" Grid.Column="0" Text="เลขที่บิล" FontSize="13" Foreground="#718096" VerticalAlignment="Center" Margin="0,0,0,10"/>
                            <TextBlock Grid.Row="0" Grid.Column="1" Text="{Binding SelectedDeliveryOrder.RefNo}" FontSize="13" FontWeight="Bold" Foreground="#2D3748" VerticalAlignment="Center" Margin="0,0,0,10"/>

                            <TextBlock Grid.Row="1" Grid.Column="0" Text="ชื่อลูกค้า" FontSize="13" Foreground="#718096" VerticalAlignment="Center" Margin="0,0,0,10"/>
                            <TextBlock Grid.Row="1" Grid.Column="1" Text="{Binding SelectedDeliveryOrder.CustomerName}" FontSize="13" FontWeight="SemiBold" Foreground="#2D3748" VerticalAlignment="Center" Margin="0,0,0,10"/>

                            <TextBlock Grid.Row="2" Grid.Column="0" Text="เบอร์โทร" FontSize="13" Foreground="#718096" VerticalAlignment="Center" Margin="0,0,0,10"/>
                            <TextBlock Grid.Row="2" Grid.Column="1" Text="{Binding SelectedDeliveryOrder.CustomerPhone}" FontSize="13" FontWeight="SemiBold" Foreground="#0B2545" VerticalAlignment="Center" Margin="0,0,0,10"/>

                            <TextBlock Grid.Row="3" Grid.Column="0" Text="ที่อยู่จัดส่ง" FontSize="13" Foreground="#718096" VerticalAlignment="Top" Margin="0,4,0,10"/>
                            <TextBlock Grid.Row="3" Grid.Column="1" Text="{Binding SelectedDeliveryOrder.CustomerAddress}" FontSize="12.5" Foreground="#2D3748" TextWrapping="Wrap" Margin="0,4,0,10"/>

                            <TextBlock Grid.Row="4" Grid.Column="0" Text="สถานะจัดส่ง" FontSize="13" FontWeight="Bold" Foreground="#2D3748" VerticalAlignment="Center"/>
                            <Border Grid.Row="4" Grid.Column="1" Background="#FFFFFF" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="8">
                                <ComboBox SelectedValue="{Binding SelectedDeliveryStatus, Mode=TwoWay}" SelectedValuePath="Content" Style="{StaticResource LightComboBoxStyle}" BorderThickness="0" Background="Transparent" Foreground="#2D3748">
                                    <ComboBoxItem Content="รอจัดส่ง"/>
                                    <ComboBoxItem Content="กำลังจัดส่ง"/>
                                    <ComboBoxItem Content="จัดส่งสำเร็จ"/>
                                    <ComboBoxItem Content="ยกเลิกรายการ"/>
                                </ComboBox>
                            </Border>
                        </Grid>
                    </Border>

                    <!-- Save & Cancel Buttons -->
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="12"/>
                            <ColumnDefinition Width="*"/>
                        </Grid.ColumnDefinitions>

                        <Button Grid.Column="0"
                                Command="{Binding CloseManageModalCommand}"
                                Content="ยกเลิก"
                                Style="{StaticResource RoseBtnStyle}"
                                Background="#E0F2FE"
                                Foreground="#718096"
                                BorderBrush="#BAE6FD"
                                BorderThickness="1"
                                Height="42"/>

                        <Button Grid.Column="2"
                                Command="{Binding SaveDeliveryStatusCommand}"
                                Content="บันทึกการอัปเดต"
                                Style="{StaticResource RoseBtnStyle}"
                                Background="#00A3E0"
                                Foreground="White"
                                Height="42"/>
                    </Grid>"""

    new_status_modal = """                    <!-- Header -->
                    <Grid Margin="0,0,0,20">
                        <TextBlock Text="🚚 อัปเดตสถานะการจัดส่งสินค้า" FontSize="20" FontWeight="Bold" Foreground="#2D3748" VerticalAlignment="Center"/>
                        
                        <!-- Circular Soft Pink Close Button ('✕') -->
                        <Button HorizontalAlignment="Right"
                                Command="{Binding CloseManageModalCommand}"
                                Style="{StaticResource ModalCloseButtonStyle}"/>
                    </Grid>

                    <!-- Form Content -->
                    <Border Background="#E0F2FE" BorderBrush="#BAE6FD" BorderThickness="1" CornerRadius="12" Padding="16" Margin="0,0,0,20">
                        <Grid>
                            <Grid.RowDefinitions>
                                <RowDefinition Height="Auto"/>
                                <RowDefinition Height="Auto"/>
                                <RowDefinition Height="Auto"/>
                                <RowDefinition Height="Auto"/>
                                <RowDefinition Height="Auto"/>
                            </Grid.RowDefinitions>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="110"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>

                            <TextBlock Grid.Row="0" Grid.Column="0" Text="เลขที่บิล" FontSize="15" Foreground="#718096" VerticalAlignment="Center" Margin="0,0,0,10"/>
                            <TextBlock Grid.Row="0" Grid.Column="1" Text="{Binding SelectedDeliveryOrder.RefNo}" FontSize="15" FontWeight="Bold" Foreground="#2D3748" VerticalAlignment="Center" Margin="0,0,0,10"/>

                            <TextBlock Grid.Row="1" Grid.Column="0" Text="ชื่อลูกค้า" FontSize="15" Foreground="#718096" VerticalAlignment="Center" Margin="0,0,0,10"/>
                            <TextBlock Grid.Row="1" Grid.Column="1" Text="{Binding SelectedDeliveryOrder.CustomerName}" FontSize="15" FontWeight="SemiBold" Foreground="#2D3748" VerticalAlignment="Center" Margin="0,0,0,10"/>

                            <TextBlock Grid.Row="2" Grid.Column="0" Text="เบอร์โทร" FontSize="15" Foreground="#718096" VerticalAlignment="Center" Margin="0,0,0,10"/>
                            <TextBlock Grid.Row="2" Grid.Column="1" Text="{Binding SelectedDeliveryOrder.CustomerPhone}" FontSize="15" FontWeight="SemiBold" Foreground="#0B2545" VerticalAlignment="Center" Margin="0,0,0,10"/>

                            <TextBlock Grid.Row="3" Grid.Column="0" Text="ที่อยู่จัดส่ง" FontSize="15" Foreground="#718096" VerticalAlignment="Top" Margin="0,4,0,10"/>
                            <TextBlock Grid.Row="3" Grid.Column="1" Text="{Binding SelectedDeliveryOrder.CustomerAddress}" FontSize="14.5" Foreground="#2D3748" TextWrapping="Wrap" Margin="0,4,0,10"/>

                            <TextBlock Grid.Row="4" Grid.Column="0" Text="สถานะจัดส่ง" FontSize="15" FontWeight="Bold" Foreground="#2D3748" VerticalAlignment="Center"/>
                            <Border Grid.Row="4" Grid.Column="1" Background="#FFFFFF" BorderBrush="#BAE6FD" BorderThickness="1.5" CornerRadius="8">
                                <ComboBox SelectedValue="{Binding SelectedDeliveryStatus, Mode=TwoWay}" SelectedValuePath="Content" Style="{StaticResource LightComboBoxStyle}" Height="44" BorderThickness="0" Background="Transparent" Foreground="#2D3748">
                                    <ComboBoxItem Content="รอจัดส่ง"/>
                                    <ComboBoxItem Content="กำลังจัดส่ง"/>
                                    <ComboBoxItem Content="จัดส่งสำเร็จ"/>
                                    <ComboBoxItem Content="ยกเลิกรายการ"/>
                                </ComboBox>
                            </Border>
                        </Grid>
                    </Border>

                    <!-- Save & Cancel Buttons -->
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="12"/>
                            <ColumnDefinition Width="*"/>
                        </Grid.ColumnDefinitions>

                        <Button Grid.Column="0"
                                Command="{Binding CloseManageModalCommand}"
                                Content="ยกเลิก"
                                Style="{StaticResource RoseBtnStyle}"
                                Background="#E0F2FE"
                                Foreground="#718096"
                                BorderBrush="#BAE6FD"
                                BorderThickness="1"
                                FontSize="16"
                                Height="46"/>

                        <Button Grid.Column="2"
                                Command="{Binding SaveDeliveryStatusCommand}"
                                Content="บันทึกการอัปเดต"
                                Style="{StaticResource RoseBtnStyle}"
                                Background="#00A3E0"
                                Foreground="White"
                                FontSize="16"
                                Height="46"/>
                    </Grid>"""
    assert old_status_modal in content, "old_status_modal not found"
    content = content.replace(old_status_modal, new_status_modal)

    with open(path, "w", encoding="utf-8") as f:
        f.write(content)
    print("Successfully updated DeliveryView.xaml")

if __name__ == "__main__":
    update_delivery_view()
