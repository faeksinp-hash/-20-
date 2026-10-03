using System;

namespace Porjai20.Models
{
    public class PurchaseOrder
    {
        public int PO_ID { get; set; }
        public int Id
        {
            get => PO_ID;
            set => PO_ID = value;
        }

        public string PO_Date { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
        public DateTime OrderDate
        {
            get
            {
                if (DateTime.TryParse(PO_Date, out var dt)) return dt;
                return DateTime.Now;
            }
            set => PO_Date = value.ToString("yyyy-MM-dd");
        }

        public int Partner_ID { get; set; }
        public int SupplierID
        {
            get => Partner_ID;
            set => Partner_ID = value;
        }
        public int Emp_ID { get; set; }

        public decimal PO_Total { get; set; }
        public decimal TotalAmount
        {
            get => PO_Total;
            set => PO_Total = value;
        }

        public string PO_Status { get; set; } = "รอรับของ";
        public string Status
        {
            get => PO_Status;
            set => PO_Status = value;
        }

        private string _poNumber = string.Empty;
        public string PONumber
        {
            get => string.IsNullOrWhiteSpace(_poNumber) ? (PO_ID > 0 ? $"PO-{PO_ID:D5}" : string.Empty) : _poNumber;
            set => _poNumber = value;
        }
        public string SupplierName { get; set; } = string.Empty;
        public DateTime ExpectedDate { get; set; } = DateTime.Now.AddDays(3);
        public DateTime ExpectedDeliveryDate
        {
            get => ExpectedDate;
            set => ExpectedDate = value;
        }

        public bool IsPending => Status == "รอดำเนินการ" || Status == "รอรับของ" || Status == "Pending";
        public bool IsReceived => Status == "ได้รับสินค้าแล้ว" || Status == "ตรวจรับแล้ว" || Status == "รับเข้าแล้ว" || Status == "เสร็จสิ้น" || Status == "Completed";
        public string DisplayStatus => IsReceived ? "ตรวจรับแล้ว" : (Status == "ยกเลิก" ? "ยกเลิก" : "รอดำเนินการ");
    }
}
