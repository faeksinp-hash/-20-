using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Porjai20.Views
{
    public partial class PurchaseOrderView : UserControl
    {
        public PurchaseOrderView()
        {
            InitializeComponent();
        }

        private void POProductComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                comboBox.IsDropDownOpen = true;
            }
        }

        private void POProductComboBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (sender is ComboBox comboBox && !comboBox.IsDropDownOpen)
            {
                comboBox.IsDropDownOpen = true;
            }
        }

        private void POCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (sender is FrameworkElement element && element.DataContext is Models.PurchaseOrder po)
                {
                    if (DataContext is ViewModels.ProductViewModel vm)
                    {
                        vm.SelectedPurchaseOrder = po;
                        if (vm.OpenEditPOModalCommand?.CanExecute(po) == true)
                        {
                            vm.OpenEditPOModalCommand.Execute(po);
                        }
                    }
                }
            }
        }
    }
}