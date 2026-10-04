using System.Windows.Controls;
using System.Windows.Input;
using Porjai20.Models;
using Porjai20.ViewModels;

namespace Porjai20.Views
{
    public partial class DeliveryView : UserControl
    {
        public DeliveryView()
        {
            InitializeComponent();
        }

        private void DeliveryRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is SalesOrder order)
            {
                if (DataContext is ProductViewModel vm)
                {
                    if (vm.ViewDeliveryDetailsCommand?.CanExecute(order) == true)
                    {
                        vm.ViewDeliveryDetailsCommand.Execute(order);
                    }
                }
            }
        }
    }
}