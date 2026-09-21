import sys
import re

def update_sales_history():
    file_path = r"d:\ShopManagement_หลัก\MatchaShop\Views\SalesHistoryView.xaml"
    with open(file_path, "r", encoding="utf-8") as f:
        content = f.read()

    # 1. Update PrimaryBtnStyle & PurpleBtnStyle
    content = re.sub(
        r'(<Style x:Key="PrimaryBtnStyle"[^>]*>[\s\S]*?<Setter Property="FontSize"\s+Value=")14("[\s\S]*?<Setter Property="Height"\s+Value=")40(")',
        r'\g<1>16\g<2>44\3',
        content
    )
    content = re.sub(
        r'(<Style x:Key="PurpleBtnStyle"[^>]*>[\s\S]*?<Setter Property="FontSize"\s+Value=")14("[\s\S]*?<Setter Property="Height"\s+Value=")40(")',
        r'\g<1>16\g<2>44\3',
        content
    )

    # 2. Update DarkTextBoxStyle
    content = re.sub(
        r'(<Style x:Key="DarkTextBoxStyle"[^>]*>[\s\S]*?<Setter Property="FontSize"\s+Value=")14(")',
        r'\g<1>16\2',
        content
    )

    # 3. Update DarkDatePickerStyle
    content = re.sub(
        r'(<Style x:Key="DarkDatePickerStyle"[^>]*>[\s\S]*?<Setter Property="Height"\s+Value=")40("[\s\S]*?<Setter Property="FontSize"\s+Value=")13(")',
        r'\g<1>44\g<2>16\3',
        content
    )

    # 4. Update ViewDetailsButtonStyle
    content = re.sub(
        r'(<Style x:Key="ViewDetailsButtonStyle"[^>]*>[\s\S]*?<Setter Property="FontSize"\s+Value=")12("[\s\S]*?<Setter Property="Height"\s+Value=")28("[\s\S]*?<Setter Property="MinWidth"\s+Value=")95(")',
        r'\g<1>14\g<2>34\g<3>105\4',
        content
    )

    # 5. Update SHColumnHeaderStyle & SHRowStyle
    content = re.sub(
        r'(<Style x:Key="SHColumnHeaderStyle"[^>]*>[\s\S]*?<Setter Property="Height"\s+Value=")46(")',
        r'\g<1>48\2',
        content
    )
    content = re.sub(
        r'(<Style x:Key="SHRowStyle"[^>]*>[\s\S]*?<Setter Property="FontSize"\s+Value=")13(")',
        r'\g<1>15\2',
        content
    )

    # 6. Update ColCenter, ColLeft, ColCenterBold, ColAmountStyle
    # Add/update FontSize in ColCenter, ColLeft
    content = content.replace(
        '''        <Style x:Key="ColCenter" TargetType="TextBlock">
            <Setter Property="HorizontalAlignment" Value="Center"/>
            <Setter Property="VerticalAlignment"   Value="Center"/>
            <Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
        </Style>''',
        '''        <Style x:Key="ColCenter" TargetType="TextBlock">
            <Setter Property="HorizontalAlignment" Value="Center"/>
            <Setter Property="VerticalAlignment"   Value="Center"/>
            <Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="FontSize"            Value="15"/>
        </Style>'''
    )
    content = content.replace(
        '''        <Style x:Key="ColLeft" TargetType="TextBlock">
            <Setter Property="HorizontalAlignment" Value="Left"/>
            <Setter Property="VerticalAlignment"   Value="Center"/>
            <Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="Margin"              Value="4,0"/>
        </Style>''',
        '''        <Style x:Key="ColLeft" TargetType="TextBlock">
            <Setter Property="HorizontalAlignment" Value="Left"/>
            <Setter Property="VerticalAlignment"   Value="Center"/>
            <Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="FontSize"            Value="15"/>
            <Setter Property="Margin"              Value="4,0"/>
        </Style>'''
    )
    content = content.replace(
        '''        <Style x:Key="ColAmountStyle" TargetType="TextBlock">
            <Setter Property="HorizontalAlignment" Value="Right"/>
            <Setter Property="VerticalAlignment"   Value="Center"/>
            <Setter Property="FontWeight"          Value="Bold"/>
            <Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="Margin"              Value="0,0,8,0"/>
            <Setter Property="Foreground"          Value="#0B2545"/>
        </Style>''',
        '''        <Style x:Key="ColAmountStyle" TargetType="TextBlock">
            <Setter Property="HorizontalAlignment" Value="Right"/>
            <Setter Property="VerticalAlignment"   Value="Center"/>
            <Setter Property="FontWeight"          Value="Bold"/>
            <Setter Property="FontSize"            Value="16"/>
            <Setter Property="FontFamily"          Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
            <Setter Property="Margin"              Value="0,0,8,0"/>
            <Setter Property="Foreground"          Value="#0B2545"/>
        </Style>'''
    )

    # 7. Update LightModalColHeaderStyle & LightModalRowStyle
    content = re.sub(
        r'(<Style x:Key="LightModalColHeaderStyle"[^>]*>[\s\S]*?<Setter Property="FontSize"\s+Value=")15("[\s\S]*?<Setter Property="Height"\s+Value=")40(")',
        r'\g<1>16\g<2>44\3',
        content
    )
    content = re.sub(
        r'(<Style x:Key="LightModalRowStyle"[^>]*>[\s\S]*?<Setter Property="FontSize"\s+Value=")13(")',
        r'\g<1>15\2',
        content
    )

    # 8. Page Header subtitle
    content = content.replace(
        '''                        <TextBlock Text="จัดการประวัติการขาย — ระบบจัดการร้าน พอใจ"
                                   FontSize="13"
                                   Foreground="#718096"
                                   Margin="0"/>''',
        '''                        <TextBlock Text="จัดการประวัติการขาย — ระบบจัดการร้าน พอใจ"
                                   FontSize="15"
                                   Foreground="#718096"
                                   Margin="0"/>'''
    )

    # 9. KPI Cards: Card 1, 2, 3
    # Card 1
    content = content.replace(
        '''                        <!-- Icon + Label -->
                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="36" Height="36" CornerRadius="10" Background="#E0F2FE" Margin="0,0,10,0">
                                <TextBlock Text="💰" FontSize="18" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="ยอดขายรวม" FontSize="13" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <!-- Value -->
                        <TextBlock Grid.Row="1"
                                   Text="{Binding TotalRevenue, StringFormat={}{0:N2}}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#0B2545"
                                   Margin="0,0,0,6"/>
                        <!-- Sub-label -->
                        <TextBlock Grid.Row="2"
                                   Text="{Binding RevenueSubLabel}"
                                   FontSize="11"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>''',
        '''                        <!-- Icon + Label -->
                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="40" Height="40" CornerRadius="10" Background="#E0F2FE" Margin="0,0,10,0">
                                <TextBlock Text="💰" FontSize="20" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="ยอดขายรวม" FontSize="16" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <!-- Value -->
                        <TextBlock Grid.Row="1"
                                   Text="{Binding TotalRevenue, StringFormat={}{0:N2}}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#0B2545"
                                   Margin="0,0,0,6"/>
                        <!-- Sub-label -->
                        <TextBlock Grid.Row="2"
                                   Text="{Binding RevenueSubLabel}"
                                   FontSize="14"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>'''
    )

    # Card 2
    content = content.replace(
        '''                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="36" Height="36" CornerRadius="10" Background="#EBF8FF" Margin="0,0,10,0">
                                <TextBlock Text="🧾" FontSize="18" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="จำนวนออเดอร์" FontSize="13" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <TextBlock Grid.Row="1"
                                   Text="{Binding TotalOrders}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#2B6CB0"
                                   Margin="0,0,0,6"/>
                        <TextBlock Grid.Row="2"
                                   Text="{Binding OrdersSubLabel}"
                                   FontSize="11"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>''',
        '''                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="40" Height="40" CornerRadius="10" Background="#EBF8FF" Margin="0,0,10,0">
                                <TextBlock Text="🧾" FontSize="20" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="จำนวนออเดอร์" FontSize="16" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <TextBlock Grid.Row="1"
                                   Text="{Binding TotalOrders}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#2B6CB0"
                                   Margin="0,0,0,6"/>
                        <TextBlock Grid.Row="2"
                                   Text="{Binding OrdersSubLabel}"
                                   FontSize="14"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>'''
    )

    # Card 3
    content = content.replace(
        '''                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="36" Height="36" CornerRadius="10" Background="#FEFCBF" Margin="0,0,10,0">
                                <TextBlock Text="📈" FontSize="18" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="ยอดเฉลี่ยต่อบิล" FontSize="13" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <TextBlock Grid.Row="1"
                                   Text="{Binding AverageTicket, StringFormat={}{0:N2}}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#D69E2E"
                                   Margin="0,0,0,6"/>
                        <TextBlock Grid.Row="2"
                                   Text="{Binding AverageSubLabel}"
                                   FontSize="11"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>''',
        '''                        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
                            <Border Width="40" Height="40" CornerRadius="10" Background="#FEFCBF" Margin="0,0,10,0">
                                <TextBlock Text="📈" FontSize="20" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="0"/>
                            </Border>
                            <TextBlock Text="ยอดเฉลี่ยต่อบิล" FontSize="16" FontWeight="SemiBold"
                                       Foreground="#718096" VerticalAlignment="Center" Margin="0"/>
                        </StackPanel>
                        <TextBlock Grid.Row="1"
                                   Text="{Binding AverageTicket, StringFormat={}{0:N2}}"
                                   FontSize="28" FontWeight="Bold"
                                   Foreground="#D69E2E"
                                   Margin="0,0,0,6"/>
                        <TextBlock Grid.Row="2"
                                   Text="{Binding AverageSubLabel}"
                                   FontSize="14"
                                   Foreground="#A0AEC0"
                                   Margin="0"/>'''
    )

    # 10. Filter / Search Bar
    content = content.replace(
        '''                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>     <!-- Search Box -->
                        <ColumnDefinition Width="16"/>
                        <ColumnDefinition Width="160"/>   <!-- Payment ComboBox -->
                        <ColumnDefinition Width="16"/>
                        <ColumnDefinition Width="Auto"/>  <!-- Search Button -->
                        <ColumnDefinition Width="8"/>
                        <ColumnDefinition Width="Auto"/>  <!-- Clear Button -->
                    </Grid.ColumnDefinitions>''',
        '''                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>     <!-- Search Box -->
                        <ColumnDefinition Width="16"/>
                        <ColumnDefinition Width="180"/>   <!-- Payment ComboBox -->
                        <ColumnDefinition Width="16"/>
                        <ColumnDefinition Width="Auto"/>  <!-- Search Button -->
                        <ColumnDefinition Width="8"/>
                        <ColumnDefinition Width="Auto"/>  <!-- Clear Button -->
                    </Grid.ColumnDefinitions>'''
    )

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
                            <TextBox x:Name="SalesSearchBox"
                                     Text="{Binding SearchKeyword, UpdateSourceTrigger=PropertyChanged}"
                                     Style="{StaticResource DarkTextBoxStyle}"
                                     Height="40"
                                     Padding="38,0,12,0"
                                     VerticalContentAlignment="Center"
                                     FontSize="13"/>
                            <!-- Placeholder -->
                            <TextBlock Text="ค้นหา รหัสใบเสร็จ, ชื่อลูกค้า..."
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
                            <TextBox x:Name="SalesSearchBox"
                                     Text="{Binding SearchKeyword, UpdateSourceTrigger=PropertyChanged}"
                                     Style="{StaticResource DarkTextBoxStyle}"
                                     Height="44"
                                     Padding="42,0,12,0"
                                     VerticalContentAlignment="Center"
                                     FontSize="16"/>
                            <!-- Placeholder -->
                            <TextBlock Text="ค้นหา รหัสใบเสร็จ, ชื่อลูกค้า..."
                                       Foreground="#A0AEC0"
                                       FontSize="15"
                                       VerticalAlignment="Center"
                                       Margin="42,0,12,0"
                                       IsHitTestVisible="False">'''
    )

    # Payment ComboBox Height & FontSize
    content = content.replace(
        '''                    <!-- Payment Method ComboBox -->
                    <Border Grid.Column="2"
                            Background="#E0F2FE"
                            BorderBrush="#BAE6FD"
                            BorderThickness="1.5"
                            CornerRadius="8">
                        <ComboBox ItemsSource="{Binding PaymentMethodOptions}"
                                  SelectedItem="{Binding SelectedPaymentMethod, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                  Style="{StaticResource DkComboBoxStyle}"
                                  BorderThickness="0"
                                  Background="Transparent"/>
                    </Border>''',
        '''                    <!-- Payment Method ComboBox -->
                    <Border Grid.Column="2"
                            Background="#E0F2FE"
                            BorderBrush="#BAE6FD"
                            BorderThickness="1.5"
                            CornerRadius="8"
                            Height="44">
                        <ComboBox ItemsSource="{Binding PaymentMethodOptions}"
                                  SelectedItem="{Binding SelectedPaymentMethod, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                  Style="{StaticResource DkComboBoxStyle}"
                                  Height="44"
                                  FontSize="16"
                                  BorderThickness="0"
                                  Background="Transparent"/>
                    </Border>'''
    )

    # Search & Clear buttons
    content = content.replace(
        '''                    <!-- Search Button -->
                    <Button Grid.Column="4"
                            Content="ค้นหา"
                            Command="{Binding SearchCommand}"
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

                    <!-- Clear / Reset Button -->
                    <Button Grid.Column="6"
                            Content="ล้าง"
                            Command="{Binding ClearFilterCommand}"
                            Style="{StaticResource SecondaryBtnStyle}"
                            Padding="16,0"
                            Height="40"/>''',
        '''                    <!-- Search Button -->
                    <Button Grid.Column="4"
                            Content="ค้นหา"
                            Command="{Binding SearchCommand}"
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
                            Command="{Binding ClearFilterCommand}"
                            Style="{StaticResource SecondaryBtnStyle}"
                            Padding="20,0"
                            Height="44"
                            FontSize="16"/>'''
    )

    # 11. Payment Badges and Status Badges in DataGrid
    content = content.replace(
        '''                                        <Border x:Name="PayBadge"
                                                CornerRadius="6" Padding="8,3"
                                                HorizontalAlignment="Center"
                                                Background="#E0F2FE">
                                            <TextBlock x:Name="PayText"
                                                       Text="{Binding PaymentMethod}"
                                                       FontSize="12" FontWeight="SemiBold"
                                                       Foreground="#0369A1"
                                                       HorizontalAlignment="Center"/>
                                        </Border>''',
        '''                                        <Border x:Name="PayBadge"
                                                CornerRadius="6" Padding="10,4"
                                                HorizontalAlignment="Center"
                                                Background="#E0F2FE">
                                            <TextBlock x:Name="PayText"
                                                       Text="{Binding PaymentMethod}"
                                                       FontSize="14" FontWeight="SemiBold"
                                                       Foreground="#0369A1"
                                                       HorizontalAlignment="Center"/>
                                        </Border>'''
    )

    content = content.replace(
        '''                                        <Border x:Name="StatusBadge"
                                                CornerRadius="6" Padding="8,3"
                                                HorizontalAlignment="Center"
                                                Background="#D1FAE5">
                                            <TextBlock x:Name="StatusText"
                                                       Text="{Binding Status}"
                                                       FontSize="12" FontWeight="Bold"
                                                       Foreground="#065F46"
                                                       HorizontalAlignment="Center"/>
                                        </Border>''',
        '''                                        <Border x:Name="StatusBadge"
                                                CornerRadius="6" Padding="10,4"
                                                HorizontalAlignment="Center"
                                                Background="#D1FAE5">
                                            <TextBlock x:Name="StatusText"
                                                       Text="{Binding Status}"
                                                       FontSize="14" FontWeight="Bold"
                                                       Foreground="#065F46"
                                                       HorizontalAlignment="Center"/>
                                        </Border>'''
    )

    # Status Bar
    content = content.replace(
        '''                        <TextBlock Text="{Binding StatusBarText}"
                                   Foreground="#718096"
                                   FontSize="12"
                                   VerticalAlignment="Center"
                                   DockPanel.Dock="Left"
                                   Margin="0"/>''',
        '''                        <TextBlock Text="{Binding StatusBarText}"
                                   Foreground="#718096"
                                   FontSize="14"
                                   VerticalAlignment="Center"
                                   DockPanel.Dock="Left"
                                   Margin="0"/>'''
    )

    # 12. Modal Overlay: Header, Inset Card, Items DataGrid, Footer
    content = content.replace(
        '''            <Border Width="600"
                    MaxHeight="720"
                    Background="#FFFFFF"
                    BorderBrush="#BAE6FD"
                    BorderThickness="1.5"
                    CornerRadius="16"
                    Padding="24"
                    HorizontalAlignment="Center"
                    VerticalAlignment="Center"
                    Effect="{StaticResource SoftShadow}">''',
        '''            <Border Width="650"
                    MaxHeight="740"
                    Background="#FFFFFF"
                    BorderBrush="#BAE6FD"
                    BorderThickness="1.5"
                    CornerRadius="16"
                    Padding="24"
                    HorizontalAlignment="Center"
                    VerticalAlignment="Center"
                    Effect="{StaticResource SoftShadow}">'''
    )

    content = content.replace(
        '''                                <TextBlock Text="รายละเอียดใบเสร็จ"
                                           FontSize="18" FontWeight="Bold"
                                           Foreground="#2D3748"
                                           Margin="0"/>
                                <TextBlock Text="{Binding SelectedOrder.RefNo, StringFormat='เลขที่ใบเสร็จ: {0}'}"
                                           FontSize="12.5" FontWeight="Bold"
                                           Foreground="#0B2545"
                                           Margin="0,2,0,0"/>''',
        '''                                <TextBlock Text="รายละเอียดใบเสร็จ"
                                           FontSize="20" FontWeight="Bold"
                                           Foreground="#2D3748"
                                           Margin="0"/>
                                <TextBlock Text="{Binding SelectedOrder.RefNo, StringFormat='เลขที่ใบเสร็จ: {0}'}"
                                           FontSize="14" FontWeight="Bold"
                                           Foreground="#0B2545"
                                           Margin="0,2,0,0"/>'''
    )

    # Inset Card
    content = content.replace(
        '''                            <!-- Date / Time & Payment Method -->
                            <StackPanel Grid.Row="0" Grid.Column="0" Margin="0,0,0,8">
                                <TextBlock Text="วันเวลาทำรายการ" FontSize="11.5" Foreground="#718096" Margin="0,0,0,2"/>
                                <TextBlock Text="{Binding SelectedOrder.Timestamp, StringFormat=dd/MM/yyyy HH:mm}"
                                           FontSize="13" FontWeight="Bold" Foreground="#2D3748"/>
                            </StackPanel>

                            <StackPanel Grid.Row="0" Grid.Column="1" Margin="0,0,0,8" HorizontalAlignment="Right">
                                <TextBlock Text="ช่องทางชำระเงิน" FontSize="11.5" Foreground="#718096" Margin="0,0,0,2"/>
                                <Border Background="#E0F2FE" CornerRadius="6" Padding="8,2">
                                    <TextBlock Text="{Binding SelectedOrder.PaymentMethod}"
                                               FontSize="12" FontWeight="Bold" Foreground="#0369A1"/>
                                </Border>
                            </StackPanel>

                            <!-- Customer Name & Status -->
                            <StackPanel Grid.Row="1" Grid.Column="0">
                                <TextBlock Text="ชื่อลูกค้า" FontSize="11.5" Foreground="#718096" Margin="0,0,0,2"/>
                                <TextBlock Text="{Binding SelectedOrder.CustomerDisplayName}"
                                           FontSize="13" FontWeight="SemiBold" Foreground="#2D3748"/>
                            </StackPanel>

                            <StackPanel Grid.Row="1" Grid.Column="1" HorizontalAlignment="Right">
                                <TextBlock Text="สถานะบิล" FontSize="11.5" Foreground="#718096" Margin="0,0,0,2"/>
                                <Border Background="#D1FAE5" CornerRadius="6" Padding="8,2">
                                    <TextBlock Text="{Binding SelectedOrder.Status}"
                                               FontSize="12" FontWeight="Bold" Foreground="#065F46"/>
                                </Border>
                            </StackPanel>''',
        '''                            <!-- Date / Time & Payment Method -->
                            <StackPanel Grid.Row="0" Grid.Column="0" Margin="0,0,0,8">
                                <TextBlock Text="วันเวลาทำรายการ" FontSize="13.5" Foreground="#718096" Margin="0,0,0,2"/>
                                <TextBlock Text="{Binding SelectedOrder.Timestamp, StringFormat=dd/MM/yyyy HH:mm}"
                                           FontSize="15" FontWeight="Bold" Foreground="#2D3748"/>
                            </StackPanel>

                            <StackPanel Grid.Row="0" Grid.Column="1" Margin="0,0,0,8" HorizontalAlignment="Right">
                                <TextBlock Text="ช่องทางชำระเงิน" FontSize="13.5" Foreground="#718096" Margin="0,0,0,2"/>
                                <Border Background="#E0F2FE" CornerRadius="6" Padding="10,4">
                                    <TextBlock Text="{Binding SelectedOrder.PaymentMethod}"
                                               FontSize="14" FontWeight="Bold" Foreground="#0369A1"/>
                                </Border>
                            </StackPanel>

                            <!-- Customer Name & Status -->
                            <StackPanel Grid.Row="1" Grid.Column="0">
                                <TextBlock Text="ชื่อลูกค้า" FontSize="13.5" Foreground="#718096" Margin="0,0,0,2"/>
                                <TextBlock Text="{Binding SelectedOrder.CustomerDisplayName}"
                                           FontSize="15" FontWeight="SemiBold" Foreground="#2D3748"/>
                            </StackPanel>

                            <StackPanel Grid.Row="1" Grid.Column="1" HorizontalAlignment="Right">
                                <TextBlock Text="สถานะบิล" FontSize="13.5" Foreground="#718096" Margin="0,0,0,2"/>
                                <Border Background="#D1FAE5" CornerRadius="6" Padding="10,4">
                                    <TextBlock Text="{Binding SelectedOrder.Status}"
                                               FontSize="14" FontWeight="Bold" Foreground="#065F46"/>
                                </Border>
                            </StackPanel>'''
    )

    # Modal Items DataGrid RowHeight & Fonts
    content = content.replace(
        '''                    <!-- Items List DataGrid (Light Modal Styling) -->
                    <ScrollViewer Grid.Row="3" MaxHeight="240" VerticalScrollBarVisibility="Hidden" HorizontalScrollBarVisibility="Disabled">
                        <DataGrid ItemsSource="{Binding SelectedOrderItems}"
                                  AutoGenerateColumns="False"
                                  IsReadOnly="True"
                                  HeadersVisibility="Column"
                                  CanUserReorderColumns="False"
                                  CanUserResizeColumns="False"
                                  CanUserSortColumns="False"
                                  RowHeight="38"''',
        '''                    <!-- Items List DataGrid (Light Modal Styling) -->
                    <ScrollViewer Grid.Row="3" MaxHeight="260" VerticalScrollBarVisibility="Hidden" HorizontalScrollBarVisibility="Disabled">
                        <DataGrid ItemsSource="{Binding SelectedOrderItems}"
                                  AutoGenerateColumns="False"
                                  IsReadOnly="True"
                                  HeadersVisibility="Column"
                                  CanUserReorderColumns="False"
                                  CanUserResizeColumns="False"
                                  CanUserSortColumns="False"
                                  RowHeight="44"'''
    )

    # Modal Items DataGrid TextBlock FontSize
    content = content.replace(
        '''                                <DataGridTextColumn Header="รายการสินค้า"
                                                    Binding="{Binding ProductName}"
                                                    Width="2*">
                                    <DataGridTextColumn.ElementStyle>
                                        <Style TargetType="TextBlock">
                                            <Setter Property="Foreground" Value="#2D3748"/>
                                            <Setter Property="VerticalAlignment" Value="Center"/>
                                            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
                                        </Style>
                                    </DataGridTextColumn.ElementStyle>
                                </DataGridTextColumn>
                                <DataGridTextColumn Header="จำนวน"
                                                    Binding="{Binding Quantity}"
                                                    Width="65">
                                    <DataGridTextColumn.ElementStyle>
                                        <Style TargetType="TextBlock">
                                            <Setter Property="Foreground" Value="#718096"/>
                                            <Setter Property="HorizontalAlignment" Value="Center"/>
                                            <Setter Property="VerticalAlignment" Value="Center"/>
                                        </Style>
                                    </DataGridTextColumn.ElementStyle>
                                </DataGridTextColumn>
                                <DataGridTextColumn Header="ราคา/หน่วย"
                                                    Binding="{Binding UnitPrice, StringFormat={}{0:N2}}"
                                                    Width="95">
                                    <DataGridTextColumn.ElementStyle>
                                        <Style TargetType="TextBlock">
                                            <Setter Property="Foreground" Value="#718096"/>
                                            <Setter Property="HorizontalAlignment" Value="Right"/>
                                            <Setter Property="VerticalAlignment" Value="Center"/>
                                            <Setter Property="FontSize" Value="14"/>
                                            <Setter Property="Margin" Value="0,0,4,0"/>
                                        </Style>
                                    </DataGridTextColumn.ElementStyle>
                                </DataGridTextColumn>
                                <DataGridTextColumn Header="รวมเงิน"
                                                    Binding="{Binding Total, StringFormat={}{0:N2}}"
                                                    Width="105">
                                    <DataGridTextColumn.ElementStyle>
                                        <Style TargetType="TextBlock">
                                            <Setter Property="Foreground" Value="#0B2545"/>
                                            <Setter Property="FontWeight" Value="Bold"/>
                                            <Setter Property="HorizontalAlignment" Value="Right"/>
                                            <Setter Property="VerticalAlignment" Value="Center"/>
                                            <Setter Property="FontSize" Value="14"/>
                                            <Setter Property="Margin" Value="0,0,4,0"/>
                                        </Style>
                                    </DataGridTextColumn.ElementStyle>
                                </DataGridTextColumn>''',
        '''                                <DataGridTextColumn Header="รายการสินค้า"
                                                    Binding="{Binding ProductName}"
                                                    Width="2*">
                                    <DataGridTextColumn.ElementStyle>
                                        <Style TargetType="TextBlock">
                                            <Setter Property="Foreground" Value="#2D3748"/>
                                            <Setter Property="VerticalAlignment" Value="Center"/>
                                            <Setter Property="FontSize" Value="15"/>
                                            <Setter Property="FontFamily" Value="Noto Sans Thai, Leelawadee UI, Segoe UI"/>
                                        </Style>
                                    </DataGridTextColumn.ElementStyle>
                                </DataGridTextColumn>
                                <DataGridTextColumn Header="จำนวน"
                                                    Binding="{Binding Quantity}"
                                                    Width="70">
                                    <DataGridTextColumn.ElementStyle>
                                        <Style TargetType="TextBlock">
                                            <Setter Property="Foreground" Value="#718096"/>
                                            <Setter Property="HorizontalAlignment" Value="Center"/>
                                            <Setter Property="VerticalAlignment" Value="Center"/>
                                            <Setter Property="FontSize" Value="15"/>
                                        </Style>
                                    </DataGridTextColumn.ElementStyle>
                                </DataGridTextColumn>
                                <DataGridTextColumn Header="ราคา/หน่วย"
                                                    Binding="{Binding UnitPrice, StringFormat={}{0:N2}}"
                                                    Width="105">
                                    <DataGridTextColumn.ElementStyle>
                                        <Style TargetType="TextBlock">
                                            <Setter Property="Foreground" Value="#718096"/>
                                            <Setter Property="HorizontalAlignment" Value="Right"/>
                                            <Setter Property="VerticalAlignment" Value="Center"/>
                                            <Setter Property="FontSize" Value="16"/>
                                            <Setter Property="Margin" Value="0,0,4,0"/>
                                        </Style>
                                    </DataGridTextColumn.ElementStyle>
                                </DataGridTextColumn>
                                <DataGridTextColumn Header="รวมเงิน"
                                                    Binding="{Binding Total, StringFormat={}{0:N2}}"
                                                    Width="115">
                                    <DataGridTextColumn.ElementStyle>
                                        <Style TargetType="TextBlock">
                                            <Setter Property="Foreground" Value="#0B2545"/>
                                            <Setter Property="FontWeight" Value="Bold"/>
                                            <Setter Property="HorizontalAlignment" Value="Right"/>
                                            <Setter Property="VerticalAlignment" Value="Center"/>
                                            <Setter Property="FontSize" Value="16"/>
                                            <Setter Property="Margin" Value="0,0,4,0"/>
                                        </Style>
                                    </DataGridTextColumn.ElementStyle>
                                </DataGridTextColumn>'''
    )

    # Payment Summary
    content = content.replace(
        '''                    <!-- Payment Summary Section -->
                    <StackPanel Grid.Row="5" Margin="0,0,0,16">
                        <Grid Margin="0,0,0,4">
                            <TextBlock Text="รับเงินสด" Foreground="#718096" FontSize="14"/>
                            <TextBlock Text="{Binding SelectedOrder.CashReceived, StringFormat={}{0:N2}}"
                                       FontSize="15" FontWeight="SemiBold" Foreground="#2D3748"
                                       HorizontalAlignment="Right"/>
                        </Grid>
                        <Grid Margin="0,0,0,6">
                            <TextBlock Text="เงินทอน" Foreground="#718096" FontSize="14"/>
                            <TextBlock Text="{Binding SelectedOrder.Change, StringFormat={}{0:N2}}"
                                       FontSize="15" FontWeight="SemiBold" Foreground="#D69E2E"
                                       HorizontalAlignment="Right"/>
                        </Grid>
                        <Border Height="1" Background="#BAE6FD" Margin="0,4,0,8"/>
                        <DockPanel LastChildFill="False">
                            <TextBlock Text="ยอดรวมสุทธิทั้งสิ้น"
                                       Foreground="#718096"
                                       FontSize="16" FontWeight="SemiBold"
                                       DockPanel.Dock="Left"
                                       VerticalAlignment="Center"/>
                            <TextBlock Text="{Binding SelectedOrder.TotalAmount, StringFormat={}{0:N2}}"
                                       FontSize="26" FontWeight="Bold"
                                       Foreground="#0B2545"
                                       DockPanel.Dock="Right"
                                       VerticalAlignment="Center"/>
                        </DockPanel>
                    </StackPanel>''',
        '''                    <!-- Payment Summary Section -->
                    <StackPanel Grid.Row="5" Margin="0,0,0,16">
                        <Grid Margin="0,0,0,4">
                            <TextBlock Text="รับเงินสด" Foreground="#718096" FontSize="15"/>
                            <TextBlock Text="{Binding SelectedOrder.CashReceived, StringFormat={}{0:N2}}"
                                       FontSize="16" FontWeight="SemiBold" Foreground="#2D3748"
                                       HorizontalAlignment="Right"/>
                        </Grid>
                        <Grid Margin="0,0,0,6">
                            <TextBlock Text="เงินทอน" Foreground="#718096" FontSize="15"/>
                            <TextBlock Text="{Binding SelectedOrder.Change, StringFormat={}{0:N2}}"
                                       FontSize="16" FontWeight="SemiBold" Foreground="#D69E2E"
                                       HorizontalAlignment="Right"/>
                        </Grid>
                        <Border Height="1" Background="#BAE6FD" Margin="0,4,0,8"/>
                        <DockPanel LastChildFill="False">
                            <TextBlock Text="ยอดรวมสุทธิทั้งสิ้น"
                                       Foreground="#718096"
                                       FontSize="17" FontWeight="SemiBold"
                                       DockPanel.Dock="Left"
                                       VerticalAlignment="Center"/>
                            <TextBlock Text="{Binding SelectedOrder.TotalAmount, StringFormat={}{0:N2}}"
                                       FontSize="28" FontWeight="Bold"
                                       Foreground="#0B2545"
                                       DockPanel.Dock="Right"
                                       VerticalAlignment="Center"/>
                        </DockPanel>
                    </StackPanel>'''
    )

    # Footer Action Buttons
    content = content.replace(
        '''                        <!-- Close Button -->
                        <Button Grid.Column="0"
                                Content="ปิดหน้าต่าง"
                                Command="{Binding CloseDetailModalCommand}"
                                Style="{StaticResource PurpleBtnStyle}"
                                Background="#E0F2FE"
                                Foreground="#718096"
                                BorderBrush="#BAE6FD"
                                BorderThickness="1"
                                Height="42"/>

                        <!-- Print Receipt Button -->
                        <Button Grid.Column="2"
                                x:Name="PrintReceiptBtn"
                                Content="🖨️  พิมพ์ใบเสร็จ"
                                Click="PrintReceiptBtn_Click"
                                Style="{StaticResource PurpleBtnStyle}"
                                Background="#00A3E0"
                                Foreground="White"
                                Height="42"/>''',
        '''                        <!-- Close Button -->
                        <Button Grid.Column="0"
                                Content="ปิดหน้าต่าง"
                                Command="{Binding CloseDetailModalCommand}"
                                Style="{StaticResource PurpleBtnStyle}"
                                Background="#E0F2FE"
                                Foreground="#718096"
                                BorderBrush="#BAE6FD"
                                BorderThickness="1"
                                Height="46"
                                FontSize="16"/>

                        <!-- Print Receipt Button -->
                        <Button Grid.Column="2"
                                x:Name="PrintReceiptBtn"
                                Content="🖨️  พิมพ์ใบเสร็จ"
                                Click="PrintReceiptBtn_Click"
                                Style="{StaticResource PurpleBtnStyle}"
                                Background="#00A3E0"
                                Foreground="White"
                                Height="46"
                                FontSize="16"/>'''
    )

    with open(file_path, "w", encoding="utf-8") as f:
        f.write(content)

    print("Successfully updated SalesHistoryView.xaml")

if __name__ == "__main__":
    update_sales_history()
