using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Porjai20.Models
{
    public class Customer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
        public string CustomerName
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
        public string PhoneNumber
        {
            get => Cus_Tel;
            set => Cus_Tel = value;
        }

        private int _cusPoints = 0;
        public int Cus_Points
        {
            get => _cusPoints;
            set
            {
                if (_cusPoints != value)
                {
                    _cusPoints = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Points));
                    OnPropertyChanged(nameof(TotalPoints));
                }
            }
        }
        public int Points
        {
            get => _cusPoints;
            set => Cus_Points = value;
        }

        public int TotalPoints => _cusPoints;

        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public int TotalPurchases { get; set; } = 0;
        public double TotalSpent { get; set; } = 0;

        public override string ToString()
        {
            return $"{Name} ({Phone})";
        }
    }
}
