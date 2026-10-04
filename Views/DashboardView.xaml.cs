using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Porjai20.Models;
using Porjai20.ViewModels;

namespace Porjai20.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();

            Loaded += async (s, e) =>
            {
                if (DataContext is DashboardViewModel vm)
                {
                    await vm.LoadDashboardDataAsync();
                }
            };

            IsVisibleChanged += async (s, e) =>
            {
                if (IsVisible && DataContext is DashboardViewModel vm)
                {
                    await vm.LoadDashboardDataAsync();
                }
            };
        }

        private void RecentBillsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid grid && grid.SelectedItem is RecentBillDto bill)
            {
                if (DataContext is DashboardViewModel vm)
                {
                    vm.OpenBillReceiptCommand?.Execute(bill);
                }
            }
        }

        private void RecentBillRow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender is FrameworkElement elem && elem.DataContext is RecentBillDto bill)
            {
                if (DataContext is DashboardViewModel vm)
                {
                    vm.OpenBillReceiptCommand?.Execute(bill);
                }
            }
        }
    }
}
