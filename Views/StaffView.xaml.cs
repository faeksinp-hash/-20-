using System.Windows.Controls;
using System.Windows.Input;
using Porjai20.Models;
using Porjai20.ViewModels;

namespace Porjai20.Views
{
    public partial class StaffView : UserControl
    {
        public StaffView()
        {
            InitializeComponent();
        }

        private void StaffRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is User staff)
            {
                if (DataContext is ProductViewModel vm)
                {
                    vm.SelectedStaff = staff;
                    if (vm.OpenStaffManageModalCommand?.CanExecute(staff) == true)
                    {
                        vm.OpenStaffManageModalCommand.Execute(staff);
                    }
                    else
                    {
                        vm.ExecuteOpenStaffManageModal();
                    }
                }
                else if (DataContext is StaffViewModel svm)
                {
                    svm.SelectedStaff = staff;
                    if (svm.OpenStaffManageModalCommand?.CanExecute(staff) == true)
                    {
                        svm.OpenStaffManageModalCommand.Execute(staff);
                    }
                    else
                    {
                        svm.ExecuteOpenManageModal();
                    }
                }
            }
        }
    }
}