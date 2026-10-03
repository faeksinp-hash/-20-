using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private void SalesHistoryRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is SalesOrderDisplayRow orderItem)
            {
                if (DataContext is SalesHistoryViewModel vm)
                {
                    if (vm.ViewDetailsCommand?.CanExecute(orderItem) == true)
                    {
                        vm.ViewDetailsCommand.Execute(orderItem);
                    }
                    else
                    {
                        vm.SelectedOrder = orderItem;
                        vm.IsDetailModalOpen = true;
                    }
                }
            }
        }

        private async void PrintReceiptBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SalesHistoryViewModel vm && vm.SelectedOrder != null)
            {
                await vm.ExecutePrintAsync(Window.GetWindow(this) ?? Application.Current.MainWindow);
            }
        }

        private void ModalItemsDataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is DependencyObject depObj)
            {
                var scrollViewer = FindVisualChild<ScrollViewer>(depObj);
                if (scrollViewer != null)
                {
                    if (e.Delta > 0)
                        scrollViewer.LineUp();
                    else
                        scrollViewer.LineDown();

                    e.Handled = true;
                }
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            int childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
    }
}
