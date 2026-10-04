using System;

namespace Porjai20.Models
{
    public class Expense
    {
        public int Expense_ID { get; set; }
        public int Id
        {
            get => Expense_ID;
            set => Expense_ID = value;
        }

        private string _code = string.Empty;
        public string Code
        {
            get => string.IsNullOrWhiteSpace(_code) ? $"EXP-{Expense_ID:D3}" : _code;
            set => _code = value;
        }

        public string Expense_Date { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public DateTime ExpenseDate
        {
            get
            {
                if (DateTime.TryParse(Expense_Date, out var dt)) return dt;
                return DateTime.Now;
            }
            set => Expense_Date = value.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public string Expense_Category { get; set; } = string.Empty;
        public string Category
        {
            get => Expense_Category;
            set => Expense_Category = value;
        }

        public decimal Expense_Amount { get; set; }
        public decimal Amount
        {
            get => Expense_Amount;
            set => Expense_Amount = value;
        }

        public string Expense_Note { get; set; } = string.Empty;
        public string Note
        {
            get => Expense_Note;
            set => Expense_Note = value;
        }

        public int Emp_ID { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
