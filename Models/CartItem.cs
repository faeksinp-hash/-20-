using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Porjai20.Models
{
    public class CartItem : INotifyPropertyChanged
    {
        private Product _product;
        public Product Product
        {
            get { return _product; }
            set { _product = value; OnPropertyChanged(); }
        }

        private int _quantity;
        public int Quantity
        {
            get { return _quantity; }
            set 
            { 
                _quantity = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(Total));
            }
        }

        public decimal Total => Product.Price * Quantity;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
