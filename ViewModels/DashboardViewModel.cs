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
        private string _todaySales = "฿0.00";
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

        // ─── Line Chart: Monthly Sales ───────────────────────────────────────────────
        public SeriesCollection MonthlySalesSeries { get; }
        public string[] MonthLabels { get; } =
            { "ม.ค.", "ก.พ.", "มี.ค.", "เม.ย.", "พ.ค.", "มิ.ย.", "ก.ค.", "ส.ค.", "ก.ย.", "ต.ค.", "พ.ย.", "ธ.ค." };
        public Func<double, string> MoneyFormatter { get; } = val => $"฿{val:N0}";

        // ─── Bar Chart: Weekly Sales ──────────────────────────────────────────────────
        public SeriesCollection WeeklySalesSeries { get; }
        public string[] DayLabels { get; } = { "จ.", "อ.", "พ.", "พฤ.", "ศ.", "ส.", "อา." };
        public Func<double, string> ShortMoneyFormatter { get; } = val => $"฿{val/1000:N0}K";

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

        public async Task LoadData()
        {
            try
            {
                using (var conn = _databaseService.GetConnection())
                {
                    var today = DateTime.Today.ToString("yyyy-MM-dd");
                    var yesterday = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");

                    // 1. Sales Card
                    var salesToday = await conn.ExecuteScalarAsync<decimal>(
                        "SELECT COALESCE(SUM(Sales_Total), 0) FROM tblSales_H WHERE date(Sales_Date) = date(@Today) AND (Sales_Status = 'ชำระเงินแล้ว' OR Sales_Status = 'Completed')",
                        new { Today = today });
                    var salesYesterday = await conn.ExecuteScalarAsync<decimal>(
                        "SELECT COALESCE(SUM(Sales_Total), 0) FROM tblSales_H WHERE date(Sales_Date) = date(@Yesterday) AND (Sales_Status = 'ชำระเงินแล้ว' OR Sales_Status = 'Completed')",
                        new { Yesterday = yesterday });

                    decimal salesChange = 0;
                    if (salesYesterday > 0)
                        salesChange = ((salesToday - salesYesterday) / salesYesterday) * 100;
                    else if (salesToday > 0)
                        salesChange = 100;

                    TodaySales = salesToday.ToString("C");
                    TodaySalesChange = $"{Math.Abs(salesChange):F0}% vs เมื่อวาน";
                    TodaySalesColor = salesChange >= 0 ? "#22C55E" : "#EF4444";
                    TodaySalesArrow = salesChange >= 0 ? "▲" : "▼";

                    // 2. Orders Card
                    var ordersToday = await conn.ExecuteScalarAsync<int>(
                        "SELECT COUNT(*) FROM tblSales_H WHERE date(Sales_Date) = date(@Today) AND (Sales_Status = 'ชำระเงินแล้ว' OR Sales_Status = 'Completed')",
                        new { Today = today });
                    var ordersYesterday = await conn.ExecuteScalarAsync<int>(
                        "SELECT COUNT(*) FROM tblSales_H WHERE date(Sales_Date) = date(@Yesterday) AND (Sales_Status = 'ชำระเงินแล้ว' OR Sales_Status = 'Completed')",
                        new { Yesterday = yesterday });

                    double ordersChange = 0;
                    if (ordersYesterday > 0)
                        ordersChange = ((double)(ordersToday - ordersYesterday) / ordersYesterday) * 100;
                    else if (ordersToday > 0)
                        ordersChange = 100;

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
                    var custToday = await conn.ExecuteScalarAsync<int>(
                        "SELECT COUNT(*) FROM tblCustomer WHERE date('now') = date(@Today)",
                        new { Today = today });
                    var custYesterday = 0;

                    double custChange = 0;

                    NewCustomers = custToday.ToString();
                    NewCustomersChange = $"{Math.Abs(custChange):F0}% vs เมื่อวาน";
                    NewCustomersColor = custChange >= 0 ? "#22C55E" : "#EF4444";
                    NewCustomersArrow = custChange >= 0 ? "▲" : "▼";

                    // 5. Line Chart: Monthly Sales
                    var currentYear = DateTime.Today.Year;
                    var monthlyValues = new ChartValues<double>();
                    var netValues = new ChartValues<double>();
                    for (int m = 1; m <= 12; m++)
                    {
                        var startOfMonth = new DateTime(currentYear, m, 1).ToString("yyyy-MM-dd");
                        var endOfMonth = new DateTime(currentYear, m, DateTime.DaysInMonth(currentYear, m)).ToString("yyyy-MM-dd");
                        var sales = await conn.ExecuteScalarAsync<double>(
                            "SELECT COALESCE(SUM(Sales_Total), 0) FROM tblSales_H WHERE date(Sales_Date) >= date(@Start) AND date(Sales_Date) <= date(@End) AND (Sales_Status = 'ชำระเงินแล้ว' OR Sales_Status = 'Completed')",
                            new { Start = startOfMonth, End = endOfMonth });
                        monthlyValues.Add(sales);
                        netValues.Add(sales * 0.4); // 40% margin
                    }
                    MonthlySalesSeries[0].Values = monthlyValues;
                    MonthlySalesSeries[1].Values = netValues;

                    // 6. Bar Chart: Weekly Sales
                    var weeklyValues = new ChartValues<double>();
                    for (int d = 6; d >= 0; d--)
                    {
                        var dateStr = DateTime.Today.AddDays(-d).ToString("yyyy-MM-dd");
                        var sales = await conn.ExecuteScalarAsync<double>(
                            "SELECT COALESCE(SUM(Sales_Total), 0) FROM tblSales_H WHERE date(Sales_Date) = date(@Date) AND (Sales_Status = 'ชำระเงินแล้ว' OR Sales_Status = 'Completed')",
                            new { Date = dateStr });
                        weeklyValues.Add(sales);
                    }
                    WeeklySalesSeries[0].Values = weeklyValues;

                    // 7. Recent Orders Grid
                    var recentOrdersList = new List<RecentOrderRow>();
                    var orders = await conn.QueryAsync<SalesOrder>("SELECT Sales_ID AS Sales_ID, Sales_ID AS Id, * FROM tblSales_H ORDER BY Sales_Date DESC LIMIT 5");
                    foreach (var order in orders)
                    {
                        var itemsCount = await conn.ExecuteScalarAsync<int>(
                            "SELECT COALESCE(SUM(Sales_Qty), 0) FROM tblSalesDetail WHERE Sales_ID = @OrderId",
                            new { OrderId = order.Id });
                        string statusColor = "#22C55E";
                        string statusText = "สำเร็จ";

                        recentOrdersList.Add(new RecentOrderRow
                        {
                            OrderId = order.RefNo,
                            Customer = "ลูกค้าทั่วไป",
                            PurchaseType = "รับที่ร้าน",
                            Items = itemsCount,
                            Total = order.TotalAmount.ToString("C"),
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
                            Revenue = ((decimal)row.Revenue).ToString("C"),
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
}
