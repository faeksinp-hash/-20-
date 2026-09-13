using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Porjai20.ViewModels;

namespace Porjai20.Views
{
    public partial class StockInView : UserControl
    {
        public StockInView()
        {
            InitializeComponent();
        }

        private void PendingOrdersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Verify that the user double-clicked an actual row, not headers or blank space
            var dep = e.OriginalSource as System.Windows.DependencyObject;
            while (dep != null && !(dep is DataGridRow))
            {
                if (dep is System.Windows.Controls.Primitives.DataGridColumnHeader) return;
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep is DataGridRow && DataContext is ProductViewModel vm && vm.SelectedReceiptPO != null)
            {
                vm.OpenGoodsReceiptModalCommand?.Execute(vm.SelectedReceiptPO);
            }
        }
    }
}