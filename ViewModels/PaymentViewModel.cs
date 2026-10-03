using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

        private decimal _totalAmountBeforeDiscount;
        public decimal TotalAmountBeforeDiscount
        {
            get => _totalAmountBeforeDiscount;
            set
            {
                if (SetProperty(ref _totalAmountBeforeDiscount, value))
                {
                    Recalculate();
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

        public decimal NetPayableAmount => Math.Max(0, TotalAmountBeforeDiscount + ShippingFee - DiscountAmount);

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
                int maxByBill = (int)Math.Floor(TotalAmountBeforeDiscount + ShippingFee);
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
        }

        private void Recalculate()
        {
            OnPropertyChanged(nameof(NetPayableAmount));
            OnPropertyChanged(nameof(ChangeAmount));
            OnPropertyChanged(nameof(EarnedPoints));
            OnPropertyChanged(nameof(MaxRedeemablePoints));
        }
    }
}
