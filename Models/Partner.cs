using System;

namespace Porjai20.Models
{
    public class Partner
    {
        public int Partner_ID { get; set; }
        public int Id
        {
            get => Partner_ID;
            set => Partner_ID = value;
        }

        private string _code = string.Empty;
        public string Code
        {
            get => string.IsNullOrWhiteSpace(_code) ? $"SUP-{Partner_ID:D4}" : _code;
            set => _code = value;
        }

        public string Partner_Name { get; set; } = string.Empty;
        public string Name
        {
            get => Partner_Name;
            set => Partner_Name = value;
        }

        public string Partner_Address { get; set; } = string.Empty;
        public string Address
        {
            get => Partner_Address;
            set => Partner_Address = value;
        }

        public string Partner_Tel { get; set; } = string.Empty;
        public string Phone
        {
            get => Partner_Tel;
            set => Partner_Tel = value;
        }

        public string Partner_Contact { get; set; } = string.Empty;
        public string ContactPerson
        {
            get => Partner_Contact;
            set => Partner_Contact = value;
        }

        public string Email { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public DateTime RegisteredDate { get; set; } = DateTime.Now;
    }
}
