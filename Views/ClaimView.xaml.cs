using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Porjai20.Models;
using Porjai20.ViewModels;

namespace Porjai20.Views
{
    public partial class ClaimView : UserControl
    {
        public ClaimView()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                if (DataContext is ProductViewModel vm)
                {
                    await vm.RefreshDocumentNumbersCacheAsync();
                    UpdateSaveButtonState();
                }
            };
            ModalGrid.IsVisibleChanged += async (s, e) =>
            {
                if (ModalGrid.Visibility == Visibility.Visible && DataContext is ProductViewModel vm)
                {
                    vm.ClearGhostText();
                    await vm.RefreshDocumentNumbersCacheAsync();
                    UpdateSaveButtonState();
                }
            };
            txtReceiptNo.TextChanged += (s, e) =>
            {
                CommandManager.InvalidateRequerySuggested();
                UpdateSaveButtonState();
            };
        }

        private void ClaimRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is Claim claim)
            {
                if (DataContext is ProductViewModel vm)
                {
                    if (vm.ViewClaimDetailsCommand?.CanExecute(claim) == true)
                    {
                        vm.ViewClaimDetailsCommand.Execute(claim);
                    }
                }
            }
        }

        private void UpdateSaveButtonState()
        {
            if (DataContext is ProductViewModel vm)
            {
                bool isNew = vm.SelectedClaim == null || vm.SelectedClaim.Id == 0;
                btnSaveClaim.IsEnabled = isNew ? vm.CanSaveClaim() : vm.CanUpdateClaim();
            }
        }

        private void TxtReceiptNo_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is ProductViewModel vm && !string.IsNullOrEmpty(vm.GhostTextSuggestion))
            {
                // ตรวจสอบหากผู้ใช้กดปุ่ม Key.Tab หรือ (Key.Right ในขณะที่เคอร์เซอร์อยู่ท้ายสุดของข้อความ)
                if (e.Key == Key.Tab || (e.Key == Key.Right && txtReceiptNo.CaretIndex == txtReceiptNo.Text.Length))
                {
                    // แทนที่ข้อความใน TextBox ด้วยเลขที่บิลเต็ม 14 หลักทันที
                    string completed = vm.GetCanonicalDocumentNumber(txtReceiptNo.Text) ?? (txtReceiptNo.Text + vm.GhostTextSuggestion);
                    txtReceiptNo.Text = completed;
                    txtReceiptNo.CaretIndex = txtReceiptNo.Text.Length;
                    vm.ClearGhostText();
                    e.Handled = true; // ป้องกันไม่ให้ปุ่ม Tab เลื่อนโฟกัสกระโดดไปช่องอื่น
                }
            }
        }
    }
}