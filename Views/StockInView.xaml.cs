using System.Windows.Controls;
using System.Windows.Input;
using Porjai20.Models;
using Porjai20.ViewModels;

namespace Porjai20.Views
{
    public partial class StockInView : UserControl
    {
        public StockInView()
        {
            InitializeComponent();
        }

        private void PendingOrdersRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is PurchaseOrder po)
            {
                if (DataContext is ProductViewModel vm)
                {
                    vm.SelectedReceiptPO = po;
                    if (vm.OpenGoodsReceiptModalCommand?.CanExecute(po) == true)
                    {
                        vm.OpenGoodsReceiptModalCommand.Execute(po);
                    }
                }
            }
        }
    }
}