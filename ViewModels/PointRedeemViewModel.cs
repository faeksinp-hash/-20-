using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Porjai20.Common;
using Porjai20.Models;
using Porjai20.Services;

namespace Porjai20.ViewModels
{
    /// <summary>
    /// ViewModel สำหรับการ์ดโปรโมชั่นแลกแต้มแต่ละรายการในหน้าต่างแลกแต้มสะสม
    /// </summary>
    public class RedeemPromotionItemViewModel : ViewModelBase
    {
        private bool _isSelected;
        private int _customPoints;
        private string _customPointsText = "";

        public PromotionModel? Promotion { get; }
        public bool IsStandardRedeem { get; }
        public string PromoId => Promotion?.PromoID ?? "STANDARD-1TO1";
        public string PromoName { get; }
        public int PointsRequired => IsStandardRedeem ? _customPoints : (Promotion?.PointsRequired ?? 0);
        public decimal DiscountAmount => IsStandardRedeem ? (_customPoints * 1.00m) : (Promotion?.DiscountAmount ?? 0m);
        public decimal MinSpend => Promotion?.MinSpend ?? 0m;

        public bool IsPointsEnough { get; }
        public bool IsMinSpendMet { get; }
        public bool HasTargetProduct { get; }
        public bool IsEligible { get; }
        public string? IneligibleReason { get; }
        public string TargetProductNames { get; } = "";

