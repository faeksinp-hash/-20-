using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Porjai20.Models
{
    public class SalesOrder : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Sales_ID { get; set; }
        public int Id
        {
            get => Sales_ID;
            set => Sales_ID = value;
        }

        public string RefNo { get; set; } = string.Empty;

        public string Sales_Date { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        public DateTime Timestamp
        {
            get
            {
                if (DateTime.TryParse(Sales_Date, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dt)) return dt;
                if (DateTime.TryParse(Sales_Date, out var dt2)) return dt2;
                return DateTime.Now;
            }
            set => Sales_Date = value.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        }

        public int Cus_ID { get; set; }
        public int CustomerId
        {
            get => Cus_ID;
            set => Cus_ID = value;
        }

        public Customer? Customer { get; set; }

        private string _customerName = string.Empty;
        public string CustomerName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_customerName) && _customerName != "-")
                    return _customerName;
                if (Customer != null && !string.IsNullOrWhiteSpace(Customer.Name))
                    return Customer.Name;
                return "ลูกค้าทั่วไป";
            }
            set
            {
                if (_customerName != value)
                {
                    _customerName = value ?? string.Empty;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CustomerDisplayName));
                }
            }
        }

        public string CustomerDisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_customerName) && _customerName != "-")
                    return _customerName;
                if (Customer != null && !string.IsNullOrWhiteSpace(Customer.Name))
                    return Customer.Name;
                return "ลูกค้าทั่วไป";
            }
        }

        public int Emp_ID { get; set; }

        public decimal Sales_Total { get; set; }
        public decimal TotalAmount
        {
            get => Sales_Total;
            set => Sales_Total = value;
        }

        public decimal Sales_Cash { get; set; }
        public decimal CashReceived
        {
            get => Sales_Cash;
            set => Sales_Cash = value;
        }

        public decimal Sales_Change { get; set; }
        public decimal Change
        {
            get => Sales_Change;
            set => Sales_Change = value;
        }

        public string Sales_PaymentType { get; set; } = "เงินสด";
        public string PaymentMethod
        {
            get => Sales_PaymentType;
            set => Sales_PaymentType = value;
        }

        public string Sales_Status { get; set; } = "ชำระเงินแล้ว";
        public string Status
        {
            get => Sales_Status;
            set => Sales_Status = value;
        }

        // Delivery Info
        public bool IsDelivery { get; set; }

        private string _customerPhone = string.Empty;
        public string CustomerPhone
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_customerPhone) && _customerPhone != "-")
                    return _customerPhone;
                if (Customer != null && !string.IsNullOrWhiteSpace(Customer.Phone))
                    return Customer.Phone;
                return "-";
            }
            set
            {
                if (_customerPhone != value)
                {
                    _customerPhone = value ?? string.Empty;
                    OnPropertyChanged();
                }
            }
        }

        private string _customerAddress = string.Empty;
        public string CustomerAddress
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_customerAddress) && _customerAddress != "ไม่ระบุที่อยู่")
                    return _customerAddress;
                if (Customer != null && !string.IsNullOrWhiteSpace(Customer.Address))
                    return Customer.Address;
                return "ไม่ระบุที่อยู่";
            }
            set
            {
                if (_customerAddress != value)
                {
                    _customerAddress = value ?? string.Empty;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CustomerDisplayAddress));
                }
            }
        }

        public string CustomerDisplayAddress
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_customerAddress) && _customerAddress != "ไม่ระบุที่อยู่")
                    return _customerAddress;
                if (Customer != null && !string.IsNullOrWhiteSpace(Customer.Address))
                    return Customer.Address;
                return "ไม่ระบุที่อยู่";
            }
        }

        private string _deliveryStatus = "รอจัดส่ง";
        public string DeliveryStatus
        {
            get => _deliveryStatus;
            set
            {
                if (_deliveryStatus != value)
                {
                    _deliveryStatus = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}