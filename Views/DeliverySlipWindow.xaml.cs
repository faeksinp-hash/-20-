using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Porjai20.Models;
using Porjai20.Services;
using Dapper;

namespace Porjai20.Views
{
    public partial class DeliverySlipWindow : Window
    {
        private SalesOrder _order;
        private DatabaseService _databaseService;

        public DeliverySlipWindow(SalesOrder order)
        {
            InitializeComponent();
            _order = order;
            _databaseService = new DatabaseService();

            LoadOrderDetails();
        }

        private void LoadOrderDetails()
        {
            // Bind header and customer info
            this.DataContext = _order;

            // Load Items
            using (var conn = _databaseService.GetConnection())
            {
                string sql = @"
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
                    WHERE d.Sales_ID = @OrderId";
                var items = conn.Query<SalesOrderItem>(sql, new { OrderId = _order.Id }).ToList();
                
                // Optional: Recalculate generic "Id" for row numbers display instead of database IDs
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].Id = i + 1;
                }
                
                ItemsGrid.ItemsSource = items;
            }
        }
    }
}
