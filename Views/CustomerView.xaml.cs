using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Porjai20.Models;
using Porjai20.ViewModels;

namespace Porjai20.Views
{
    public partial class CustomerView : UserControl
    {
        public CustomerView()
        {
            InitializeComponent();
        }

        private void DrawerScrollViewer_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            // ระงับไม่ให้ ScrollViewer เลื่อนตำแหน่งอัตโนมัติตาม Focus
            e.Handled = true;
        }

        private void CustomerRow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Do not open drawer if user clicked a button or interactive child element
            if (e.OriginalSource is DependencyObject dep && FindVisualParent<Button>(dep) != null)
            {
                return;
            }

            if (sender is DataGridRow row && row.Item is Customer customer)
            {
                OpenDrawerForCustomer(customer);
            }
        }

        private void Backdrop_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CloseDrawer();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (IsDrawerOpen())
                {
                    CloseDrawer();
                    e.Handled = true;
                    return;
                }
            }
            base.OnPreviewKeyDown(e);
        }

        private bool IsDrawerOpen()
        {
            if (DataContext is ProductViewModel vm)
            {
                return vm.IsCustomerDrawerOpen;
            }
            if (DataContext is CustomerViewModel cvm)
            {
                return cvm.IsCustomerDrawerOpen;
            }
            return false;
        }

        private void OpenDrawerForCustomer(Customer customer)
        {
            if (DataContext is ProductViewModel vm)
            {
                vm.ExecuteOpenCustomerDrawer(customer);
            }
            else if (DataContext is CustomerViewModel cvm)
            {
                cvm.ExecuteOpenCustomerDrawer(customer);
            }
        }

        private void CloseDrawer()
        {
            if (DataContext is ProductViewModel vm)
            {
                vm.ExecuteCloseCustomerDrawer();
            }
            else if (DataContext is CustomerViewModel cvm)
            {
                cvm.ExecuteCloseCustomerDrawer();
            }
        }

        private static T? FindVisualParent<T>(DependencyObject? child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent) return parent;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }
    }
}