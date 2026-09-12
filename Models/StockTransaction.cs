using System;

namespace Porjai20.Models
{
    public class StockTransaction
    {
        public int Detail_ID { get; set; }
        public int Id
        {
            get => Detail_ID;
            set => Detail_ID = value;
        }

        public int StockIn_ID { get; set; }
        public int PO_ID { get; set; }
        public int Emp_ID { get; set; }

        public int Pro_ID { get; set; }
        public int ProductId
        {
            get => Pro_ID;
            set => Pro_ID = value;
        }

        public int StockIn_Qty { get; set; }
        public int Quantity
        {
            get => StockIn_Qty;
            set => StockIn_Qty = value;
        }

        public string Note { get; set; } = string.Empty;
        public string StockIn_Date { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        public string ProductName { get; set; } = string.Empty;
        public decimal UnitCost { get; set; }
        public string RefNo { get; set; } = string.Empty;
        public string Supplier { get; set; } = string.Empty;
        public DateTime Timestamp
        {
            get
            {
                if (DateTime.TryParse(StockIn_Date, out var dt)) return dt;
                return DateTime.Now;
            }
            set => StockIn_Date = value.ToString("yyyy-MM-dd HH:mm:ss");
        }
        public string ProductCode { get; set; } = string.Empty;
        public decimal TotalCost => Quantity * UnitCost;
    }
}
