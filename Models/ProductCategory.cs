using System;

namespace Porjai20.Models
{
    public class ProductCategory
    {
        public int CategoryId { get; set; }
        public int Id
        {
            get => CategoryId;
            set => CategoryId = value;
        }

        public string CategoryCode { get; set; } = string.Empty;
        public string Code
        {
            get => CategoryCode;
            set => CategoryCode = value;
        }

        public string CategoryName { get; set; } = string.Empty;
        public string Name
        {
            get => CategoryName;
            set => CategoryName = value;
        }

        public string Description { get; set; } = string.Empty;
        public int ProductCount { get; set; } = 0;
    }
}
