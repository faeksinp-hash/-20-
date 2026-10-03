using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dapper;
using Porjai20.Models;
using Porjai20.Services;
using Porjai20.Views;

namespace Porjai20.ViewModels
{
    /// <summary>
    /// ViewModel for the Sales History screen (SalesHistoryView.xaml).
    /// Handles KPI cards, filtering, searching, and detail lookup.
    /// </summary>
    public class SalesHistoryViewModel : ViewModelBase
    {
        // ── Services ─────────────────────────────────────────────────────
        private readonly DatabaseService _db;
        private readonly IDialogService _dialogService;

        // ── Collections ──────────────────────────────────────────────────
        /// <summary>Master list bound to the main DataGrid.</summary>
        public ObservableCollection<SalesOrderDisplayRow> SalesOrders { get; } = new();

        /// <summary>Items for the selected order (detail panel).</summary>
        public ObservableCollection<SalesOrderItem> SelectedOrderItems { get; } = new();

        /// <summary>Payment method filter options.</summary>
        public ObservableCollection<string> PaymentMethodOptions { get; } = new()
        {
            "ทั้งหมด", "เงินสด", "โอนเงิน", "บัตรเครดิต"
        };

        /// <summary>Date range filter options.</summary>
        public ObservableCollection<string> DateFilterOptions { get; } = new()
        {
            "วันที่ทั้งหมด", "วันนี้", "7 วันล่าสุด", "เดือนนี้", "ปีนี้"
        };

        private string _selectedDateFilter = "วันที่ทั้งหมด";
        public string SelectedDateFilter
        {
            get => _selectedDateFilter;
            set
            {
                if (SetProperty(ref _selectedDateFilter, value))
                {
                    ApplyDateFilterRange();
                    ExecuteSearch();
                }
            }
        }

        /// <summary>Alias for SalesOrders to match standard footer bindings.</summary>
        public ObservableCollection<SalesOrderDisplayRow> FilteredSales => SalesOrders;

        // ── KPI Properties ───────────────────────────────────────────────
        private decimal _totalRevenue;
        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set => SetProperty(ref _totalRevenue, value);
        }

        private int _totalOrders;
        public int TotalOrders
        {
            get => _totalOrders;
            set => SetProperty(ref _totalOrders, value);
        }

        private decimal _averageTicket;
        public decimal AverageTicket
        {
            get => _averageTicket;
            set => SetProperty(ref _averageTicket, value);
        }

        private string _revenueSubLabel = "ตามช่วงเวลาที่เลือก";
        public string RevenueSubLabel
        {
            get => _revenueSubLabel;
            set => SetProperty(ref _revenueSubLabel, value);
        }

        private string _ordersSubLabel = "ตามช่วงเวลาที่เลือก";
        public string OrdersSubLabel
        {
            get => _ordersSubLabel;
            set => SetProperty(ref _ordersSubLabel, value);
        }

        private string _averageSubLabel = "ต่อบิลเฉลี่ย";
        public string AverageSubLabel
        {
            get => _averageSubLabel;
            set => SetProperty(ref _averageSubLabel, value);
        }

