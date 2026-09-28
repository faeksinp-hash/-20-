using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Porjai20.ViewModels;

namespace Porjai20.Views
{
    public partial class PosView : UserControl
    {
        public PosView()
        {
            InitializeComponent();
            this.Loaded += PosView_Loaded;
        }

        private void PosView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PosViewModel vm)
            {
                vm.IsDeliveryModalOpen = false;
                vm.IsAddNewAddressMode = false;
                if (vm.Products.Count == 0 && vm.LoadProductsCommand.CanExecute(null))
                {
                    vm.LoadProductsCommand.Execute(null);
                }
            }
            else if (DataContext is not PosViewModel)
            {
                var newVm = new PosViewModel();
                newVm.IsDeliveryModalOpen = false;
                newVm.IsAddNewAddressMode = false;
                this.DataContext = newVm;
            }
        }

        private void ChkEnableMember_Unchecked(object sender, RoutedEventArgs e)
        {
            if (DataContext is PosViewModel vm)
            {
                vm.SelectedCustomer = null;
                vm.CustomerSearchText = "";
                vm.MemberSearchResultText = "";
                vm.IsCustomerDropDownOpen = false;
                vm.IsManualMemberSearchMode = false;
            }
        }

        private void BtnSearchCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PosViewModel vm && vm.SearchCustomerCommand.CanExecute(null))
            {
                vm.SearchCustomerCommand.Execute(null);
            }
        }
    }
}