using System;
using System.Collections.Generic;
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
        private SalesOrder? _order;
        private List<SalesOrderItem> _items;
        private DatabaseService _databaseService;

        public SalesOrder? Order => _order;
        public List<SalesOrderItem> Items => _items;
        public decimal Subtotal => _items?.Sum(i => i.Total) ?? 0m;
        public decimal ShippingFee => (_order != null && _order.TotalAmount > Subtotal && Subtotal > 0) ? (_order.TotalAmount - Subtotal) : 0m;
        public decimal GrandTotal => _order?.TotalAmount ?? Subtotal;

        public DeliverySlipWindow(SalesOrder? order, IEnumerable<SalesOrderItem>? items = null)
        {
            InitializeComponent();
            _order = order;
            _databaseService = new DatabaseService();

            if (items != null && items.Any())
            {
                _items = items.ToList();
            }
            else if (_order != null && _order.Id > 0)
            {
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
                    _items = conn.Query<SalesOrderItem>(sql, new { OrderId = _order.Id }).ToList();
                }
            }
            else
            {
                _items = new List<SalesOrderItem>();
            }

            for (int i = 0; i < _items.Count; i++)
            {
                _items[i].Id = i + 1;
            }

            this.DataContext = this;
        }

        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.PrintHelper.PrintVisual(PrintArea, "Delivery Slip - " + (_order?.RefNo ?? ""), HeaderActionsBar, this);
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
