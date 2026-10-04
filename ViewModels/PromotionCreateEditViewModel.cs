using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Porjai20.Models;
using Porjai20.Services;

namespace Porjai20.ViewModels
{
    public class ProductSelectableItem : ViewModelBase
    {
        public Product Product { get; }
        private readonly Action? _onChanged;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value))
                {
                    _onChanged?.Invoke();
                }
            }
        }

        public int Id => Product.Id;
        public string Name => Product.Name;
        public decimal Price => Product.Price;
        public decimal Cost => Product.Cost;
        public int Stock => Product.Stock;
        public string PriceFormatted => $"{Price:N2}";
        public string StockFormatted => $"{Stock:N0}";

        public ProductSelectableItem(Product product, bool isSelected = false, Action? onChanged = null)
        {
            Product = product;
            _isSelected = isSelected;
            _onChanged = onChanged;
        }
    }

    /// <summary>
    /// ViewModel สำหรับหน้าจอสร้างและแก้ไขโปรโมชั่นแบบ 5 ขั้นตอน (PromotionCreateEditView.xaml)
    /// รองรับ Real-time Live Preview, คำนวณต้นทุนร้านค้าต่อครั้ง, ตารางเลือกสินค้า, และสลับโหมดบันทึกร่าง/เปิดใช้งาน
    /// </summary>
    public class PromotionCreateEditViewModel : ViewModelBase
    {
        private readonly PromotionService _promoService;
        private readonly IDialogService _dialog;

        private PromotionModel _model = new();
        private bool _isEditMode;
        private string _headerTitle = "สร้างโปรโมชั่นใหม่";

        private DateTime? _selectedStartDate = DateTime.Today;
        private DateTime? _selectedEndDate = DateTime.Today.AddMonths(1);

        private ObservableCollection<ProductSelectableItem> _allSelectableProducts = new();
        private ObservableCollection<ProductSelectableItem> _filteredSelectableProducts = new();
        private ObservableCollection<Product> _selectedProducts = new();
        private ObservableCollection<Product> _availableGiftProducts = new();
        private Product? _selectedGiftProduct;

        private string _productSearchKeyword = string.Empty;
        private bool _isSameProductFreebie = true;

        public event Action? Saved;
        public event Action? Cancelled;

        public PromotionCreateEditViewModel() : this(PromotionService.Instance, DialogService.Instance)
        {
        }

        public PromotionCreateEditViewModel(PromotionService promoService, IDialogService dialog)
        {
            _promoService = promoService;
            _dialog = dialog;

            // Commands
            SelectPromoTypeCommand = new RelayCommand(param =>
            {
                if (param is string type)
                {
                    Model.PromoType = type;
                    OnPropertyChanged(nameof(IsDiscountStep));
                    OnPropertyChanged(nameof(IsRedeemStep));
                    OnPropertyChanged(nameof(IsGiftStep));
                    OnPropertyChanged(nameof(IsFreebieStep));
                    UpdateLivePreview();
                }
            });

            SetTargetScopeCommand = new RelayCommand(param =>
            {
                if (param is string scope)
                {
                    Model.TargetScope = scope;
                    OnPropertyChanged(nameof(IsSpecificProductsScope));
                    OnPropertyChanged(nameof(IsAllStoreScope));
                    UpdateLivePreview();
                }
            });

            ToggleProductSelectionCommand = new RelayCommand(param =>
            {
                if (param is ProductSelectableItem item)
                {
                    item.IsSelected = !item.IsSelected;
                    SyncSelectedProductsFromCheckboxes();
                    UpdateLivePreview();
                }
            });

            RemoveSelectedProductCommand = new RelayCommand(param =>
            {
                if (param is Product prod)
                {
                    var match = _allSelectableProducts.FirstOrDefault(p => p.Id == prod.Id);
                    if (match != null) match.IsSelected = false;
                    _selectedProducts.Remove(prod);
                    SyncProductIdsToModel();
                    UpdateLivePreview();
                }
            });

            SaveAndActivateCommand = new RelayCommand(async _ => await SaveAsync(isActive: true, isDraft: false));
            SaveAsDraftCommand = new RelayCommand(async _ => await SaveAsync(isActive: false, isDraft: true));
            CancelCommand = new RelayCommand(_ => Cancelled?.Invoke());

            _ = InitializeProductsAsync();
        }

        #region Properties

        public PromotionModel Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (SetProperty(ref _isEditMode, value))
                {
                    HeaderTitle = value ? "แก้ไขโปรโมชั่น" : "สร้างโปรโมชั่นใหม่";
                }
            }
        }

        public string HeaderTitle
        {
            get => _headerTitle;
            set => SetProperty(ref _headerTitle, value);
        }

        public DateTime? SelectedStartDate
        {
            get => _selectedStartDate;
            set
            {
                if (SetProperty(ref _selectedStartDate, value) && value.HasValue)
                {
                    Model.StartDate = value.Value.ToString("yyyy-MM-dd");
                    UpdateLivePreview();
                }
            }
        }

        public DateTime? SelectedEndDate
        {
            get => _selectedEndDate;
            set
            {
                if (SetProperty(ref _selectedEndDate, value) && value.HasValue)
                {
                    Model.EndDate = value.Value.ToString("yyyy-MM-dd");
                    UpdateLivePreview();
                }
            }
        }

        public ObservableCollection<ProductSelectableItem> FilteredSelectableProducts
        {
            get => _filteredSelectableProducts;
            set => SetProperty(ref _filteredSelectableProducts, value);
        }

        public ObservableCollection<Product> SelectedProducts
        {
            get => _selectedProducts;
            set => SetProperty(ref _selectedProducts, value);
        }

        public ObservableCollection<Product> AvailableGiftProducts
        {
            get => _availableGiftProducts;
            set => SetProperty(ref _availableGiftProducts, value);
        }

        public Product? SelectedGiftProduct
        {
            get => _selectedGiftProduct;
            set
            {
                if (SetProperty(ref _selectedGiftProduct, value))
                {
                    if (value != null)
                    {
                        Model.FreeProductID = value.Id.ToString();
                        Model.FreeProductName = value.Name;
                        Model.ItemCost = value.Cost;
                    }
                    else
                    {
                        Model.FreeProductID = null;
                        Model.FreeProductName = null;
                        Model.ItemCost = SelectedProducts.FirstOrDefault()?.Cost ?? 0m;
                    }
                    UpdateLivePreview();
                }
            }
        }

        public bool IsSameProductFreebie
        {
            get => _isSameProductFreebie;
            set
            {
                if (SetProperty(ref _isSameProductFreebie, value))
                {
                    if (value)
                    {
                        SelectedGiftProduct = null;
                    }
                    UpdateLivePreview();
                }
            }
        }

        public string ProductSearchKeyword
        {
            get => _productSearchKeyword;
            set
            {
                if (SetProperty(ref _productSearchKeyword, value))
                {
                    FilterSelectableProducts();
                }
            }
        }

        public bool IsDiscountStep => Model.PromoType == "Discount";
        public bool IsRedeemStep => Model.PromoType == "Redeem";
        public bool IsGiftStep => Model.PromoType == "Gift";
        public bool IsFreebieStep => Model.PromoType == "Freebie";

        public bool IsAllStoreScope => Model.TargetScope == "AllStore";
        public bool IsSpecificProductsScope => Model.TargetScope == "SpecificProducts";

        // Live Preview Strings
        private string _livePreviewCalculationText = string.Empty;
        public string LivePreviewCalculationText
        {
            get => _livePreviewCalculationText;
            set => SetProperty(ref _livePreviewCalculationText, value);
        }

        private string _livePreviewCostText = string.Empty;
        public string LivePreviewCostText
        {
            get => _livePreviewCostText;
            set => SetProperty(ref _livePreviewCostText, value);
        }

        private decimal _estimatedStoreCost;
        public decimal EstimatedStoreCost
        {
            get => _estimatedStoreCost;
            set
            {
                if (SetProperty(ref _estimatedStoreCost, value))
                {
                    OnPropertyChanged(nameof(EstimatedStoreCostFormatted));
                }
            }
        }
        public string EstimatedStoreCostFormatted => $"{EstimatedStoreCost:N2}";

        #endregion

        #region Commands

        public ICommand SelectPromoTypeCommand { get; }
        public ICommand SetTargetScopeCommand { get; }
        public ICommand ToggleProductSelectionCommand { get; }
        public ICommand RemoveSelectedProductCommand { get; }
        public ICommand SaveAndActivateCommand { get; }
        public ICommand SaveAsDraftCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Methods

        public async Task InitializeProductsAsync()
        {
            var prods = await _promoService.GetAllActiveProductsAsync();
            _allSelectableProducts.Clear();
            _availableGiftProducts.Clear();

            foreach (var p in prods.Where(p => p.Stock > 0))
            {
                _allSelectableProducts.Add(new ProductSelectableItem(p, false, () =>
                {
                    SyncSelectedProductsFromCheckboxes();
                    UpdateLivePreview();
                }));
                _availableGiftProducts.Add(p);
            }

            FilterSelectableProducts();
            UpdateLivePreview();
        }

        public void LoadForNew()
        {
            IsEditMode = false;
            HeaderTitle = "สร้างโปรโมชั่นใหม่";

            Model = new PromotionModel
            {
                PromoID = $"PROMO-{DateTime.Now:MMddHHmm}",
                PromoCode = string.Empty,
                PromoName = string.Empty,
                PromoType = "Discount",
                TargetScope = "SpecificProducts",
                DiscountType = "Cash",
                DiscountAmount = 0,
                MinSpend = 0,
                BuyQuantity = 2,
                FreeQuantity = 1,
                CanRepeat = true,
                IsMemberOnly = false,
                CanCombine = false,
                MaxUsagePerCustomer = 0,
                TotalQuota = 0,
                StartDate = DateTime.Today.ToString("yyyy-MM-dd"),
                EndDate = DateTime.Today.AddMonths(1).ToString("yyyy-MM-dd"),
                IsActive = true,
                IsDraft = false,
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            SelectedStartDate = DateTime.Today;
            SelectedEndDate = DateTime.Today.AddMonths(1);
            SelectedProducts.Clear();
            SelectedGiftProduct = null;
            IsSameProductFreebie = true;

            // Reset selection checkboxes
            foreach (var item in _allSelectableProducts)
            {
                item.IsSelected = false;
            }

            NotifyAllStepChanges();
            UpdateLivePreview();
        }

        public void LoadForEdit(PromotionModel source)
        {
            IsEditMode = true;
            HeaderTitle = $"แก้ไขโปรโมชั่น: {source.PromoID}";

            Model = source.Clone();

            if (DateTime.TryParse(Model.StartDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var s))
                SelectedStartDate = s;
            else if (DateTime.TryParse(Model.StartDate, out var sf))
                SelectedStartDate = sf;
            else
                SelectedStartDate = DateTime.Today;

            if (DateTime.TryParse(Model.EndDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var e))
                SelectedEndDate = e;
            else if (DateTime.TryParse(Model.EndDate, out var ef))
                SelectedEndDate = ef;
            else
                SelectedEndDate = DateTime.Today.AddMonths(1);

            // Re-sync products selection
            SelectedProducts.Clear();
            var targetIds = new HashSet<string>(Model.ProductIds);

            foreach (var item in _allSelectableProducts)
            {
                bool isSel = targetIds.Contains(item.Id.ToString());
                item.IsSelected = isSel;
                if (isSel)
                {
                    SelectedProducts.Add(item.Product);
                }
            }

            if (!string.IsNullOrWhiteSpace(Model.FreeProductID))
            {
                IsSameProductFreebie = false;
                SelectedGiftProduct = _availableGiftProducts.FirstOrDefault(p => p.Id.ToString() == Model.FreeProductID);
            }
            else
            {
                IsSameProductFreebie = true;
                SelectedGiftProduct = null;
            }

            NotifyAllStepChanges();
            UpdateLivePreview();
        }

        private void FilterSelectableProducts()
        {
            var query = _allSelectableProducts.Where(p => p.Stock > 0);
            if (!string.IsNullOrWhiteSpace(ProductSearchKeyword))
            {
                string kw = ProductSearchKeyword.Trim();
                query = query.Where(p =>
                    p.Name.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    p.Product.Code.Contains(kw, StringComparison.OrdinalIgnoreCase));
            }

            FilteredSelectableProducts = new ObservableCollection<ProductSelectableItem>(query.ToList());
        }

        private void SyncSelectedProductsFromCheckboxes()
        {
            SelectedProducts.Clear();
            foreach (var item in _allSelectableProducts.Where(x => x.IsSelected))
            {
                SelectedProducts.Add(item.Product);
            }
            SyncProductIdsToModel();
        }

        private void SyncProductIdsToModel()
        {
            Model.ProductIds = SelectedProducts.Select(p => p.Id.ToString()).ToList();
            Model.ProductNames = SelectedProducts.Select(p => p.Name).ToList();

            if (IsSameProductFreebie && SelectedProducts.Count > 0)
            {
                Model.ItemCost = SelectedProducts[0].Cost;
            }
        }

        public void UpdateLivePreview()
        {
            SyncProductIdsToModel();

            string promoName = string.IsNullOrWhiteSpace(Model.PromoName) ? "โปรโมชั่น" : Model.PromoName;
            string targetProdName = SelectedProducts.Count > 0 ? SelectedProducts[0].Name : "สินค้าที่กำหนด";
            decimal targetProdPrice = SelectedProducts.Count > 0 ? SelectedProducts[0].Price : 20.00m;
            decimal targetProdCost = SelectedProducts.Count > 0 ? SelectedProducts[0].Cost : 12.00m;

            string freeProdName = !IsSameProductFreebie && SelectedGiftProduct != null
                ? SelectedGiftProduct.Name
                : targetProdName;

            decimal freeProdCost = !IsSameProductFreebie && SelectedGiftProduct != null
                ? SelectedGiftProduct.Cost
                : targetProdCost;

            switch (Model.PromoType)
            {
                case "Freebie":
                    {
                        int buyQ = Model.BuyQuantity > 0 ? Model.BuyQuantity : 2;
                        int freeQ = Model.FreeQuantity > 0 ? Model.FreeQuantity : 1;
                        decimal buyTotal = buyQ * targetProdPrice;
                        int totalItems = buyQ + freeQ;

                        LivePreviewCalculationText =
                            $"ตัวอย่างการคิดเงิน: ซื้อ{targetProdName} {buyQ} ชิ้น ({buyTotal:N2} บาท) " +
                            $"แถม{freeProdName} {freeQ} ชิ้น ลูกค้าจ่าย {buyTotal:N2} บาท ได้ {totalItems} ชิ้น " +
                            $"ตัดสต็อก{freeProdName} {freeQ} ชิ้น (ราคา 0.00 บาท)";

                        EstimatedStoreCost = freeQ * freeProdCost;
                        LivePreviewCostText = $"ร้านเสียต่อครั้ง: ต้นทุน{freeProdName} {freeQ} ชิ้น [{EstimatedStoreCostFormatted}] บาท ต่อครั้ง";
                    }
                    break;

                case "Gift":
                    {
                        decimal minSpend = Model.MinSpend > 0 ? Model.MinSpend : 200m;
                        int freeQ = Model.FreeQuantity > 0 ? Model.FreeQuantity : 1;

                        LivePreviewCalculationText =
                            $"ตัวอย่างการคิดเงิน: ยอดซื้อสินค้าครบ {minSpend:N2} บาท ได้รับฟรี {freeProdName} {freeQ} ชิ้น " +
                            $"บันทึกลงในบิลราคา 0.00 บาท ตัดสต็อก {freeQ} ชิ้น";

                        EstimatedStoreCost = freeQ * freeProdCost;
                        LivePreviewCostText = $"ร้านเสียต่อครั้ง: ต้นทุน{freeProdName} {freeQ} ชิ้น [{EstimatedStoreCostFormatted}] บาท ต่อครั้ง";
                    }
                    break;

                case "Redeem":
                    {
                        int points = Model.PointsRequired > 0 ? Model.PointsRequired : 50;
                        decimal discount = Model.DiscountAmount > 0 ? Model.DiscountAmount : 15m;
                        decimal minSpend = Model.MinSpend > 0 ? Model.MinSpend : 50m;
                        decimal net = Math.Max(0, minSpend - discount);

                        LivePreviewCalculationText =
                            $"ตัวอย่างการคิดเงิน: สมาชิกซื้อครบ {minSpend:N2} บาท ใช้ {points:N0} แต้ม " +
                            $"แลกส่วนลด {discount:N2} บาท ลูกค้าจ่ายสุทธิ {net:N2} บาท";

                        EstimatedStoreCost = discount;
                        LivePreviewCostText = $"ร้านเสียต่อครั้ง: มูลค่าส่วนลดเงินสด [{EstimatedStoreCostFormatted}] บาท ต่อครั้ง";
                    }
                    break;

                case "Discount":
                default:
                    {
                        decimal minSpend = Model.MinSpend > 0 ? Model.MinSpend : 100m;
                        decimal discountVal = Model.DiscountAmount > 0 ? Model.DiscountAmount : 10m;
                        decimal actualDiscount;

                        if (Model.DiscountType == "Percentage")
                        {
                            actualDiscount = minSpend * (discountVal / 100m);
                            if (Model.MaxDiscountAmount.HasValue && Model.MaxDiscountAmount.Value > 0)
                            {
                                actualDiscount = Math.Min(actualDiscount, Model.MaxDiscountAmount.Value);
                            }
                            decimal net = Math.Max(0, minSpend - actualDiscount);
                            LivePreviewCalculationText =
                                $"ตัวอย่างการคิดเงิน: ยอดซื้อ {minSpend:N2} บาท ลด {discountVal:N0}% " +
                                $"เป็นส่วนลด {actualDiscount:N2} บาท ลูกค้าจ่ายสุทธิ {net:N2} บาท";
                        }
                        else
                        {
                            actualDiscount = Math.Min(discountVal, minSpend);
                            decimal net = Math.Max(0, minSpend - actualDiscount);
                            LivePreviewCalculationText =
                                $"ตัวอย่างการคิดเงิน: ยอดซื้อ {minSpend:N2} บาท ได้รับส่วนลดทันที {actualDiscount:N2} บาท " +
                                $"ลูกค้าจ่ายสุทธิ {net:N2} บาท";
                        }

                        EstimatedStoreCost = actualDiscount;
                        LivePreviewCostText = $"ร้านเสียต่อครั้ง: มูลค่าส่วนลด [{EstimatedStoreCostFormatted}] บาท ต่อครั้ง";
                    }
                    break;
            }

            OnPropertyChanged(nameof(EstimatedStoreCost));
            OnPropertyChanged(nameof(EstimatedStoreCostFormatted));
            OnPropertyChanged(nameof(LivePreviewCalculationText));
            OnPropertyChanged(nameof(LivePreviewCostText));
        }

        private void NotifyAllStepChanges()
        {
            OnPropertyChanged(nameof(IsDiscountStep));
            OnPropertyChanged(nameof(IsRedeemStep));
            OnPropertyChanged(nameof(IsGiftStep));
            OnPropertyChanged(nameof(IsFreebieStep));
            OnPropertyChanged(nameof(IsAllStoreScope));
            OnPropertyChanged(nameof(IsSpecificProductsScope));
        }

        public async Task SaveAsync(bool isActive, bool isDraft)
        {
            if (string.IsNullOrWhiteSpace(Model.PromoID))
            {
                _dialog.ShowWarning("ข้อมูลไม่ครบถ้วน", "กรุณาระบุรหัสโปรโมชั่น");
                return;
            }

            if (string.IsNullOrWhiteSpace(Model.PromoName))
            {
                _dialog.ShowWarning("ข้อมูลไม่ครบถ้วน", "กรุณาระบุชื่อโปรโมชั่น");
                return;
            }

            if (Model.TargetScope == "SpecificProducts" && SelectedProducts.Count == 0)
            {
                _dialog.ShowWarning("ข้อมูลไม่ครบถ้วน", "กรณีเลือก 'เลือกสินค้าเอง' กรุณาเลือกสินค้าเข้าร่วมอย่างน้อย 1 ชิ้น");
                return;
            }

            if (Model.PromoType == "Freebie" && (Model.BuyQuantity <= 0 || Model.FreeQuantity <= 0))
            {
                _dialog.ShowWarning("ข้อมูลไม่ถูกต้อง", "สำหรับโปรโมชั่นแถม ต้องระบุจำนวนที่ซื้อและจำนวนที่แถมมากกว่า 0");
                return;
            }

            if (Model.PromoType == "Redeem" && Model.PointsRequired <= 0)
            {
                _dialog.ShowWarning("ข้อมูลไม่ถูกต้อง", "สำหรับสิทธิ์แลกแต้ม ต้องระบุจำนวนแต้มที่ต้องใช้มากกว่า 0");
                return;
            }

            // Sync dates
            if (SelectedStartDate.HasValue)
                Model.StartDate = SelectedStartDate.Value.ToString("yyyy-MM-dd");
            if (SelectedEndDate.HasValue)
                Model.EndDate = SelectedEndDate.Value.ToString("yyyy-MM-dd");

            Model.IsActive = isActive;
            Model.IsDraft = isDraft;

            SyncProductIdsToModel();

            // Save to Database
            bool success = await _promoService.SavePromotionAsync(Model, Model.ProductIds);
            if (success)
            {
                string msg = isDraft
                    ? $"บันทึกโปรโมชั่น '{Model.PromoName}' เป็นฉบับร่างเรียบร้อยแล้ว"
                    : $"บันทึกและเปิดใช้งานโปรโมชั่น '{Model.PromoName}' เรียบร้อยแล้ว";

                _dialog.ShowSuccess("บันทึกสำเร็จ", msg);
                Saved?.Invoke();
            }
            else
            {
                _dialog.ShowError("เกิดข้อผิดพลาด", "ไม่สามารถบันทึกข้อมูลโปรโมชั่นลงฐานข้อมูลได้");
            }
        }

        #endregion
    }
}
