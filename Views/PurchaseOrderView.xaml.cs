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
    }
}