using System;
using System.Collections.Generic;
using System.Linq;
using Porjai20.Models;

namespace Porjai20.ViewModels
{
    /// <summary>
    /// ViewModel สำหรับหน้าต่างใบเสร็จรับเงิน (ReceiptWindow)
    /// </summary>
    public class ReceiptViewModel : ViewModelBase
    {
        private SalesOrder _order;
        private List<SalesOrderItem> _items;

        public SalesOrder Order
        {
            get => _order;
            set
            {
                if (SetProperty(ref _order, value))
                {
                    OnPropertyChanged(nameof(RefNo));
                    OnPropertyChanged(nameof(Timestamp));
                    OnPropertyChanged(nameof(CustomerName));
                    OnPropertyChanged(nameof(PaymentMethod));
                    OnPropertyChanged(nameof(SubtotalAmount));
                    OnPropertyChanged(nameof(DiscountAmount));
                    OnPropertyChanged(nameof(HasDiscount));
                    OnPropertyChanged(nameof(TotalAmount));
                    OnPropertyChanged(nameof(CashReceived));
                    OnPropertyChanged(nameof(Change));
                }
            }
        }

        public List<SalesOrderItem> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public string RefNo => Order?.RefNo ?? string.Empty;
        public DateTime Timestamp => Order?.Timestamp ?? DateTime.Now;
        public string CustomerName => Order?.CustomerName ?? "ลูกค้าทั่วไป";
        public string PaymentMethod => Order?.PaymentMethod ?? "เงินสด";

        /// <summary>ยอดรวมสินค้าก่อนลด (Subtotal Amount)</summary>
        public decimal SubtotalAmount => Order != null ? Order.SubtotalAmount : 0m;

        /// <summary>ยอดส่วนลดรวม (Discount Amount)</summary>
        public decimal DiscountAmount => Order != null ? Order.DiscountAmount : 0m;

        /// <summary>มีส่วนลดหรือไม่ (แสดงแถวส่วนลดเมื่อ > 0)</summary>
        public bool HasDiscount => DiscountAmount > 0;

        /// <summary>ยอดรวมสุทธิ (Net Total Amount)</summary>
        public decimal TotalAmount => Order != null ? Order.TotalAmount : 0m;

        /// <summary>จำนวนเงินสดที่รับมา</summary>
        public decimal CashReceived => Order != null ? Order.CashReceived : 0m;

        /// <summary>เงินทอนลูกค้า</summary>
        public decimal Change => Order != null ? Order.Change : 0m;

        public ReceiptViewModel(SalesOrder order, IEnumerable<SalesOrderItem> items)
        {
            _order = order ?? new SalesOrder();
            _items = items?.ToList() ?? new List<SalesOrderItem>();
        }
    }
}
