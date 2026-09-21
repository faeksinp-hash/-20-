using System;
using System.Linq;
using System.Windows;
using Dapper;
using Porjai20.Services;

namespace Porjai20.Views
{
    public partial class CustomerHistoryWindow : Window
    {
        private readonly DatabaseService _databaseService;
        private string _customerPhone;
        private string _customerName;

        public CustomerHistoryWindow(string customerPhone, string customerName)
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _customerPhone = customerPhone;
            _customerName = customerName;

            Loaded += CustomerHistoryWindow_Loaded;
        }

        private void CustomerHistoryWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CustomerNameTextBlock.Text = _customerName;
            CustomerPhoneTextBlock.Text = _customerPhone;
            
            LoadHistory();
        }

        private void LoadHistory()
        {
            // Query sales orders with matching phone or name
            using (var connection = _databaseService.GetConnection())
            {
                string sql = @"
                    SELECT Sales_ID AS Sales_ID, Sales_ID AS Id, * FROM tblSales_H 
                    ORDER BY Sales_Date DESC";
                
                var history = connection.Query<Models.SalesOrder>(sql, new { Phone = _customerPhone, Name = _customerName }).ToList();
                
                HistoryDataGrid.ItemsSource = history;
                
                // Calculate Summary
                int totalOrders = history.Count;
                decimal totalSpent = history.Sum(h => h.TotalAmount);
                
                TotalOrdersTextBlock.Text = totalOrders.ToString();
                TotalSpentTextBlock.Text = totalSpent.ToString("N2");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
