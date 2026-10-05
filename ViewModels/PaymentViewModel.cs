using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Porjai20.Models;

namespace Porjai20.ViewModels
{
    public class PaymentViewModel : ViewModelBase
    {
        private Customer? _currentMember;
        public Customer? CurrentMember
        {
            get => _currentMember;
            set
            {
                if (SetProperty(ref _currentMember, value))
                {
                    CurrentPoints = value?.Points ?? 0;
                    OnPropertyChanged(nameof(CustomerName));
                    OnPropertyChanged(nameof(EarnedPoints));
                    ResetRedemption();
                    RecalculateAutoPromotion();
                }
            }
        }

        public string CustomerName => CurrentMember?.Name ?? "ลูกค้าทั่วไป";

        private int _currentPoints;
        public int CurrentPoints
        {
            get => _currentPoints;
            set
            {
                if (SetProperty(ref _currentPoints, value))
                {
                    OnPropertyChanged(nameof(MaxRedeemablePoints));
                }
            }
        }

        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                if (SetProperty(ref _totalAmount, value))
                {
                    OnPropertyChanged(nameof(SubtotalAmount));
                    OnPropertyChanged(nameof(DisplaySubtotal));
                    OnPropertyChanged(nameof(TotalAmountBeforeDiscount));
                    OnPropertyChanged(nameof(NetPayableAmount));
                    OnPropertyChanged(nameof(ChangeAmount));
                    OnPropertyChanged(nameof(EarnedPoints));
                    OnPropertyChanged(nameof(MaxRedeemablePoints));
                }
            }
        }

        public decimal SubtotalAmount
        {
            get => TotalAmount;
            set => TotalAmount = value;
        }

        public decimal DisplaySubtotal
        {
            get => TotalAmount;
            set => TotalAmount = value;
        }

        private decimal _totalAmountBeforeDiscount;
        public decimal TotalAmountBeforeDiscount
        {
            get => _totalAmountBeforeDiscount > 0 ? _totalAmountBeforeDiscount : _totalAmount;
            set
            {
                if (SetProperty(ref _totalAmountBeforeDiscount, value))
                {
                    if (_totalAmount <= 0)
                    {
                        _totalAmount = value + AutoIncludedGiftAmount;
                        OnPropertyChanged(nameof(TotalAmount));
                        OnPropertyChanged(nameof(SubtotalAmount));
                    }
                    OnPropertyChanged(nameof(DisplaySubtotal));
                    Recalculate();
                }
            }
        }

        private decimal _autoIncludedGiftAmount;
        public decimal AutoIncludedGiftAmount
        {
            get => _autoIncludedGiftAmount;
            set
            {
                if (SetProperty(ref _autoIncludedGiftAmount, value))
                {
                    OnPropertyChanged(nameof(DisplaySubtotal));
                    OnPropertyChanged(nameof(SubtotalAmount));
                    OnPropertyChanged(nameof(TotalAmount));
                    OnPropertyChanged(nameof(NetPayableAmount));
                    OnPropertyChanged(nameof(ChangeAmount));
                    OnPropertyChanged(nameof(EarnedPoints));
                    OnPropertyChanged(nameof(MaxRedeemablePoints));
                }
            }
        }

        private decimal _totalDiscount;
        public decimal TotalDiscount
        {
            get => _totalDiscount > 0 ? _totalDiscount : (PromotionDiscountAmount + DiscountAmount);
            set
            {
                if (SetProperty(ref _totalDiscount, value))
                {
                    OnPropertyChanged(nameof(NetPayableAmount));
                    OnPropertyChanged(nameof(ChangeAmount));
                    OnPropertyChanged(nameof(EarnedPoints));
                }
            }
        }

        private decimal _shippingFee;
        public decimal ShippingFee
        {
            get => _shippingFee;
            set
            {
                if (SetProperty(ref _shippingFee, value))
                {
                    Recalculate();
                }
            }
        }

        private int _usedPoints;
        public int UsedPoints
        {
            get => _usedPoints;
            set
            {
                if (SetProperty(ref _usedPoints, value))
                {
                    _discountAmount = _usedPoints * 1.00m;
                    OnPropertyChanged(nameof(DiscountAmount));
                    OnPropertyChanged(nameof(HasPointDiscount));
                    OnPropertyChanged(nameof(HasDiscount));
                    Recalculate();
                }
            }
        }

        private decimal _discountAmount;
        public decimal DiscountAmount
        {
            get => _discountAmount;
            set
            {
                if (SetProperty(ref _discountAmount, value))
                {
                    OnPropertyChanged(nameof(HasDiscount));
                    OnPropertyChanged(nameof(HasPointDiscount));
                    Recalculate();
                }
            }
        }

        public bool HasDiscount => DiscountAmount > 0;
        public bool HasPointDiscount => UsedPoints > 0;

        private bool _isDiscountApplied;
        public bool IsDiscountApplied
        {
            get => _isDiscountApplied || HasPointDiscount;
            set
            {
                if (SetProperty(ref _isDiscountApplied, value))
                {
                    OnPropertyChanged(nameof(DiscountText));
                    OnPropertyChanged(nameof(HasDiscount));
                    OnPropertyChanged(nameof(HasPointDiscount));
                }
            }
        }

        public string DiscountText => $"ใช้ส่วนลด: -{DiscountAmount:N2} บาท (-{UsedPoints} แต้ม)";

        public ICommand CancelDiscountCommand => CancelPointDiscountCommand;

        private bool _hasAutoPromotion;
        public bool HasAutoPromotion
        {
            get => _hasAutoPromotion;
            set => SetProperty(ref _hasAutoPromotion, value);
        }

        private string _autoPromotionName = "";
        public string AutoPromotionName
        {
            get => _autoPromotionName;
            set => SetProperty(ref _autoPromotionName, value);
        }

        private decimal _promotionDiscountAmount;
        public decimal PromotionDiscountAmount
        {
            get => _promotionDiscountAmount;
            set
            {
                if (SetProperty(ref _promotionDiscountAmount, value))
                {
                    OnPropertyChanged(nameof(NetPayableAmount));
                    OnPropertyChanged(nameof(ChangeAmount));
                    OnPropertyChanged(nameof(EarnedPoints));
                }
            }
        }

        public string? AppliedPromoID { get; set; }
        public PromotionModel? AppliedAutoPromotion { get; set; }
        public List<PromotionModel> AppliedPromotions { get; set; } = new();
        public ObservableCollection<Services.AppliedPromotionResult> AppliedPromotionDetails { get; } = new();

        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        private IEnumerable<CartItem>? _cartItems;
        public IEnumerable<CartItem>? CartItems
        {
            get => _cartItems;
            set
            {
                _cartItems = value;
                if (_cartItems != null)
                {
                    decimal itemsSum = _cartItems.Sum(x => x.Total);
                    if (itemsSum > 0)
                    {
                        _totalAmountBeforeDiscount = itemsSum;
                        _totalAmount = itemsSum + _autoIncludedGiftAmount;
                        OnPropertyChanged(nameof(TotalAmountBeforeDiscount));
                        OnPropertyChanged(nameof(TotalAmount));
                        OnPropertyChanged(nameof(SubtotalAmount));
                        OnPropertyChanged(nameof(DisplaySubtotal));
                    }
                }
                RecalculateAutoPromotion();
            }
        }

        private bool _isMemberChecked;
        public bool IsMemberChecked
        {
            get => _isMemberChecked;
            set
            {
                if (SetProperty(ref _isMemberChecked, value))
                {
                    RecalculateAutoPromotion();
                }
            }
        }

        public decimal NetPayableAmount => Math.Max(0, (TotalAmount > 0 ? TotalAmount : (DisplaySubtotal + ShippingFee)) - (TotalDiscount > 0 ? TotalDiscount : (PromotionDiscountAmount + DiscountAmount)));

        private decimal _cashReceived;
        public decimal CashReceived
        {
            get => _cashReceived;
            set
            {
                if (SetProperty(ref _cashReceived, value))
                {
                    OnPropertyChanged(nameof(ChangeAmount));
                }
            }
        }

        public decimal ChangeAmount => Math.Max(0, CashReceived - NetPayableAmount);

        public int EarnedPoints => (int)Math.Floor(NetPayableAmount / 100m);

        public int MaxRedeemablePoints
        {
            get
            {
                int available = CurrentPoints;
                decimal baseAmount = TotalAmount > 0 ? TotalAmount : (DisplaySubtotal + ShippingFee);
                int maxByBill = (int)Math.Floor(baseAmount);
                return Math.Max(0, Math.Min(available, maxByBill));
            }
        }

        private bool _isPointRedeemModalOpen;
        public bool IsPointRedeemModalOpen
        {
            get => _isPointRedeemModalOpen;
            set => SetProperty(ref _isPointRedeemModalOpen, value);
        }

        private int _redeemPoints;
        public int RedeemPoints
        {
            get => _redeemPoints;
            set
            {
                if (SetProperty(ref _redeemPoints, value))
                {
                    string textVal = value > 0 ? value.ToString() : "";
                    if (_inputRedeemPointsText != textVal)
                    {
                        _inputRedeemPointsText = textVal;
                        OnPropertyChanged(nameof(InputRedeemPointsText));
                    }
                    UpdateCalculatedDiscount();
                }
            }
        }

        private string _inputRedeemPointsText = "";
        public string InputRedeemPointsText
        {
            get => _inputRedeemPointsText;
            set
            {
                if (SetProperty(ref _inputRedeemPointsText, value))
                {
                    int.TryParse(value, out int parsed);
                    if (_redeemPoints != parsed)
                    {
                        _redeemPoints = parsed;
                        OnPropertyChanged(nameof(RedeemPoints));
                    }
                    UpdateCalculatedDiscount();
                }
            }
        }

        private decimal _calculatedDiscount;
        public decimal CalculatedDiscount
        {
            get => _calculatedDiscount;
            set => SetProperty(ref _calculatedDiscount, value);
        }

        private string _redeemValidationMessage = "";
        public string RedeemValidationMessage
        {
            get => _redeemValidationMessage;
            set
            {
                if (SetProperty(ref _redeemValidationMessage, value))
                {
                    OnPropertyChanged(nameof(HasRedeemValidationMessage));
                }
            }
        }
        public bool HasRedeemValidationMessage => !string.IsNullOrEmpty(RedeemValidationMessage);

        public ICommand OpenPointRedeemCommand { get; }
        public ICommand ClosePointRedeemModalCommand { get; }
        public ICommand ConfirmPointRedeemCommand { get; }
        public ICommand CancelPointDiscountCommand { get; }

        public PaymentViewModel()
        {
            OpenPointRedeemCommand = new RelayCommand(_ => OpenPointRedeem());
            ClosePointRedeemModalCommand = new RelayCommand(_ => { IsPointRedeemModalOpen = false; });
            ConfirmPointRedeemCommand = new RelayCommand(_ => ConfirmPointRedeem());
            CancelPointDiscountCommand = new RelayCommand(_ => ResetRedemption());
            RecalculateAutoPromotion();
        }

        public PaymentViewModel(IEnumerable<CartItem>? cartItems, decimal subtotal = 0m, Customer? member = null) : this()
        {
            if (member != null)
            {
                CurrentMember = member;
            }
            if (cartItems != null)
            {
                _cartItems = cartItems;
                decimal itemsSum = cartItems.Sum(x => x.Total);
                if (subtotal <= 0) subtotal = itemsSum;
            }
            if (subtotal > 0)
            {
                _totalAmountBeforeDiscount = subtotal;
                _totalAmount = subtotal;
                OnPropertyChanged(nameof(TotalAmountBeforeDiscount));
                OnPropertyChanged(nameof(TotalAmount));
                OnPropertyChanged(nameof(SubtotalAmount));
                OnPropertyChanged(nameof(DisplaySubtotal));
            }
            RecalculateAutoPromotion();
        }

        public PaymentViewModel(IEnumerable<CartItem>? cartItems, Customer? member) : this(cartItems, 0m, member)
        {
        }

        public void OpenPointRedeem()
        {
            RedeemValidationMessage = "";
            int initial = UsedPoints > 0 ? UsedPoints : Math.Min(CurrentPoints, MaxRedeemablePoints);
            RedeemPoints = initial;
            InputRedeemPointsText = initial > 0 ? initial.ToString() : "";
            UpdateCalculatedDiscount();
            IsPointRedeemModalOpen = true;
        }

        public void UpdateCalculatedDiscount()
        {
            RedeemValidationMessage = "";
            if (string.IsNullOrWhiteSpace(InputRedeemPointsText))
            {
                CalculatedDiscount = 0;
                return;
            }

            if (!int.TryParse(InputRedeemPointsText.Trim(), out int points))
            {
                RedeemValidationMessage = "กรุณากรอกตัวเลขจำนวนเต็ม";
                CalculatedDiscount = 0;
                return;
            }

            if (points < 0)
            {
                RedeemValidationMessage = "จำนวนแต้มต้องมากกว่า 0";
                CalculatedDiscount = 0;
                return;
            }

            int maxAllowed = MaxRedeemablePoints;
            if (points > maxAllowed)
            {
                points = maxAllowed;
                _redeemPoints = points;
                OnPropertyChanged(nameof(RedeemPoints));
                _inputRedeemPointsText = points.ToString();
                OnPropertyChanged(nameof(InputRedeemPointsText));
                RedeemValidationMessage = $"ปรับเป็นแต้มสูงสุดที่ใช้ได้ ({maxAllowed} แต้ม) อัตโนมัติ";
            }

            CalculatedDiscount = points * 1.00m;
        }

        public void ConfirmPointRedeem()
        {
            if (!int.TryParse(InputRedeemPointsText?.Trim(), out int points) || points <= 0)
            {
                RedeemValidationMessage = "กรุณากรอกจำนวนแต้มที่ถูกต้อง (> 0)";
                return;
            }

            int maxAllowed = MaxRedeemablePoints;
            if (points > maxAllowed) points = maxAllowed;

            if (points <= 0)
            {
                RedeemValidationMessage = "ไม่สามารถใช้แต้มได้ในบิลนี้";
                return;
            }

            UsedPoints = points;
            DiscountAmount = points * 1.00m;
            IsDiscountApplied = true;
            OnPropertyChanged(nameof(IsDiscountApplied));
            OnPropertyChanged(nameof(DiscountText));
            IsPointRedeemModalOpen = false;
        }

        public void ResetRedemption()
        {
            UsedPoints = 0;
            DiscountAmount = 0;
            RedeemPoints = 0;
            InputRedeemPointsText = "";
            CalculatedDiscount = 0;
            RedeemValidationMessage = "";
            IsDiscountApplied = false;
            OnPropertyChanged(nameof(IsDiscountApplied));
            OnPropertyChanged(nameof(DiscountText));
        }

        private void Recalculate()
        {
            OnPropertyChanged(nameof(NetPayableAmount));
            OnPropertyChanged(nameof(ChangeAmount));
            OnPropertyChanged(nameof(EarnedPoints));
            OnPropertyChanged(nameof(MaxRedeemablePoints));
            RecalculateAutoPromotion();
        }

        public void Initialize()
        {
            RecalculateAutoPromotion();
        }

        public void PaymentView_Loaded(object? sender = null, EventArgs? e = null)
        {
            RecalculateAutoPromotion();
        }

        public void OnLoaded()
        {
            RecalculateAutoPromotion();
        }

        public void RecalculateAutoPromotion()
        {
            _ = RecalculateAutoPromotionAsync();
        }

        public async Task RecalculateAutoPromotionAsync()
        {
            try
            {
                bool isMember = CurrentMember != null || IsMemberChecked;
                var allPromos = await Services.PromotionService.Instance.GetAllPromotionsAsync();

                // 1. รวมยอดสินค้าเดิมจากตะกร้า (สมุด 5 เล่ม x 10 = 50.00)
                decimal cartTotal = CartItems?.Sum(x => x.TotalPrice) ?? 0m;
                if (cartTotal <= 0 && _totalAmountBeforeDiscount > 0)
                {
                    cartTotal = _totalAmountBeforeDiscount;
                }
                else if (cartTotal > 0 && _totalAmountBeforeDiscount <= 0)
                {
                    _totalAmountBeforeDiscount = cartTotal;
                    OnPropertyChanged(nameof(TotalAmountBeforeDiscount));
                }

                var bestPromo = Services.PromotionService.Instance.EvaluateAutoPromotions(
                    CartItems ?? new List<CartItem>(),
                    isMember,
                    cartTotal,
                    allPromos);

                HasAutoPromotion = bestPromo != null && bestPromo.DiscountAmount > 0;
                AutoPromotionName = bestPromo != null 
                    ? (!string.IsNullOrWhiteSpace(bestPromo.PromoName) ? bestPromo.PromoName : (bestPromo.Promotion?.ConditionDescription ?? ""))
                    : "";
                PromotionDiscountAmount = bestPromo?.DiscountAmount ?? 0m;
                AppliedPromoID = bestPromo?.PromoID;
                AppliedAutoPromotion = bestPromo?.Promotion;
                AppliedPromotions = bestPromo?.AppliedPromotions ?? new List<PromotionModel>();

                // 2. รวมมูลค่าสินค้าของแจก/แถมที่ระบบเพิ่มให้ (ดินสอ 1 แท่ง = 10.00)
                decimal freeGiftTotal = bestPromo?.AutoIncludedAmount ?? 0m;
                if (freeGiftTotal <= 0 && AppliedPromotions != null && AppliedPromotions.Count > 0)
                {
                    freeGiftTotal = AppliedPromotions
                        .Where(p => p.PromoType == "Gift" || p.PromoType == "Freebie")
                        .Sum(p => p.GiftItemValue);
                }
                if (freeGiftTotal <= 0 && bestPromo?.AppliedItems != null)
                {
                    freeGiftTotal = bestPromo.AppliedItems
                        .Where(p => p.IsAutoIncluded || p.PromoType == "Gift" || p.PromoType == "Freebie")
                        .Sum(p => p.AutoIncludedAmount > 0 ? p.AutoIncludedAmount : (p.IsAutoIncluded ? p.DiscountAmount : 0m));
                }

                AutoIncludedGiftAmount = freeGiftTotal;

                // 3. กำหนดค่ายอดรวมสินค้าก่อนลด (50.00 + 10.00 = 60.00)
                TotalAmount = cartTotal + freeGiftTotal;

                // 4. คำนวณส่วนลดรวมทั้งหมด (25.00 + 10.00 = 35.00)
                TotalDiscount = (AppliedPromotions != null && AppliedPromotions.Count > 0 
                    ? AppliedPromotions.Sum(p => p.DiscountAmount) 
                    : PromotionDiscountAmount) + DiscountAmount;

                AppliedPromotionDetails.Clear();
                if (bestPromo?.AppliedItems != null && bestPromo.AppliedItems.Count > 0)
                {
                    foreach (var item in bestPromo.AppliedItems)
                    {
                        AppliedPromotionDetails.Add(item);
                    }
                }
                else if (bestPromo != null && bestPromo.DiscountAmount > 0)
                {
                    AppliedPromotionDetails.Add(new Services.AppliedPromotionResult
                    {
                        PromoID = bestPromo.PromoID,
                        PromoName = bestPromo.PromoName,
                        DiscountAmount = bestPromo.DiscountAmount,
                        DisplayText = !string.IsNullOrWhiteSpace(bestPromo.PromoName) ? $"ส่วนลดโปรโมชั่น ({bestPromo.PromoName}):" : "ส่วนลดโปรโมชั่น:",
                        TextColor = "#E11D48"
                    });
                }

                // 5. คำนวณยอดสุทธิที่ต้องชำระ (60.00 - 35.00 = 25.00)
                OnPropertyChanged(nameof(TotalAmount));
                OnPropertyChanged(nameof(SubtotalAmount));
                OnPropertyChanged(nameof(DisplaySubtotal));
                OnPropertyChanged(nameof(TotalAmountBeforeDiscount));
                OnPropertyChanged(nameof(TotalDiscount));
                OnPropertyChanged(nameof(AppliedPromotionDetails));
                OnPropertyChanged(nameof(AppliedPromotions));
                OnPropertyChanged(nameof(PromotionDiscountAmount));
                OnPropertyChanged(nameof(NetPayableAmount));
                OnPropertyChanged(nameof(ChangeAmount));
                OnPropertyChanged(nameof(EarnedPoints));
            }
            catch
            {
                decimal fallbackCart = CartItems?.Sum(x => x.TotalPrice) ?? 0m;
                if (fallbackCart <= 0 && _totalAmountBeforeDiscount > 0) fallbackCart = _totalAmountBeforeDiscount;
                TotalAmount = fallbackCart;
                TotalDiscount = DiscountAmount;
                AutoIncludedGiftAmount = 0m;
                HasAutoPromotion = false;
                AutoPromotionName = "";
                PromotionDiscountAmount = 0;
                AppliedPromoID = null;
                AppliedAutoPromotion = null;
                AppliedPromotions = new List<PromotionModel>();
                AppliedPromotionDetails.Clear();

                OnPropertyChanged(nameof(TotalAmount));
                OnPropertyChanged(nameof(SubtotalAmount));
                OnPropertyChanged(nameof(DisplaySubtotal));
                OnPropertyChanged(nameof(TotalAmountBeforeDiscount));
                OnPropertyChanged(nameof(TotalDiscount));
                OnPropertyChanged(nameof(AppliedPromotionDetails));
                OnPropertyChanged(nameof(AppliedPromotions));
                OnPropertyChanged(nameof(PromotionDiscountAmount));
                OnPropertyChanged(nameof(NetPayableAmount));
                OnPropertyChanged(nameof(ChangeAmount));
                OnPropertyChanged(nameof(EarnedPoints));
            }
        }
    }
}
