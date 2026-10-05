using System;
using System.Collections.Generic;
using System.Windows;
using Porjai20.Models;
using Porjai20.ViewModels;

namespace Porjai20.Views
{
    public partial class PointRedeemDialog : Window
    {
        public PointRedeemViewModel ViewModel { get; }

        public int CurrentPoints => ViewModel.CurrentPoints;
        public int RedeemedPoints => ViewModel.SelectedPointsToUse;
        public decimal DiscountValue => ViewModel.SelectedDiscount;
        public PromotionModel? SelectedPromotion => ViewModel.SelectedPromo;

        public PointRedeemDialog(PointRedeemViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = viewModel;

            ViewModel.Confirmed += vm =>
            {
                DialogResult = true;
                Close();
            };

            ViewModel.Cancelled += () =>
            {
                DialogResult = false;
                Close();
            };
        }

        public PointRedeemDialog(
            Customer? member = null,
            IEnumerable<CartItem>? cartItems = null,
            decimal subtotal = 0m,
            string? initialPromoId = null,
            int initialPoints = 0)
            : this(new PointRedeemViewModel(member, cartItems, subtotal, initialPromoId, initialPoints))
        {
        }

        public PointRedeemDialog(
            string customerName = "",
            int currentPoints = 0,
            int maxAllowedPoints = 0,
            int initialPoints = 0)
            : this(
                new Customer
                {
                    Name = string.IsNullOrWhiteSpace(customerName) ? "สมาชิก" : customerName,
                    CustomerName = string.IsNullOrWhiteSpace(customerName) ? "สมาชิก" : customerName,
                    Points = currentPoints
                },
                null,
                maxAllowedPoints > 0 ? (decimal)maxAllowedPoints : 0m,
                null,
                initialPoints)
        {
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CanConfirmRedeem)
            {
                DialogResult = true;
                Close();
            }
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
