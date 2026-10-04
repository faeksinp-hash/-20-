using System;
using System.Windows;
using System.Windows.Controls;

namespace Porjai20.Views
{
    public partial class PointRedeemDialog : Window
    {
        public int CurrentPoints { get; set; }
        public int MaxAllowedPoints { get; set; }
        public int RedeemedPoints { get; private set; }
        public decimal DiscountValue => RedeemedPoints * 1.00m;

        public PointRedeemDialog(string customerName = "", int currentPoints = 0, int maxAllowedPoints = 0, int initialPoints = 0)
        {
            InitializeComponent();
            CurrentPoints = currentPoints;
            MaxAllowedPoints = Math.Max(0, Math.Min(currentPoints, maxAllowedPoints));

            runCustomerName.Text = string.IsNullOrWhiteSpace(customerName) ? "สมาชิก" : customerName;
            runCurrentPoints.Text = currentPoints.ToString("N0");

            int startPoints = initialPoints > 0 ? Math.Min(initialPoints, MaxAllowedPoints) : 0;
            txtInputPoints.Text = startPoints > 0 ? startPoints.ToString() : "";
            UpdateCalculation();
        }

        private void TxtInputPoints_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCalculation();
        }

        private void UpdateCalculation()
        {
            if (txtValidationMessage == null || txtCalculatedDiscount == null) return;

            txtValidationMessage.Visibility = Visibility.Collapsed;
            txtValidationMessage.Text = "";

            string text = txtInputPoints.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                txtCalculatedDiscount.Text = "0.00";
                return;
            }

            if (!int.TryParse(text, out int points))
            {
                txtValidationMessage.Text = "กรุณากรอกตัวเลขจำนวนเต็ม";
                txtValidationMessage.Visibility = Visibility.Visible;
                txtCalculatedDiscount.Text = "0.00";
                return;
            }

            if (points < 0)
            {
                txtValidationMessage.Text = "จำนวนแต้มต้องมากกว่า 0";
                txtValidationMessage.Visibility = Visibility.Visible;
                txtCalculatedDiscount.Text = "0.00";
                return;
            }

            if (points > MaxAllowedPoints)
            {
                points = MaxAllowedPoints;
                txtInputPoints.Text = points.ToString();
                txtInputPoints.CaretIndex = txtInputPoints.Text.Length;
                txtValidationMessage.Text = $"ปรับเป็นแต้มสูงสุดที่ใช้ได้ ({MaxAllowedPoints} แต้ม) อัตโนมัติ";
                txtValidationMessage.Visibility = Visibility.Visible;
            }

            decimal discount = points * 1.00m;
            txtCalculatedDiscount.Text = discount.ToString("N2");
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            string text = txtInputPoints.Text.Trim();
            if (!int.TryParse(text, out int points) || points <= 0)
            {
                txtValidationMessage.Text = "กรุณากรอกจำนวนแต้มที่ถูกต้อง (> 0)";
                txtValidationMessage.Visibility = Visibility.Visible;
                return;
            }

            if (points > MaxAllowedPoints)
            {
                points = MaxAllowedPoints;
            }

            if (points <= 0)
            {
                txtValidationMessage.Text = "ไม่สามารถใช้แต้มได้ในบิลนี้";
                txtValidationMessage.Visibility = Visibility.Visible;
                return;
            }

            RedeemedPoints = points;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
