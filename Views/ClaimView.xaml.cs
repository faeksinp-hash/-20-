using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
                }
            };
            ModalGrid.IsVisibleChanged += async (s, e) =>
            {
                if (ModalGrid.Visibility == Visibility.Visible && DataContext is ProductViewModel vm)
                {
                    vm.GhostTextSuggestion = string.Empty;
                    await vm.RefreshDocumentNumbersCacheAsync();
                }
            };
        }

        private void TxtReceiptNo_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is ProductViewModel vm && !string.IsNullOrEmpty(vm.GhostTextSuggestion))
            {
                // ตรวจสอบหากผู้ใช้กดปุ่ม Key.Tab หรือ (Key.Right ในขณะที่เคอร์เซอร์อยู่ท้ายสุดของข้อความ)
                if (e.Key == Key.Tab || (e.Key == Key.Right && txtReceiptNo.CaretIndex == txtReceiptNo.Text.Length))
                {
                    // แทนที่ข้อความใน TextBox ด้วยข้อความแนะนำเต็มทันที
                    string completed = vm.GetCanonicalDocumentNumber(txtReceiptNo.Text) ?? vm.GhostTextSuggestion;
                    txtReceiptNo.Text = completed;
                    txtReceiptNo.CaretIndex = txtReceiptNo.Text.Length;
                    vm.GhostTextSuggestion = string.Empty;
                    e.Handled = true; // ป้องกันไม่ให้ปุ่ม Tab เลื่อนโฟกัสกระโดดไปช่องอื่น
                }
            }
        }
    }
}