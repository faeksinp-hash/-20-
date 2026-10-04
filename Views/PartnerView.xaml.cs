using System.Windows.Controls;
using System.Windows.Input;
using Porjai20.Models;
using Porjai20.ViewModels;

namespace Porjai20.Views
{
    public partial class PartnerView : UserControl
    {
        public PartnerView()
        {
            InitializeComponent();
        }

        private void PartnerRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is Partner partner)
            {
                if (DataContext is ProductViewModel vm)
                {
                    vm.SelectedPartner = partner;
                    if (vm.OpenPartnerManageModalCommand?.CanExecute(partner) == true)
                    {
                        vm.OpenPartnerManageModalCommand.Execute(partner);
                    }
                    else
                    {
                        vm.ExecuteOpenPartnerManageModal();
                    }
                }
                else if (DataContext is PartnerViewModel pvm)
                {
                    pvm.SelectedPartner = partner;
                    if (pvm.OpenPartnerManageModalCommand?.CanExecute(partner) == true)
                    {
                        pvm.OpenPartnerManageModalCommand.Execute(partner);
                    }
                    else
                    {
                        pvm.ExecuteOpenManageModal();
                    }
                }
            }
        }
    }
}