        // ── Filter Properties ────────────────────────────────────────────
        private DateTime _startDate = DateTime.Today;
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                {
                    ExecuteSearch();
                }
            }
        }

        private DateTime _endDate = DateTime.Today;
        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                {
                    ExecuteSearch();
                }
            }
        }

        private string _searchKeyword = string.Empty;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set => SetProperty(ref _searchKeyword, value);
        }

        private string _selectedPaymentMethod = "ทั้งหมด";
        public string SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                if (SetProperty(ref _selectedPaymentMethod, value))
                {
                    ExecuteSearch();
                }
            }
        }

        // ── Selection & State ────────────────────────────────────────────
        private SalesOrderDisplayRow _selectedOrder;
        public SalesOrderDisplayRow SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (SetProperty(ref _selectedOrder, value))
                {
                    OnPropertyChanged(nameof(SelectedSale));
                    OnPropertyChanged(nameof(IsOrderSelected));
                    OnPropertyChanged(nameof(CanVoidOrder));
                    OnPropertyChanged(nameof(StatusBarText));
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                    _ = LoadOrderItemsAsync(_selectedOrder?.Id ?? 0);
                }
            }
        }

        public SalesOrderDisplayRow SelectedSale => SelectedOrder;

        public bool IsOrderSelected => _selectedOrder != null;

        private bool _isDetailModalOpen;
        public bool IsDetailModalOpen
        {
            get => _isDetailModalOpen;
            set
            {
                if (SetProperty(ref _isDetailModalOpen, value) && !value)
                {
                    _selectedOrder = null;
                    OnPropertyChanged(nameof(SelectedOrder));
                    OnPropertyChanged(nameof(IsOrderSelected));
                    OnPropertyChanged(nameof(CanVoidOrder));
                }
            }
        }

        public bool CanVoidOrder =>
            IsOrderSelected &&
            (_selectedOrder.Status == "ชำระเงินแล้ว" || _selectedOrder.Status == "Completed");

        private string _statusBarText = "แสดงผล 0 รายการ";
        public string StatusBarText
        {
            get => _statusBarText;
            set => SetProperty(ref _statusBarText, value);
        }

        // ── Commands ─────────────────────────────────────────────────────
        public ICommand SearchCommand           { get; }
        public ICommand ClearFilterCommand      { get; }
        public ICommand ViewDetailsCommand      { get; }
        public ICommand CloseDetailCommand      { get; }
        public ICommand CloseDetailModalCommand { get; }
        public ICommand PrintOrderCommand       { get; }
        public ICommand PrintReceiptCommand     { get; }
        public ICommand VoidOrderCommand        { get; }
        public ICommand ExportExcelCommand      { get; }
        public ICommand QuickFilterTodayCommand { get; }
        public ICommand QuickFilterWeekCommand  { get; }
        public ICommand QuickFilterMonthCommand { get; }

        // ── Universal In-Page Alert Modal Properties ──────────────────────────
        private bool _isMainAlertOpen;
        public bool IsMainAlertOpen
        {
            get => _isMainAlertOpen;
            set => SetProperty(ref _isMainAlertOpen, value);
        }

        private string _mainAlertTitle = "แจ้งเตือน";
        public string MainAlertTitle
        {
            get => _mainAlertTitle;
            set => SetProperty(ref _mainAlertTitle, value);
        }

        private string _mainAlertMessage = string.Empty;
        public string MainAlertMessage
        {
            get => _mainAlertMessage;
            set => SetProperty(ref _mainAlertMessage, value);
        }

        private string _mainAlertIcon = "⚠️";
        public string MainAlertIcon
        {
            get => _mainAlertIcon;
            set => SetProperty(ref _mainAlertIcon, value);
        }

        public ICommand CloseMainAlertCommand { get; set; }

        public void ShowAlert(string message, string title = "แจ้งเตือน", string icon = "⚠️")
        {
            MainAlertTitle = title;
            MainAlertMessage = message;
            MainAlertIcon = icon;
            IsMainAlertOpen = true;

            if (icon == "❌")
            {
                _dialogService.ShowError(title, message);
            }
            else if (icon == "✅")
            {
                _dialogService.ShowSuccess(title, message);
            }
            else
            {
                _dialogService.ShowWarning(title, message);
            }
        }

        // ── Universal In-Page Confirm Modal Properties ────────────────────────
        private bool _isMainConfirmOpen;
        public bool IsMainConfirmOpen
        {
            get => _isMainConfirmOpen;
            set => SetProperty(ref _isMainConfirmOpen, value);
        }

        private string _mainConfirmTitle = "ยืนยันการทำรายการ";
        public string MainConfirmTitle
        {
            get => _mainConfirmTitle;
            set => SetProperty(ref _mainConfirmTitle, value);
        }

        private string _mainConfirmMessage = string.Empty;
        public string MainConfirmMessage
        {
            get => _mainConfirmMessage;
            set => SetProperty(ref _mainConfirmMessage, value);
        }

        private System.Action? _pendingConfirmAction;

        public ICommand ConfirmMainActionCommand { get; set; }
        public ICommand CancelMainActionCommand { get; set; }

        public void ShowConfirm(string message, System.Action onConfirm, string title = "ยืนยันการทำรายการ")
        {
            MainConfirmTitle = title;
            MainConfirmMessage = message;
            _pendingConfirmAction = onConfirm;
            IsMainConfirmOpen = true;

            if (_dialogService.ShowConfirm(title, message))
            {
                _pendingConfirmAction?.Invoke();
                _pendingConfirmAction = null;
                IsMainConfirmOpen = false;
            }
        }

        // ── Constructors ──────────────────────────────────────────────────
        /// <summary>
        /// Parameterless constructor required by WPF XAML parser.
        /// </summary>
        public SalesHistoryViewModel() : this(new DatabaseService(), DialogService.Instance)
        {
        }

        public SalesHistoryViewModel(IDialogService dialogService) : this(new DatabaseService(), dialogService)
        {
        }

        public SalesHistoryViewModel(DatabaseService? db, IDialogService? dialogService)
        {
            _db = db ?? new DatabaseService();
            _dialogService = dialogService ?? DialogService.Instance;

            CloseMainAlertCommand = new RelayCommand(_ => IsMainAlertOpen = false);
            ConfirmMainActionCommand = new RelayCommand(_ =>
            {
                IsMainConfirmOpen = false;
                _pendingConfirmAction?.Invoke();
                _pendingConfirmAction = null;
            });
            CancelMainActionCommand = new RelayCommand(_ =>
            {
                IsMainConfirmOpen = false;
                _pendingConfirmAction = null;
            });


            // Wire up commands
            SearchCommand           = new RelayCommand(_ => ExecuteSearch());
            ClearFilterCommand      = new RelayCommand(_ => ExecuteClearFilter());
            ViewDetailsCommand      = new RelayCommand(p => ExecuteViewDetails(p as SalesOrderDisplayRow));
            CloseDetailModalCommand = new RelayCommand(_ => { IsDetailModalOpen = false; SelectedOrder = null; });
            CloseDetailCommand      = CloseDetailModalCommand;
            PrintOrderCommand       = new RelayCommand(_ => ExecutePrint(), _ => IsOrderSelected);
            PrintReceiptCommand     = new RelayCommand(_ => ExecutePrint());
            VoidOrderCommand        = new RelayCommand(_ => ExecuteVoid(), _ => CanVoidOrder);
            ExportExcelCommand      = new RelayCommand(_ => ExecuteExportExcel());
            QuickFilterTodayCommand = new RelayCommand(_ => SelectedDateFilter = "วันนี้");
            QuickFilterWeekCommand  = new RelayCommand(_ => SelectedDateFilter = "7 วันล่าสุด");
            QuickFilterMonthCommand = new RelayCommand(_ => SelectedDateFilter = "เดือนนี้");

            // Load all data on startup (Default: "วันที่ทั้งหมด")
            _selectedDateFilter = "วันที่ทั้งหมด";
            ExecuteSearch();
        }

        /// <summary>
        /// Public method called on view loaded or navigation to refresh sales history.
        /// </summary>
        public void LoadSalesHistory()
        {
            ExecuteSearch();
        }

        public void RefreshData()
        {
            ExecuteSearch();
        }

        // ── Data Loading ─────────────────────────────────────────────────

        /// <summary>
        /// Loads sales orders from SQLite matching the current filter criteria,
        /// then refreshes the KPI cards.
        /// </summary>
        private async void ExecuteSearch()
        {
            SalesOrders.Clear();
            SelectedOrder = null;

            try
            {
                using var conn = _db.GetConnection();

                // Build dynamic SQL
                var sql = @"SELECT
                                s.Sales_ID AS Sales_ID,
                                s.Sales_ID AS Id,
                                s.RefNo,
                                s.Sales_Date,
                                s.Cus_ID,
                                s.Sales_Total AS TotalAmount,
                                s.Sales_Cash AS CashReceived,
                                s.Sales_Change AS Change,
                                COALESCE(s.Sales_PaymentType, 'เงินสด') AS PaymentMethod,
                                COALESCE(s.Sales_Status, 'ชำระเงินแล้ว') AS Status,
                                COUNT(i.Detail_ID) AS ItemCount,
                                c.Cus_ID AS SplitCusId,
                                c.Cus_ID AS Cus_ID,
                                c.Cus_ID AS Id,
                                c.Cus_Code AS Code,
                                COALESCE(c.Cus_Name, 'ลูกค้าทั่วไป') AS Cus_Name,
                                COALESCE(c.Cus_Name, 'ลูกค้าทั่วไป') AS Name,
                                c.Cus_Address AS Address,
                                c.Cus_Tel AS Phone,
                                COALESCE(c.Cus_Points, 0) AS Points
                            FROM tblSales_H s
                            LEFT JOIN tblSalesDetail i ON i.Sales_ID = s.Sales_ID
                            LEFT JOIN tblCustomer c ON s.Cus_ID = c.Cus_ID
                            WHERE 1=1";

                var parameters = new DynamicParameters();

                // Date range filter (if not "วันที่ทั้งหมด")
                if (SelectedDateFilter != "วันที่ทั้งหมด")
                {
                    sql += @" AND (
                                date(s.Sales_Date) BETWEEN date(@Start) AND date(@End)
                                OR date(s.Sales_Date) BETWEEN date(@StartTh) AND date(@EndTh)
                            )";
                    parameters.Add("Start",   StartDate.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                    parameters.Add("End",     EndDate.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                    parameters.Add("StartTh", StartDate.Date.AddYears(543).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                    parameters.Add("EndTh",   EndDate.Date.AddYears(543).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                }

                // Keyword filter
                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    sql += " AND (s.RefNo LIKE @Kw OR COALESCE(c.Cus_Name, 'ลูกค้าทั่วไป') LIKE @Kw OR c.Cus_Tel LIKE @Kw)";
                    parameters.Add("Kw", $"%{SearchKeyword.Trim()}%");
                }

                // Payment method filter
                if (!string.IsNullOrWhiteSpace(SelectedPaymentMethod) &&
                    SelectedPaymentMethod != "ทั้งหมด")
                {
                    if (SelectedPaymentMethod == "เงินสด")
                    {
                        sql += @" AND (s.Sales_PaymentType IS NULL 
                                    OR TRIM(s.Sales_PaymentType) = '' 
                                    OR s.Sales_PaymentType LIKE '%เงินสด%' 
                                    OR s.Sales_PaymentType LIKE '%Cash%')";
                    }
                    else if (SelectedPaymentMethod == "โอนเงิน")
                    {
                        sql += @" AND (s.Sales_PaymentType LIKE '%โอน%' 
                                    OR s.Sales_PaymentType LIKE '%QR%' 
                                    OR s.Sales_PaymentType LIKE '%Transfer%' 
                                    OR s.Sales_PaymentType LIKE '%สแกน%' 
                                    OR s.Sales_PaymentType LIKE '%PromptPay%')";
                    }
                    else if (SelectedPaymentMethod == "บัตรเครดิต")
                    {
                        sql += @" AND (s.Sales_PaymentType LIKE '%บัตร%' 
                                    OR s.Sales_PaymentType LIKE '%Credit%' 
                                    OR s.Sales_PaymentType LIKE '%Card%' 
                                    OR s.Sales_PaymentType LIKE '%เดบิต%')";
                    }
                    else
                    {
                        sql += " AND s.Sales_PaymentType = @PayMethod";
                        parameters.Add("PayMethod", SelectedPaymentMethod);
                    }
                }

                sql += " GROUP BY s.Sales_ID ORDER BY s.Sales_Date DESC";

                var rows = await conn.QueryAsync<SalesOrderDisplayRow, Customer, SalesOrderDisplayRow>(
                    sql,
                    (order, customer) =>
                    {
                        order.Customer = customer;
                        if (customer != null && !string.IsNullOrWhiteSpace(customer.Name))
                        {
                            order.CustomerName = customer.Name;
                        }
                        else
                        {
                            order.CustomerName = "ลูกค้าทั่วไป";
                        }

                        // Normalize payment method for consistent badge display and receipt dialog
                        var rawPay = order.PaymentMethod ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(rawPay) || 
                            rawPay.Contains("เงินสด", StringComparison.OrdinalIgnoreCase) || 
                            rawPay.Contains("Cash", StringComparison.OrdinalIgnoreCase))
                        {
                            order.PaymentMethod = "เงินสด";
                        }
                        else if (rawPay.Contains("โอน", StringComparison.OrdinalIgnoreCase) || 
                                 rawPay.Contains("QR", StringComparison.OrdinalIgnoreCase) || 
                                 rawPay.Contains("Transfer", StringComparison.OrdinalIgnoreCase) || 
                                 rawPay.Contains("สแกน", StringComparison.OrdinalIgnoreCase) ||
                                 rawPay.Contains("PromptPay", StringComparison.OrdinalIgnoreCase))
                        {
                            order.PaymentMethod = "โอนเงิน";
                        }
                        else if (rawPay.Contains("บัตร", StringComparison.OrdinalIgnoreCase) || 
                                 rawPay.Contains("Credit", StringComparison.OrdinalIgnoreCase) || 
                                 rawPay.Contains("Card", StringComparison.OrdinalIgnoreCase) || 
                                 rawPay.Contains("เดบิต", StringComparison.OrdinalIgnoreCase))
                        {
                            order.PaymentMethod = "บัตรเครดิต";
                        }

                        return order;
                    },
                    parameters,
                    splitOn: "SplitCusId"
                );

                foreach (var row in rows)
                {
                    SalesOrders.Add(row);
                }

                // Update KPIs
                RefreshKpis();
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                ShowAlert($"เกิดข้อผิดพลาดในการโหลดข้อมูล:\n{ex.Message}", "ข้อผิดพลาด", "❌");
            }
        }

        /// <summary>Loads line items for the given order ID into SelectedOrderItems.</summary>
        public async Task LoadOrderItemsAsync(int orderId)
        {
            SelectedOrderItems.Clear();
            if (orderId == 0) return;

            try
            {
                using var conn = _db.GetConnection();
                const string sql = @"
                    SELECT 
                        d.Detail_ID AS Detail_ID, 
                        d.Detail_ID AS Id, 
                        d.Sales_ID, 
                        d.Pro_ID, 
                        COALESCE(p.Pro_Name, CAST(d.Pro_ID AS TEXT)) AS ProductName, 
                        d.Pro_Price AS UnitPrice, 
                        d.Sales_Qty AS Quantity, 
                        d.Sales_Subtotal AS Total 
                    FROM tblSalesDetail d
                    LEFT JOIN tblProduct p ON d.Pro_ID = p.Pro_ID
                    WHERE d.Sales_ID = @Id 
                    ORDER BY d.Detail_ID";
                var items = await conn.QueryAsync<SalesOrderItem>(sql, new { Id = orderId });
                foreach (var item in items)
                    SelectedOrderItems.Add(item);
            }
            catch (Exception ex)
            {
                ShowAlert($"โหลดรายการสินค้าล้มเหลว:\n{ex.Message}", "ข้อผิดพลาด", "❌");
            }
        }

        // ── KPI Calculations ─────────────────────────────────────────────
        private void RefreshKpis()
        {
            TotalRevenue  = SalesOrders.Where(o => o.Status == "ชำระเงินแล้ว" || o.Status == "Completed" || string.IsNullOrWhiteSpace(o.Status)).Sum(o => o.TotalAmount);
            TotalOrders   = SalesOrders.Count(o => o.Status == "ชำระเงินแล้ว" || o.Status == "Completed" || string.IsNullOrWhiteSpace(o.Status));
            AverageTicket = TotalOrders > 0 ? TotalRevenue / TotalOrders : 0m;

            // Sub-labels
            string label;
            if (SelectedDateFilter == "วันที่ทั้งหมด")
            {
                label = "ข้อมูลทั้งหมด";
            }
            else if (SelectedDateFilter == "วันนี้")
            {
                label = $"วันที่ {DateTime.Today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}";
            }
            else if (SelectedDateFilter == "7 วันล่าสุด")
            {
                label = $"{StartDate.ToString("dd/MM/yy", CultureInfo.InvariantCulture)} – {EndDate.ToString("dd/MM/yy", CultureInfo.InvariantCulture)}";
            }
            else if (SelectedDateFilter == "เดือนนี้")
            {
                label = $"เดือน {DateTime.Today.ToString("MM/yyyy", CultureInfo.InvariantCulture)}";
            }
            else if (SelectedDateFilter == "ปีนี้")
            {
                label = $"ปี {DateTime.Today.Year}";
            }
            else
            {
                label = StartDate.Date == EndDate.Date
                    ? $"วันที่ {StartDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}"
                    : $"{StartDate.ToString("dd/MM/yy", CultureInfo.InvariantCulture)} – {EndDate.ToString("dd/MM/yy", CultureInfo.InvariantCulture)}";
            }

            RevenueSubLabel  = label;
            OrdersSubLabel   = label;
            AverageSubLabel  = $"จาก {TotalOrders} บิล";
        }

        private void UpdateStatusBar()
        {
            StatusBarText = $"แสดงผล {SalesOrders.Count:N0} รายการ";
            OnPropertyChanged(nameof(FilteredSales));
        }

        // ── Command Handlers ─────────────────────────────────────────────
        private void ExecuteClearFilter()
        {
            _searchKeyword         = string.Empty;
            _selectedPaymentMethod = "ทั้งหมด";
            _selectedDateFilter    = "วันที่ทั้งหมด";
            ApplyDateFilterRange();
            OnPropertyChanged(nameof(SearchKeyword));
            OnPropertyChanged(nameof(SelectedPaymentMethod));
            OnPropertyChanged(nameof(SelectedDateFilter));
            ExecuteSearch();
        }

        private void ApplyDateFilterRange()
        {
            switch (SelectedDateFilter)
            {
                case "วันนี้":
                    _startDate = DateTime.Today;
                    _endDate = DateTime.Today;
                    break;
                case "7 วันล่าสุด":
                    _startDate = DateTime.Today.AddDays(-7);
                    _endDate = DateTime.Today;
                    break;
                case "เดือนนี้":
                    _startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    _endDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
                    break;
                case "ปีนี้":
                    _startDate = new DateTime(DateTime.Today.Year, 1, 1);
                    _endDate = new DateTime(DateTime.Today.Year, 12, 31);
                    break;
                case "วันที่ทั้งหมด":
                default:
                    _startDate = DateTime.Today;
                    _endDate = DateTime.Today;
                    break;
            }
            OnPropertyChanged(nameof(StartDate));
            OnPropertyChanged(nameof(EndDate));
        }

        private void ExecuteViewDetails(SalesOrderDisplayRow row)
        {
            if (row == null) return;
            SelectedOrder = row;
            IsDetailModalOpen = true;
        }

        public async Task ExecutePrintAsync(Window? explicitOwner = null)
        {
            if (SelectedOrder == null) return;

            try
            {
                if (SelectedOrderItems == null || SelectedOrderItems.Count == 0)
                {
                    await LoadOrderItemsAsync(SelectedOrder.Id);
                }

                // Format payment cash/change matching POS logic
                bool isCash = string.Equals(SelectedOrder.PaymentMethod, "เงินสด", StringComparison.OrdinalIgnoreCase)
                           || string.Equals(SelectedOrder.PaymentMethod, "Cash", StringComparison.OrdinalIgnoreCase);

                decimal effectiveCash;
                decimal effectiveChange;

                if (isCash)
                {
                    effectiveCash = SelectedOrder.CashReceived > 0 ? SelectedOrder.CashReceived : SelectedOrder.TotalAmount;
                    effectiveChange = SelectedOrder.Change >= 0 ? SelectedOrder.Change : (effectiveCash - SelectedOrder.TotalAmount);
                    if (effectiveChange < 0) effectiveChange = 0;
                }
                else
                {
                    effectiveCash = SelectedOrder.TotalAmount;
                    effectiveChange = 0;
                }

                var order = new SalesOrder
                {
                    Sales_ID = SelectedOrder.Id,
                    RefNo = !string.IsNullOrWhiteSpace(SelectedOrder.RefNo) ? SelectedOrder.RefNo : $"SALE-{SelectedOrder.Id:D6}",
                    Sales_Date = SelectedOrder.Sales_Date,
                    CustomerName = !string.IsNullOrWhiteSpace(SelectedOrder.CustomerDisplayName) ? SelectedOrder.CustomerDisplayName : "ลูกค้าทั่วไป",
                    TotalAmount = SelectedOrder.TotalAmount,
                    CashReceived = effectiveCash,
                    Change = effectiveChange,
                    PaymentMethod = !string.IsNullOrWhiteSpace(SelectedOrder.PaymentMethod) ? SelectedOrder.PaymentMethod : "เงินสด",
                    Status = !string.IsNullOrWhiteSpace(SelectedOrder.Status) ? SelectedOrder.Status : "ชำระเงินแล้ว"
                };

                var items = SelectedOrderItems.Select(item => new SalesOrderItem
                {
                    Pro_ID = item.Pro_ID,
                    ProductName = !string.IsNullOrWhiteSpace(item.ProductName) ? item.ProductName : $"สินค้า #{item.Pro_ID}",
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    Total = item.Total
                }).ToList();

                var receiptWindow = new ReceiptWindow(order, items);
                receiptWindow.Owner = explicitOwner
                                     ?? Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                                     ?? Application.Current?.MainWindow;
                receiptWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                ShowAlert($"เกิดข้อผิดพลาดในการพิมพ์ใบเสร็จ:\n{ex.Message}", "ข้อผิดพลาด", "❌");
            }
        }

        private async void ExecutePrint()
        {
            await ExecutePrintAsync(Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) ?? Application.Current?.MainWindow);
        }

        private void ExecuteVoid()
        {
            if (!CanVoidOrder) return;

            ShowConfirm(
                $"ยืนยันการยกเลิกบิล {SelectedOrder.RefNo}?\nการดำเนินการนี้ไม่สามารถย้อนกลับได้",
                async () =>
                {
                    try
                    {
                        using var conn = _db.GetConnection();
                        const string sql = "UPDATE tblSales_H SET Sales_Status = 'ยกเลิก' WHERE Sales_ID = @Id";
                        await conn.ExecuteAsync(sql, new { Id = SelectedOrder.Id });

                        // Refresh
                        ExecuteSearch();
                    }
                    catch (Exception ex)
                    {
                        ShowAlert($"ยกเลิกบิลล้มเหลว:\n{ex.Message}", "ข้อผิดพลาด", "❌");
                    }
                },
                "ยืนยันการยกเลิก");
        }

        private void ExecuteExportExcel()
        {
            if (SalesOrders == null || SalesOrders.Count == 0)
            {
                Views.CustomMessageBox.Show("ไม่พบรายการบิลที่ต้องการส่งออก กรุณาค้นหาหรือกรองข้อมูลก่อน", "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string defaultFileName = $"รายงานประวัติการขาย_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            string? filePath = ExcelExportService.PromptSaveExcelPath(defaultFileName);
            if (string.IsNullOrWhiteSpace(filePath)) return;

            try
            {
                string periodInfo = StartDate.Date == EndDate.Date
                    ? $"วันที่ {StartDate:dd/MM/yyyy}"
                    : $"{StartDate:dd/MM/yyyy} – {EndDate:dd/MM/yyyy}";

                ExcelExportService.ExportSalesOrders(SalesOrders, periodInfo, filePath);

                var res = Views.CustomMessageBox.Show(
                    $"บันทึกไฟล์รายงาน Excel เรียบร้อยแล้ว\nที่อยู่ไฟล์: {filePath}\n\nคุณต้องการเปิดไฟล์ขึ้นมาดูทันทีหรือไม่?",
                    "ส่งออกข้อมูลสำเร็จ",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (res == MessageBoxResult.Yes)
                {
                    if (!ExcelExportService.TryOpenFile(filePath, out string? err))
                    {
                        Views.CustomMessageBox.Show($"ไม่สามารถเปิดไฟล์ได้: {err}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (System.IO.IOException)
            {
                Views.CustomMessageBox.Show("ไม่สามารถบันทึกไฟล์ได้ เนื่องจากไฟล์นี้กำลังถูกเปิดใช้งานอยู่ในโปรแกรมอื่น (เช่น Microsoft Excel)\n\nกรุณาปิดไฟล์ดังกล่าวแล้วลองกดบันทึกใหม่อีกครั้ง", "ไม่สามารถบันทึกไฟล์ได้", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                Views.CustomMessageBox.Show($"เกิดข้อผิดพลาดในการส่งออก Excel: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void QuickFilter(DateTime start, DateTime end)
        {
            _startDate = start;
            _endDate   = end;
            OnPropertyChanged(nameof(StartDate));
            OnPropertyChanged(nameof(EndDate));
            ExecuteSearch();
        }
    }

    // ── Display Row Model (wraps SalesOrder + derived ItemCount) ────────
    /// <summary>
    /// Projection model used in the DataGrid.
    /// Extends SalesOrder with the pre-computed ItemCount column.
    /// </summary>
    public class SalesOrderDisplayRow : SalesOrder
    {
        /// <summary>Total number of line items in this order.</summary>
        public int ItemCount { get; set; }
    }
}
