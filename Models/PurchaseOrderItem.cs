using System;
using Porjai20.ViewModels;

namespace Porjai20.Models
{
    public class PurchaseOrderItem : ViewModelBase
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

        private decimal _poCost;
        public decimal PO_Cost
        {
            get => _poCost;
            set
            {
                if (SetProperty(ref _poCost, value))
                {
                    OnPropertyChanged(nameof(CostPrice));
                    OnPropertyChanged(nameof(TotalPrice));
                    OnPropertyChanged(nameof(PO_Subtotal));
                    ItemChanged?.Invoke();
                }
            }
        }

        public decimal CostPrice
        {
            get => PO_Cost;
            set => PO_Cost = value;
        }

        private int _poQty = 1;
        public int PO_Qty
        {
            get => _poQty;
            set
            {
                if (SetProperty(ref _poQty, value))
                {
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(TotalPrice));
                    OnPropertyChanged(nameof(PO_Subtotal));
                    ItemChanged?.Invoke();
                }
            }
        }

        public int Quantity
        {
            get => PO_Qty;
            set => PO_Qty = value;
        }

        public decimal PO_Subtotal
        {
            get => Quantity * CostPrice;
            set { }
        }

        public decimal TotalPrice => Quantity * CostPrice;

        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        public Action? ItemChanged { get; set; }
    }
}
