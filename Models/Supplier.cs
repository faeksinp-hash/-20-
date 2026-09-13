using System;

namespace Porjai20.Models
{
    public class Supplier
    {
        public int SupplierID { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? PhoneNumber { get; set; }

        public override string ToString() => SupplierName;
    }
}