        public string DisplayMinSpend => MinSpend > 0 ? $"ยอดซื้อขั้นต่ำ {MinSpend:N2} บาท" : "ไม่มียอดซื้อขั้นต่ำ";
        public string DisplayDiscount => $"-{DiscountAmount:N2} บาท";
        public string DisplayPointsText => IsStandardRedeem ? "อัตรา 1 แต้ม = 1 บาท" : $"ใช้ {Promotion?.PointsRequired ?? 0} แต้ม";

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value))
                {
                    OnPropertyChanged(nameof(CardBackground));
                    OnPropertyChanged(nameof(CardBorderBrush));
                    OnPropertyChanged(nameof(CardBorderThickness));
                }
            }
        }

        public string CardBackground => IsSelected ? "#F0F9FF" : (IsEligible ? "#FFFFFF" : "#F8FAFC");
        public string CardBorderBrush => IsSelected ? "#0284C7" : (IsEligible ? "#38BDF8" : "#E2E8F0");
        public double CardBorderThickness => IsSelected ? 2.0 : 1.5;

        public int MaxCustomPoints { get; set; }

        public string CustomPointsText
        {
            get => _customPointsText;
            set
            {
                if (SetProperty(ref _customPointsText, value))
                {
                    if (int.TryParse(value, out int p))
                    {
                        if (p > MaxCustomPoints) p = MaxCustomPoints;
                        if (p < 0) p = 0;
                        _customPoints = p;
                    }
                    else
                    {
                        _customPoints = 0;
                    }
                    OnPropertyChanged(nameof(PointsRequired));
                    OnPropertyChanged(nameof(DiscountAmount));
                    OnPropertyChanged(nameof(DisplayDiscount));
                }
            }
        }

        /// <summary>
        /// Constructor สำหรับโปรโมชั่นประเภท 'Redeem' จากฐานข้อมูล
        /// </summary>
        public RedeemPromotionItemViewModel(
            PromotionModel promo,
            int memberPoints,
            decimal subtotal,
            IEnumerable<CartItem>? cartItems)
        {
            Promotion = promo;
            IsStandardRedeem = false;
            PromoName = !string.IsNullOrWhiteSpace(promo.PromoName)
                ? promo.PromoName
                : $"ใช้ {promo.PointsRequired} แต้ม แลกส่วนลด {promo.DiscountAmount:N2} บาท";

            // ตรวจสอบสินค้าที่ร่วมรายการ (ถ้ามี)
            bool targetOk = true;
            if (promo.TargetScope == "SpecificProducts" && promo.ProductIds != null && promo.ProductIds.Count > 0)
            {
                var items = cartItems?.ToList() ?? new List<CartItem>();
                targetOk = items.Any(ci =>
                    promo.ProductIds.Contains(ci.Product.Id.ToString()) ||
                    (!string.IsNullOrEmpty(ci.Product.Code) && promo.ProductIds.Contains(ci.Product.Code)) ||
                    promo.ProductIds.Contains($"P-{ci.Product.Id:D4}"));

                if (promo.ProductNames != null && promo.ProductNames.Count > 0)
                {
                    TargetProductNames = "ใช้กับ: " + string.Join(", ", promo.ProductNames);
                }
            }
            HasTargetProduct = targetOk;

            // ประเมินเงื่อนไข
            IsPointsEnough = memberPoints >= promo.PointsRequired;
            IsMinSpendMet = promo.MinSpend <= 0 || subtotal >= promo.MinSpend;
            IsEligible = IsPointsEnough && IsMinSpendMet && HasTargetProduct;

            // ระบุเหตุผลที่ไม่ผ่านเกณฑ์ (ถ้ามี)
            if (!IsPointsEnough)
            {
                IneligibleReason = $"⚠️ แต้มสะสมไม่พอ (ต้องใช้ {promo.PointsRequired} แต้ม)";
            }
            else if (!IsMinSpendMet)
            {
                decimal diff = promo.MinSpend - subtotal;
                IneligibleReason = $"⚠️ ยอดซื้อยังขาดอีก {diff:N2} บาท (ขั้นต่ำ {promo.MinSpend:N2} บาท)";
            }
            else if (!HasTargetProduct)
            {
                IneligibleReason = "⚠️ ไม่มีสินค้าที่ร่วมรายการในตะกร้า";
            }
            else
            {
                IneligibleReason = null;
            }
        }

        /// <summary>
        /// Constructor สำหรับตัวเลือกแลกแต้มทั่วไป (1 แต้ม = 1 บาท)
        /// </summary>
        public RedeemPromotionItemViewModel(int memberPoints, decimal subtotal, int initialPoints = 0)
        {
            Promotion = null;
            IsStandardRedeem = true;
            PromoName = "แลกเป็นส่วนลดทั่วไป (1 แต้ม = 1 บาท)";
            MaxCustomPoints = Math.Max(0, Math.Min(memberPoints, (int)Math.Floor(subtotal)));
            _customPoints = initialPoints > 0 ? Math.Min(initialPoints, MaxCustomPoints) : Math.Min(memberPoints, MaxCustomPoints);
            _customPointsText = _customPoints.ToString();

            IsPointsEnough = memberPoints > 0;
            IsMinSpendMet = subtotal > 0;
            HasTargetProduct = true;
            IsEligible = memberPoints > 0 && subtotal > 0 && MaxCustomPoints > 0;

            if (memberPoints <= 0)
            {
                IneligibleReason = "⚠️ คุณไม่มีแต้มสะสมคงเหลือ";
            }
            else if (subtotal <= 0)
            {
                IneligibleReason = "⚠️ ยอดรวมสินค้าเป็น 0 บาท";
            }
            else
            {
                IneligibleReason = null;
            }
        }

        public void SetCustomPoints(int points)
        {
            if (points > MaxCustomPoints) points = MaxCustomPoints;
            if (points < 0) points = 0;
            _customPoints = points;
            _customPointsText = points.ToString();
            OnPropertyChanged(nameof(CustomPointsText));
            OnPropertyChanged(nameof(PointsRequired));
            OnPropertyChanged(nameof(DiscountAmount));
            OnPropertyChanged(nameof(DisplayDiscount));
        }
    }

    /// <summary>
    /// ViewModel สำหรับหน้าต่าง Modal แลกแต้มสะสม (PointRedeemDialog)
    /// เชื่อมต่อกับโปรโมชั่นประเภท 'Redeem' ใน tblPromotion และมีตัวเลือก 1:1 ทั่วไป
    /// </summary>
    public class PointRedeemViewModel : ViewModelBase
    {
        private Customer? _currentMember;
        private decimal _subtotalAmount;
        private RedeemPromotionItemViewModel? _selectedItem;
        private int _selectedPointsToUse;
        private decimal _selectedDiscount;
        private string _validationMessage = "";

        public Customer? CurrentMember
        {
            get => _currentMember;
            set
            {
                if (SetProperty(ref _currentMember, value))
                {
                    OnPropertyChanged(nameof(CustomerName));
                    OnPropertyChanged(nameof(CurrentPoints));
                    OnPropertyChanged(nameof(CustomerPhone));
                }
            }
        }

        public string CustomerName => !string.IsNullOrWhiteSpace(_currentMember?.CustomerName) 
            ? _currentMember.CustomerName 
            : (!string.IsNullOrWhiteSpace(_currentMember?.Name) ? _currentMember.Name : "สมาชิก");

        public string CustomerPhone => !string.IsNullOrWhiteSpace(_currentMember?.PhoneNumber)
            ? _currentMember.PhoneNumber
            : (!string.IsNullOrWhiteSpace(_currentMember?.Phone) ? _currentMember.Phone : "-");

        public int CurrentPoints => _currentMember?.Points ?? 0;

        public decimal SubtotalAmount
        {
            get => _subtotalAmount;
            set => SetProperty(ref _subtotalAmount, value);
        }

        public ObservableCollection<RedeemPromotionItemViewModel> PromoCards { get; } = new();

        public RedeemPromotionItemViewModel? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    UpdateSelection();
                }
            }
        }

        public PromotionModel? SelectedPromo => _selectedItem?.Promotion;
        public bool IsStandardRedeemSelected => _selectedItem?.IsStandardRedeem == true;

        public int SelectedPointsToUse
        {
            get => _selectedPointsToUse;
            set
            {
                if (SetProperty(ref _selectedPointsToUse, value))
                {
                    OnPropertyChanged(nameof(CanConfirmRedeem));
                }
            }
        }

        public decimal SelectedDiscount
        {
            get => _selectedDiscount;
            set
            {
                if (SetProperty(ref _selectedDiscount, value))
                {
                    OnPropertyChanged(nameof(CanConfirmRedeem));
                }
            }
        }

        public bool HasSelection => _selectedItem != null && _selectedItem.IsEligible;

        public bool CanConfirmRedeem => _selectedItem != null &&
                                        _selectedItem.IsEligible &&
                                        SelectedPointsToUse > 0 &&
                                        SelectedDiscount > 0;

        public string ValidationMessage
        {
            get => _validationMessage;
            set
            {
                if (SetProperty(ref _validationMessage, value))
                {
                    OnPropertyChanged(nameof(HasValidationMessage));
                }
            }
        }

        public bool HasValidationMessage => !string.IsNullOrWhiteSpace(_validationMessage);

        public ICommand SelectCardCommand { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<PointRedeemViewModel>? Confirmed;
        public event Action? Cancelled;

        public PointRedeemViewModel(
            Customer? member,
            IEnumerable<CartItem>? cartItems,
            decimal subtotal,
            string? initialPromoId = null,
            int initialPoints = 0)
        {
            _currentMember = member;
            _subtotalAmount = subtotal;

            SelectCardCommand = new RelayCommand(p =>
            {
                if (p is RedeemPromotionItemViewModel card && card.IsEligible)
                {
                    SelectItem(card);
                }
            });

            ConfirmCommand = new RelayCommand(_ => ExecuteConfirm(), _ => CanConfirmRedeem);
            CancelCommand = new RelayCommand(_ => ExecuteCancel());

            LoadPromotions(member, cartItems, subtotal, initialPromoId, initialPoints);
        }

        public void LoadPromotions(
            Customer? member,
            IEnumerable<CartItem>? cartItems,
            decimal subtotal,
            string? initialPromoId = null,
            int initialPoints = 0)
        {
            PromoCards.Clear();
            int points = member?.Points ?? 0;

            // 1. ดึงโปรโมชั่นประเภท 'Redeem' จาก PromotionService
            var redeemPromos = PromotionService.Instance.GetActiveRedeemPromotionsSync();

            RedeemPromotionItemViewModel? toSelect = null;

            foreach (var promo in redeemPromos)
            {
                var card = new RedeemPromotionItemViewModel(promo, points, subtotal, cartItems);
                PromoCards.Add(card);

                if (!string.IsNullOrEmpty(initialPromoId) && promo.PromoID == initialPromoId && card.IsEligible)
                {
                    toSelect = card;
                }
            }

            // 2. เพิ่มตัวเลือกแลกแต้มทั่วไป (1 แต้ม = 1 บาท)
            var standardCard = new RedeemPromotionItemViewModel(points, subtotal, initialPoints);
            PromoCards.Add(standardCard);

            if (toSelect == null && (initialPoints > 0 || string.IsNullOrEmpty(initialPromoId)))
            {
                if (initialPoints > 0 && standardCard.IsEligible)
                {
                    standardCard.SetCustomPoints(initialPoints);
                    toSelect = standardCard;
                }
                else
                {
                    // เลือกโปรโมชั่นแรกที่ผ่านเกณฑ์ (ถ้ามี)
                    toSelect = PromoCards.FirstOrDefault(c => c.IsEligible);
                }
            }

            if (toSelect != null && toSelect.IsEligible)
            {
                SelectItem(toSelect);
            }
            else
            {
                UpdateSelection();
            }
        }

        public void SelectItem(RedeemPromotionItemViewModel card)
        {
            foreach (var c in PromoCards)
            {
                c.IsSelected = (c == card);
            }
            _selectedItem = card;
            OnPropertyChanged(nameof(SelectedItem));
            UpdateSelection();
        }

        public void UpdateSelection()
        {
            ValidationMessage = "";
            if (_selectedItem == null || !_selectedItem.IsEligible)
            {
                SelectedPointsToUse = 0;
                SelectedDiscount = 0m;
                OnPropertyChanged(nameof(HasSelection));
                OnPropertyChanged(nameof(CanConfirmRedeem));
                OnPropertyChanged(nameof(SelectedPromo));
                OnPropertyChanged(nameof(IsStandardRedeemSelected));
                return;
            }

            SelectedPointsToUse = _selectedItem.PointsRequired;
            SelectedDiscount = _selectedItem.DiscountAmount;

            OnPropertyChanged(nameof(HasSelection));
            OnPropertyChanged(nameof(CanConfirmRedeem));
            OnPropertyChanged(nameof(SelectedPromo));
            OnPropertyChanged(nameof(IsStandardRedeemSelected));
        }

        private void ExecuteConfirm()
        {
            if (!CanConfirmRedeem)
            {
                ValidationMessage = "กรุณาเลือกโปรโมชั่นแลกแต้มที่ต้องการใช้";
                return;
            }

            Confirmed?.Invoke(this);
        }

        private void ExecuteCancel()
        {
            Cancelled?.Invoke();
        }
    }
}
