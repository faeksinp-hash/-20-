using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Porjai20.Models;
using Porjai20.Services;

namespace Porjai20.Views
{
    public partial class POReceiptWindow : Window
    {
        private PurchaseOrder _order;
        private DatabaseService _databaseService;

        public POReceiptWindow(PurchaseOrder order)
        {
            InitializeComponent();
            _order = order;
            _databaseService = new DatabaseService();

            LoadOrderDetails();
        }

        private void LoadOrderDetails()
        {
            this.DataContext = _order;
            var items = _databaseService.GetPurchaseOrderItems(_order.Id).ToList();
            ItemsGrid.ItemsSource = items;
        }

        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.PrintHelper.PrintVisual(PrintArea, "Purchase Order Receipt", PrintBtn);
        }
    }
}
