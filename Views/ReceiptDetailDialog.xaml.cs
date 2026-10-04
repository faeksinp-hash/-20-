using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Porjai20.Common;
using Porjai20.Models;

namespace Porjai20.Views
{
    public partial class ReceiptDetailDialog : Window
    {
        public ReceiptDetailDialog(SalesOrder order, IEnumerable<SalesOrderItem> items)
        {
            InitializeComponent();
            DataContext = new ReceiptDetailViewModel(order, items);

            MouseDown += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    DragMove();
                }
            };
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
                return;
            }
            base.OnKeyDown(e);
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var title = "Sales Receipt";
                if (DataContext is ReceiptDetailViewModel vm && !string.IsNullOrWhiteSpace(vm.RefNo))
                {
                    title += " - " + vm.RefNo;
                }
                PrintHelper.PrintVisual(ReceiptArea, title);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการพิมพ์: {ex.Message}", "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    public class ReceiptDetailViewModel
    {
        public string RefNo { get; set; } = string.Empty;
        public string FormattedDate { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Sales_Status { get; set; } = string.Empty;
        public string StatusBg { get; set; } = "#DCFCE7";
        public string StatusFg { get; set; } = "#16A34A";
        public List<SalesOrderItem> Items { get; set; } = new();
        public decimal SubtotalAmount { get; set; }
        public bool HasDiscount { get; set; }
        public decimal DiscountAmount { get; set; }
        public bool HasPointsUsed { get; set; }
        public int PointsUsed { get; set; }
        public decimal Sales_Total { get; set; }
        public string Sales_PaymentType { get; set; } = string.Empty;
        public bool HasPointsEarned { get; set; }
        public int PointsEarned { get; set; }

        public ReceiptDetailViewModel(SalesOrder order, IEnumerable<SalesOrderItem> items)
        {
            if (order == null) return;

            RefNo = order.RefNo;
            FormattedDate = DateTimeHelper.ToBuddhistEra(order.Timestamp, "dd/MM/yyyy HH:mm");
            CustomerName = order.CustomerDisplayName;
            Sales_Status = string.IsNullOrWhiteSpace(order.Sales_Status) ? "ชำระเงินแล้ว" : order.Sales_Status;

            bool isCancelled = Sales_Status.Contains("ยกเลิก", StringComparison.OrdinalIgnoreCase) ||
                              Sales_Status.Contains("Void", StringComparison.OrdinalIgnoreCase);
            StatusBg = isCancelled ? "#FEE2E2" : "#DCFCE7";
            StatusFg = isCancelled ? "#DC2626" : "#16A34A";

            Items = items != null ? items.ToList() : new List<SalesOrderItem>();
            SubtotalAmount = Items.Sum(i => i.Total);
            DiscountAmount = order.DiscountAmount;
            HasDiscount = DiscountAmount > 0;
            PointsUsed = order.PointsUsed;
            HasPointsUsed = PointsUsed > 0;
            Sales_Total = order.Sales_Total;
            Sales_PaymentType = string.IsNullOrWhiteSpace(order.Sales_PaymentType) ? "เงินสด" : order.Sales_PaymentType;
            PointsEarned = order.PointsEarned;
            HasPointsEarned = PointsEarned > 0;
        }
    }
}
