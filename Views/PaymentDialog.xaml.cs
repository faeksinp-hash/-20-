using System.Windows;
using Porjai20.ViewModels;

namespace Porjai20.Views
{
    public partial class PaymentDialog : Window
    {
        public PaymentViewModel ViewModel => (PaymentViewModel)DataContext;

        public PaymentDialog()
        {
            InitializeComponent();
            DataContext = new PaymentViewModel();
        }

        public PaymentDialog(PaymentViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
