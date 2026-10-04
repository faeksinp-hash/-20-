using System;

namespace Porjai20.Models
{
    public class TodaySalesSummaryDto
    {
        public decimal TotalSales { get; set; }
        public decimal TotalCost { get; set; }
        public decimal EstimatedProfit => TotalSales - TotalCost;
        public int TotalBills { get; set; }
        public int TotalItems { get; set; }
        public decimal YesterdaySalesSameTime { get; set; }
        public decimal YesterdaySalesTotal { get; set; }
        public int YesterdayBillsTotal { get; set; }

        public decimal AveragePerBill => TotalBills > 0 ? (TotalSales / TotalBills) : 0m;
        public double AverageItemsPerBill => TotalBills > 0 ? ((double)TotalItems / TotalBills) : 0.0;

        public decimal GrowthPercent
        {
            get
            {
                if (YesterdaySalesSameTime > 0)
                    return ((TotalSales - YesterdaySalesSameTime) / YesterdaySalesSameTime) * 100m;
                if (TotalSales > 0)
                    return 100m;
                return 0m;
            }
        }

        public decimal MemberSalesAmount { get; set; }
        public double MemberSalesPercent => TotalSales > 0 ? ((double)MemberSalesAmount / (double)TotalSales) * 100.0 : 0.0;
    }

    public class RecentBillDto
    {
        public int SalesID { get; set; }
        public string RefNo { get; set; } = string.Empty;
        public string SalesTime { get; set; } = string.Empty;
        public string CustomerName { get; set; } = "ลูกค้าทั่วไป";
        public bool IsMember { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "สำเร็จ";
        public bool IsLast { get; set; }

        public string ReceiptNo => string.IsNullOrWhiteSpace(RefNo) ? $"SALE-{SalesID:D5}" : RefNo;
        public string FullReceiptNo => ReceiptNo;
        public string SaleId => ReceiptNo;

        public string FormattedTotal => TotalAmount.ToString("N2");
        public string MemberBadgeText => IsMember ? "สมาชิก" : "ลูกค้าทั่วไป";
    }

    public class TopSellingProductDto
    {
        public int Rank { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal TotalSalesAmount { get; set; }

        public string FormattedTotal => TotalSalesAmount.ToString("N2");
    }

    public class PendingTasksSummaryDto
    {
        public int PendingDeliveryCount { get; set; }   // 1. จัดส่ง
        public int PendingClaimCount { get; set; }      // 2. เคลม
        public int NeedReorderCount { get; set; }       // 3. สั่งซื้อ (หมดสต็อก + ใกล้หมด)
        public int PendingReceiveCount { get; set; }    // 4. รับเข้า

        // Compatibility aliases
        public int OutOfStockCount
        {
            get => NeedReorderCount;
            set => NeedReorderCount = value;
        }

        public int PendingReceiveOrderCount
        {
            get => PendingReceiveCount;
            set => PendingReceiveCount = value;
        }

        public int ActiveUrgentTasksCategoryCount =>
            (PendingDeliveryCount > 0 ? 1 : 0) +
            (PendingClaimCount > 0 ? 1 : 0) +
            (NeedReorderCount > 0 ? 1 : 0) +
            (PendingReceiveCount > 0 ? 1 : 0);

        public int TotalUrgentTasksCount => ActiveUrgentTasksCategoryCount;
        public int TotalPendingCount => PendingDeliveryCount + PendingClaimCount + NeedReorderCount + PendingReceiveCount;
    }

    public class PendingTasksDto : PendingTasksSummaryDto
    {
    }

    public class PendingTaskDto : PendingTasksDto
    {
    }

    public class HourlySalesDto
    {
        public string HourLabel { get; set; } = string.Empty;
        public int HourNumber { get; set; }
        public int TodayBillCount { get; set; }
        public int YesterdayBillCount { get; set; }
    }

    public class CustomerSummaryDto
    {
        public int NewCustomersToday { get; set; }
        public int TotalCustomers { get; set; }
        public int PointsGivenToday { get; set; }
        public int PointsRedeemedToday { get; set; }
        public int PromotionsUsedToday { get; set; }
    }

    public class SalesOrderRecord
    {
        public int Sales_ID { get; set; }
        public string? RefNo { get; set; }
        public string? Sales_Date { get; set; }
        public double Sales_Total { get; set; }
        public double Sales_Cash { get; set; }
        public double Sales_Change { get; set; }
        public string? Sales_PaymentType { get; set; }
        public string? Sales_Status { get; set; }
        public int? Cus_ID { get; set; }
        public double DiscountAmount { get; set; }
        public int PointsUsed { get; set; }
        public int PointsEarned { get; set; }
        public string? CustomerName { get; set; }
        public DateTime? NormalizedDate { get; set; }
    }

    public class CustomerRecord
    {
        public int Cus_ID { get; set; }
        public string? Cus_RegDate { get; set; }
    }
}

