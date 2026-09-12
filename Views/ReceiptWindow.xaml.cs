using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Porjai20.Models;

namespace Porjai20.Views
{
    public partial class ReceiptWindow : Window
    {
        private SalesOrder _order;
        
        public ReceiptWindow(SalesOrder order, IEnumerable<SalesOrderItem> items)
        {
            InitializeComponent();
            _order = order;
            
            // Set datacontext for the basic fields
            this.DataContext = _order;
            
            // Bind the items to the grid
            ItemsGrid.ItemsSource = items.ToList();
        }

        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                // Hide button during printing
                PrintBtn.Visibility = Visibility.Collapsed;
                
                // Print the visual area
                printDialog.PrintVisual(PrintArea, "Sales Receipt");
                
                // Restore button
                PrintBtn.Visibility = Visibility.Visible;
                
                // Close window after printing
                this.Close();
            }
        }
    }
}
