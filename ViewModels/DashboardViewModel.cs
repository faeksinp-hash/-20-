using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Dapper;
using LiveCharts;
using LiveCharts.Wpf;
using Porjai20.Common;
using Porjai20.Models;
using Porjai20.Services;
using Porjai20.Views;

namespace Porjai20.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly DatabaseService _databaseService;

        // ═══════════════════════════════════════════════════════════════
        // HEADER PROPERTIES
        // ═══════════════════════════════════════════════════════════════
        private string _currentDateThai = string.Empty;
        public string CurrentDateThai
        {
            get => _currentDateThai;
            set => SetProperty(ref _currentDateThai, value);
        }

        // ═══════════════════════════════════════════════════════════════
        // ROW 1: KPI CARDS
        // ═══════════════════════════════════════════════════════════════
        // Card 1: ยอดขายวันนี้
        private string _todaySales = "0.00";
        public string TodaySales
        {
            get => _todaySales;
            set => SetProperty(ref _todaySales, value);
        }

        private string _todaySalesGrowthText = "+0.0% เทียบเมื่อวาน";
        public string TodaySalesGrowthText
        {
            get => _todaySalesGrowthText;
            set => SetProperty(ref _todaySalesGrowthText, value);
        }

        private bool _isSalesGrowthPositive = true;
        public bool IsSalesGrowthPositive
        {
            get => _isSalesGrowthPositive;
            set => SetProperty(ref _isSalesGrowthPositive, value);
        }

        // Card 2: กำไรโดยประมาณ
        private string _estimatedProfit = "0.00";
        public string EstimatedProfit
        {
            get => _estimatedProfit;
            set => SetProperty(ref _estimatedProfit, value);
        }

        // Card 3: บิลวันนี้
        private string _todayBillsCount = "0";
        public string TodayBillsCount
        {
            get => _todayBillsCount;
            set => SetProperty(ref _todayBillsCount, value);
        }

        private string _averagePerBillText = "เฉลี่ยต่อบิล: 0.00 บาท";
        public string AveragePerBillText
        {
            get => _averagePerBillText;
            set => SetProperty(ref _averagePerBillText, value);
        }

        // Card 4: สินค้าที่ขายแล้ว & ยอดจากสมาชิก
        private string _todayItemsSold = "0";
        public string TodayItemsSold
        {
            get => _todayItemsSold;
            set => SetProperty(ref _todayItemsSold, value);
        }

        private string _itemsPerBillText = "เฉลี่ย 0.0 ชิ้น/บิล";
        public string ItemsPerBillText
        {
            get => _itemsPerBillText;
            set => SetProperty(ref _itemsPerBillText, value);
        }

        private string _memberSalesToday = "0.00";
        public string MemberSalesToday
        {
            get => _memberSalesToday;
            set => SetProperty(ref _memberSalesToday, value);
        }

        private string _memberSalesPercentText = "คิดเป็น 0.0% ของยอดขาย";
        public string MemberSalesPercentText
        {
            get => _memberSalesPercentText;
            set => SetProperty(ref _memberSalesPercentText, value);
        }

        // ═══════════════════════════════════════════════════════════════
        // ROW 2: PENDING TASKS (ต้องทำวันนี้ - 4 หมวดหมู่งานหลัก)
        // ═══════════════════════════════════════════════════════════════
        private int _pendingDeliveryCount;
        public int PendingDeliveryCount
        {
            get => _pendingDeliveryCount;
            set { SetProperty(ref _pendingDeliveryCount, value); OnPropertyChanged(nameof(HasPendingDelivery)); NotifyTaskChanges(); }
        }

        private int _pendingClaimCount;
        public int PendingClaimCount
        {
            get => _pendingClaimCount;
            set { SetProperty(ref _pendingClaimCount, value); OnPropertyChanged(nameof(HasPendingClaim)); NotifyTaskChanges(); }
        }

        private int _needReorderCount;
        public int NeedReorderCount
        {
            get => _needReorderCount;
            set { SetProperty(ref _needReorderCount, value); OnPropertyChanged(nameof(HasNeedReorder)); NotifyTaskChanges(); }
        }

        private int _pendingReceiveCount;
        public int PendingReceiveCount
        {
            get => _pendingReceiveCount;
            set { SetProperty(ref _pendingReceiveCount, value); OnPropertyChanged(nameof(HasPendingReceive)); NotifyTaskChanges(); }
        }

        // Boolean Flags สำหรับควบคุม Visibility ของแต่ละแถว (Count > 0 ถึงจะแสดง)
        public bool HasPendingDelivery => PendingDeliveryCount > 0;
        public bool HasPendingClaim => PendingClaimCount > 0;
        public bool HasNeedReorder => NeedReorderCount > 0;
        public bool HasPendingReceive => PendingReceiveCount > 0;

        // Backward compatible aliases
        public int OutOfStockCount
        {
            get => NeedReorderCount;
            set => NeedReorderCount = value;
        }
        public bool HasOutOfStock => HasNeedReorder;
        public bool HasPendingClaims => HasPendingClaim;
        public int PendingReceiveOrderCount
        {
            get => PendingReceiveCount;
            set => PendingReceiveCount = value;
        }
        public bool HasPendingReceives => HasPendingReceive;

        // จำนวนประเภทงานที่มีงานค้างจริง (Active Categories Count)
        public int ActiveUrgentTasksCategoryCount => 
            (HasPendingDelivery ? 1 : 0) + 
            (HasPendingClaim ? 1 : 0) + 
            (HasNeedReorder ? 1 : 0) + 
            (HasPendingReceive ? 1 : 0);

        public int TotalUrgentTasksCount => ActiveUrgentTasksCategoryCount;
        public int TotalPendingTasks => ActiveUrgentTasksCategoryCount;

        public bool HasAnyPendingTasks => ActiveUrgentTasksCategoryCount > 0;
        public bool HasAnyUrgentTasks => HasAnyPendingTasks;

        private void NotifyTaskChanges()
        {
            OnPropertyChanged(nameof(ActiveUrgentTasksCategoryCount));
            OnPropertyChanged(nameof(TotalUrgentTasksCount));
            OnPropertyChanged(nameof(TotalPendingTasks));
            OnPropertyChanged(nameof(HasAnyPendingTasks));
            OnPropertyChanged(nameof(HasAnyUrgentTasks));
            OnPropertyChanged(nameof(HasOutOfStock));
            OnPropertyChanged(nameof(HasPendingClaims));
            OnPropertyChanged(nameof(HasPendingReceives));
        }

        // ═══════════════════════════════════════════════════════════════
        // ROW 2: HOURLY CHART
        // ═══════════════════════════════════════════════════════════════
        public SeriesCollection HourlySalesSeries { get; }
        private string[] _hourlyLabels = Array.Empty<string>();
        public string[] HourlyLabels
        {
            get => _hourlyLabels;
            set => SetProperty(ref _hourlyLabels, value);
        }

        public Func<double, string> IntegerFormatter { get; } = val =>
        {
            if (val < 0) return string.Empty;
            return ((int)Math.Round(val)).ToString(CultureInfo.InvariantCulture);
        };

        private bool _hasHourlyData;
        public bool HasHourlyData
        {
            get => _hasHourlyData;
            set => SetProperty(ref _hasHourlyData, value);
        }

        // ═══════════════════════════════════════════════════════════════
        // ROW 3: COLUMN 1 - CUSTOMER STATS
        // ═══════════════════════════════════════════════════════════════
        private int _newCustomersToday = 0;
        public int NewCustomersToday
        {
            get => _newCustomersToday;
            set
            {
                if (SetProperty(ref _newCustomersToday, value))
                {
                    OnPropertyChanged(nameof(NewMembersTodayCount));
                }
            }
        }
        public int NewMembersTodayCount => NewCustomersToday;

        private int _totalCustomers = 0;
        public int TotalCustomers
        {
            get => _totalCustomers;
            set
            {
                if (SetProperty(ref _totalCustomers, value))
                {
                    OnPropertyChanged(nameof(TotalMembersCount));
                }
            }
        }
        public int TotalMembersCount => TotalCustomers;

        private int _pointsGivenToday = 0;
        public int PointsGivenToday
        {
            get => _pointsGivenToday;
            set => SetProperty(ref _pointsGivenToday, value);
        }

        private int _pointsRedeemedToday = 0;
        public int PointsRedeemedToday
        {
            get => _pointsRedeemedToday;
            set => SetProperty(ref _pointsRedeemedToday, value);
        }

        private int _promotionsUsedToday = 0;
        public int PromotionsUsedToday
        {
            get => _promotionsUsedToday;
            set => SetProperty(ref _promotionsUsedToday, value);
        }

        // ═══════════════════════════════════════════════════════════════
        // ROW 3: COLUMN 2 - RECENT BILLS
        // ═══════════════════════════════════════════════════════════════
        public ObservableCollection<RecentBillDto> RecentBills { get; }

        private bool _hasRecentBills;
        public bool HasRecentBills
        {
            get => _hasRecentBills;
            set => SetProperty(ref _hasRecentBills, value);
        }

        // ═══════════════════════════════════════════════════════════════
        // ROW 3: COLUMN 3 - TOP SELLING PRODUCTS
        // ═══════════════════════════════════════════════════════════════
        public ObservableCollection<TopSellingProductDto> TopSellingProducts { get; }

        private List<TopSellingProductDto> _topSellingToday = new();
        private List<TopSellingProductDto> _topSelling7Days = new();

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private int _selectedPeriodDays = 1; // 1 = Today, 7 = 7 Days
        public int SelectedPeriodDays
        {
            get => _selectedPeriodDays;
            set
            {
                if (SetProperty(ref _selectedPeriodDays, value))
                {
                    OnPropertyChanged(nameof(IsTodaySelected));
                    OnPropertyChanged(nameof(IsWeekSelected));
                    OnPropertyChanged(nameof(IsPeriodTodaySelected));
                    OnPropertyChanged(nameof(IsPeriod7DaysSelected));
                    OnPropertyChanged(nameof(IsTodayFilterSelected));
                    OnPropertyChanged(nameof(IsWeekFilterSelected));
                }
            }
        }

        public bool IsTodaySelected => SelectedPeriodDays == 1;
        public bool IsWeekSelected => SelectedPeriodDays == 7;
        public bool IsPeriodTodaySelected => IsTodaySelected;
        public bool IsPeriod7DaysSelected => IsWeekSelected;

        public bool IsTodayFilterSelected
        {
            get => SelectedPeriodDays == 1;
            set
            {
                if (value && SelectedPeriodDays != 1)
                {
                    _ = LoadTopSellingProductsAsync(isSevenDays: false);
                }
            }
        }

        public bool IsWeekFilterSelected
        {
            get => SelectedPeriodDays == 7;
            set
            {
                if (value && SelectedPeriodDays != 7)
                {
                    _ = LoadTopSellingProductsAsync(isSevenDays: true);
                }
            }
        }

        private bool _hasTopSellingProducts;
        public bool HasTopSellingProducts
        {
            get => _hasTopSellingProducts;
            set => SetProperty(ref _hasTopSellingProducts, value);
        }

        // ═══════════════════════════════════════════════════════════════
        // COMMANDS
        // ═══════════════════════════════════════════════════════════════
        public ICommand NavigateToPOSCommand { get; }
        public ICommand GoToPosCommand => NavigateToPOSCommand;
        public ICommand SwitchToPosCommand => NavigateToPOSCommand;

        public ICommand NavigateToDeliveryCommand { get; }
        public ICommand OpenDeliveryCommand => NavigateToDeliveryCommand;

        public ICommand NavigateToPurchaseOrderCommand { get; }
        public ICommand OpenPurchaseOrderCommand => NavigateToPurchaseOrderCommand;

        public ICommand NavigateToClaimCommand { get; }
        public ICommand OpenClaimCommand => NavigateToClaimCommand;

        public ICommand NavigateToReceiveProductCommand { get; }
        public ICommand NavigateToStockInCommand => NavigateToReceiveProductCommand;
        public ICommand SwitchToStockInCommand => NavigateToReceiveProductCommand;

        public ICommand OpenCustomerCommand { get; }
        public ICommand NavigateToCustomerCommand => OpenCustomerCommand;

        public ICommand OpenSalesHistoryCommand { get; }
        public ICommand NavigateToSalesHistoryCommand => OpenSalesHistoryCommand;

        public ICommand SelectTodayTopSellingCommand { get; private set; }
        public ICommand SelectPeriodTodayCommand => SelectTodayTopSellingCommand;
        public ICommand FilterTodayCommand => SelectTodayTopSellingCommand;

        public ICommand SelectWeekTopSellingCommand { get; private set; }
        public ICommand SelectPeriod7DaysCommand => SelectWeekTopSellingCommand;
        public ICommand FilterWeekCommand => SelectWeekTopSellingCommand;

        public ICommand OpenBillReceiptCommand { get; }

        // ═══════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════════
        public DashboardViewModel()
        {
            _databaseService = new DatabaseService();

            UpdateThaiDateHeader();

            // Initialize Collections
            RecentBills = new ObservableCollection<RecentBillDto>();
            TopSellingProducts = new ObservableCollection<TopSellingProductDto>();

            // Setup Hourly Chart Series
            HourlySalesSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "วันนี้",
                    Values = new ChartValues<int>(),
                    Fill = new SolidColorBrush(Color.FromRgb(0x02, 0x84, 0xC7)), // ฟ้าเข้ม #0284C7
                    MaxColumnWidth = 14,
                    ColumnPadding = 2,
                    DataLabels = false
                },
                new ColumnSeries
                {
                    Title = "เมื่อวาน",
                    Values = new ChartValues<int>(),
                    Fill = new SolidColorBrush(Color.FromRgb(0xCB, 0xD5, 0xE1)), // เทาอ่อน #CBD5E1
                    MaxColumnWidth = 14,
                    ColumnPadding = 2,
                    DataLabels = false
                }
            };

            // Setup Navigation Commands
            NavigateToPOSCommand = new RelayCommand(_ => NavigateToView("pos"));
            NavigateToDeliveryCommand = new RelayCommand(_ => NavigateToView("delivery"));
            NavigateToPurchaseOrderCommand = new RelayCommand(_ => NavigateToView("purchase_order"));
            NavigateToClaimCommand = new RelayCommand(_ => NavigateToView("claim"));
            NavigateToReceiveProductCommand = new RelayCommand(_ => NavigateToView("stock_in"));
            OpenCustomerCommand = new RelayCommand(_ => NavigateToView("customer"));
            OpenSalesHistoryCommand = new RelayCommand(_ => NavigateToView("sales_history"));

            // Tab commands for Best Sellers (with robust try-catch and safe async loading)
            SelectTodayTopSellingCommand = new RelayCommand(async _ =>
            {
                try
                {
                    await LoadTopSellingProductsAsync(isSevenDays: false);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Dashboard] SelectTodayTopSellingCommand error: {ex.Message}");
                }
            });

            SelectWeekTopSellingCommand = new RelayCommand(async _ =>
            {
                try
                {
                    await LoadTopSellingProductsAsync(isSevenDays: true);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Dashboard] SelectWeekTopSellingCommand error: {ex.Message}");
                }
            });

            // Receipt modal command
            OpenBillReceiptCommand = new RelayCommand(async param =>
            {
                if (param is RecentBillDto bill)
                {
                    await OpenBillReceiptAsync(bill);
                }
            });

            // Initial async data load
            _ = LoadDashboardDataAsync();
        }

        private void UpdateThaiDateHeader()
        {
            var now = DateTime.Now;
            string[] thaiDays = { "อาทิตย์", "จันทร์", "อังคาร", "พุธ", "พฤหัสบดี", "ศุกร์", "เสาร์" };
            string[] thaiMonths = { "", "ม.ค.", "ก.พ.", "มี.ค.", "เม.ย.", "พ.ค.", "มิ.ย.", "ก.ค.", "ส.ค.", "ก.ย.", "ต.ค.", "พ.ย.", "ธ.ค." };
            int thaiYear = now.Year > 2400 ? now.Year : now.Year + 543;
            CurrentDateThai = $"{thaiDays[(int)now.DayOfWeek]} {now.Day} {thaiMonths[now.Month]} {thaiYear}";
        }

        private void NavigateToView(string viewName)
        {
            if (App.Current?.MainWindow?.DataContext is ProductViewModel mainVm)
            {
                switch (viewName)
                {
                    case "pos":
                        mainVm.SwitchToPosCommand?.Execute(null);
                        break;
                    case "delivery":
                        mainVm.CurrentView = "delivery";
                        break;
                    case "purchase_order":
                        if (mainVm.OpenPurchaseOrderCommand != null && mainVm.OpenPurchaseOrderCommand.CanExecute(null))
                            mainVm.OpenPurchaseOrderCommand.Execute(null);
                        else
                            mainVm.CurrentView = "purchase_order";
                        break;
                    case "claim":
                        if (mainVm.OpenClaimCommand != null && mainVm.OpenClaimCommand.CanExecute(null))
                            mainVm.OpenClaimCommand.Execute(null);
                        else
                            mainVm.CurrentView = "claim";
                        break;
                    case "stock_in":
                        if (mainVm.SwitchToStockInCommand != null && mainVm.SwitchToStockInCommand.CanExecute(null))
                            mainVm.SwitchToStockInCommand.Execute(null);
                        else
                            mainVm.CurrentView = "stock_in";
                        break;
                    case "customer":
                        if (mainVm.OpenCustomerCommand != null && mainVm.OpenCustomerCommand.CanExecute(null))
                            mainVm.OpenCustomerCommand.Execute(null);
                        else
                            mainVm.CurrentView = "customer";
                        break;
                    case "sales_history":
                        if (mainVm.OpenSalesHistoryCommand != null && mainVm.OpenSalesHistoryCommand.CanExecute(null))
                            mainVm.OpenSalesHistoryCommand.Execute(null);
                        else
                            mainVm.CurrentView = "sales_history";
                        break;
                    default:
                        mainVm.CurrentView = viewName;
                        break;
                }
            }
        }

        public static DateTime? ParseDateToInvariant(string? dateStr)
        {
            return DateTimeHelper.ParseDateToInvariant(dateStr);
        }

        public Task LoadData() => LoadDashboardDataAsync();

        // ═══════════════════════════════════════════════════════════════
        // MAIN DATA LOADING (SINGLE DATABASE CONNECTION BLOCK)
        // ═══════════════════════════════════════════════════════════════
        public async Task LoadDashboardDataAsync()
        {
            try
            {
                UpdateThaiDateHeader();

                using (var conn = _databaseService.GetConnection())
                {
                    var now = DateTime.Now;
                    var today = DateTime.Today;
                    var yesterday = today.AddDays(-1);

                    // 1) Query Active Sales Orders
                    var allSales = (await conn.QueryAsync<SalesOrderRecord>(
                        @"SELECT s.Sales_ID, s.RefNo, s.Sales_Date, s.Sales_Total, s.Sales_Status, s.Cus_ID, 
                                 s.DiscountAmount, s.PointsUsed, s.PointsEarned 
                          FROM tblSales_H s 
                          WHERE s.Sales_Status IS NULL OR (s.Sales_Status != 'ยกเลิก' AND s.Sales_Status != 'Cancelled')")).ToList();

                    foreach (var s in allSales)
                    {
                        s.NormalizedDate = ParseDateToInvariant(s.Sales_Date);
                    }

                    var salesTodayList = allSales.Where(s => s.NormalizedDate.HasValue && s.NormalizedDate.Value.Date == today.Date).ToList();
                    var salesYesterdayList = allSales.Where(s => s.NormalizedDate.HasValue && s.NormalizedDate.Value.Date == yesterday.Date).ToList();

                    // Sales up to the same time yesterday
                    var salesYesterdaySameTimeList = salesYesterdayList.Where(s => s.NormalizedDate.HasValue && s.NormalizedDate.Value.TimeOfDay <= now.TimeOfDay).ToList();

                    decimal salesToday = salesTodayList.Sum(s => (decimal)s.Sales_Total);
                    decimal salesYesterdaySameTime = salesYesterdaySameTimeList.Sum(s => (decimal)s.Sales_Total);
                    int ordersToday = salesTodayList.Count;

                    // Calculate Growth
                    decimal salesGrowth = 0m;
                    if (salesYesterdaySameTime > 0)
                    {
                        salesGrowth = ((salesToday - salesYesterdaySameTime) / salesYesterdaySameTime) * 100m;
                    }
                    else if (salesToday > 0)
                    {
                        salesGrowth = 100m;
                    }

                    IsSalesGrowthPositive = salesGrowth >= 0;
                    string sign = salesGrowth >= 0 ? "+" : "";
                    TodaySales = salesToday.ToString("N2", CultureInfo.InvariantCulture);
                    TodaySalesGrowthText = $"{sign}{salesGrowth:F1}% เทียบเมื่อวาน";

                    // 2) Query Sales Items & Costs for Today's Profit & Items count
                    decimal totalCostToday = 0m;
                    int totalItemsToday = 0;

                    if (salesTodayList.Count > 0)
                    {
                        var todayOrderIds = salesTodayList.Select(s => s.Sales_ID).ToList();
                        var detailRows = await conn.QueryAsync(
                            @"SELECT d.Sales_ID, d.Pro_ID, d.Sales_Qty, d.Sales_Subtotal,
                                     COALESCE(p.Pro_Cost, p.Pro_Price * 0.7, 0) AS UnitCost
                              FROM tblSalesDetail d
                              LEFT JOIN tblProduct p ON d.Pro_ID = p.Pro_ID
                              WHERE d.Sales_ID IN @Ids",
                            new { Ids = todayOrderIds });

                        foreach (var d in detailRows)
                        {
                            int qty = Convert.ToInt32(d.Sales_Qty);
                            decimal unitCost = Convert.ToDecimal(d.UnitCost);
                            totalItemsToday += qty;
                            totalCostToday += (qty * unitCost);
                        }
                    }

                    decimal profitToday = salesToday - totalCostToday;
                    EstimatedProfit = profitToday.ToString("N2", CultureInfo.InvariantCulture);

                    // 3) Today Bills & Averages
                    TodayBillsCount = ordersToday.ToString();
                    decimal avgPerBill = ordersToday > 0 ? (salesToday / ordersToday) : 0m;
                    AveragePerBillText = $"เฉลี่ยต่อบิล: {avgPerBill:N2} บาท";

                    // 4) Items Sold & Member Sales
                    TodayItemsSold = totalItemsToday.ToString();
                    double avgItems = ordersToday > 0 ? ((double)totalItemsToday / ordersToday) : 0.0;
                    decimal memberSales = salesTodayList.Where(s => s.Cus_ID.HasValue && s.Cus_ID.Value > 0).Sum(s => (decimal)s.Sales_Total);
                    double memberPercent = salesToday > 0 ? ((double)memberSales / (double)salesToday) * 100.0 : 0.0;
                    ItemsPerBillText = $"เฉลี่ย {avgItems:F1} ชิ้น/บิล (ยอดสมาชิก {memberPercent:F0}%)";
                    MemberSalesToday = memberSales.ToString("N2", CultureInfo.InvariantCulture);
                    MemberSalesPercentText = $"คิดเป็น {memberPercent:F1}% ของยอดขาย";

                    // 5) Pending Tasks (4 หมวดหมู่งาน)
                    var pendingTasks = await _databaseService.GetPendingTasksSummaryAsync(conn);
                    PendingDeliveryCount = pendingTasks.PendingDeliveryCount;
                    PendingClaimCount = pendingTasks.PendingClaimCount;
                    NeedReorderCount = pendingTasks.NeedReorderCount;
                    PendingReceiveCount = pendingTasks.PendingReceiveCount;

                    // 6) Hourly Sales: Today vs Yesterday
                    BuildHourlyChart(salesTodayList, salesYesterdayList);

                    // 7) Customer Stats Today
                    await LoadCustomerSummaryAsync(conn, today, salesTodayList);

                    // 8) Recent 5 Bills
                    var recentList = await _databaseService.GetRecentBillsAsync(conn, 5);
                    await UpdateRecentBillsCollectionAsync(recentList);

                    // 9) Top 5 Selling Products (Today & 7 Days)
                    _topSellingToday = await _databaseService.GetTopSellingProductsAsync(conn, days: 1, limit: 5);
                    _topSelling7Days = await _databaseService.GetTopSellingProductsAsync(conn, days: 7, limit: 5);
                    var initialList = SelectedPeriodDays == 7 ? _topSelling7Days : _topSellingToday;
                    await UpdateTopSellingCollectionAsync(initialList);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Dashboard] LoadDashboardDataAsync error: {ex.Message}");
            }
        }

        private void BuildHourlyChart(List<SalesOrderRecord> salesToday, List<SalesOrderRecord> salesYesterday)
        {
            int startHour = 8;
            int endHour = 21;

            // Check if there are sales earlier than 8 or later than 21
            foreach (var s in salesToday.Concat(salesYesterday))
            {
                if (s.NormalizedDate.HasValue)
                {
                    int h = s.NormalizedDate.Value.Hour;
                    if (h < startHour) startHour = h;
                    if (h > endHour) endHour = h;
                }
            }

            int countHours = (endHour - startHour) + 1;
            var labels = new string[countHours];
            var todayCounts = new ChartValues<int>();
            var yesterdayCounts = new ChartValues<int>();

            int totalTodayHourlyBills = 0;

            for (int i = 0; i < countHours; i++)
            {
                int hour = startHour + i;
                labels[i] = $"{hour:D2}:00";

                int tCount = salesToday.Count(s => s.NormalizedDate.HasValue && s.NormalizedDate.Value.Hour == hour);
                int yCount = salesYesterday.Count(s => s.NormalizedDate.HasValue && s.NormalizedDate.Value.Hour == hour);

                todayCounts.Add(tCount);
                yesterdayCounts.Add(yCount);
                totalTodayHourlyBills += tCount;
            }

            HourlyLabels = labels;
            HourlySalesSeries[0].Values = todayCounts;
            HourlySalesSeries[1].Values = yesterdayCounts;

            HasHourlyData = totalTodayHourlyBills > 0;
        }

        private async Task LoadCustomerSummaryAsync(System.Data.IDbConnection conn, DateTime today, List<SalesOrderRecord> salesTodayList)
        {
            try
            {
                int newCust = 0;
                int totalCust = 0;

                var customers = (await conn.QueryAsync<CustomerRecord>("SELECT Cus_ID, Cus_RegDate FROM tblCustomer")).ToList();
                totalCust = customers.Count;
                foreach (var c in customers)
                {
                    var dt = ParseDateToInvariant(c.Cus_RegDate);
                    if (dt.HasValue && dt.Value.Date == today.Date)
                    {
                        newCust++;
                    }
                }

                NewCustomersToday = newCust;
                TotalCustomers = totalCust;

                int pointsGiven = salesTodayList.Sum(s => s.PointsEarned);
                int pointsRedeemed = salesTodayList.Sum(s => s.PointsUsed);
                int promoUsed = salesTodayList.Count(s => s.DiscountAmount > 0);

                PointsGivenToday = pointsGiven;
                PointsRedeemedToday = pointsRedeemed;
                PromotionsUsedToday = promoUsed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Dashboard] LoadCustomerSummaryAsync error: {ex.Message}");
            }
        }

        public async Task LoadTopSellingProductsAsync(bool isSevenDays)
        {
            try
            {
                IsBusy = true;
                int days = isSevenDays ? 7 : 1;
                SelectedPeriodDays = days;

                List<TopSellingProductDto> resultList;
                using (var conn = _databaseService.GetConnection())
                {
                    resultList = await _databaseService.GetTopSellingProductsAsync(conn, days: days, limit: 5);
                }

                if (isSevenDays)
                {
                    _topSelling7Days = resultList;
                }
                else
                {
                    _topSellingToday = resultList;
                }

                await UpdateTopSellingCollectionAsync(resultList);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Dashboard] Error loading top selling: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task UpdateTopSellingCollectionAsync(IEnumerable<TopSellingProductDto> items)
        {
            var list = items.ToList();
            if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    TopSellingProducts.Clear();
                    foreach (var item in list)
                    {
                        TopSellingProducts.Add(item);
                    }
                    HasTopSellingProducts = TopSellingProducts.Count > 0;
                });
            }
            else
            {
                TopSellingProducts.Clear();
                foreach (var item in list)
                {
                    TopSellingProducts.Add(item);
                }
                HasTopSellingProducts = TopSellingProducts.Count > 0;
            }
        }

        private async Task UpdateRecentBillsCollectionAsync(IEnumerable<RecentBillDto> items)
        {
            var list = (items ?? Enumerable.Empty<RecentBillDto>()).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                list[i].IsLast = (i == list.Count - 1);
            }

            if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    RecentBills.Clear();
                    foreach (var item in list)
                    {
                        RecentBills.Add(item);
                    }
                    HasRecentBills = RecentBills.Count > 0;
                });
            }
            else
            {
                RecentBills.Clear();
                foreach (var item in list)
                {
                    RecentBills.Add(item);
                }
                HasRecentBills = RecentBills.Count > 0;
            }
        }

        private async Task OpenBillReceiptAsync(RecentBillDto bill)
        {
            if (bill == null) return;
            try
            {
                using (var conn = _databaseService.GetConnection())
                {
                    var orderRow = await conn.QueryFirstOrDefaultAsync<SalesOrderRecord>(
                        @"SELECT s.Sales_ID, s.RefNo, s.Sales_Date, s.Sales_Total, s.Sales_Cash, s.Sales_Change, 
                                 s.Sales_PaymentType, s.Sales_Status, s.DiscountAmount, s.PointsUsed, s.PointsEarned, 
                                 s.Cus_ID, c.Cus_Name AS CustomerName 
                          FROM tblSales_H s 
                          LEFT JOIN tblCustomer c ON s.Cus_ID = c.Cus_ID 
                          WHERE s.Sales_ID = @Id",
                        new { Id = bill.SalesID });

                    if (orderRow != null)
                    {
                        var salesOrder = new SalesOrder
                        {
                            Sales_ID = orderRow.Sales_ID,
                            RefNo = string.IsNullOrWhiteSpace(orderRow.RefNo) ? $"SALE-{orderRow.Sales_ID:D5}" : orderRow.RefNo,
                            Sales_Date = orderRow.Sales_Date ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                            Sales_Total = (decimal)orderRow.Sales_Total,
                            Sales_Cash = (decimal)orderRow.Sales_Cash,
                            Sales_Change = (decimal)orderRow.Sales_Change,
                            Sales_PaymentType = orderRow.Sales_PaymentType ?? "เงินสด",
                            Sales_Status = orderRow.Sales_Status ?? "สำเร็จ",
                            DiscountAmount = (decimal)orderRow.DiscountAmount,
                            PointsUsed = orderRow.PointsUsed,
                            PointsEarned = orderRow.PointsEarned,
                            CustomerName = string.IsNullOrWhiteSpace(orderRow.CustomerName) ? "ลูกค้าทั่วไป" : orderRow.CustomerName
                        };

                        var itemRows = await conn.QueryAsync(
                            @"SELECT d.Pro_ID, COALESCE(p.Pro_Name, 'สินค้า #' || d.Pro_ID) AS Pro_Name, 
                                     d.Pro_Price, d.Sales_Qty, d.Sales_Subtotal 
                              FROM tblSalesDetail d 
                              LEFT JOIN tblProduct p ON d.Pro_ID = p.Pro_ID 
                              WHERE d.Sales_ID = @Id",
                            new { Id = bill.SalesID });

                        var items = itemRows.Select(r => new SalesOrderItem
                        {
                            Pro_ID = Convert.ToInt32(r.Pro_ID),
                            ProductName = r.Pro_Name != null ? r.Pro_Name.ToString() : $"สินค้า #{r.Pro_ID}",
                            UnitPrice = Convert.ToDecimal(r.Pro_Price),
                            Quantity = Convert.ToInt32(r.Sales_Qty),
                            Total = Convert.ToDecimal(r.Sales_Subtotal)
                        }).ToList();

                        var window = new ReceiptWindow(salesOrder, items)
                        {
                            Owner = Application.Current?.MainWindow
                        };
                        window.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Dashboard] OpenBillReceiptAsync error: {ex.Message}");
            }
        }
    }
}
