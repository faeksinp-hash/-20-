using System;
using Porjai20.ViewModels;

namespace Porjai20.Models
{
    public class GoodsReceiptItem : ViewModelBase
    {
        public int Detail_ID { get; set; }
        public int Id
        {
            get => Detail_ID;
            set => Detail_ID = value;
        }

        public int PO_ID { get; set; }
        public int PurchaseOrderId
        {
            get => PO_ID;
            set => PO_ID = value;
        }

        public int Pro_ID { get; set; }
        public int ProductId
        {
            get => Pro_ID;
            set => Pro_ID = value;
        }

        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        public int OrderedQty { get; set; }

        private decimal _costPrice;
        public decimal CostPrice
        {
            get => _costPrice;
            set
            {
                if (SetProperty(ref _costPrice, value))
                {
                    OnPropertyChanged(nameof(Subtotal));
                    OnPropertyChanged(nameof(ReceivedSubtotal));
                    ItemChanged?.Invoke();
                }
            }
        }

        private int _receivedQty;
        public int ReceivedQty
        {
            get => _receivedQty;
            set
            {
                if (SetProperty(ref _receivedQty, value))
                {
                    OnPropertyChanged(nameof(Subtotal));
                    OnPropertyChanged(nameof(ReceivedSubtotal));
                    ItemChanged?.Invoke();
                }
            }
        }

        public decimal Subtotal => ReceivedQty * CostPrice;
        public decimal ReceivedSubtotal => ReceivedQty * CostPrice;

        public Action? ItemChanged { get; set; }
    }
}
