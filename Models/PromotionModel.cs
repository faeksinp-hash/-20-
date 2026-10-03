using System;
using Porjai20.ViewModels;

namespace Porjai20.Models
{
    /// <summary>
    /// โครงสร้างข้อมูลโปรโมชั่นของร้านพอใจ ทุกอย่าง 20 บาท
    /// แมปตรงกับตาราง tblPromotion ใน SQLite
    /// </summary>
    public class PromotionModel : ViewModelBase
    {
        private string _promoId = string.Empty;
        private string _promoName = string.Empty;
        private string _promoType = "CashDiscount"; // CashDiscount, PointRedeem, FreeDelivery
        private decimal _discountAmount;
        private decimal _minSpend;
        private int _pointsRequired;
        private string _startDate = DateTime.Today.ToString("yyyy-MM-dd");
        private string _endDate = DateTime.Today.AddMonths(1).ToString("yyyy-MM-dd");
        private bool _isActive = true;
        private int _usageCount;
        private decimal _totalDiscountGiven;
        private bool _isSelected;

        /// <summary>ระบุว่าการ์ดนี้กำลังถูกเลือกอยู่หรือไม่</summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>รหัสโปรโมชั่น Primary Key เช่น 'POINTS-50', 'PROMO-10B'</summary>
        public string PromoID
        {
            get => _promoId;
            set => SetProperty(ref _promoId, value);
        }

        /// <summary>ชื่อโปรโมชั่น</summary>
        public string PromoName
        {
            get => _promoName;
            set => SetProperty(ref _promoName, value);
        }

        /// <summary>ประเภทโปรโมชั่น: CashDiscount, PointRedeem, FreeDelivery</summary>
        public string PromoType
        {
            get => _promoType;
            set
            {
                if (SetProperty(ref _promoType, value))
                {
                    OnPropertyChanged(nameof(PromoTypeDisplay));
                    OnPropertyChanged(nameof(ConditionDescription));
                    OnPropertyChanged(nameof(IsPointRedeem));
                }
            }
        }

        /// <summary>มูลค่าส่วนลด (บาท)</summary>
        public decimal DiscountAmount
        {
            get => _discountAmount;
            set
            {
                if (SetProperty(ref _discountAmount, value))
                {
                    OnPropertyChanged(nameof(DiscountAmountFormatted));
                    OnPropertyChanged(nameof(ConditionDescription));
                }
            }
        }

        /// <summary>ยอดซื้อขั้นต่ำ (บาท)</summary>
        public decimal MinSpend
        {
            get => _minSpend;
            set
            {
                if (SetProperty(ref _minSpend, value))
                {
                    OnPropertyChanged(nameof(MinSpendFormatted));
                    OnPropertyChanged(nameof(ConditionDescription));
                }
            }
        }

        /// <summary>แต้มสะสมที่ต้องใช้ (กรณีแลกแต้ม)</summary>
        public int PointsRequired
        {
            get => _pointsRequired;
            set
            {
                if (SetProperty(ref _pointsRequired, value))
                {
                    OnPropertyChanged(nameof(ConditionDescription));
                }
            }
        }

