using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Porjai20.Common;
using Porjai20.ViewModels;

namespace Porjai20.Models
{
    /// <summary>
    /// โครงสร้างข้อมูลโปรโมชั่นของร้านพอใจ ทุกอย่าง 20 บาท
    /// รองรับ 4 ประเภท: ลด (Discount), แลก (Redeem), แจก (Gift), แถม (Freebie)
    /// แมปตรงกับตาราง tblPromotion และ tblPromotionProducts ใน SQLite
    /// กฎสกุลเงิน: ห้ามมีสัญลักษณ์ ฿ บนหน้าจอเด็ดขาด แสดงเฉพาะตัวเลขทศนิยมหรือตามด้วยคำว่า "บาท"
    /// </summary>
    public class PromotionModel : ViewModelBase
    {
        private string _promoId = string.Empty;
        private string? _promoCode;
        private string _promoName = string.Empty;
        private string _promoType = "Discount"; // Discount, Redeem, Gift, Freebie
        private string _targetScope = "AllStore"; // AllStore, SpecificProducts, DeliveryFee
        private string _discountType = "Cash"; // Cash, Percentage
        private decimal _discountAmount;
        private decimal? _maxDiscountAmount;
        private decimal _minSpend;
        private int _pointsRequired;
        private int _buyQuantity = 1;
        private int _freeQuantity = 1;
        private string? _freeProductId;
        private string? _freeProductName;
        private bool _canRepeat = true;
        private bool _isMemberOnly;
        private bool _canCombine;
        private int _maxUsagePerCustomer;
        private int _totalQuota;
        private int _currentUsage;
        private string _startDate = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        private string _endDate = DateTime.Today.AddMonths(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        private bool _isActive = true;
        private bool _isDraft;
        private string _createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        private bool _isSelected;
        private List<string> _productIds = new();
        private List<string> _productNames = new();
        private decimal _itemCost;

        #region Core Database Mapped Properties

        /// <summary>รหัสโปรโมชั่น Primary Key เช่น 'PROMO-01', 'BUY2FREE1'</summary>
        public string PromoID
        {
            get => _promoId;
            set => SetProperty(ref _promoId, value);
        }

        /// <summary>รหัสโปรโมชั่นที่ต้องกรอก (ว่าง = ทำงานอัตโนมัติที่ POS ไม่ต้องกรอกรหัส)</summary>
        public string? PromoCode
        {
            get => _promoCode;
            set
            {
                if (SetProperty(ref _promoCode, value))
                {
                    OnPropertyChanged(nameof(HasPromoCode));
                    OnPropertyChanged(nameof(CodeOrAutoText));
                }
            }
        }

        /// <summary>ชื่อโปรโมชั่น</summary>
        public string PromoName
        {
            get => _promoName;
            set => SetProperty(ref _promoName, value);
        }

        /// <summary>ประเภทโปรโมชั่น: 'Discount' (ลด), 'Redeem' (แลก), 'Gift' (แจก), 'Freebie' (แถม)</summary>
        public string PromoType
        {
            get => _promoType;
            set
            {
                if (SetProperty(ref _promoType, value))
                {
                    NotifyTypeChange();
                }
            }
        }

        /// <summary>ขอบเขต: 'AllStore' (ทั้งร้าน/ทั้งบิล), 'SpecificProducts' (เลือกสินค้าเอง), 'DeliveryFee' (ค่าจัดส่ง)</summary>
        public string TargetScope
        {
            get => _targetScope;
            set
            {
                if (SetProperty(ref _targetScope, value))
                {
                    OnPropertyChanged(nameof(IsAllStore));
                    OnPropertyChanged(nameof(IsSpecificProducts));
                    OnPropertyChanged(nameof(TargetScopeDisplay));
                }
            }
        }

        /// <summary>ประเภทส่วนลด: 'Cash' (เงินสดบาท), 'Percentage' (เปอร์เซ็นต์ %)</summary>
        public string DiscountType
        {
            get => _discountType;
            set
            {
                if (SetProperty(ref _discountType, value))
                {
                    OnPropertyChanged(nameof(IsPercentageDiscount));
                    OnPropertyChanged(nameof(ConditionDescription));
                    OnPropertyChanged(nameof(HighlightSummary));
                }
            }
        }

        /// <summary>มูลค่าส่วนลด (บาท) หรือ เปอร์เซ็นต์ (%)</summary>
        public decimal DiscountAmount
        {
            get => _discountAmount;
            set
            {
                if (SetProperty(ref _discountAmount, value))
                {
                    OnPropertyChanged(nameof(DiscountAmountFormatted));
                    OnPropertyChanged(nameof(ConditionDescription));
                    OnPropertyChanged(nameof(HighlightSummary));
                }
            }
        }

        /// <summary>เพดานส่วนลดสูงสุด (กรณี %)</summary>
        public decimal? MaxDiscountAmount
        {
            get => _maxDiscountAmount;
            set
            {
                if (SetProperty(ref _maxDiscountAmount, value))
                {
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
                    OnPropertyChanged(nameof(HighlightSummary));
                }
            }
        }

        /// <summary>แต้มสะสมที่ต้องใช้ (กรณีแลก)</summary>
        public int PointsRequired
        {
            get => _pointsRequired;
            set
            {
                if (SetProperty(ref _pointsRequired, value))
                {
                    OnPropertyChanged(nameof(ConditionDescription));
                    OnPropertyChanged(nameof(HighlightSummary));
                }
            }
        }

        /// <summary>จำนวนที่ต้องซื้อ (กรณีแถม)</summary>
        public int BuyQuantity
        {
            get => _buyQuantity;
            set
            {
                if (SetProperty(ref _buyQuantity, value))
                {
                    OnPropertyChanged(nameof(ConditionDescription));
                    OnPropertyChanged(nameof(HighlightSummary));
                }
            }
        }

        /// <summary>จำนวนที่แถม (กรณีแถม หรือแจก)</summary>
        public int FreeQuantity
        {
            get => _freeQuantity;
            set
            {
                if (SetProperty(ref _freeQuantity, value))
                {
                    OnPropertyChanged(nameof(ConditionDescription));
                    OnPropertyChanged(nameof(HighlightSummary));
                    OnPropertyChanged(nameof(EstimatedCostFormatted));
                }
            }
        }

        /// <summary>รหัสสินค้าที่แถม (กรณีแถมตัวอื่น) หรือ NULL/ว่าง (แถมตัวเดิม)</summary>
        public string? FreeProductID
        {
            get => _freeProductId;
            set
            {
                if (SetProperty(ref _freeProductId, value))
                {
                    OnPropertyChanged(nameof(ConditionDescription));
                    OnPropertyChanged(nameof(HighlightSummary));
                }
            }
        }

        /// <summary>ชื่อสินค้าของแถม (สำหรับแสดงผล)</summary>
        public string? FreeProductName
        {
            get => _freeProductName;
            set
            {
                if (SetProperty(ref _freeProductName, value))
                {
                    OnPropertyChanged(nameof(ConditionDescription));
                    OnPropertyChanged(nameof(HighlightSummary));
                }
            }
        }

        /// <summary>ซื้อเบิ้ลได้ไหม เช่น ซื้อ 4 แถม 2 (1 = ได้, 0 = ครั้งเดียวต่อบิล)</summary>
        public bool CanRepeat
        {
            get => _canRepeat;
            set => SetProperty(ref _canRepeat, value);
        }

        /// <summary>เฉพาะสมาชิกเท่านั้น (1 = ใช่, 0 = ทุกคน)</summary>
        public bool IsMemberOnly
        {
            get => _isMemberOnly;
            set
            {
                if (SetProperty(ref _isMemberOnly, value))
                {
                    OnPropertyChanged(nameof(CustomerAudienceText));
                }
            }
        }

        /// <summary>ใช้ร่วมกับโปรอื่นได้ไหม (1 = ได้, 0 = ไม่ได้)</summary>
        public bool CanCombine
        {
            get => _canCombine;
            set
            {
                if (SetProperty(ref _canCombine, value))
                {
                    OnPropertyChanged(nameof(CanCombineText));
                }
            }
        }

        /// <summary>โควตาต่อคน (0 = ไม่จำกัด)</summary>
        public int MaxUsagePerCustomer
        {
            get => _maxUsagePerCustomer;
            set
            {
                if (SetProperty(ref _maxUsagePerCustomer, value))
                {
                    OnPropertyChanged(nameof(MaxUsagePerCustomerText));
                }
            }
        }

        /// <summary>จำนวนสิทธิ์ทั้งหมด (0 = ไม่จำกัด)</summary>
        public int TotalQuota
        {
            get => _totalQuota;
            set
            {
                if (SetProperty(ref _totalQuota, value))
                {
                    OnPropertyChanged(nameof(TargetScopeDisplay));
                }
            }
        }

        /// <summary>จำนวนครั้งที่ใช้ไปแล้ว</summary>
        public int CurrentUsage
        {
            get => _currentUsage;
            set
            {
                if (SetProperty(ref _currentUsage, value))
                {
                    OnPropertyChanged(nameof(TargetScopeDisplay));
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
                    NotifyDateChange();
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
                    NotifyDateChange();
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
                    NotifyStatusChange();
                }
            }
        }

        /// <summary>ฉบับร่าง (1 = ฉบับร่าง เช่น ทำสำเนามา, 0 = สมบูรณ์)</summary>
        public bool IsDraft
        {
            get => _isDraft;
            set
            {
                if (SetProperty(ref _isDraft, value))
                {
                    NotifyStatusChange();
                }
            }
        }

        /// <summary>วันที่สร้างรายการ</summary>
        public string CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        #endregion

        #region Extra Helper / UI State Properties

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>รายการ Product ID ที่ผูกกับโปรโมชั่นนี้ (tblPromotionProducts)</summary>
        public List<string> ProductIds
        {
            get => _productIds;
            set
            {
                if (SetProperty(ref _productIds, value))
                {
                    OnPropertyChanged(nameof(TargetScopeDisplay));
                    OnPropertyChanged(nameof(HighlightSummary));
                }
            }
        }

        /// <summary>รายชื่อสินค้าที่ผูก (เพื่อนำมาแสดงผล)</summary>
        public List<string> ProductNames
        {
            get => _productNames;
            set
            {
                if (SetProperty(ref _productNames, value))
                {
                    OnPropertyChanged(nameof(TargetScopeDisplay));
                    OnPropertyChanged(nameof(HighlightSummary));
                }
            }
        }

        /// <summary>ต้นทุนต่อชิ้นของของแถม (นำมาจาก tblProduct.Pro_Cost)</summary>
        public decimal ItemCost
        {
            get => _itemCost;
            set
            {
                if (SetProperty(ref _itemCost, value))
                {
                    OnPropertyChanged(nameof(EstimatedCost));
                    OnPropertyChanged(nameof(EstimatedCostFormatted));
                }
            }
        }

        public decimal EstimatedCost => ItemCost * (FreeQuantity > 0 ? FreeQuantity : 1);
        public string EstimatedCostFormatted => $"{EstimatedCost:N2}";

        #endregion

        #region Computed Display Properties (No ฿ Allowed)

        /// <summary>จำนวนครั้งที่ใช้ไปแล้ว (UsageCount สำหรับระบบขายหน้าร้าน)</summary>
        public int UsageCount
        {
            get => _currentUsage;
            set
            {
                if (SetProperty(ref _currentUsage, value))
                {
                    OnPropertyChanged(nameof(CurrentUsage));
                    OnPropertyChanged(nameof(TargetScopeDisplay));
                }
            }
        }

        public bool IsExpired
        {
            get
            {
                var end = ParseDateSafe(EndDate);
                if (end.HasValue)
                {
                    return end.Value.Date < DateTime.Today;
                }
                return false;
            }
        }

        /// <summary>สามารถสลับสวิตช์ได้หรือไม่ (ถ้าหมดอายุ ล็อกห้ามกด)</summary>
        public bool CanToggleSwitch => !IsExpired;

        public string SwitchNote => IsExpired ? "ปิดอัตโนมัติ" : (IsActive ? "เปิดใช้งาน" : "ปิดใช้งาน");

        public bool HasPromoCode => !string.IsNullOrWhiteSpace(PromoCode);

        public string CodeOrAutoText => HasPromoCode ? PromoCode! : "ใช้อัตโนมัติ ไม่ต้องกรอกรหัส";

        public bool IsAllStore => TargetScope == "AllStore";
        public bool IsSpecificProducts => TargetScope == "SpecificProducts";
        public bool IsPercentageDiscount => DiscountType == "Percentage";

        public bool IsDiscountType => PromoType == "Discount";
        public bool IsRedeemType => PromoType == "Redeem";
        public bool IsGiftType => PromoType == "Gift";
        public bool IsFreebieType => PromoType == "Freebie";

        /// <summary>ชื่อสั้นประเภทโปรโมชั่น 4 ตัว</summary>
        public string PromoTypeBadgeText => PromoType switch
        {
            "Discount" => "ลด",
            "Redeem"   => "แลก",
            "Gift"     => "แจก",
            "Freebie"  => "แถม",
            _          => PromoType
        };

        public string PromoTypeDisplay => PromoType switch
        {
            "Discount" => "ส่วนลด (ลด)",
            "Redeem"   => "สิทธิ์แลกแต้ม (แลก)",
            "Gift"     => "ของสมนาคุณ (แจก)",
            "Freebie"  => "ซื้อแถมฟรี (แถม)",
            _          => PromoType
        };

        public string PromoTypeBadgeBg => PromoType switch
        {
            "Discount" => "#E0F2FE", // ฟ้าอ่อน
            "Redeem"   => "#FEF3C7", // เหลืองส้มอ่อน
            "Gift"     => "#DCFCE7", // เขียวอ่อน
            "Freebie"  => "#F3E8FF", // ม่วงอ่อน
            _          => "#F1F5F9"
        };

        public string PromoTypeBadgeFg => PromoType switch
        {
            "Discount" => "#0284C7", // ฟ้าเข้ม
            "Redeem"   => "#D97706", // ส้มเข้ม
            "Gift"     => "#16A34A", // เขียวเข้ม
            "Freebie"  => "#9333EA", // ม่วงเข้ม
            _          => "#475569"
        };

        public string PromoTypeBadgeBorder => PromoType switch
        {
            "Discount" => "#BAE6FD",
            "Redeem"   => "#FDE68A",
            "Gift"     => "#BBF7D0",
            "Freebie"  => "#E9D5FF",
            _          => "#CBD5E1"
        };

        public string StatusDisplay
        {
            get
            {
                if (IsExpired) return "หมดอายุ";
                if (IsDraft) return "ฉบับร่าง";
                return IsActive ? "เปิดใช้งาน" : "ปิดใช้งาน";
            }
        }

        public string StatusBadgeBg
        {
            get
            {
                if (IsExpired) return "#FEE2E2"; // แดงอ่อน
                if (IsDraft) return "#F1F5F9";   // ฟ้าเทา
                return IsActive ? "#DCFCE7" : "#F1F5F9"; // เขียว หรือ เทา
            }
        }

        public string StatusBadgeFg
        {
            get
            {
                if (IsExpired) return "#DC2626"; // แดงเข้ม
                if (IsDraft) return "#475569";   // เทาเข้ม
                return IsActive ? "#16A34A" : "#64748B"; // เขียว หรือ เทากลาง
            }
        }

        public string ConditionDescription
        {
            get
            {
                switch (PromoType)
                {
                    case "Freebie":
                        string targetProd = ProductNames.FirstOrDefault() ?? "สินค้าที่กำหนด";
                        string freeProd = string.IsNullOrWhiteSpace(FreeProductName) ? targetProd : FreeProductName;
                        return $"ซื้อ {targetProd} ครบ {BuyQuantity} ชิ้น แถมฟรี {freeProd} {FreeQuantity} ชิ้น";

                    case "Gift":
                        string giftName = string.IsNullOrWhiteSpace(FreeProductName) ? "ของพรีเมียม" : FreeProductName;
                        return MinSpend > 0
                            ? $"ซื้อครบ {MinSpend:N0} บาท แจกฟรี {giftName} {FreeQuantity} ชิ้น"
                            : $"แจกฟรี {giftName} {FreeQuantity} ชิ้น";

                    case "Redeem":
                        string minSpendText = MinSpend > 0 ? $" ยอดซื้อขั้นต่ำ {MinSpend:N0} บาท" : "";
                        return $"ใช้ {PointsRequired:N0} แต้ม แลกส่วนลด {DiscountAmount:N0} บาท{minSpendText}";

                    case "Discount":
                    default:
                        if (DiscountType == "Percentage")
                        {
                            string cap = MaxDiscountAmount.HasValue && MaxDiscountAmount.Value > 0
                                ? $" (สูงสุด {MaxDiscountAmount.Value:N0} บาท)"
                                : "";
                            string min = MinSpend > 0 ? $" ยอดซื้อขั้นต่ำ {MinSpend:N0} บาท" : " ไม่มีขั้นต่ำ";
                            return $"ลดทันที {DiscountAmount:N0}%{cap}{min}";
                        }
                        else
                        {
                            string min = MinSpend > 0 ? $" เมื่อซื้อครบ {MinSpend:N0} บาท" : " ไม่มีขั้นต่ำ";
                            return $"ลดทันที {DiscountAmount:N0} บาท{min}";
                        }
                }
            }
        }

        public string TargetScopeDisplay
        {
            get
            {
                string baseScope;
                if (TargetScope == "SpecificProducts")
                {
                    if (ProductNames.Count == 1)
                        baseScope = $"ใช้กับ: {ProductNames[0]}";
                    else if (ProductNames.Count > 1)
                        baseScope = $"ใช้กับ: {ProductNames[0]} และอีก {ProductNames.Count - 1} รายการ";
                    else
                        baseScope = "ใช้กับ: สินค้าที่กำหนด";
                }
                else if (TargetScope == "DeliveryFee")
                {
                    baseScope = "ใช้กับ: ค่าจัดส่ง";
                }
                else
                {
                    baseScope = "ใช้กับ: ทั้งบิล";
                }

                if (TotalQuota > 0)
                {
                    int remaining = Math.Max(0, TotalQuota - CurrentUsage);
                    return $"{baseScope} - เหลือ {remaining} สิทธิ์";
                }

                return baseScope;
            }
        }

        public string HighlightSummary
        {
            get
            {
                switch (PromoType)
                {
                    case "Freebie":
                        string target = ProductNames.FirstOrDefault() ?? "สินค้าที่เลือก";
                        string free = string.IsNullOrWhiteSpace(FreeProductName) ? target : FreeProductName;
                        int totalItems = BuyQuantity + FreeQuantity;
                        return $"ลูกค้าซื้อ {target} {BuyQuantity} ชิ้น ได้ {free} เพิ่มฟรี {FreeQuantity} ชิ้น รวม {totalItems} ชิ้น";

                    case "Gift":
                        string gift = string.IsNullOrWhiteSpace(FreeProductName) ? "ของสมนาคุณ" : FreeProductName;
                        return $"ลูกค้าซื้อครบ {MinSpend:N0} บาท รับฟรี {gift} {FreeQuantity} ชิ้น";

                    case "Redeem":
                        return $"ลูกค้าสมาชิกใช้ {PointsRequired:N0} แต้ม แลกรับส่วนลด {DiscountAmount:N0} บาท";

                    case "Discount":
                    default:
                        if (DiscountType == "Percentage")
                            return $"ลูกค้าได้รับส่วนลด {DiscountAmount:N0}% จากยอดซื้อเมื่อซื้อครบ {MinSpend:N0} บาท";
                        return $"ลูกค้าได้รับส่วนลด {DiscountAmount:N0} บาท เมื่อซื้อครบ {MinSpend:N0} บาท";
                }
            }
        }

        public string CustomerAudienceText => IsMemberOnly ? "เฉพาะสมาชิก" : "ทุกคน";
        public string CanCombineText => CanCombine ? "ได้" : "ไม่ได้";
        public string MaxUsagePerCustomerText => MaxUsagePerCustomer > 0 ? $"{MaxUsagePerCustomer} ครั้ง/คน" : "ไม่จำกัด";

        public string StartDateFormatted => ThaiDateHelper.FormatToThaiBuddhistDisplay(StartDate);
        public string EndDateFormatted => ThaiDateHelper.FormatToThaiBuddhistDisplay(EndDate);
        public string DateRangeDisplay => $"{StartDateFormatted} - {EndDateFormatted}";

        public string MinSpendFormatted => $"{MinSpend:N0}";
        public string DiscountAmountFormatted => $"{DiscountAmount:N0}";

        /// <summary>
        /// แปลงสตริงวันที่เป็น DateTime อย่างปลอดภัย รองรับทั้ง ค.ศ. (2026) และ พ.ศ. (2569)
        /// ตรวจจับและลดทอนปีที่ถูกบวก 543 ซ้ำซ้อน (เช่น 3655, 4198) ให้กลับมาเป็นปี ค.ศ. สากล (2026)
        /// </summary>
        public static bool SafeParseDate(string? dateStr, out DateTime date)
        {
            date = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(dateStr)) return false;

            date = ThaiDateHelper.ParseToUniversalDate(dateStr);
            return true;
        }

