using System;

namespace Porjai20.Models
{
    public class CustomerPurchaseHistoryDto
    {
        public int SalesId { get; set; }
        public string RefNo { get; set; } = string.Empty;
        public string RawDate { get; set; } = string.Empty;
        public string FormattedDate { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string FormattedTotal => TotalAmount.ToString("N2");
        public int PointsEarned { get; set; }
        public int PointsUsed { get; set; }
        public bool UsedPoints => PointsUsed > 0;
        public string PointsDisplay
        {
            get
            {
                if (PointsEarned > 0) return $"+{PointsEarned}";
                if (PointsUsed > 0) return $"-{PointsUsed}";
                return "0";
            }
        }
        public string PointsColor
        {
            get
            {
                if (PointsEarned > 0) return "#16A34A";
                if (PointsUsed > 0) return "#DC2626";
                return "#64748B";
            }
        }
        public string SalesStatus { get; set; } = "ชำระเงินแล้ว";
        public bool IsCancelled => !string.IsNullOrEmpty(SalesStatus) && (SalesStatus.Contains("ยกเลิก") || SalesStatus.Contains("Void", StringComparison.OrdinalIgnoreCase));
    }
}
