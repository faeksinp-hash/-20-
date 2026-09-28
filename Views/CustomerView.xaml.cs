using System.Windows.Controls;
using System.Windows.Input;
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

        private void CustomerRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is Customer customer)
            {
                if (DataContext is ProductViewModel vm)
                {
                    vm.SelectedCustomer = customer;
                    if (vm.OpenManageModalCommand?.CanExecute(customer) == true)
                    {
                        vm.OpenManageModalCommand.Execute(customer);
                    }
                    else
                    {
                        vm.ExecuteOpenManageModal();
                    }
                }
                else if (DataContext is CustomerViewModel cvm)
                {
                    cvm.SelectedCustomer = customer;
                    if (cvm.OpenManageModalCommand?.CanExecute(customer) == true)
                    {
                        cvm.OpenManageModalCommand.Execute(customer);
                    }
                    else
                    {
                        cvm.ExecuteOpenManageModal();
                    }
                }
            }
        }
    }
}