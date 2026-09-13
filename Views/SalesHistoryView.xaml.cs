using System.Windows;
using System.Windows.Controls;
using Porjai20.ViewModels;

namespace Porjai20.Views
{
    public partial class SalesHistoryView : UserControl
    {
        public SalesHistoryView()
        {
            InitializeComponent();
            Loaded += SalesHistoryView_Loaded;
            IsVisibleChanged += SalesHistoryView_IsVisibleChanged;
        }

        private void SalesHistoryView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SalesHistoryViewModel vm)
            {
                vm.LoadSalesHistory();
            }
        }

        private void SalesHistoryView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is true && DataContext is SalesHistoryViewModel vm)
            {
                vm.LoadSalesHistory();
            }
        }

        private async void PrintReceiptBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SalesHistoryViewModel vm && vm.SelectedOrder != null)
            {
                await vm.ExecutePrintAsync(Window.GetWindow(this) ?? Application.Current.MainWindow);
            }
        }
    }
}