        /// <summary>
        /// แปลงสตริงวันที่เป็น DateTime อย่างปลอดภัย รองรับทั้ง ค.ศ. (AD) และ พ.ศ. (BE)
        /// คืนค่าเป็น DateTime ค.ศ. สากล (CE) สำหรับเปรียบเทียบตรรกะและส่งเข้า DatePicker
        /// </summary>
        public static DateTime? ParseDateSafe(string? dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr)) return null;
            return ThaiDateHelper.ParseToUniversalDate(dateStr);
        }

        private static string FormatDateThai(string? ymd)
        {
            return ThaiDateHelper.FormatToThaiBuddhistDisplay(ymd);
        }

        private void NotifyTypeChange()
        {
            OnPropertyChanged(nameof(PromoTypeDisplay));
            OnPropertyChanged(nameof(PromoTypeBadgeText));
            OnPropertyChanged(nameof(PromoTypeBadgeBg));
            OnPropertyChanged(nameof(PromoTypeBadgeFg));
            OnPropertyChanged(nameof(PromoTypeBadgeBorder));
            OnPropertyChanged(nameof(ConditionDescription));
            OnPropertyChanged(nameof(HighlightSummary));
            OnPropertyChanged(nameof(IsDiscountType));
            OnPropertyChanged(nameof(IsRedeemType));
            OnPropertyChanged(nameof(IsGiftType));
            OnPropertyChanged(nameof(IsFreebieType));
        }

        private void NotifyStatusChange()
        {
            OnPropertyChanged(nameof(StatusDisplay));
            OnPropertyChanged(nameof(StatusBadgeBg));
            OnPropertyChanged(nameof(StatusBadgeFg));
            OnPropertyChanged(nameof(SwitchNote));
            OnPropertyChanged(nameof(CanToggleSwitch));
        }

        private void NotifyDateChange()
        {
            OnPropertyChanged(nameof(IsExpired));
            OnPropertyChanged(nameof(StatusDisplay));
            OnPropertyChanged(nameof(StatusBadgeBg));
            OnPropertyChanged(nameof(StatusBadgeFg));
            OnPropertyChanged(nameof(SwitchNote));
            OnPropertyChanged(nameof(CanToggleSwitch));
            OnPropertyChanged(nameof(StartDateFormatted));
            OnPropertyChanged(nameof(EndDateFormatted));
            OnPropertyChanged(nameof(DateRangeDisplay));
        }

        public PromotionModel Clone()
        {
            return new PromotionModel
            {
                PromoID = this.PromoID,
                PromoCode = this.PromoCode,
                PromoName = this.PromoName,
                PromoType = this.PromoType,
                TargetScope = this.TargetScope,
                DiscountType = this.DiscountType,
                DiscountAmount = this.DiscountAmount,
                MaxDiscountAmount = this.MaxDiscountAmount,
                MinSpend = this.MinSpend,
                PointsRequired = this.PointsRequired,
                BuyQuantity = this.BuyQuantity,
                FreeQuantity = this.FreeQuantity,
                FreeProductID = this.FreeProductID,
                FreeProductName = this.FreeProductName,
                CanRepeat = this.CanRepeat,
                IsMemberOnly = this.IsMemberOnly,
                CanCombine = this.CanCombine,
                MaxUsagePerCustomer = this.MaxUsagePerCustomer,
                TotalQuota = this.TotalQuota,
                CurrentUsage = this.CurrentUsage,
                StartDate = this.StartDate,
                EndDate = this.EndDate,
                IsActive = this.IsActive,
                IsDraft = this.IsDraft,
                CreatedAt = this.CreatedAt,
                ItemCost = this.ItemCost,
                ProductIds = new List<string>(this.ProductIds),
                ProductNames = new List<string>(this.ProductNames)
            };
        }

        #endregion
    }
}
