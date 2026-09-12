using System;

namespace Porjai20.Models
{
    public class Claim
    {
        public int Claim_ID { get; set; }
        public int Id
        {
            get => Claim_ID;
            set => Claim_ID = value;
        }

        public int Sales_ID { get; set; }
        public int Pro_ID { get; set; }
        public int ProductId
        {
            get => Pro_ID;
            set => Pro_ID = value;
        }

        public string Claim_Date { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public DateTime CreatedDate
        {
            get
            {
                if (DateTime.TryParse(Claim_Date, out var dt)) return dt;
                return DateTime.Now;
            }
            set => Claim_Date = value.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public string Claim_Reason { get; set; } = string.Empty;
        public string Reason
        {
            get => Claim_Reason;
            set => Claim_Reason = value;
        }

        public string Claim_Status { get; set; } = "รอดำเนินการ";
        public string Status
        {
            get => Claim_Status;
            set => Claim_Status = value;
        }

        public string Claim_Action { get; set; } = "เปลี่ยนสินค้าใหม่";
        public string ClaimAction
        {
            get => string.IsNullOrWhiteSpace(Claim_Action) ? "เปลี่ยนสินค้าใหม่" : Claim_Action;
            set => Claim_Action = value;
        }

        private string _claimNo = string.Empty;
        public string ClaimNo
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_claimNo)) return _claimNo;
                if (Claim_ID > 0)
                {
                    int thaiYear = CreatedDate.Year > 2500 ? CreatedDate.Year : CreatedDate.Year + 543;
                    return $"CLM-{thaiYear}{CreatedDate:MMdd}{Claim_ID:D3}";
                }
                return string.Empty;
            }
            set => _claimNo = value;
        }

        public string ClaimType { get; set; } = "ลูกค้า";

        public string ClaimTypeDisplay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ClaimType)) return "ลูกค้า";
                if (ClaimType.Equals("Customer", StringComparison.OrdinalIgnoreCase) || ClaimType.Equals("ลูกค้า", StringComparison.OrdinalIgnoreCase))
                    return "ลูกค้า";
                if (ClaimType.Equals("Supplier", StringComparison.OrdinalIgnoreCase) || ClaimType.Equals("Partner", StringComparison.OrdinalIgnoreCase) || ClaimType.Equals("บริษัทคู่ค้า", StringComparison.OrdinalIgnoreCase) || ClaimType.Equals("ซัพพลายเออร์", StringComparison.OrdinalIgnoreCase))
                    return "บริษัทคู่ค้า";
                return ClaimType;
            }
        }
        public string SalesOrderRefNo { get; set; } = string.Empty;
        public DateTime? SaleDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string StockInRefNo { get; set; } = string.Empty;
        public DateTime? StockInDate { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public string Note { get; set; } = string.Empty;
        public DateTime? UpdatedDate { get; set; }

        public bool IsWithinCustomerLimit => SaleDate.HasValue && (DateTime.Now - SaleDate.Value).TotalDays <= 2;
        public bool IsWithinSupplierLimit => StockInDate.HasValue && (DateTime.Now - StockInDate.Value).TotalDays <= 7;
        public double DaysElapsed => SaleDate.HasValue ? (DateTime.Now - SaleDate.Value).TotalDays
                                   : StockInDate.HasValue ? (DateTime.Now - StockInDate.Value).TotalDays : 0;
    }

    public class ClaimedReceiptDetails
    {
        public int Sales_ID { get; set; }
        public int Pro_ID { get; set; }
        public DateTime SaleDate { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
    }

    public class ClaimedStockInDetails
    {
        public int Pro_ID { get; set; }
        public DateTime StockInDate { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string ProductName { get; set; } = string.Empty;
    }
}
