using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Porjai20.Models;
using Porjai20.ViewModels;

namespace Porjai20.Views
{
    public partial class ReceiptWindow : Window
    {
        private SalesOrder _order;
        public ReceiptViewModel ViewModel { get; }
        
        public ReceiptWindow(SalesOrder order, IEnumerable<SalesOrderItem> items)
        {
            InitializeComponent();
            _order = order;
            var itemList = items?.ToList() ?? new List<SalesOrderItem>();
            ViewModel = new ReceiptViewModel(_order, itemList);
            
            // Set DataContext for the receipt template
            this.DataContext = ViewModel;
            
            // Bind the items to the grid
            ItemsGrid.ItemsSource = itemList;
        }

        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.PrintHelper.PrintVisual(PrintArea, "Sales Receipt", PrintBtn, this);
        }
    }
}
