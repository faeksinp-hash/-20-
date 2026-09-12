using System;

namespace Porjai20.Models
{
    public class Customer
    {
        public int Cus_ID { get; set; }
        public int Id
        {
            get => Cus_ID;
            set => Cus_ID = value;
        }

        public string Code { get; set; } = string.Empty;

        public string Cus_Name { get; set; } = string.Empty;
        public string Name
        {
            get => Cus_Name;
            set => Cus_Name = value;
        }

        public string Cus_Address { get; set; } = string.Empty;
        public string Address
        {
            get => Cus_Address;
            set => Cus_Address = value;
        }

        public string Cus_Tel { get; set; } = string.Empty;
        public string Phone
        {
            get => Cus_Tel;
            set => Cus_Tel = value;
        }

        public int Cus_Points { get; set; } = 0;
        public int Points
        {
            get => Cus_Points;
            set => Cus_Points = value;
        }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public int TotalPurchases { get; set; } = 0;
        public double TotalSpent { get; set; } = 0;

        public override string ToString()
        {
            return $"{Name} ({Phone})";
        }
    }
}