        /// <summary>วันที่เริ่มต้น (YYYY-MM-DD)</summary>
        public string StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                {
                    OnPropertyChanged(nameof(IsExpired));
                    OnPropertyChanged(nameof(StatusDisplay));
                    OnPropertyChanged(nameof(DateRangeDisplay));
                }
            }
        }

        /// <summary>วันที่สิ้นสุด (YYYY-MM-DD)</summary>
        public string EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                {
                    OnPropertyChanged(nameof(IsExpired));
                    OnPropertyChanged(nameof(StatusDisplay));
                    OnPropertyChanged(nameof(DateRangeDisplay));
                }
            }
        }

        /// <summary>สถานะเปิดใช้งาน (1 = เปิดใช้งาน, 0 = ปิด)</summary>
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (SetProperty(ref _isActive, value))
                {
                    OnPropertyChanged(nameof(StatusDisplay));
                    OnPropertyChanged(nameof(StatusBadgeBg));
                    OnPropertyChanged(nameof(StatusBadgeFg));
                }
            }
        }

        /// <summary>จำนวนครั้งที่ใช้สิทธิ์ไปแล้ว</summary>
        public int UsageCount
        {
            get => _usageCount;
            set => SetProperty(ref _usageCount, value);
        }

        /// <summary>ยอดส่วนลดที่ให้ไปทั้งสิ้น (บาท)</summary>
        public decimal TotalDiscountGiven
        {
            get => _totalDiscountGiven;
            set
            {
                if (SetProperty(ref _totalDiscountGiven, value))
                {
                    OnPropertyChanged(nameof(TotalDiscountGivenFormatted));
                }
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  COMPUTED DISPLAY PROPERTIES (ห้ามใช้สัญลักษณ์ ฿ เด็ดขาด)
        // ════════════════════════════════════════════════════════════════

        /// <summary>ตรวจสอบว่าโปรโมชั่นหมดอายุหรือไม่</summary>
        public bool IsExpired
        {
            get
            {
                if (DateTime.TryParse(EndDate, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var end))
                {
                    return end.Date < DateTime.Today;
                }
                if (DateTime.TryParse(EndDate, out var fallbackEnd))
                {
                    return fallbackEnd.Date < DateTime.Today;
                }
                return false;
            }
        }

        public bool IsPointRedeem => PromoType == "PointRedeem";

        public string PromoTypeDisplay => PromoType switch
        {
            "CashDiscount" => "ส่วนลดเงินสด",
            "PointRedeem"  => "สิทธิ์แลกแต้ม",
            "FreeDelivery" => "คูปอง & ส่งฟรี",
            _              => PromoType
        };

        public string StatusDisplay
        {
            get
            {
                if (IsExpired) return "หมดอายุ";
                return IsActive ? "เปิดใช้งาน" : "ปิดใช้งาน";
            }
        }

        public string StatusBadgeBg
        {
            get
            {
                if (IsExpired) return "#FEE2E2";
                return IsActive ? "#DCFCE7" : "#F1F5F9";
            }
        }

        public string StatusBadgeFg
        {
            get
            {
                if (IsExpired) return "#DC2626";
                return IsActive ? "#166534" : "#64748B";
            }
        }

        public string ConditionDescription
        {
            get
            {
                if (PromoType == "PointRedeem")
                {
                    string minText = MinSpend > 0 ? $" ยอดซื้อขั้นต่ำ {MinSpend:N0} บาท" : "";
                    return $"ใช้ {PointsRequired:N0} แต้ม แลกส่วนลด {DiscountAmount:N0} บาท{minText}";
                }
                if (PromoType == "FreeDelivery")
                {
                    return MinSpend > 0
                        ? $"ใช้ได้กับยอดซื้อขั้นต่ำ {MinSpend:N0} บาท"
                        : "ฟรีค่าส่งสำหรับทุกคำสั่งซื้อ";
                }
                return MinSpend > 0
                    ? $"ใช้ได้กับยอดซื้อขั้นต่ำ {MinSpend:N0} บาท"
                    : $"ลดทันที {DiscountAmount:N0} บาท ไม่มีขั้นต่ำ";
            }
        }

        public string MinSpendFormatted => $"{MinSpend:N0}";
        public string DiscountAmountFormatted => $"{DiscountAmount:N0}";
        public string TotalDiscountGivenFormatted => $"{TotalDiscountGiven:N2}";
        public string DateRangeDisplay => $"{StartDate} ถึง {EndDate}";

        public PromotionModel Clone()
        {
            return new PromotionModel
            {
                PromoID = this.PromoID,
                PromoName = this.PromoName,
                PromoType = this.PromoType,
                DiscountAmount = this.DiscountAmount,
                MinSpend = this.MinSpend,
                PointsRequired = this.PointsRequired,
                StartDate = this.StartDate,
                EndDate = this.EndDate,
                IsActive = this.IsActive,
                UsageCount = this.UsageCount,
                TotalDiscountGiven = this.TotalDiscountGiven
            };
        }
    }
}
