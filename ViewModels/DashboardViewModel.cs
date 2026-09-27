using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Wpf;
using Dapper;
using Porjai20.Models;
using Porjai20.Services;

namespace Porjai20.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly DatabaseService _databaseService;

        // ─── Summary Cards ───────────────────────────────────────────────────────────
        private string _todaySales = "0.00";
        public string TodaySales
        {
            get => _todaySales;
            set => SetProperty(ref _todaySales, value);
        }

        private string _todaySalesChange = "0% vs เมื่อวาน";
        public string TodaySalesChange
        {
            get => _todaySalesChange;
            set => SetProperty(ref _todaySalesChange, value);
        }

        private string _todaySalesColor = "#9CA3AF";
        public string TodaySalesColor
        {
            get => _todaySalesColor;
            set => SetProperty(ref _todaySalesColor, value);
        }

        private string _todaySalesArrow = "";
        public string TodaySalesArrow
        {
            get => _todaySalesArrow;
            set => SetProperty(ref _todaySalesArrow, value);
        }

        private string _todayOrders = "0";
        public string TodayOrders
        {
            get => _todayOrders;
            set => SetProperty(ref _todayOrders, value);
        }

        private string _todayOrdersChange = "0% vs เมื่อวาน";
        public string TodayOrdersChange
        {
            get => _todayOrdersChange;
            set => SetProperty(ref _todayOrdersChange, value);
        }

        private string _todayOrdersColor = "#9CA3AF";
        public string TodayOrdersColor
        {
            get => _todayOrdersColor;
            set => SetProperty(ref _todayOrdersColor, value);
        }

        private string _todayOrdersArrow = "";
        public string TodayOrdersArrow
        {
            get => _todayOrdersArrow;
            set => SetProperty(ref _todayOrdersArrow, value);
        }

        private string _todayStock = "0";
        public string TodayStock
        {
            get => _todayStock;
            set => SetProperty(ref _todayStock, value);
        }

        private string _todayStockChange = "คงเหลือในคลัง";
        public string TodayStockChange
        {
            get => _todayStockChange;
            set => SetProperty(ref _todayStockChange, value);
        }

        private string _todayStockColor = "#22C55E";
        public string TodayStockColor
        {
            get => _todayStockColor;
            set => SetProperty(ref _todayStockColor, value);
        }

        private string _todayStockArrow = "●";
        public string TodayStockArrow
        {
            get => _todayStockArrow;
            set => SetProperty(ref _todayStockArrow, value);
        }

        private string _newCustomers = "0";
        public string NewCustomers
        {
            get => _newCustomers;
            set => SetProperty(ref _newCustomers, value);
        }

        private string _newCustomersChange = "0% vs เมื่อวาน";
        public string NewCustomersChange
        {
            get => _newCustomersChange;
            set => SetProperty(ref _newCustomersChange, value);
        }

        private string _newCustomersColor = "#9CA3AF";
        public string NewCustomersColor
        {
            get => _newCustomersColor;
            set => SetProperty(ref _newCustomersColor, value);
        }

        private string _newCustomersArrow = "";
        public string NewCustomersArrow
        {
            get => _newCustomersArrow;
            set => SetProperty(ref _newCustomersArrow, value);
        }

        // ─── Dynamic Header Labels ───────────────────────────────────────────────────
        private string _weeklyDateRangeText = "";
        public string WeeklyDateRangeText
        {
            get => _weeklyDateRangeText;
            set => SetProperty(ref _weeklyDateRangeText, value);
        }

        private string _monthlyYearText = $"ปี {DateTime.Today.Year}";
        public string MonthlyYearText
        {
            get => _monthlyYearText;
            set => SetProperty(ref _monthlyYearText, value);
        }

        // ─── Line Chart: Monthly Sales ───────────────────────────────────────────────
        public SeriesCollection MonthlySalesSeries { get; }
        public string[] MonthLabels { get; } =
            { "ม.ค.", "ก.พ.", "มี.ค.", "เม.ย.", "พ.ค.", "มิ.ย.", "ก.ค.", "ส.ค.", "ก.ย.", "ต.ค.", "พ.ย.", "ธ.ค." };
        public Func<double, string> MoneyFormatter { get; } = val => $"{val:N0}";

        // ─── Bar Chart: Weekly Sales ──────────────────────────────────────────────────
        public SeriesCollection WeeklySalesSeries { get; }
        public string[] DayLabels { get; } = { "จ.", "อ.", "พ.", "พฤ.", "ศ.", "ส.", "อา." };
        public Func<double, string> ShortMoneyFormatter { get; } = val => val >= 1000 ? $"{val / 1000:F1}K" : $"{val:N0}";

        // ─── Recent Orders ────────────────────────────────────────────────────────────
        public ObservableCollection<RecentOrderRow> RecentOrders { get; }

        // ─── Top Products ─────────────────────────────────────────────────────────────
        public ObservableCollection<TopProductRow> TopProducts { get; }

        // ─── Navigation Command ────────────────────────────────────────────────────────
        public ICommand GoToSalesHistoryCommand { get; }

        public DashboardViewModel()
        {
            _databaseService = new DatabaseService();

            // Navigation command routing to Sales History
            GoToSalesHistoryCommand = new RelayCommand(_ =>
            {
                if (App.Current?.MainWindow?.DataContext is ProductViewModel mainVm)
                {
                    mainVm.OpenSalesHistoryCommand?.Execute(null);
                }
            });

            // Monthly Line Chart — initialized empty
            MonthlySalesSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "รายรับ",
                    Values = new ChartValues<double>(),
                    Stroke = new SolidColorBrush(Color.FromRgb(0x7B, 0x3F, 0xF2)),
                    Fill   = new LinearGradientBrush(
                        Color.FromArgb(120, 0x7B, 0x3F, 0xF2),
                        Color.FromArgb(10,  0x7B, 0x3F, 0xF2),
                        new System.Windows.Point(0, 0),
                        new System.Windows.Point(0, 1)),
                    StrokeThickness = 2.5,
                    PointGeometry = null,
                    LineSmoothness = 0.8,
                },
                new LineSeries
                {
                    Title = "ยอดสุทธิ",
                    Values = new ChartValues<double>(),
                    Stroke = new SolidColorBrush(Color.FromRgb(0x00, 0xC6, 0xFF)),
                    Fill   = new SolidColorBrush(Colors.Transparent),
                    StrokeThickness = 2,
                    StrokeDashArray = new System.Windows.Media.DoubleCollection { 4, 2 },
                    PointGeometry = null,
                    LineSmoothness = 0.8,
                },
            };

            // Weekly Bar Chart — initialized empty
            WeeklySalesSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "ยอดขาย",
                    Values = new ChartValues<double>(),
                    Fill   = new SolidColorBrush(Color.FromRgb(0x7B, 0x3F, 0xF2)),
                    ColumnPadding = 6,
                    MaxColumnWidth = 30,
                }
            };

            // Recent Orders & Top Products — Observables
            RecentOrders = new ObservableCollection<RecentOrderRow>();
            TopProducts = new ObservableCollection<TopProductRow>();

            // Trigger asynchronous database load
            _ = LoadData();
        }

        public static DateTime? ParseDateToInvariant(string? dateStr)
        {
            return Common.DateTimeHelper.ParseDateToInvariant(dateStr);
        }

        public Task LoadDashboardData() => LoadData();

        public async Task LoadData()
        {
            try
            {
                using (var conn = _databaseService.GetConnection())
                {
                    var today = DateTime.Today;
                    var yesterday = today.AddDays(-1);

                    // Dynamic Date range calculation for current week (Monday to Sunday)
                    int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                    DateTime monday = today.AddDays(-diff).Date;
                    DateTime sunday = monday.AddDays(6).Date;

                    string[] thaiMonths = { "", "ม.ค.", "ก.พ.", "มี.ค.", "เม.ย.", "พ.ค.", "มิ.ย.", "ก.ค.", "ส.ค.", "ก.ย.", "ต.ค.", "พ.ย.", "ธ.ค." };
                    if (monday.Month == sunday.Month)
                        WeeklyDateRangeText = $"{monday.Day}–{sunday.Day} {thaiMonths[monday.Month]} {monday.Year}";
                    else
                        WeeklyDateRangeText = $"{monday.Day} {thaiMonths[monday.Month]} – {sunday.Day} {thaiMonths[sunday.Month]} {monday.Year}";

                    MonthlyYearText = $"ปี {today.Year}";

                    // Query all active sales orders (exclude Cancelled / ยกเลิก)
                    var allSales = (await conn.QueryAsync<SalesOrderRecord>(
                        "SELECT Sales_ID, RefNo, Sales_Date, Sales_Total, Sales_Status, Cus_ID FROM tblSales_H WHERE Sales_Status IS NULL OR (Sales_Status != 'ยกเลิก' AND Sales_Status != 'Cancelled')")).ToList();

                    // Convert all sales dates into invariant NormalizedDate
                    foreach (var s in allSales)
                    {
                        s.NormalizedDate = ParseDateToInvariant(s.Sales_Date);
                    }

                    // 1. Sales Card: Today vs Yesterday
                    var salesTodayList = allSales.Where(s => s.NormalizedDate.HasValue && s.NormalizedDate.Value.Date == today.Date).ToList();
                    var salesYesterdayList = allSales.Where(s => s.NormalizedDate.HasValue && s.NormalizedDate.Value.Date == yesterday.Date).ToList();

                    decimal salesToday = salesTodayList.Sum(s => (decimal)s.Sales_Total);
                    decimal salesYesterday = salesYesterdayList.Sum(s => (decimal)s.Sales_Total);

                    decimal salesChange = 0;
                    if (salesYesterday > 0)
                        salesChange = ((salesToday - salesYesterday) / salesYesterday) * 100m;
                    else if (salesToday > 0)
                        salesChange = 100m;

                    TodaySales = salesToday.ToString("N2");
                    TodaySalesChange = $"{Math.Abs(salesChange):F0}% vs เมื่อวาน";
                    TodaySalesColor = salesChange >= 0 ? "#22C55E" : "#EF4444";
                    TodaySalesArrow = salesChange >= 0 ? "▲" : "▼";

                    // 2. Orders Card: Today vs Yesterday
                    int ordersToday = salesTodayList.Count;
                    int ordersYesterday = salesYesterdayList.Count;

                    double ordersChange = 0;
                    if (ordersYesterday > 0)
                        ordersChange = ((double)(ordersToday - ordersYesterday) / ordersYesterday) * 100.0;
                    else if (ordersToday > 0)
                        ordersChange = 100.0;

                    TodayOrders = ordersToday.ToString();
                    TodayOrdersChange = $"{Math.Abs(ordersChange):F0}% vs เมื่อวาน";
                    TodayOrdersColor = ordersChange >= 0 ? "#22C55E" : "#EF4444";
                    TodayOrdersArrow = ordersChange >= 0 ? "▲" : "▼";

                    // 3. Stock Card
                    var stockToday = await conn.ExecuteScalarAsync<int>("SELECT COALESCE(SUM(Pro_Qty), 0) FROM tblProduct");
                    TodayStock = stockToday.ToString();
                    TodayStockChange = "คงเหลือในคลัง";
                    TodayStockColor = "#22C55E";
                    TodayStockArrow = "●";

                    // 4. New Customers Card
                    int custToday = 0;
                    int custYesterday = 0;
                    int totalCust = 0;

                    try
                    {
                        var customers = (await conn.QueryAsync<CustomerRecord>("SELECT Cus_ID, Cus_RegDate FROM tblCustomer")).ToList();
                        totalCust = customers.Count;
                        foreach (var c in customers)
                        {
                            var dt = ParseDateToInvariant(c.Cus_RegDate);
                            if (dt.HasValue)
                            {
                                if (dt.Value.Date == today.Date) custToday++;
                                else if (dt.Value.Date == yesterday.Date) custYesterday++;
                            }
                        }
                    }
                    catch
                    {
                        totalCust = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM tblCustomer");
                    }

                    NewCustomers = custToday.ToString();
                    if (custToday > 0)
                    {
                        double custChange = 0;
                        if (custYesterday > 0)
                            custChange = ((double)(custToday - custYesterday) / custYesterday) * 100.0;
                        else
                            custChange = 100.0;

                        NewCustomersChange = $"{Math.Abs(custChange):F0}% vs เมื่อวาน";
                        NewCustomersColor = custChange >= 0 ? "#22C55E" : "#EF4444";
                        NewCustomersArrow = custChange >= 0 ? "▲" : "▼";
                    }
                    else
                    {
                        if (custYesterday > 0)
                        {
                            NewCustomersChange = "100% vs เมื่อวาน";
                            NewCustomersColor = "#EF4444";
                            NewCustomersArrow = "▼";
                        }
                        else
                        {
                            NewCustomersChange = totalCust > 0 ? $"ลูกค้าทั้งหมด {totalCust} ท่าน" : "0% vs เมื่อวาน";
                            NewCustomersColor = "#9CA3AF";
                            NewCustomersArrow = "●";
                        }
                    }

                    // 5. Line Chart: Monthly Sales (Jan to Dec of Current Year)
                    int currentYear = today.Year;
                    var monthlyValues = new ChartValues<double>();
                    var netValues = new ChartValues<double>();
                    for (int m = 1; m <= 12; m++)
                    {
                        double monthSales = (double)allSales
                            .Where(s => s.NormalizedDate.HasValue && s.NormalizedDate.Value.Year == currentYear && s.NormalizedDate.Value.Month == m)
                            .Sum(s => (decimal)s.Sales_Total);

                        monthlyValues.Add(monthSales);
                        netValues.Add(monthSales * 0.4); // 40% margin
                    }
                    MonthlySalesSeries[0].Values = monthlyValues;
                    MonthlySalesSeries[1].Values = netValues;

                    // 6. Bar Chart: Weekly Sales (Monday to Sunday of Current Week)
                    var weeklyValues = new ChartValues<double>();
                    for (int i = 0; i < 7; i++)
                    {
                        DateTime dayDate = monday.AddDays(i);
                        double daySales = (double)allSales
                            .Where(s => s.NormalizedDate.HasValue && s.NormalizedDate.Value.Date == dayDate.Date)
                            .Sum(s => (decimal)s.Sales_Total);

                        weeklyValues.Add(daySales);
                    }
                    WeeklySalesSeries[0].Values = weeklyValues;

                    // 7. Recent Orders Grid
                    var recentOrdersList = new List<RecentOrderRow>();
                    string recentSql = @"
                        SELECT s.Sales_ID AS Sales_ID, s.Sales_ID AS Id, s.RefNo, s.Sales_Date, s.Sales_Total, s.Sales_Status,
                               s.Sales_PaymentType, s.Cus_ID, c.Cus_Name AS CustomerName
                        FROM tblSales_H s
                        LEFT JOIN tblCustomer c ON s.Cus_ID = c.Cus_ID
                        ORDER BY s.Sales_ID DESC
                        LIMIT 5";
                    var orders = await conn.QueryAsync(recentSql);
                    foreach (var order in orders)
                    {
                        int orderId = Convert.ToInt32(order.Sales_ID);
                        var itemsCount = await conn.ExecuteScalarAsync<int>(
                            "SELECT COALESCE(SUM(Sales_Qty), 0) FROM tblSalesDetail WHERE Sales_ID = @OrderId",
                            new { OrderId = orderId });

                        string statusText = "สำเร็จ";
                        string statusColor = "#22C55E";
                        string rawStatus = order.Sales_Status != null ? order.Sales_Status.ToString() : "";
                        if (rawStatus == "ยกเลิก" || rawStatus == "Cancelled")
                        {
                            statusText = "ยกเลิก";
                            statusColor = "#EF4444";
                        }

                        string customerName = order.CustomerName != null ? order.CustomerName.ToString() : "";
                        if (string.IsNullOrWhiteSpace(customerName) || customerName == "-")
                        {
                            customerName = "ลูกค้าทั่วไป";
                        }

                        decimal totalAmt = Convert.ToDecimal(order.Sales_Total);

                        recentOrdersList.Add(new RecentOrderRow
                        {
                            OrderId = order.RefNo ?? $"SALE-{orderId}",
                            Customer = customerName,
                            PurchaseType = "รับที่ร้าน",
                            Items = itemsCount,
                            Total = totalAmt.ToString("N2"),
                            Status = statusText,
                            StatusColor = statusColor
                        });
                    }
                    RecentOrders.Clear();
                    foreach (var item in recentOrdersList)
                    {
                        RecentOrders.Add(item);
                    }

                    // 8. Top Products Grid
                    var topProductsList = new List<TopProductRow>();
                    var topProductsQuery = @"
                        SELECT p.Pro_Name AS Name, p.Pro_Category AS Category, SUM(i.Sales_Qty) as Sold, SUM(i.Sales_Subtotal) as Revenue
                        FROM tblSalesDetail i
                        JOIN tblProduct p ON i.Pro_ID = p.Pro_ID
                        GROUP BY i.Pro_ID, p.Pro_Name, p.Pro_Category
                        ORDER BY Sold DESC
                        LIMIT 5";
                    var topRows = await conn.QueryAsync(topProductsQuery);
                    int rank = 1;
                    foreach (var row in topRows)
                    {
                        topProductsList.Add(new TopProductRow
                        {
                            Rank = rank++,
                            Name = row.Name,
                            Category = row.Category,
                            Sold = (int)row.Sold,
                            Revenue = ((decimal)row.Revenue).ToString("N2"),
                            Trend = "+15%"
                        });
                    }
                    TopProducts.Clear();
                    foreach (var item in topProductsList)
                    {
                        TopProducts.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
            }
        }
    }

    public class RecentOrderRow
    {
        public string OrderId      { get; set; } = string.Empty;
        public string Customer     { get; set; } = string.Empty;
        public string PurchaseType { get; set; } = string.Empty;
        public int    Items        { get; set; }
        public string Total        { get; set; } = string.Empty;
        public string Status       { get; set; } = string.Empty;
        public string StatusColor  { get; set; } = string.Empty;
    }

    public class TopProductRow
    {
        public int    Rank     { get; set; }
        public string Name     { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int    Sold     { get; set; }
        public string Revenue  { get; set; } = string.Empty;
        public string Trend    { get; set; } = string.Empty;
    }

    internal class SalesOrderRecord
    {
        public int Sales_ID { get; set; }
        public string? RefNo { get; set; }
        public string? Sales_Date { get; set; }
        public double Sales_Total { get; set; }
        public string? Sales_Status { get; set; }
        public int? Cus_ID { get; set; }
        public DateTime? NormalizedDate { get; set; }
    }

    internal class CustomerRecord
    {
        public int Cus_ID { get; set; }
        public string? Cus_RegDate { get; set; }
    }
}

