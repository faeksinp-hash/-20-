using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dapper;
using Porjai20.Models;
using Porjai20.Services;
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
            }
        }

        private void BtnSearchCustomer_Click(object sender, RoutedEventArgs e)
        {
            string input = txtMemberSearchPhone.Text?.Trim().Replace("-", "").Replace(" ", "") ?? "";
            if (string.IsNullOrEmpty(input)) return;

            try
            {
                var dbService = new DatabaseService();
                using (var conn = dbService.GetConnection())
                {
                    string sql = @"SELECT Cus_ID as Id, Cus_Code as Code, Cus_Name as Name, 
                                          Cus_Address as Address, Cus_Tel as Phone, Cus_Points as Points 
                                   FROM tblCustomer";
                    var customers = conn.Query<Customer>(sql).ToList();
                    var matched = customers.Where(c => 
                        (!string.IsNullOrEmpty(c.Phone) && c.Phone.Replace("-", "").Replace(" ", "").Contains(input)) ||
                        (!string.IsNullOrEmpty(c.Name) && c.Name.Contains(input)) ||
                        (!string.IsNullOrEmpty(c.Code) && c.Code.Contains(input))
                    ).ToList();

                    if (matched.Any())
                    {
                        if (DataContext is PosViewModel vm)
                        {
                            vm.CustomerList.Clear();
                            foreach (var c in matched) vm.CustomerList.Add(c);
                            vm.SelectedCustomer = matched[0];
                            vm.IsMemberSelected = true;
                            vm.CustomerSearchText = matched[0].Phone;
                        }
                    }
                    else
                    {
                        MessageBox.Show("ไม่พบข้อมูลสมาชิกเบอร์นี้ในระบบ", "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการค้นหา: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}