using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Porjai20.Models
{
    public class Product : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Pro_ID { get; set; }
        public int Id
        {
            get => Pro_ID;
            set => Pro_ID = value;
        }

        public string Pro_Barcode { get; set; } = string.Empty;
        public string Code
        {
            get => string.IsNullOrWhiteSpace(Pro_Barcode) ? (Pro_ID > 0 ? $"P-{Pro_ID:D4}" : string.Empty) : Pro_Barcode;
            set => Pro_Barcode = value;
        }

        public string Pro_Name { get; set; } = string.Empty;
        public string Name
        {
            get => Pro_Name;
            set => Pro_Name = value;
        }

        public decimal Pro_Price { get; set; }
        public decimal Price
        {
            get => Pro_Price;
            set => Pro_Price = value;
        }

        public decimal Pro_Cost { get; set; }
        public decimal Cost
        {
            get => Pro_Cost;
            set => Pro_Cost = value;
        }

        public int Pro_Qty { get; set; }
        public int Stock
        {
            get => Pro_Qty;
            set
            {
                if (Pro_Qty != value)
                {
                    Pro_Qty = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StockStatus));
                }
            }
        }

        public int Pro_MinQty { get; set; } = 5;
        public int ReorderPoint
        {
            get => Pro_MinQty;
            set
            {
                if (Pro_MinQty != value)
                {
                    Pro_MinQty = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StockStatus));
                }
            }
        }

        public string StockStatus
        {
            get
            {
                if (Stock <= 0) return "หมดสต็อก";
                if (Stock <= ReorderPoint) return "ใกล้หมด";
                return "ปกติ";
            }
        }

        public int StatusPriority => Stock <= 0 ? 1 : (Stock <= ReorderPoint ? 2 : 3);
        public int ProductID => Pro_ID;

        public string Pro_Category { get; set; } = string.Empty;
        public string Category
        {
            get => Pro_Category;
            set => Pro_Category = value;
        }

        private string _proImage = string.Empty;
        public string Pro_Image 
        { 
            get => _proImage; 
            set => _proImage = value ?? string.Empty; 
        }

        public string ImagePath
        {
            get => DisplayImagePath;
            set => Pro_Image = value;
        }

        public string Image => DisplayImagePath;
        public string image => DisplayImagePath;
        public string ImageUrl => DisplayImagePath;
        public string image_url => DisplayImagePath;
        public string ImgPath => DisplayImagePath;
        public string img_path => DisplayImagePath;

        public bool HasImagePath => !string.IsNullOrWhiteSpace(DisplayImagePath);

        public string DisplayImagePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_proImage))
                    return string.Empty;

                string trimmed = _proImage.Trim();

                if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                    trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
                {
                    return trimmed;
                }

                if (System.IO.File.Exists(trimmed))
                {
                    return trimmed;
                }

                try
                {
                    string baseDir = System.AppDomain.CurrentDomain.BaseDirectory;
                    string relPath = System.IO.Path.Combine(baseDir, trimmed);
                    if (System.IO.File.Exists(relPath))
                    {
                        return relPath;
                    }

                    string fileName = System.IO.Path.GetFileName(trimmed);
                    string imagesFolder = System.IO.Path.Combine(baseDir, "Images", fileName);
                    if (System.IO.File.Exists(imagesFolder))
                    {
                        return imagesFolder;
                    }

                    string appData = System.IO.Path.Combine(
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                        "MatchaShop", "Images", fileName);
                    if (System.IO.File.Exists(appData))
                    {
                        return appData;
                    }
                }
                catch { }

                return trimmed;
            }
        }

        public int CategoryId { get; set; }
        public int CategoryID
        {
            get => CategoryId;
            set => CategoryId = value;
        }

        private string _unit = string.Empty;
        public string Pro_Unit
        {
            get => _unit;
            set => _unit = value ?? string.Empty;
        }

        public string Unit
        {
            get => _unit ?? string.Empty;
            set => _unit = value ?? string.Empty;
        }
        public string Description { get; set; } = string.Empty;

        public decimal SellingPrice
        {
            get => Price;
            set => Price = value;
        }

        public decimal CostPrice
        {
            get => Cost;
            set => Cost = value;
        }

        public int StockQuantity
        {
            get => Stock;
            set => Stock = value;
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    SelectionChangedAction?.Invoke();
                }
            }
        }

        public Action? SelectionChangedAction { get; set; }

        public override string ToString() => Name;
    }
}
