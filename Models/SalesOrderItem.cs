namespace Porjai20.Models
{
    public class SalesOrderItem
    {
        public int Detail_ID { get; set; }
        public int Id
        {
            get => Detail_ID;
            set => Detail_ID = value;
        }

        public int Sales_ID { get; set; }
        public int SalesOrderId
        {
            get => Sales_ID;
            set => Sales_ID = value;
        }

        public int Pro_ID { get; set; }
        public int ProductId
        {
            get => Pro_ID;
            set => Pro_ID = value;
        }

        public decimal Pro_Price { get; set; }
        public decimal UnitPrice
        {
            get => Pro_Price;
            set => Pro_Price = value;
        }

        public int Sales_Qty { get; set; }
        public int Quantity
        {
            get => Sales_Qty;
            set => Sales_Qty = value;
        }

        public decimal Sales_Subtotal { get; set; }
        public decimal Total
        {
            get => Sales_Subtotal;
            set => Sales_Subtotal = value;
        }

        public string ProductName { get; set; } = string.Empty;
    }
}
