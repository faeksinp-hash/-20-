import re

def update_claim_view():
    path = r"d:\ShopManagement_หลัก\MatchaShop\Views\ClaimView.xaml"
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

    # 2. Update RoseBtnStyle
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

    # 3. Update Detail Modal Action Button Styles
    old_modal_btns = """        <!-- Detail Modal Action Button Styles -->
        <Style x:Key="ApproveModalBtnStyle" TargetType="Button">
            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="Background" Value="#D1FAE5"/>
            <Setter Property="Foreground" Value="#065F46"/>
            <Setter Property="FontSize" Value="14"/>
            <Setter Property="FontWeight" Value="SemiBold"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Cursor" Value="Hand"/>
            <Setter Property="Height" Value="42"/>"""
    new_modal_btns = """        <!-- Detail Modal Action Button Styles -->
        <Style x:Key="ApproveModalBtnStyle" TargetType="Button">
            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="Background" Value="#D1FAE5"/>
            <Setter Property="Foreground" Value="#065F46"/>
            <Setter Property="FontSize" Value="16"/>
            <Setter Property="FontWeight" Value="SemiBold"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Cursor" Value="Hand"/>
            <Setter Property="Height" Value="46"/>"""
    assert old_modal_btns in content, "old_modal_btns not found"
    content = content.replace(old_modal_btns, new_modal_btns)

    content = content.replace("""        <Style x:Key="ReplaceModalBtnStyle" TargetType="Button">
            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="Background" Value="#F3E8FF"/>
            <Setter Property="Foreground" Value="#6B21A8"/>
            <Setter Property="FontSize" Value="14"/>
            <Setter Property="FontWeight" Value="SemiBold"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Cursor" Value="Hand"/>
            <Setter Property="Height" Value="42"/>""",
            """        <Style x:Key="ReplaceModalBtnStyle" TargetType="Button">
            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="Background" Value="#F3E8FF"/>
            <Setter Property="Foreground" Value="#6B21A8"/>
            <Setter Property="FontSize" Value="16"/>
            <Setter Property="FontWeight" Value="SemiBold"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Cursor" Value="Hand"/>
            <Setter Property="Height" Value="46"/>""")

    content = content.replace("""        <Style x:Key="RejectModalBtnStyle" TargetType="Button">
            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="Background" Value="#FEE2E2"/>
            <Setter Property="Foreground" Value="#991B1B"/>
            <Setter Property="FontSize" Value="14"/>
            <Setter Property="FontWeight" Value="SemiBold"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Cursor" Value="Hand"/>
            <Setter Property="Height" Value="42"/>""",
            """        <Style x:Key="RejectModalBtnStyle" TargetType="Button">
            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="Background" Value="#FEE2E2"/>
            <Setter Property="Foreground" Value="#991B1B"/>
            <Setter Property="FontSize" Value="16"/>
            <Setter Property="FontWeight" Value="SemiBold"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Cursor" Value="Hand"/>
            <Setter Property="Height" Value="46"/>""")

    content = content.replace("""        <Style x:Key="CloseModalBtnStyle" TargetType="Button">
            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="Background" Value="#E0F2FE"/>
            <Setter Property="Foreground" Value="#0369A1"/>
            <Setter Property="BorderBrush" Value="#BAE6FD"/>
            <Setter Property="BorderThickness" Value="1"/>
            <Setter Property="FontSize" Value="14"/>
            <Setter Property="FontWeight" Value="SemiBold"/>
            <Setter Property="Cursor" Value="Hand"/>
            <Setter Property="Height" Value="42"/>""",
            """        <Style x:Key="CloseModalBtnStyle" TargetType="Button">
            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="Background" Value="#E0F2FE"/>
            <Setter Property="Foreground" Value="#0369A1"/>
            <Setter Property="BorderBrush" Value="#BAE6FD"/>
            <Setter Property="BorderThickness" Value="1"/>
            <Setter Property="FontSize" Value="16"/>
            <Setter Property="FontWeight" Value="SemiBold"/>
            <Setter Property="Cursor" Value="Hand"/>
            <Setter Property="Height" Value="46"/>""")

    # 4. ViewDetailsButtonStyle
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
            <Setter Property="Height"          Value="28"/>"""
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
            <Setter Property="Padding"         Value="10,2"/>
            <Setter Property="MinWidth"        Value="110"/>"""
    assert old_view_btn in content, "old_view_btn not found"
    content = content.replace(old_view_btn, new_view_btn)

    # 5. DataGrid Header & Row styles
    content = content.replace("""        <Style x:Key="ClaimColumnHeaderStyle" TargetType="DataGridColumnHeader">
            <Setter Property="Background"              Value="#F1F5F9"/>
            <Setter Property="Foreground"              Value="#000000"/>
            <Setter Property="FontWeight"              Value="Bold"/>
            <Setter Property="FontSize"                Value="16"/>
            <Setter Property="Height"                  Value="46"/>""",
            """        <Style x:Key="ClaimColumnHeaderStyle" TargetType="DataGridColumnHeader">
            <Setter Property="Background"              Value="#F1F5F9"/>
            <Setter Property="Foreground"              Value="#000000"/>
            <Setter Property="FontWeight"              Value="Bold"/>
            <Setter Property="FontSize"                Value="16"/>
            <Setter Property="Height"                  Value="48"/>""")

    content = content.replace("""        <Style x:Key="ClaimRowStyle" TargetType="DataGridRow">
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Foreground" Value="#2D3748"/>
            <Setter Property="FontSize"   Value="13"/>""",
            """        <Style x:Key="ClaimRowStyle" TargetType="DataGridRow">
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Foreground" Value="#2D3748"/>
            <Setter Property="FontSize"   Value="15"/>
            <Setter Property="MinHeight"  Value="48"/>""")

    # 6. ColCenter / ColLeft / ColCenterBold
    old_col_helpers = """        <!-- Column Text Helpers -->
        <Style x:Key="ColCenter" TargetType="TextBlock">
            <Setter Property="HorizontalAlignment" Value="Center"/>
            <Setter Property="VerticalAlignment"   Value="Center"/>
            <Setter Property="Foreground"          Value="#1E293B"/>
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
        </Style>"""
    new_col_helpers = """        <!-- Column Text Helpers -->
        <Style x:Key="ColCenter" TargetType="TextBlock">
            <Setter Property="HorizontalAlignment" Value="Center"/>
            <Setter Property="VerticalAlignment"   Value="Center"/>
            <Setter Property="Foreground"          Value="#1E293B"/>
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
        </Style>"""
    assert old_col_helpers in content, "old_col_helpers not found"
    content = content.replace(old_col_helpers, new_col_helpers)

    # 7. Header Subtitle
    content = content.replace("""                        <TextBlock Text="จัดการการเคลมสินค้า — ระบบจัดการร้าน พอใจ"
                                   FontSize="13"
                                   Foreground="#718096"
                                   Margin="0"/>""",
                              """                        <TextBlock Text="จัดการการเคลมสินค้า — ระบบจัดการร้าน พอใจ"
                                   FontSize="15"
                                   Foreground="#718096"
                                   Margin="0"/>""")

    # 8. KPI Summary Cards (3 cards)
    old_kpi1 = """                        <!-- Icon + Label -->
                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="36" Height="36" CornerRadius="10" Background="#FEF3C7" Margin="0,0,10,0">
                                <TextBlock Text="⏳" FontSize="18" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="เคลมรอดำเนินการ" FontSize="13" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <!-- Value -->
                        <TextBlock Grid.Row="1"
                                   Text="{Binding PendingClaimsCount}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#D69E2E"
                                   Margin="0,0,0,6"/>
                        <!-- Sub-label -->
                        <TextBlock Grid.Row="2"
                                   Text="รอตรวจสอบ/ดำเนินการ"
                                   FontSize="11"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>"""
    new_kpi1 = """                        <!-- Icon + Label -->
                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="40" Height="40" CornerRadius="10" Background="#FEF3C7" Margin="0,0,10,0">
                                <TextBlock Text="⏳" FontSize="20" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="เคลมรอดำเนินการ" FontSize="16" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <!-- Value -->
                        <TextBlock Grid.Row="1"
                                   Text="{Binding PendingClaimsCount}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#D69E2E"
                                   Margin="0,0,0,6"/>
                        <!-- Sub-label -->
                        <TextBlock Grid.Row="2"
                                   Text="รอตรวจสอบ/ดำเนินการ"
                                   FontSize="14"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>"""
    assert old_kpi1 in content, "old_kpi1 not found"
    content = content.replace(old_kpi1, new_kpi1)

    old_kpi2 = """                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="36" Height="36" CornerRadius="10" Background="#EBF8FF" Margin="0,0,10,0">
                                <TextBlock Text="🔄" FontSize="18" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="อนุมัติเคลม / เปลี่ยนสินค้า" FontSize="13" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <TextBlock Grid.Row="1"
                                   Text="{Binding ApprovedClaimsCount}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#2B6CB0"
                                   Margin="0,0,0,6"/>
                        <TextBlock Grid.Row="2"
                                   Text="ได้รับของใหม่ / อนุมัติการคืนเงิน"
                                   FontSize="11"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>"""
    new_kpi2 = """                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="40" Height="40" CornerRadius="10" Background="#EBF8FF" Margin="0,0,10,0">
                                <TextBlock Text="🔄" FontSize="20" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="อนุมัติเคลม / เปลี่ยนสินค้า" FontSize="16" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <TextBlock Grid.Row="1"
                                   Text="{Binding ApprovedClaimsCount}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#2B6CB0"
                                   Margin="0,0,0,6"/>
                        <TextBlock Grid.Row="2"
                                   Text="ได้รับของใหม่ / อนุมัติการคืนเงิน"
                                   FontSize="14"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>"""
    assert old_kpi2 in content, "old_kpi2 not found"
    content = content.replace(old_kpi2, new_kpi2)

    old_kpi3 = """                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="36" Height="36" CornerRadius="10" Background="#D1FAE5" Margin="0,0,10,0">
                                <TextBlock Text="✅" FontSize="18" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="เคลมสำเร็จทั้งหมด" FontSize="13" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <TextBlock Grid.Row="1"
                                   Text="{Binding CompletedClaimsCount}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#059669"
                                   Margin="0,0,0,6"/>
                        <TextBlock Grid.Row="2"
                                   Text="ดำเนินการเสร็จสิ้นเรียบร้อย"
                                   FontSize="11"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>"""
    new_kpi3 = """                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="40" Height="40" CornerRadius="10" Background="#D1FAE5" Margin="0,0,10,0">
                                <TextBlock Text="✅" FontSize="20" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="เคลมสำเร็จทั้งหมด" FontSize="16" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <TextBlock Grid.Row="1"
                                   Text="{Binding CompletedClaimsCount}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#059669"
                                   Margin="0,0,0,6"/>
                        <TextBlock Grid.Row="2"
                                   Text="ดำเนินการเสร็จสิ้นเรียบร้อย"
                                   FontSize="14"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>"""
    assert old_kpi3 in content, "old_kpi3 not found"
    content = content.replace(old_kpi3, new_kpi3)

    # 9. Filter / Search Bar
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
                            <TextBox x:Name="ClaimSearchBox"
                                     Text="{Binding ClaimSearchKeyword, UpdateSourceTrigger=PropertyChanged}"
                                     Style="{StaticResource LightTextBoxStyle}"
                                     Height="40"
                                     Padding="38,0,12,0"
                                     VerticalContentAlignment="Center"
                                     FontSize="13"/>
                            <!-- Placeholder -->
                            <TextBlock Text="ค้นหา เลขที่เคลม, ชื่อสินค้า, ลูกค้า..."
                                       Foreground="#A0AEC0"
                                       FontSize="13"
                                       VerticalAlignment="Center"
                                       Margin="38,0,12,0"
                                       IsHitTestVisible="False">
                                <TextBlock.Style>
                                    <Style TargetType="TextBlock">
                                        <Setter Property="Visibility" Value="Collapsed"/>
                                        <Style.Triggers>
                                            <DataTrigger Binding="{Binding Text, ElementName=ClaimSearchBox}" Value="">
                                                <Setter Property="Visibility" Value="Visible"/>
                                            </DataTrigger>
                                        </Style.Triggers>
                                    </Style>
                                </TextBlock.Style>
                            </TextBlock>
                        </Grid>
                    </Border>

                    <!-- Claim Type Dropdown -->
                    <Border Grid.Column="2"
                            Background="#E0F2FE"
                            BorderBrush="#BAE6FD"
                            BorderThickness="1.5"
                            CornerRadius="8">
                        <ComboBox ItemsSource="{Binding ClaimTypeOptions}"
                                  SelectedItem="{Binding ClaimType}"
                                  Style="{StaticResource LightComboBoxStyle}"
                                  BorderThickness="0"
                                  Background="Transparent"/>
                    </Border>

                    <!-- Status Filter Dropdown -->
                    <Border Grid.Column="4"
                            Background="#E0F2FE"
                            BorderBrush="#BAE6FD"
                            BorderThickness="1.5"
                            CornerRadius="8">
                        <ComboBox ItemsSource="{Binding ClaimStatusOptions}"
                                  SelectedItem="{Binding ClaimStatusFilter}"
                                  Style="{StaticResource LightComboBoxStyle}"
                                  BorderThickness="0"
                                  Background="Transparent"/>
                    </Border>

                    <!-- Search Button -->
                    <Button Grid.Column="6"
                            Content="ค้นหา"
                            Command="{Binding SearchClaimCommand}"
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

                    <!-- Add Claim Button -->
                    <Button Grid.Column="8"
                            Content="➕ บันทึกเคลม"
                            Command="{Binding OpenAddModalCommand}" IsEnabled="{Binding IsReadOnlyMode, Converter={StaticResource InverseBoolConverter}}"
                            Style="{StaticResource RoseBtnStyle}"
                            Background="#00A6E6"
                            Foreground="White"
                            Padding="16,0"
                            Height="40"/>

                    <!-- Clear / Reset Button -->
                    <Button Grid.Column="10"
                            Content="ล้าง"
                            Command="{Binding ClearClaimFilterCommand}"
                            Style="{StaticResource SecondaryBtnStyle}"
                            Padding="14,0"
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
                            <TextBox x:Name="ClaimSearchBox"
                                     Text="{Binding ClaimSearchKeyword, UpdateSourceTrigger=PropertyChanged}"
                                     Style="{StaticResource LightTextBoxStyle}"
                                     Height="44"
                                     Padding="38,0,12,0"
                                     VerticalContentAlignment="Center"
                                     FontSize="16"/>
                            <!-- Placeholder -->
                            <TextBlock Text="ค้นหา เลขที่เคลม, ชื่อสินค้า, ลูกค้า..."
                                       Foreground="#A0AEC0"
                                       FontSize="15"
                                       VerticalAlignment="Center"
                                       Margin="38,0,12,0"
                                       IsHitTestVisible="False">
                                <TextBlock.Style>
                                    <Style TargetType="TextBlock">
                                        <Setter Property="Visibility" Value="Collapsed"/>
                                        <Style.Triggers>
                                            <DataTrigger Binding="{Binding Text, ElementName=ClaimSearchBox}" Value="">
                                                <Setter Property="Visibility" Value="Visible"/>
                                            </DataTrigger>
                                        </Style.Triggers>
                                    </Style>
                                </TextBlock.Style>
                            </TextBlock>
                        </Grid>
                    </Border>

                    <!-- Claim Type Dropdown -->
                    <Border Grid.Column="2"
                            Background="#E0F2FE"
                            BorderBrush="#BAE6FD"
                            BorderThickness="1.5"
                            CornerRadius="8">
                        <ComboBox ItemsSource="{Binding ClaimTypeOptions}"
                                  SelectedItem="{Binding ClaimType}"
                                  Style="{StaticResource LightComboBoxStyle}"
                                  Height="44"
                                  FontSize="15"
                                  BorderThickness="0"
                                  Background="Transparent"/>
                    </Border>

                    <!-- Status Filter Dropdown -->
                    <Border Grid.Column="4"
                            Background="#E0F2FE"
                            BorderBrush="#BAE6FD"
                            BorderThickness="1.5"
                            CornerRadius="8">
                        <ComboBox ItemsSource="{Binding ClaimStatusOptions}"
                                  SelectedItem="{Binding ClaimStatusFilter}"
                                  Style="{StaticResource LightComboBoxStyle}"
                                  Height="44"
                                  FontSize="15"
                                  BorderThickness="0"
                                  Background="Transparent"/>
                    </Border>

                    <!-- Search Button -->
                    <Button Grid.Column="6"
                            Content="ค้นหา"
                            Command="{Binding SearchClaimCommand}"
                            Background="#1E88E5"
                            Cursor="Hand"
                            Padding="22,0"
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

                    <!-- Add Claim Button -->
                    <Button Grid.Column="8"
                            Content="➕ บันทึกเคลม"
                            Command="{Binding OpenAddModalCommand}" IsEnabled="{Binding IsReadOnlyMode, Converter={StaticResource InverseBoolConverter}}"
                            Style="{StaticResource RoseBtnStyle}"
                            Background="#00A6E6"
                            Foreground="White"
                            Padding="16,0"
                            Height="44"/>

                    <!-- Clear / Reset Button -->
                    <Button Grid.Column="10"
                            Content="ล้าง"
                            Command="{Binding ClearClaimFilterCommand}"
                            Style="{StaticResource SecondaryBtnStyle}"
                            FontSize="16"
                            Padding="14,0"
                            Height="44"/>"""
    assert old_filter in content, "old_filter not found"
    content = content.replace(old_filter, new_filter)

    # 10. DataGrid Cells
    # Col 4
    old_dg_col4 = """                                        <StackPanel VerticalAlignment="Center" Margin="4,0">
                                            <TextBlock Text="{Binding CustomerName}" FontWeight="Bold" Foreground="#2D3748" FontSize="13" TextTrimming="CharacterEllipsis"/>
                                            <StackPanel Orientation="Horizontal" Margin="0,2,0,0">
                                                <TextBlock Text="บิล: " FontSize="11" Foreground="#718096"/>
                                                <TextBlock Text="{Binding SalesOrderRefNo}" FontSize="11" Foreground="#718096"/>
                                                <TextBlock Text="{Binding StockInRefNo}" FontSize="11" Foreground="#718096"/>
                                            </StackPanel>
                                        </StackPanel>"""
    new_dg_col4 = """                                        <StackPanel VerticalAlignment="Center" Margin="4,0">
                                            <TextBlock Text="{Binding CustomerName}" FontWeight="Bold" Foreground="#2D3748" FontSize="14.5" TextTrimming="CharacterEllipsis"/>
                                            <StackPanel Orientation="Horizontal" Margin="0,2,0,0">
                                                <TextBlock Text="บิล: " FontSize="13" Foreground="#718096"/>
                                                <TextBlock Text="{Binding SalesOrderRefNo}" FontSize="13" Foreground="#718096"/>
                                                <TextBlock Text="{Binding StockInRefNo}" FontSize="13" Foreground="#718096"/>
                                            </StackPanel>
                                        </StackPanel>"""
    assert old_dg_col4 in content, "old_dg_col4 not found"
    content = content.replace(old_dg_col4, new_dg_col4)

    # Col 5
    old_dg_col5 = """                                        <StackPanel VerticalAlignment="Center" Margin="4,0">
                                            <TextBlock Text="{Binding ProductName}" FontWeight="SemiBold" Foreground="#2D3748" FontSize="13" TextTrimming="CharacterEllipsis"/>
                                            <TextBlock Text="{Binding ProductCode, StringFormat='รหัส: {0}'}" FontSize="11" Foreground="#718096"/>
                                        </StackPanel>"""
    new_dg_col5 = """                                        <StackPanel VerticalAlignment="Center" Margin="4,0">
                                            <TextBlock Text="{Binding ProductName}" FontWeight="SemiBold" Foreground="#2D3748" FontSize="14.5" TextTrimming="CharacterEllipsis"/>
                                            <TextBlock Text="{Binding ProductCode, StringFormat='รหัส: {0}'}" FontSize="13" Foreground="#718096"/>
                                        </StackPanel>"""
    assert old_dg_col5 in content, "old_dg_col5 not found"
    content = content.replace(old_dg_col5, new_dg_col5)

    # Bottom status bar
    content = content.replace("""                        <TextBlock Text="{Binding Claims.Count, StringFormat=แสดงทั้งหมด {0} รายการเคลมสินค้า}"
                                   Foreground="#718096"
                                   FontSize="12"
                                   VerticalAlignment="Center"
                                   DockPanel.Dock="Left"
                                   Margin="0"/>""",
                              """                        <TextBlock Text="{Binding Claims.Count, StringFormat=แสดงทั้งหมด {0} รายการเคลมสินค้า}"
                                   Foreground="#718096"
                                   FontSize="14"
                                   VerticalAlignment="Center"
                                   DockPanel.Dock="Left"
                                   Margin="0"/>""")

    # 11. Modal Overlay: Product Claim Details
    old_detail_card = """                            <!-- Customer Name & Phone -->
                            <StackPanel Grid.Row="0" Grid.Column="0" Margin="0,0,0,10">
                                <TextBlock Text="ผู้แจ้งเคลม / ชื่อลูกค้า" FontSize="13" Foreground="#718096" Margin="0,0,0,3"/>
                                <TextBlock Text="{Binding SelectedClaim.CustomerName}"
                                           FontSize="15.5" FontWeight="Bold" Foreground="#2D3748"/>
                                <TextBlock Text="{Binding SelectedClaim.CustomerPhone, StringFormat='📞 {0}'}"
                                           FontSize="14" Foreground="#0B2545" Margin="0,3,0,0"/>
                            </StackPanel>

                            <!-- Claim Action / Method Badge (รูปแบบการเคลม) -->
                            <StackPanel Grid.Row="0" Grid.Column="1" Margin="0,0,0,10" HorizontalAlignment="Right">
                                <TextBlock Text="รูปแบบการเคลม" FontSize="13" Foreground="#718096" Margin="0,0,0,3"/>
                                <Border Background="#E0F2FE" BorderBrush="#BAE6FD" BorderThickness="1" CornerRadius="6" Padding="10,4">
                                    <TextBlock Text="{Binding SelectedClaim.ClaimAction}"
                                               FontSize="13.5" FontWeight="Bold" Foreground="#0369A1"/>
                                </Border>
                            </StackPanel>

                            <!-- Ref Receipt / StockIn No -->
                            <StackPanel Grid.Row="1" Grid.Column="0">
                                <TextBlock Text="เลขที่ใบเสร็จอ้างอิง" FontSize="13" Foreground="#718096" Margin="0,0,0,3"/>
                                <TextBlock Text="{Binding SelectedClaim.SalesOrderRefNo}"
                                           FontSize="14" FontWeight="Bold" Foreground="#2D3748"/>
                                <TextBlock Text="{Binding SelectedClaim.StockInRefNo}"
                                           FontSize="14" FontWeight="Bold" Foreground="#2D3748"/>
                            </StackPanel>

                            <!-- Date / Time -->
                            <StackPanel Grid.Row="1" Grid.Column="1" HorizontalAlignment="Right">
                                <TextBlock Text="วันที่แจ้งเคลม" FontSize="13" Foreground="#718096" Margin="0,0,0,3"/>
                                <TextBlock Text="{Binding SelectedClaim.CreatedDate, StringFormat=dd/MM/yyyy HH:mm}"
                                           FontSize="14" FontWeight="SemiBold" Foreground="#2D3748"/>
                            </StackPanel>"""

    new_detail_card = """                            <!-- Customer Name & Phone -->
                            <StackPanel Grid.Row="0" Grid.Column="0" Margin="0,0,0,10">
                                <TextBlock Text="ผู้แจ้งเคลม / ชื่อลูกค้า" FontSize="13.5" Foreground="#718096" Margin="0,0,0,3"/>
                                <TextBlock Text="{Binding SelectedClaim.CustomerName}"
                                           FontSize="16" FontWeight="Bold" Foreground="#2D3748"/>
                                <TextBlock Text="{Binding SelectedClaim.CustomerPhone, StringFormat='📞 {0}'}"
                                           FontSize="15" Foreground="#0B2545" Margin="0,3,0,0"/>
                            </StackPanel>

                            <!-- Claim Action / Method Badge (รูปแบบการเคลม) -->
                            <StackPanel Grid.Row="0" Grid.Column="1" Margin="0,0,0,10" HorizontalAlignment="Right">
                                <TextBlock Text="รูปแบบการเคลม" FontSize="13.5" Foreground="#718096" Margin="0,0,0,3"/>
                                <Border Background="#E0F2FE" BorderBrush="#BAE6FD" BorderThickness="1" CornerRadius="6" Padding="10,4">
                                    <TextBlock Text="{Binding SelectedClaim.ClaimAction}"
                                               FontSize="14" FontWeight="Bold" Foreground="#0369A1"/>
                                </Border>
                            </StackPanel>

                            <!-- Ref Receipt / StockIn No -->
                            <StackPanel Grid.Row="1" Grid.Column="0">
                                <TextBlock Text="เลขที่ใบเสร็จอ้างอิง" FontSize="13.5" Foreground="#718096" Margin="0,0,0,3"/>
                                <TextBlock Text="{Binding SelectedClaim.SalesOrderRefNo}"
                                           FontSize="15" FontWeight="Bold" Foreground="#2D3748"/>
                                <TextBlock Text="{Binding SelectedClaim.StockInRefNo}"
                                           FontSize="15" FontWeight="Bold" Foreground="#2D3748"/>
                            </StackPanel>

                            <!-- Date / Time -->
                            <StackPanel Grid.Row="1" Grid.Column="1" HorizontalAlignment="Right">
                                <TextBlock Text="วันที่แจ้งเคลม" FontSize="13.5" Foreground="#718096" Margin="0,0,0,3"/>
                                <TextBlock Text="{Binding SelectedClaim.CreatedDate, StringFormat=dd/MM/yyyy HH:mm}"
                                           FontSize="15" FontWeight="SemiBold" Foreground="#2D3748"/>
                            </StackPanel>"""
    assert old_detail_card in content, "old_detail_card not found"
    content = content.replace(old_detail_card, new_detail_card)

    # Product Details Box & Defect / Reason Box inside Detail Modal
    old_product_info_box = """                                <StackPanel>
                                    <TextBlock Text="📦 รายละเอียดสินค้าเคลม" FontSize="15" FontWeight="Bold" Foreground="#2D3748" Margin="0,0,0,10"/>
                                    <Grid Margin="0,0,0,6">
                                        <TextBlock Text="ชื่อสินค้า:" Foreground="#718096" FontSize="14"/>
                                        <TextBlock Text="{Binding SelectedClaim.ProductName}" Foreground="#2D3748" FontWeight="Bold" FontSize="15" HorizontalAlignment="Right" TextTrimming="CharacterEllipsis"/>
                                    </Grid>
                                    <Grid Margin="0,0,0,6">
                                        <TextBlock Text="รหัสสินค้า:" Foreground="#718096" FontSize="14"/>
                                        <TextBlock Text="{Binding SelectedClaim.ProductCode}" Foreground="#2D3748" FontSize="14" HorizontalAlignment="Right"/>
                                    </Grid>
                                    <Grid Margin="0,0,0,6">
                                        <TextBlock Text="จำนวนที่แจ้งเคลม:" Foreground="#718096" FontSize="14"/>
                                        <TextBlock Text="{Binding SelectedClaim.Quantity, StringFormat='{}{0} ชิ้น'}" Foreground="#0B2545" FontWeight="Bold" FontSize="15" HorizontalAlignment="Right"/>
                                    </Grid>
                                    <Grid>
                                        <TextBlock Text="ประเภทการเคลม:" Foreground="#718096" FontSize="14"/>
                                        <TextBlock Text="{Binding SelectedClaim.ClaimType}" Foreground="#2B6CB0" FontWeight="SemiBold" FontSize="14" HorizontalAlignment="Right"/>
                                    </Grid>
                                </StackPanel>"""
    new_product_info_box = """                                <StackPanel>
                                    <TextBlock Text="📦 รายละเอียดสินค้าเคลม" FontSize="16" FontWeight="Bold" Foreground="#2D3748" Margin="0,0,0,10"/>
                                    <Grid Margin="0,0,0,6">
                                        <TextBlock Text="ชื่อสินค้า:" Foreground="#718096" FontSize="14.5"/>
                                        <TextBlock Text="{Binding SelectedClaim.ProductName}" Foreground="#2D3748" FontWeight="Bold" FontSize="16" HorizontalAlignment="Right" TextTrimming="CharacterEllipsis"/>
                                    </Grid>
                                    <Grid Margin="0,0,0,6">
                                        <TextBlock Text="รหัสสินค้า:" Foreground="#718096" FontSize="14.5"/>
                                        <TextBlock Text="{Binding SelectedClaim.ProductCode}" Foreground="#2D3748" FontSize="14.5" HorizontalAlignment="Right"/>
                                    </Grid>
                                    <Grid Margin="0,0,0,6">
                                        <TextBlock Text="จำนวนที่แจ้งเคลม:" Foreground="#718096" FontSize="14.5"/>
                                        <TextBlock Text="{Binding SelectedClaim.Quantity, StringFormat='{}{0} ชิ้น'}" Foreground="#0B2545" FontWeight="Bold" FontSize="16" HorizontalAlignment="Right"/>
                                    </Grid>
                                    <Grid>
                                        <TextBlock Text="ประเภทการเคลม:" Foreground="#718096" FontSize="14.5"/>
                                        <TextBlock Text="{Binding SelectedClaim.ClaimType}" Foreground="#2B6CB0" FontWeight="SemiBold" FontSize="14.5" HorizontalAlignment="Right"/>
                                    </Grid>
                                </StackPanel>"""
    assert old_product_info_box in content, "old_product_info_box not found"
    content = content.replace(old_product_info_box, new_product_info_box)

    old_defect_box = """                                <StackPanel>
                                    <TextBlock Text="⚠️ อาการ / เหตุผลที่แจ้งเคลม" FontSize="15" FontWeight="Bold" Foreground="#2D3748" Margin="0,0,0,8"/>
                                    <TextBlock Text="{Binding SelectedClaim.Reason}" FontSize="14" Foreground="#2D3748" TextWrapping="Wrap" LineHeight="20" Margin="0,0,0,10"/>
                                    <TextBlock Text="📝 หมายเหตุเพิ่มเติม" FontSize="14" FontWeight="Bold" Foreground="#718096" Margin="0,4,0,4"/>
                                    <TextBlock Text="{Binding SelectedClaim.Note}" FontSize="14" Foreground="#718096" TextWrapping="Wrap" LineHeight="19"/>
                                </StackPanel>"""
    new_defect_box = """                                <StackPanel>
                                    <TextBlock Text="⚠️ อาการ / เหตุผลที่แจ้งเคลม" FontSize="16" FontWeight="Bold" Foreground="#2D3748" Margin="0,0,0,8"/>
                                    <TextBlock Text="{Binding SelectedClaim.Reason}" FontSize="15" Foreground="#2D3748" TextWrapping="Wrap" LineHeight="22" Margin="0,0,0,10"/>
                                    <TextBlock Text="📝 หมายเหตุเพิ่มเติม" FontSize="14.5" FontWeight="Bold" Foreground="#718096" Margin="0,4,0,4"/>
                                    <TextBlock Text="{Binding SelectedClaim.Note}" FontSize="14.5" Foreground="#718096" TextWrapping="Wrap" LineHeight="20"/>
                                </StackPanel>"""
    assert old_defect_box in content, "old_defect_box not found"
    content = content.replace(old_defect_box, new_defect_box)

    # 12. Add/Edit Claim Modal Form
    # Modal Header Subtitle
    content = content.replace("""                                    <TextBlock Text="ระบบบันทึกและตรวจสอบสถานะการเคลม / เปลี่ยนคืนสินค้า (POS)"
                                               FontSize="12.5" Foreground="#64748B"
                                               Margin="0,3,0,0"/>""",
                              """                                    <TextBlock Text="ระบบบันทึกและตรวจสอบสถานะการเคลม / เปลี่ยนคืนสินค้า (POS)"
                                               FontSize="14" Foreground="#64748B"
                                               Margin="0,3,0,0"/>""")

    # Section 1: Inputs
    old_sec1 = """                                        <!-- ComboBox ประเภทผู้เคลม -->
                                        <StackPanel Grid.Column="0">
                                            <TextBlock Text="ประเภทผู้เคลม" FontSize="12" Foreground="#64748B" FontWeight="SemiBold" Margin="0,0,0,5"/>
                                            <Border Background="#FFFFFF" BorderBrush="#CBD5E1" BorderThickness="1" CornerRadius="8" Height="38">
                                                <ComboBox ItemsSource="{Binding ClaimTypeOptions}"
                                                          SelectedItem="{Binding ClaimType, Mode=TwoWay}"
                                                          Style="{StaticResource LightComboBoxStyle}"
                                                          Height="36" VerticalContentAlignment="Center" Padding="8,0" BorderThickness="0"/>
                                            </Border>
                                        </StackPanel>

                                        <!-- TextBox เลขที่ใบเสร็จ พร้อมปุ่มไอคอนแว่นขยาย (🔍) -->
                                        <StackPanel Grid.Column="2">
                                            <TextBlock FontSize="12" Foreground="#64748B" FontWeight="SemiBold" Margin="0,0,0,5">
                                                <TextBlock.Style>
                                                    <Style TargetType="TextBlock">
                                                        <Setter Property="Text" Value="เลขที่ใบเสร็จ"/>
                                                        <Style.Triggers>
                                                            <DataTrigger Binding="{Binding ClaimType}" Value="บริษัทคู่ค้า">
                                                                <Setter Property="Text" Value="เลขที่ใบรับสินค้า"/>
                                                            </DataTrigger>
                                                        </Style.Triggers>
                                                    </Style>
                                                </TextBlock.Style>
                                            </TextBlock>
                                            <Border Background="White" BorderBrush="#CBD5E1" BorderThickness="1" CornerRadius="6" Height="38">
                                                <Grid>
                                                    <Grid.ColumnDefinitions>
                                                        <ColumnDefinition Width="*"/>
                                                        <ColumnDefinition Width="38"/>
                                                    </Grid.ColumnDefinitions>

                                                    <Grid Grid.Column="0" VerticalAlignment="Center">
                                                        <!-- Layer 1 (ด้านหลัง): Ghost Text ข้อความแนะนำสีเทาจาง (แสดงเฉพาะส่วนที่เหลือต่อท้ายเคอร์เซอร์ ไม่เหลื่อมซ้อนกับข้อความที่พิมพ์) -->
                                                        <TextBlock x:Name="txtGhostText"
                                                                   FontFamily="{Binding ElementName=txtReceiptNo, Path=FontFamily}"
                                                                   FontSize="{Binding ElementName=txtReceiptNo, Path=FontSize}"
                                                                   FontWeight="{Binding ElementName=txtReceiptNo, Path=FontWeight}"
                                                                   IsHitTestVisible="False"
                                                                   Margin="2,0,0,0"
                                                                   Padding="{Binding ElementName=txtReceiptNo, Path=Padding}"
                                                                   VerticalAlignment="Center">
                                                            <Run Text="{Binding GhostTextPrefix}" Foreground="Transparent"/>
                                                            <Run Text="{Binding GhostTextSuggestion}" Foreground="#94A3B8"/>
                                                        </TextBlock>

                                                        <!-- Layer 2 (ด้านหน้า): TextBox รับค่าจริง พื้นหลังโปร่งใส -->
                                                        <TextBox x:Name="txtReceiptNo"
                                                                 Background="Transparent"
                                                                 BorderThickness="0"
                                                                 FontSize="14"
                                                                 Padding="10,0"
                                                                 Margin="0"
                                                                 Foreground="#1E293B"
                                                                 CaretBrush="#1E293B"
                                                                 VerticalAlignment="Center"
                                                                 VerticalContentAlignment="Center"
                                                                 Style="{StaticResource LightTextBoxStyle}"
                                                                 Text="{Binding SearchReceiptText, UpdateSourceTrigger=PropertyChanged}"
                                                                 PreviewKeyDown="TxtReceiptNo_PreviewKeyDown">
                                                            <TextBox.InputBindings>
                                                                <KeyBinding Key="Enter" Command="{Binding LookupReceiptCommand}"/>
                                                            </TextBox.InputBindings>
                                                        </TextBox>
                                                    </Grid>

                                                    <!-- Search Button (🔍) -->
                                                    <Button Grid.Column="1"
                                                            Command="{Binding LookupReceiptCommand}"
                                                            Background="Transparent"
                                                            BorderThickness="0"
                                                            Cursor="Hand"
                                                            ToolTip="ค้นหาข้อมูลบิล">
                                                        <Button.Template>
                                                            <ControlTemplate TargetType="Button">
                                                                <Border x:Name="B" Background="{TemplateBinding Background}" CornerRadius="0,6,6,0">
                                                                    <TextBlock Text="🔍" FontSize="14" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                                                </Border>
                                                                <ControlTemplate.Triggers>
                                                                    <Trigger Property="IsMouseOver" Value="True">
                                                                        <Setter TargetName="B" Property="Background" Value="#E0F2FE"/>
                                                                    </Trigger>
                                                                </ControlTemplate.Triggers>
                                                            </ControlTemplate>
                                                        </Button.Template>
                                                    </Button>
                                                </Grid>
                                            </Border>
                                        </StackPanel>

                                        <!-- DatePicker วันที่ซื้อสินค้า -->
                                        <StackPanel Grid.Column="4">
                                            <TextBlock FontSize="12" Foreground="#64748B" FontWeight="SemiBold" Margin="0,0,0,5">
                                                <TextBlock.Style>
                                                    <Style TargetType="TextBlock">
                                                        <Setter Property="Text" Value="วันที่ซื้อสินค้า"/>
                                                        <Style.Triggers>
                                                            <DataTrigger Binding="{Binding ClaimType}" Value="บริษัทคู่ค้า">
                                                                <Setter Property="Text" Value="วันที่รับสินค้าเข้า"/>
                                                            </DataTrigger>
                                                        </Style.Triggers>
                                                    </Style>
                                                </TextBlock.Style>
                                            </TextBlock>
                                            <Border Background="#FFFFFF" BorderBrush="#CBD5E1" BorderThickness="1" CornerRadius="8" Height="38">
                                                <DatePicker Height="36" VerticalContentAlignment="Center" Background="Transparent" BorderThickness="0" Foreground="#1E293B" IsEnabled="False">
                                                    <DatePicker.Style>
                                                        <Style TargetType="DatePicker" BasedOn="{StaticResource LightDatePickerStyle}">
                                                            <Setter Property="SelectedDate" Value="{Binding ClaimSaleDate, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>
                                                            <Style.Triggers>
                                                                <DataTrigger Binding="{Binding ClaimType}" Value="บริษัทคู่ค้า">
                                                                    <Setter Property="SelectedDate" Value="{Binding ClaimStockInDate, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>
                                                                </DataTrigger>
                                                            </Style.Triggers>
                                                        </Style>
                                                    </DatePicker.Style>
                                                </DatePicker>
                                            </Border>
                                        </StackPanel>"""

    new_sec1 = """                                        <!-- ComboBox ประเภทผู้เคลม -->
                                        <StackPanel Grid.Column="0">
                                            <TextBlock Text="ประเภทผู้เคลม" FontSize="13.5" Foreground="#64748B" FontWeight="SemiBold" Margin="0,0,0,5"/>
                                            <Border Background="#FFFFFF" BorderBrush="#CBD5E1" BorderThickness="1" CornerRadius="8" Height="44">
                                                <ComboBox ItemsSource="{Binding ClaimTypeOptions}"
                                                          SelectedItem="{Binding ClaimType, Mode=TwoWay}"
                                                          Style="{StaticResource LightComboBoxStyle}"
                                                          FontSize="15"
                                                          Height="42" VerticalContentAlignment="Center" Padding="8,0" BorderThickness="0"/>
                                            </Border>
                                        </StackPanel>

                                        <!-- TextBox เลขที่ใบเสร็จ พร้อมปุ่มไอคอนแว่นขยาย (🔍) -->
                                        <StackPanel Grid.Column="2">
                                            <TextBlock FontSize="13.5" Foreground="#64748B" FontWeight="SemiBold" Margin="0,0,0,5">
                                                <TextBlock.Style>
                                                    <Style TargetType="TextBlock">
                                                        <Setter Property="Text" Value="เลขที่ใบเสร็จ"/>
                                                        <Style.Triggers>
                                                            <DataTrigger Binding="{Binding ClaimType}" Value="บริษัทคู่ค้า">
                                                                <Setter Property="Text" Value="เลขที่ใบรับสินค้า"/>
                                                            </DataTrigger>
                                                        </Style.Triggers>
                                                    </Style>
                                                </TextBlock.Style>
                                            </TextBlock>
                                            <Border Background="White" BorderBrush="#CBD5E1" BorderThickness="1" CornerRadius="6" Height="44">
                                                <Grid>
                                                    <Grid.ColumnDefinitions>
                                                        <ColumnDefinition Width="*"/>
                                                        <ColumnDefinition Width="42"/>
                                                    </Grid.ColumnDefinitions>

                                                    <Grid Grid.Column="0" VerticalAlignment="Center">
                                                        <!-- Layer 1 (ด้านหลัง): Ghost Text ข้อความแนะนำสีเทาจาง (แสดงเฉพาะส่วนที่เหลือต่อท้ายเคอร์เซอร์ ไม่เหลื่อมซ้อนกับข้อความที่พิมพ์) -->
                                                        <TextBlock x:Name="txtGhostText"
                                                                   FontFamily="{Binding ElementName=txtReceiptNo, Path=FontFamily}"
                                                                   FontSize="{Binding ElementName=txtReceiptNo, Path=FontSize}"
                                                                   FontWeight="{Binding ElementName=txtReceiptNo, Path=FontWeight}"
                                                                   IsHitTestVisible="False"
                                                                   Margin="2,0,0,0"
                                                                   Padding="{Binding ElementName=txtReceiptNo, Path=Padding}"
                                                                   VerticalAlignment="Center">
                                                            <Run Text="{Binding GhostTextPrefix}" Foreground="Transparent"/>
                                                            <Run Text="{Binding GhostTextSuggestion}" Foreground="#94A3B8"/>
                                                        </TextBlock>

                                                        <!-- Layer 2 (ด้านหน้า): TextBox รับค่าจริง พื้นหลังโปร่งใส -->
                                                        <TextBox x:Name="txtReceiptNo"
                                                                 Background="Transparent"
                                                                 BorderThickness="0"
                                                                 FontSize="15"
                                                                 Padding="10,0"
                                                                 Margin="0"
                                                                 Foreground="#1E293B"
                                                                 CaretBrush="#1E293B"
                                                                 VerticalAlignment="Center"
                                                                 VerticalContentAlignment="Center"
                                                                 Style="{StaticResource LightTextBoxStyle}"
                                                                 Text="{Binding SearchReceiptText, UpdateSourceTrigger=PropertyChanged}"
                                                                 PreviewKeyDown="TxtReceiptNo_PreviewKeyDown">
                                                            <TextBox.InputBindings>
                                                                <KeyBinding Key="Enter" Command="{Binding LookupReceiptCommand}"/>
                                                            </TextBox.InputBindings>
                                                        </TextBox>
                                                    </Grid>

                                                    <!-- Search Button (🔍) -->
                                                    <Button Grid.Column="1"
                                                            Command="{Binding LookupReceiptCommand}"
                                                            Background="Transparent"
                                                            BorderThickness="0"
                                                            Cursor="Hand"
                                                            ToolTip="ค้นหาข้อมูลบิล">
                                                        <Button.Template>
                                                            <ControlTemplate TargetType="Button">
                                                                <Border x:Name="B" Background="{TemplateBinding Background}" CornerRadius="0,6,6,0">
                                                                    <TextBlock Text="🔍" FontSize="16" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                                                </Border>
                                                                <ControlTemplate.Triggers>
                                                                    <Trigger Property="IsMouseOver" Value="True">
                                                                        <Setter TargetName="B" Property="Background" Value="#E0F2FE"/>
                                                                    </Trigger>
                                                                </ControlTemplate.Triggers>
                                                            </ControlTemplate>
                                                        </Button.Template>
                                                    </Button>
                                                </Grid>
                                            </Border>
                                        </StackPanel>

                                        <!-- DatePicker วันที่ซื้อสินค้า -->
                                        <StackPanel Grid.Column="4">
                                            <TextBlock FontSize="13.5" Foreground="#64748B" FontWeight="SemiBold" Margin="0,0,0,5">
                                                <TextBlock.Style>
                                                    <Style TargetType="TextBlock">
                                                        <Setter Property="Text" Value="วันที่ซื้อสินค้า"/>
                                                        <Style.Triggers>
                                                            <DataTrigger Binding="{Binding ClaimType}" Value="บริษัทคู่ค้า">
                                                                <Setter Property="Text" Value="วันที่รับสินค้าเข้า"/>
                                                            </DataTrigger>
                                                        </Style.Triggers>
                                                    </Style>
                                                </TextBlock.Style>
                                            </TextBlock>
                                            <Border Background="#FFFFFF" BorderBrush="#CBD5E1" BorderThickness="1" CornerRadius="8" Height="44">
                                                <DatePicker Height="42" VerticalContentAlignment="Center" Background="Transparent" BorderThickness="0" Foreground="#1E293B" IsEnabled="False">
                                                    <DatePicker.Style>
                                                        <Style TargetType="DatePicker" BasedOn="{StaticResource LightDatePickerStyle}">
                                                            <Setter Property="SelectedDate" Value="{Binding ClaimSaleDate, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>
                                                            <Style.Triggers>
                                                                <DataTrigger Binding="{Binding ClaimType}" Value="บริษัทคู่ค้า">
                                                                    <Setter Property="SelectedDate" Value="{Binding ClaimStockInDate, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>
                                                                </DataTrigger>
                                                            </Style.Triggers>
                                                        </Style>
                                                    </DatePicker.Style>
                                                </DatePicker>
                                            </Border>
                                        </StackPanel>"""
    assert old_sec1 in content, "old_sec1 not found"
    content = content.replace(old_sec1, new_sec1)

    # Banner message & badge
    content = content.replace("""                                                <TextBlock Text="ℹ️ " FontSize="13"/>
                                                <TextBlock Text="เงื่อนไขระยะเวลา: ลูกค้า ไม่เกิน 2 วัน / คู่ค้า ไม่เกิน 7 วัน"
                                                           FontSize="12" Foreground="#1E40AF" FontWeight="SemiBold"/>""",
                              """                                                <TextBlock Text="ℹ️ " FontSize="14"/>
                                                <TextBlock Text="เงื่อนไขระยะเวลา: ลูกค้า ไม่เกิน 2 วัน / คู่ค้า ไม่เกิน 7 วัน"
                                                           FontSize="13.5" Foreground="#1E40AF" FontWeight="SemiBold"/>""")

    content = content.replace("""                                                <TextBlock Text="{Binding ClaimValidationMessage}" FontSize="12" FontWeight="Bold">""",
                              """                                                <TextBlock Text="{Binding ClaimValidationMessage}" FontSize="13.5" FontWeight="Bold">""")

    # Customer & Supplier cards inside Add Form
    content = content.replace("""                                            <TextBlock Text="ชื่อลูกค้า" FontSize="10.5" Foreground="#64748B" FontWeight="SemiBold"/>
                                            <TextBlock FontSize="13" FontWeight="Bold" Foreground="#0F172A" TextTrimming="CharacterEllipsis">""",
                              """                                            <TextBlock Text="ชื่อลูกค้า" FontSize="13" Foreground="#64748B" FontWeight="SemiBold"/>
                                            <TextBlock FontSize="15" FontWeight="Bold" Foreground="#0F172A" TextTrimming="CharacterEllipsis">""")

    content = content.replace("""                                            <TextBlock Text="เบอร์โทรศัพท์" FontSize="10.5" Foreground="#64748B" FontWeight="SemiBold"/>
                                            <TextBlock FontSize="13" FontWeight="Bold" Foreground="#0284C7" TextTrimming="CharacterEllipsis">""",
                              """                                            <TextBlock Text="เบอร์โทรศัพท์" FontSize="13" Foreground="#64748B" FontWeight="SemiBold"/>
                                            <TextBlock FontSize="15" FontWeight="Bold" Foreground="#0284C7" TextTrimming="CharacterEllipsis">""")

    content = content.replace("""                                            <TextBlock Text="ผู้ส่งเคลม: " FontSize="12.5" Foreground="#64748B" FontWeight="SemiBold"/>
                                            <TextBlock Text="{Binding ClaimSenderName}" FontSize="13" FontWeight="Bold" Foreground="#0F172A"/>""",
                              """                                            <TextBlock Text="ผู้ส่งเคลม: " FontSize="13.5" Foreground="#64748B" FontWeight="SemiBold"/>
                                            <TextBlock Text="{Binding ClaimSenderName}" FontSize="15" FontWeight="Bold" Foreground="#0F172A"/>""")

    content = content.replace("""                                            <TextBlock Text="เบอร์โทรศัพท์: " FontSize="12.5" Foreground="#64748B" FontWeight="SemiBold"/>
                                            <TextBlock Text="{Binding ClaimSenderPhone}" FontSize="13" FontWeight="Bold" Foreground="#0284C7"/>""",
                              """                                            <TextBlock Text="เบอร์โทรศัพท์: " FontSize="13.5" Foreground="#64748B" FontWeight="SemiBold"/>
                                            <TextBlock Text="{Binding ClaimSenderPhone}" FontSize="15" FontWeight="Bold" Foreground="#0284C7"/>""")

    content = content.replace("""                                            <TextBlock Text="ถึง: " FontSize="12.5" Foreground="#64748B" FontWeight="SemiBold"/>
                                            <TextBlock FontSize="13" FontWeight="Bold" Foreground="#0F172A" TextTrimming="CharacterEllipsis" MaxWidth="320">""",
                              """                                            <TextBlock Text="ถึง: " FontSize="13.5" Foreground="#64748B" FontWeight="SemiBold"/>
                                            <TextBlock FontSize="15" FontWeight="Bold" Foreground="#0F172A" TextTrimming="CharacterEllipsis" MaxWidth="320">""")

    content = content.replace("""                                            <TextBlock Text="เบอร์โทรศัพท์: " FontSize="12.5" Foreground="#64748B" FontWeight="SemiBold"/>
                                            <TextBlock FontSize="13" FontWeight="Bold" Foreground="#0284C7" TextTrimming="CharacterEllipsis">""",
                              """                                            <TextBlock Text="เบอร์โทรศัพท์: " FontSize="13.5" Foreground="#64748B" FontWeight="SemiBold"/>
                                            <TextBlock FontSize="15" FontWeight="Bold" Foreground="#0284C7" TextTrimming="CharacterEllipsis">""")

    # Items table header & rows
    content = content.replace("""                            <!-- ตารางแสดงรายการเคลมหลัก -->
                            <TextBlock Text="รายการสินค้าที่ต้องการเคลม" FontSize="12.5" FontWeight="Bold" Foreground="#334155" Margin="0,0,0,6"/>""",
                              """                            <!-- ตารางแสดงรายการเคลมหลัก -->
                            <TextBlock Text="รายการสินค้าที่ต้องการเคลม" FontSize="16" FontWeight="Bold" Foreground="#334155" Margin="0,0,0,6"/>""")

    old_items_header = """                                                    <!-- Table Header -->
                                                    <Border Background="#F1F5F9" Padding="10,6" BorderBrush="#E2E8F0" BorderThickness="0,0,0,1">
                                                        <Grid>
                                                            <Grid.ColumnDefinitions>
                                                                <ColumnDefinition Width="130"/>
                                                                <ColumnDefinition Width="*"/>
                                                                <ColumnDefinition Width="100"/>
                                                                <ColumnDefinition Width="120"/>
                                                                <ColumnDefinition Width="45"/>
                                                            </Grid.ColumnDefinitions>
                                                            <TextBlock Grid.Column="0" Text="รหัสสินค้า" FontSize="11.5" FontWeight="Bold" Foreground="#64748B"/>
                                                            <TextBlock Grid.Column="1" Text="ชื่อสินค้า" FontSize="11.5" FontWeight="Bold" Foreground="#64748B"/>
                                                            <TextBlock Grid.Column="2" Text="ตามใบเสร็จ" FontSize="11.5" FontWeight="Bold" Foreground="#64748B" TextAlignment="Center"/>
                                                            <TextBlock Grid.Column="3" Text="จำนวนเคลม" FontSize="11.5" FontWeight="Bold" Foreground="#64748B" TextAlignment="Center"/>
                                                            <TextBlock Grid.Column="4" Text="" FontSize="11.5"/>
                                                        </Grid>
                                                    </Border>"""
    new_items_header = """                                                    <!-- Table Header -->
                                                    <Border Background="#F1F5F9" Padding="10,8" BorderBrush="#E2E8F0" BorderThickness="0,0,0,1">
                                                        <Grid>
                                                            <Grid.ColumnDefinitions>
                                                                <ColumnDefinition Width="140"/>
                                                                <ColumnDefinition Width="*"/>
                                                                <ColumnDefinition Width="110"/>
                                                                <ColumnDefinition Width="130"/>
                                                                <ColumnDefinition Width="45"/>
                                                            </Grid.ColumnDefinitions>
                                                            <TextBlock Grid.Column="0" Text="รหัสสินค้า" FontSize="14" FontWeight="Bold" Foreground="#64748B"/>
                                                            <TextBlock Grid.Column="1" Text="ชื่อสินค้า" FontSize="14" FontWeight="Bold" Foreground="#64748B"/>
                                                            <TextBlock Grid.Column="2" Text="ตามใบเสร็จ" FontSize="14" FontWeight="Bold" Foreground="#64748B" TextAlignment="Center"/>
                                                            <TextBlock Grid.Column="3" Text="จำนวนเคลม" FontSize="14" FontWeight="Bold" Foreground="#64748B" TextAlignment="Center"/>
                                                            <TextBlock Grid.Column="4" Text="" FontSize="14"/>
                                                        </Grid>
                                                    </Border>"""
    assert old_items_header in content, "old_items_header not found"
    content = content.replace(old_items_header, new_items_header)

    old_items_row = """                                                <Border BorderBrush="#F1F5F9" BorderThickness="0,0,0,1" Padding="10,6">
                                                    <Grid VerticalAlignment="Center">
                                                        <Grid.ColumnDefinitions>
                                                            <ColumnDefinition Width="130"/>
                                                            <ColumnDefinition Width="*"/>
                                                            <ColumnDefinition Width="100"/>
                                                            <ColumnDefinition Width="120"/>
                                                            <ColumnDefinition Width="45"/>
                                                        </Grid.ColumnDefinitions>
                                                        <TextBlock Grid.Column="0" Text="{Binding ProductCode}" FontSize="12" Foreground="#64748B" VerticalAlignment="Center"/>
                                                        <TextBlock Grid.Column="1" Text="{Binding ProductName}" FontSize="13" FontWeight="SemiBold" Foreground="#0F172A" VerticalAlignment="Center" TextTrimming="CharacterEllipsis"/>
                                                        <TextBlock Grid.Column="2" Text="{Binding ReceiptQty, StringFormat='{}{0} ชิ้น'}" FontSize="12.5" Foreground="#64748B" TextAlignment="Center" VerticalAlignment="Center"/>
                                                        
                                                        <!-- Claim Quantity TextBox -->
                                                        <Border Grid.Column="3" Background="#F8FAFC" BorderBrush="#CBD5E1" BorderThickness="1" CornerRadius="6" Width="70" Height="28" HorizontalAlignment="Center">
                                                            <TextBox Text="{Binding ClaimQty, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                                                     Style="{StaticResource LightTextBoxStyle}"
                                                                     TextAlignment="Center"
                                                                     FontSize="12.5"
                                                                     FontWeight="Bold"
                                                                     Foreground="#0284C7"/>
                                                        </Border>"""
    new_items_row = """                                                <Border BorderBrush="#F1F5F9" BorderThickness="0,0,0,1" Padding="10,8">
                                                    <Grid VerticalAlignment="Center">
                                                        <Grid.ColumnDefinitions>
                                                            <ColumnDefinition Width="140"/>
                                                            <ColumnDefinition Width="*"/>
                                                            <ColumnDefinition Width="110"/>
                                                            <ColumnDefinition Width="130"/>
                                                            <ColumnDefinition Width="45"/>
                                                        </Grid.ColumnDefinitions>
                                                        <TextBlock Grid.Column="0" Text="{Binding ProductCode}" FontSize="13.5" Foreground="#64748B" VerticalAlignment="Center"/>
                                                        <TextBlock Grid.Column="1" Text="{Binding ProductName}" FontSize="14.5" FontWeight="SemiBold" Foreground="#0F172A" VerticalAlignment="Center" TextTrimming="CharacterEllipsis"/>
                                                        <TextBlock Grid.Column="2" Text="{Binding ReceiptQty, StringFormat='{}{0} ชิ้น'}" FontSize="13.5" Foreground="#64748B" TextAlignment="Center" VerticalAlignment="Center"/>
                                                        
                                                        <!-- Claim Quantity TextBox -->
                                                        <Border Grid.Column="3" Background="#F8FAFC" BorderBrush="#CBD5E1" BorderThickness="1" CornerRadius="6" Width="80" Height="34" HorizontalAlignment="Center">
                                                            <TextBox Text="{Binding ClaimQty, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                                                     Style="{StaticResource LightTextBoxStyle}"
                                                                     TextAlignment="Center"
                                                                     FontSize="14"
                                                                     FontWeight="Bold"
                                                                     Foreground="#0284C7"/>
                                                        </Border>"""
    assert old_items_row in content, "old_items_row not found"
    content = content.replace(old_items_row, new_items_row)

    # Dynamic Selector header and rows
    old_dyn_hdr = """                                    <!-- TextBlock หัวข้อ -->
                                    <DockPanel LastChildFill="False" Margin="0,0,0,8">
                                        <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
                                            <TextBlock Text="🧾 " FontSize="14"/>
                                            <TextBlock Text="รายการสินค้าในใบเสร็จ " FontSize="13" FontWeight="Bold" Foreground="#0284C7"/>
                                            <TextBlock Text="{Binding ClaimSalesOrderRefNo, StringFormat='[{0}] '}" FontSize="13" FontWeight="Bold" Foreground="#0369A1"/>
                                            <TextBlock Text="— เลือกเฉพาะที่ต้องการเคลม" FontSize="12" Foreground="#0284C7"/>
                                        </StackPanel>

                                        <TextBlock Text="{Binding AvailableReceiptItems.Count, StringFormat='(พบทั้งหมด {0} รายการ)'}"
                                                   DockPanel.Dock="Right" FontSize="11" Foreground="#0369A1" VerticalAlignment="Center"/>
                                    </DockPanel>"""
    new_dyn_hdr = """                                    <!-- TextBlock หัวข้อ -->
                                    <DockPanel LastChildFill="False" Margin="0,0,0,8">
                                        <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
                                            <TextBlock Text="🧾 " FontSize="15"/>
                                            <TextBlock Text="รายการสินค้าในใบเสร็จ " FontSize="14.5" FontWeight="Bold" Foreground="#0284C7"/>
                                            <TextBlock Text="{Binding ClaimSalesOrderRefNo, StringFormat='[{0}] '}" FontSize="14.5" FontWeight="Bold" Foreground="#0369A1"/>
                                            <TextBlock Text="— เลือกเฉพาะที่ต้องการเคลม" FontSize="13.5" Foreground="#0284C7"/>
                                        </StackPanel>

                                        <TextBlock Text="{Binding AvailableReceiptItems.Count, StringFormat='(พบทั้งหมด {0} รายการ)'}"
                                                   DockPanel.Dock="Right" FontSize="13" Foreground="#0369A1" VerticalAlignment="Center"/>
                                    </DockPanel>"""
    assert old_dyn_hdr in content, "old_dyn_hdr not found"
    content = content.replace(old_dyn_hdr, new_dyn_hdr)

    old_dyn_row = """                                                                <!-- Product Info -->
                                                                <StackPanel Grid.Column="1" VerticalAlignment="Center">
                                                                    <TextBlock Text="{Binding ProductName}" FontSize="12.5" FontWeight="SemiBold" Foreground="#0F172A" TextTrimming="CharacterEllipsis"/>
                                                                    <TextBlock Text="{Binding ProductCode, StringFormat='รหัส: {0}'}" FontSize="11" Foreground="#64748B"/>
                                                                </StackPanel>

                                                                <!-- Purchased Qty -->
                                                                <TextBlock Grid.Column="2" Text="{Binding ReceiptQty, StringFormat='ซื้อ {0} ชิ้น'}" FontSize="12" Foreground="#475569" VerticalAlignment="Center" TextAlignment="Center"/>

                                                                <!-- Claim Qty Input -->
                                                                <StackPanel Grid.Column="3" Orientation="Horizontal" HorizontalAlignment="Right" VerticalAlignment="Center">
                                                                    <TextBlock Text="เคลม: " FontSize="11.5" Foreground="#64748B" VerticalAlignment="Center" Margin="0,0,4,0"/>
                                                                    <Border Background="#F8FAFC" BorderBrush="#CBD5E1" BorderThickness="1" CornerRadius="4" Width="55" Height="26">
                                                                        <TextBox Text="{Binding ClaimQty, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                                                                 Style="{StaticResource LightTextBoxStyle}"
                                                                                 TextAlignment="Center"
                                                                                 FontSize="12"
                                                                                 FontWeight="Bold"
                                                                                 Foreground="#0284C7"/>
                                                                    </Border>
                                                                    <TextBlock Text=" ชิ้น" FontSize="11.5" Foreground="#64748B" VerticalAlignment="Center" Margin="4,0,0,0"/>
                                                                </StackPanel>"""
    new_dyn_row = """                                                                <!-- Product Info -->
                                                                <StackPanel Grid.Column="1" VerticalAlignment="Center">
                                                                    <TextBlock Text="{Binding ProductName}" FontSize="14" FontWeight="SemiBold" Foreground="#0F172A" TextTrimming="CharacterEllipsis"/>
                                                                    <TextBlock Text="{Binding ProductCode, StringFormat='รหัส: {0}'}" FontSize="12.5" Foreground="#64748B"/>
                                                                </StackPanel>

                                                                <!-- Purchased Qty -->
                                                                <TextBlock Grid.Column="2" Text="{Binding ReceiptQty, StringFormat='ซื้อ {0} ชิ้น'}" FontSize="13.5" Foreground="#475569" VerticalAlignment="Center" TextAlignment="Center"/>

                                                                <!-- Claim Qty Input -->
                                                                <StackPanel Grid.Column="3" Orientation="Horizontal" HorizontalAlignment="Right" VerticalAlignment="Center">
                                                                    <TextBlock Text="เคลม: " FontSize="13" Foreground="#64748B" VerticalAlignment="Center" Margin="0,0,4,0"/>
                                                                    <Border Background="#F8FAFC" BorderBrush="#CBD5E1" BorderThickness="1" CornerRadius="4" Width="65" Height="32">
                                                                        <TextBox Text="{Binding ClaimQty, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                                                                 Style="{StaticResource LightTextBoxStyle}"
                                                                                 TextAlignment="Center"
                                                                                 FontSize="13.5"
                                                                                 FontWeight="Bold"
                                                                                 Foreground="#0284C7"/>
                                                                    </Border>
                                                                    <TextBlock Text=" ชิ้น" FontSize="13" Foreground="#64748B" VerticalAlignment="Center" Margin="4,0,0,0"/>
                                                                </StackPanel>"""
    assert old_dyn_row in content, "old_dyn_row not found"
    content = content.replace(old_dyn_row, new_dyn_row)

    # Dynamic Add button
    old_dyn_btn = """                                    <!-- ปุ่มสีฟ้าสด "เพิ่มรายการที่เลือก" -->
                                    <Button Content="➕ เพิ่มรายการที่เลือก"
                                            Command="{Binding AddSelectedReceiptItemsCommand}"
                                            Background="#0284C7"
                                            Foreground="White"
                                            FontWeight="Bold"
                                            FontSize="12.5"
                                            Height="32"
                                            Cursor="Hand"
                                            HorizontalAlignment="Right"
                                            Padding="14,0">"""
    new_dyn_btn = """                                    <!-- ปุ่มสีฟ้าสด "เพิ่มรายการที่เลือก" -->
                                    <Button Content="➕ เพิ่มรายการที่เลือก"
                                            Command="{Binding AddSelectedReceiptItemsCommand}"
                                            Background="#0284C7"
                                            Foreground="White"
                                            FontWeight="Bold"
                                            FontSize="14"
                                            Height="38"
                                            Cursor="Hand"
                                            HorizontalAlignment="Right"
                                            Padding="16,0">"""
    assert old_dyn_btn in content, "old_dyn_btn not found"
    content = content.replace(old_dyn_btn, new_dyn_btn)

    # Reason section
    old_reason = """                            <!-- TextBox "เหตุผลการเคลม" -->
                            <StackPanel Margin="0,0,0,14">
                                <TextBlock FontSize="12" FontWeight="SemiBold" Margin="0,0,0,5">
                                    <TextBlock.Inlines>
                                        <Run Text="เหตุผลการเคลม" Foreground="#64748B"/>
                                        <Run Text=" *" Foreground="#0284C7"/>
                                    </TextBlock.Inlines>
                                </TextBlock>
                                <Border Background="#FFFFFF" BorderBrush="#CBD5E1" BorderThickness="1" CornerRadius="8">
                                    <TextBox Text="{Binding ClaimReason, UpdateSourceTrigger=PropertyChanged}"
                                             Style="{StaticResource LightTextBoxStyle}"
                                             Padding="10,8"
                                             Height="56"
                                             TextWrapping="Wrap"
                                             AcceptsReturn="True"
                                             VerticalScrollBarVisibility="Auto"/>
                                </Border>
                            </StackPanel>"""
    new_reason = """                            <!-- TextBox "เหตุผลการเคลม" -->
                            <StackPanel Margin="0,0,0,14">
                                <TextBlock FontSize="14" FontWeight="SemiBold" Margin="0,0,0,5">
                                    <TextBlock.Inlines>
                                        <Run Text="เหตุผลการเคลม" Foreground="#64748B"/>
                                        <Run Text=" *" Foreground="#0284C7"/>
                                    </TextBlock.Inlines>
                                </TextBlock>
                                <Border Background="#FFFFFF" BorderBrush="#CBD5E1" BorderThickness="1" CornerRadius="8">
                                    <TextBox Text="{Binding ClaimReason, UpdateSourceTrigger=PropertyChanged}"
                                             Style="{StaticResource LightTextBoxStyle}"
                                             Padding="10,8"
                                             Height="70"
                                             FontSize="15"
                                             TextWrapping="Wrap"
                                             AcceptsReturn="True"
                                             VerticalScrollBarVisibility="Auto"/>
                                </Border>
                            </StackPanel>"""
    assert old_reason in content, "old_reason not found"
    content = content.replace(old_reason, new_reason)

    # Section 3: Controls
    old_sec3 = """                                    <!-- ปุ่ม "เลือกสินค้าจากใบเสร็จ" (มีไอคอนบิล + ขอบมน) -->
                                    <Button Grid.Column="0"
                                            Command="{Binding ToggleReceiptProductSelectorCommand}"
                                            Height="38"
                                            Cursor="Hand"
                                            Padding="14,0"
                                            VerticalAlignment="Bottom">
                                        <Button.Template>
                                            <ControlTemplate TargetType="Button">
                                                <Border x:Name="B" Background="#E0F2FE" BorderBrush="#38BDF8" BorderThickness="1.5" CornerRadius="8" Padding="{TemplateBinding Padding}">
                                                    <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
                                                        <TextBlock Text="🧾 " FontSize="14"/>
                                                        <TextBlock Text="เลือกสินค้าจากใบเสร็จ" FontSize="12.5" FontWeight="Bold" Foreground="#0369A1"/>
                                                    </StackPanel>
                                                </Border>
                                                <ControlTemplate.Triggers>
                                                    <Trigger Property="IsMouseOver" Value="True">
                                                        <Setter TargetName="B" Property="Background" Value="#BAE6FD"/>
                                                    </Trigger>
                                                    <Trigger Property="IsPressed" Value="True">
                                                        <Setter TargetName="B" Property="Background" Value="#7DD3FC"/>
                                                    </Trigger>
                                                </ControlTemplate.Triggers>
                                            </ControlTemplate>
                                        </Button.Template>
                                    </Button>

                                    <!-- Dropdown รูปแบบการเคลม -->
                                    <StackPanel Grid.Column="2">
                                        <TextBlock Text="รูปแบบการเคลม" FontSize="11.5" Foreground="#64748B" FontWeight="SemiBold" Margin="0,0,0,4"/>
                                        <Border Background="#FFFFFF" BorderBrush="#CBD5E1" BorderThickness="1" CornerRadius="8" Height="38">
                                            <ComboBox ItemsSource="{Binding ClaimActionOptions}"
                                                      SelectedItem="{Binding ClaimAction}"
                                                      Style="{StaticResource LightComboBoxStyle}"
                                                      Height="36" VerticalContentAlignment="Center" Padding="8,0" BorderThickness="0"/>
                                        </Border>
                                    </StackPanel>

                                    <!-- Dropdown สถานะเคลม -->
                                    <StackPanel Grid.Column="4">
                                        <TextBlock Text="สถานะเคลม" FontSize="11.5" Foreground="#64748B" FontWeight="SemiBold" Margin="0,0,0,4"/>
                                        <Border Background="#FFFFFF" BorderBrush="#CBD5E1" BorderThickness="1" CornerRadius="8" Height="38">
                                            <ComboBox ItemsSource="{Binding ClaimFormStatusOptions}"
                                                      SelectedItem="{Binding ClaimStatus}"
                                                      Style="{StaticResource LightComboBoxStyle}"
                                                      Height="36" VerticalContentAlignment="Center" Padding="8,0" BorderThickness="0"/>
                                        </Border>
                                    </StackPanel>"""

    new_sec3 = """                                    <!-- ปุ่ม "เลือกสินค้าจากใบเสร็จ" (มีไอคอนบิล + ขอบมน) -->
                                    <Button Grid.Column="0"
                                            Command="{Binding ToggleReceiptProductSelectorCommand}"
                                            Height="44"
                                            Cursor="Hand"
                                            Padding="14,0"
                                            VerticalAlignment="Bottom">
                                        <Button.Template>
                                            <ControlTemplate TargetType="Button">
                                                <Border x:Name="B" Background="#E0F2FE" BorderBrush="#38BDF8" BorderThickness="1.5" CornerRadius="8" Padding="{TemplateBinding Padding}">
                                                    <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
                                                        <TextBlock Text="🧾 " FontSize="15"/>
                                                        <TextBlock Text="เลือกสินค้าจากใบเสร็จ" FontSize="14.5" FontWeight="Bold" Foreground="#0369A1"/>
                                                    </StackPanel>
                                                </Border>
                                                <ControlTemplate.Triggers>
                                                    <Trigger Property="IsMouseOver" Value="True">
                                                        <Setter TargetName="B" Property="Background" Value="#BAE6FD"/>
                                                    </Trigger>
                                                    <Trigger Property="IsPressed" Value="True">
                                                        <Setter TargetName="B" Property="Background" Value="#7DD3FC"/>
                                                    </Trigger>
                                                </ControlTemplate.Triggers>
                                            </ControlTemplate>
                                        </Button.Template>
                                    </Button>

                                    <!-- Dropdown รูปแบบการเคลม -->
                                    <StackPanel Grid.Column="2">
                                        <TextBlock Text="รูปแบบการเคลม" FontSize="13.5" Foreground="#64748B" FontWeight="SemiBold" Margin="0,0,0,4"/>
                                        <Border Background="#FFFFFF" BorderBrush="#CBD5E1" BorderThickness="1" CornerRadius="8" Height="44">
                                            <ComboBox ItemsSource="{Binding ClaimActionOptions}"
                                                      SelectedItem="{Binding ClaimAction}"
                                                      Style="{StaticResource LightComboBoxStyle}"
                                                      FontSize="15"
                                                      Height="42" VerticalContentAlignment="Center" Padding="8,0" BorderThickness="0"/>
                                        </Border>
                                    </StackPanel>

                                    <!-- Dropdown สถานะเคลม -->
                                    <StackPanel Grid.Column="4">
                                        <TextBlock Text="สถานะเคลม" FontSize="13.5" Foreground="#64748B" FontWeight="SemiBold" Margin="0,0,0,4"/>
                                        <Border Background="#FFFFFF" BorderBrush="#CBD5E1" BorderThickness="1" CornerRadius="8" Height="44">
                                            <ComboBox ItemsSource="{Binding ClaimFormStatusOptions}"
                                                      SelectedItem="{Binding ClaimStatus}"
                                                      Style="{StaticResource LightComboBoxStyle}"
                                                      FontSize="15"
                                                      Height="42" VerticalContentAlignment="Center" Padding="8,0" BorderThickness="0"/>
                                        </Border>
                                    </StackPanel>"""
    assert old_sec3 in content, "old_sec3 not found"
    content = content.replace(old_sec3, new_sec3)

    # Footer Bar
    old_footer = """                            <!-- ข้อความชิดซ้ายสีเทา -->
                            <TextBlock Text="* กรุณาตรวจสอบข้อมูลและจำนวนสินค้าก่อนกดยืนยัน"
                                       FontSize="12" Foreground="#64748B"
                                       VerticalAlignment="Center"
                                       DockPanel.Dock="Left"/>

                            <!-- กลุ่มปุ่มชิดขวา: ยกเลิก และ บันทึกข้อมูล -->
                            <StackPanel Orientation="Horizontal" DockPanel.Dock="Right">
                                <!-- ปุ่ม "ยกเลิก" -->
                                <Button Content="ยกเลิก"
                                        Command="{Binding CloseModalCommand}"
                                        Width="100" Height="38"
                                        Margin="0,0,10,0"
                                        Cursor="Hand">
                                    <Button.Template>
                                        <ControlTemplate TargetType="Button">
                                            <Border x:Name="B" Background="#FFFFFF" BorderBrush="#CBD5E1" BorderThickness="1" CornerRadius="8">
                                                <TextBlock Text="{TemplateBinding Content}" Foreground="#475569" FontWeight="SemiBold" FontSize="13" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                            </Border>
                                            <ControlTemplate.Triggers>
                                                <Trigger Property="IsMouseOver" Value="True">
                                                    <Setter TargetName="B" Property="Background" Value="#F1F5F9"/>
                                                </Trigger>
                                            </ControlTemplate.Triggers>
                                        </ControlTemplate>
                                    </Button.Template>
                                </Button>

                                <!-- ปุ่ม "บันทึกข้อมูล" (สีฟ้า) -->
                                <Button x:Name="btnSaveClaim"
                                        Width="130" Height="38">
                                    <Button.Style>
                                        <Style TargetType="Button">
                                            <Setter Property="Command" Value="{Binding UpdateClaimCommand}"/>
                                            <Setter Property="Content" Value="💾 บันทึกการแก้ไข"/>
                                            <Setter Property="Cursor" Value="Hand"/>
                                            <Setter Property="Template">
                                                <Setter.Value>
                                                    <ControlTemplate TargetType="Button">
                                                        <Border x:Name="B" Background="#0284C7" CornerRadius="8">
                                                            <TextBlock Text="{TemplateBinding Content}" Foreground="White" FontWeight="Bold" FontSize="13" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                                        </Border>
                                                        <ControlTemplate.Triggers>
                                                            <Trigger Property="IsMouseOver" Value="True">
                                                                <Setter TargetName="B" Property="Background" Value="#0369A1"/>
                                                            </Trigger>
                                                            <Trigger Property="IsEnabled" Value="False">
                                                                <Setter TargetName="B" Property="Background" Value="#BAE6FD"/>
                                                                <Setter TargetName="B" Property="Opacity" Value="0.55"/>
                                                                <Setter Property="Foreground" Value="#94A3B8"/>
                                                            </Trigger>
                                                        </ControlTemplate.Triggers>
                                                    </ControlTemplate>
                                                </Setter.Value>
                                            </Setter>"""

    new_footer = """                            <!-- ข้อความชิดซ้ายสีเทา -->
                            <TextBlock Text="* กรุณาตรวจสอบข้อมูลและจำนวนสินค้าก่อนกดยืนยัน"
                                       FontSize="13.5" Foreground="#64748B"
                                       VerticalAlignment="Center"
                                       DockPanel.Dock="Left"/>

                            <!-- กลุ่มปุ่มชิดขวา: ยกเลิก และ บันทึกข้อมูล -->
                            <StackPanel Orientation="Horizontal" DockPanel.Dock="Right">
                                <!-- ปุ่ม "ยกเลิก" -->
                                <Button Content="ยกเลิก"
                                        Command="{Binding CloseModalCommand}"
                                        Width="110" Height="44"
                                        Margin="0,0,10,0"
                                        Cursor="Hand">
                                    <Button.Template>
                                        <ControlTemplate TargetType="Button">
                                            <Border x:Name="B" Background="#FFFFFF" BorderBrush="#CBD5E1" BorderThickness="1" CornerRadius="8">
                                                <TextBlock Text="{TemplateBinding Content}" Foreground="#475569" FontWeight="SemiBold" FontSize="15" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                            </Border>
                                            <ControlTemplate.Triggers>
                                                <Trigger Property="IsMouseOver" Value="True">
                                                    <Setter TargetName="B" Property="Background" Value="#F1F5F9"/>
                                                </Trigger>
                                            </ControlTemplate.Triggers>
                                        </ControlTemplate>
                                    </Button.Template>
                                </Button>

                                <!-- ปุ่ม "บันทึกข้อมูล" (สีฟ้า) -->
                                <Button x:Name="btnSaveClaim"
                                        Width="145" Height="44">
                                    <Button.Style>
                                        <Style TargetType="Button">
                                            <Setter Property="Command" Value="{Binding UpdateClaimCommand}"/>
                                            <Setter Property="Content" Value="💾 บันทึกการแก้ไข"/>
                                            <Setter Property="Cursor" Value="Hand"/>
                                            <Setter Property="Template">
                                                <Setter.Value>
                                                    <ControlTemplate TargetType="Button">
                                                        <Border x:Name="B" Background="#0284C7" CornerRadius="8">
                                                            <TextBlock Text="{TemplateBinding Content}" Foreground="White" FontWeight="Bold" FontSize="15" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                                        </Border>
                                                        <ControlTemplate.Triggers>
                                                            <Trigger Property="IsMouseOver" Value="True">
                                                                <Setter TargetName="B" Property="Background" Value="#0369A1"/>
                                                            </Trigger>
                                                            <Trigger Property="IsEnabled" Value="False">
                                                                <Setter TargetName="B" Property="Background" Value="#BAE6FD"/>
                                                                <Setter TargetName="B" Property="Opacity" Value="0.55"/>
                                                                <Setter Property="Foreground" Value="#94A3B8"/>
                                                            </Trigger>
                                                        </ControlTemplate.Triggers>
                                                    </ControlTemplate>
                                                </Setter.Value>
                                            </Setter>"""
    assert old_footer in content, "old_footer not found"
    content = content.replace(old_footer, new_footer)

    with open(path, "w", encoding="utf-8") as f:
        f.write(content)
    print("Successfully updated ClaimView.xaml")

if __name__ == "__main__":
    update_claim_view()
