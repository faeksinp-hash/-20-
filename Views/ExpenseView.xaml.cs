using System.Windows.Controls;
using System.Windows.Input;
using Porjai20.Models;
using Porjai20.ViewModels;

namespace Porjai20.Views
{
    public partial class ExpenseView : UserControl
    {
        public ExpenseView()
        {
            InitializeComponent();
        }

        private void ExpenseRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is Expense expense)
            {
                if (DataContext is ProductViewModel vm)
                {
                    vm.SelectedExpense = expense;
                    if (vm.OpenManageExpenseModalCommand?.CanExecute(expense) == true)
                    {
                        vm.OpenManageExpenseModalCommand.Execute(expense);
                    }
                    else
                    {
                        vm.ExecuteOpenManageExpenseModal();
                    }
                }
                else if (DataContext is ExpenseViewModel evm)
                {
                    evm.SelectedExpense = expense;
                    if (evm.OpenManageExpenseModalCommand?.CanExecute(expense) == true)
                    {
                        evm.OpenManageExpenseModalCommand.Execute(expense);
                    }
                    else
                    {
                        evm.ExecuteOpenManageModal();
                    }
                }
            }
        }
    }
}