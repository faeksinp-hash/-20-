using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Porjai20.Common;
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
                    OnPropertyChanged(nameof(DiscountPlaceholderText));
                    ValidateDiscountValue(isStrict: false);
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

            _model.PropertyChanged += OnModelPropertyChanged;
            _ = InitializeProductsAsync();
        }

        #region Properties

        public PromotionModel Model
        {
            get => _model;
            set
            {
                if (_model != null)
                {
                    _model.PropertyChanged -= OnModelPropertyChanged;
                }
                if (SetProperty(ref _model, value))
                {
                    if (_model != null)
                    {
                        _model.PropertyChanged += OnModelPropertyChanged;
                    }
                    OnPropertyChanged(nameof(DiscountPlaceholderText));
                    ValidateDiscountValue(isStrict: false);
                }
            }
        }

        private string _discountValue = string.Empty;
        public string DiscountValue
        {
            get => _discountValue;
            set
            {
                if (SetProperty(ref _discountValue, value))
                {
                    if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ||
                        decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out d))
                    {
                        Model.DiscountAmount = d;
                    }
                    else
                    {
                        Model.DiscountAmount = 0;
                    }

                    ValidateDiscountValue(isStrict: false);
                    UpdateLivePreview();
                }
            }
        }

        private bool _hasDiscountValueError;
        public bool HasDiscountValueError
        {
            get => _hasDiscountValueError;
            set
            {
                if (SetProperty(ref _hasDiscountValueError, value))
                {
                    OnPropertyChanged(nameof(IsInvalidPercentageDiscount));
                }
            }
        }

        private string _discountErrorMessage = string.Empty;
        public string DiscountErrorMessage
        {
            get => _discountErrorMessage;
            set => SetProperty(ref _discountErrorMessage, value);
        }

        public string DiscountPlaceholderText =>
            Model?.DiscountType == "Percentage" ? "เช่น 10" : "เช่น 20";

        public bool IsInvalidPercentageDiscount => HasDiscountValueError;

        #region Safe Numeric String Inputs for UI Binding

        private string _minSpendInput = "0";
        public string MinSpendInput
        {
            get => _minSpendInput;
            set
            {
                if (SetProperty(ref _minSpendInput, value))
                {
                    if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ||
                        decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out parsed))
                    {
                        Model.MinSpend = parsed;
                    }
                    else if (string.IsNullOrWhiteSpace(value))
                    {
                        Model.MinSpend = 0m;
                    }
                    UpdateLivePreview();
                }
            }
        }

        private string _maxDiscountAmountInput = string.Empty;
        public string MaxDiscountAmountInput
        {
            get => _maxDiscountAmountInput;
            set
            {
                if (SetProperty(ref _maxDiscountAmountInput, value))
                {
                    if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ||
                        decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out parsed))
                    {
                        Model.MaxDiscountAmount = parsed;
                    }
                    else if (string.IsNullOrWhiteSpace(value))
                    {
                        Model.MaxDiscountAmount = null;
                    }
                    UpdateLivePreview();
                }
            }
        }

        private string _buyQuantityInput = "2";
        public string BuyQuantityInput
        {
            get => _buyQuantityInput;
            set
            {
                if (SetProperty(ref _buyQuantityInput, value))
                {
                    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                    {
                        Model.BuyQuantity = parsed;
                    }
                    else if (string.IsNullOrWhiteSpace(value))
                    {
                        Model.BuyQuantity = 0;
                    }
                    UpdateLivePreview();
                }
            }
        }

        private string _freeQuantityInput = "1";
        public string FreeQuantityInput
        {
            get => _freeQuantityInput;
            set
            {
                if (SetProperty(ref _freeQuantityInput, value))
                {
                    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                    {
                        Model.FreeQuantity = parsed;
                    }
                    else if (string.IsNullOrWhiteSpace(value))
                    {
                        Model.FreeQuantity = 0;
                    }
                    UpdateLivePreview();
                }
            }
        }

        private string _pointsRequiredInput = "0";
        public string PointsRequiredInput
        {
            get => _pointsRequiredInput;
            set
            {
                if (SetProperty(ref _pointsRequiredInput, value))
                {
                    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                    {
                        Model.PointsRequired = parsed;
                    }
                    else if (string.IsNullOrWhiteSpace(value))
                    {
                        Model.PointsRequired = 0;
                    }
                    UpdateLivePreview();
                }
            }
        }

        private string _redeemDiscountInput = "0";
        public string RedeemDiscountInput
        {
            get => _redeemDiscountInput;
            set
            {
                if (SetProperty(ref _redeemDiscountInput, value))
                {
                    if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ||
                        decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out parsed))
                    {
                        Model.DiscountAmount = parsed;
                    }
                    else if (string.IsNullOrWhiteSpace(value))
                    {
                        Model.DiscountAmount = 0m;
                    }
                    UpdateLivePreview();
                }
            }
        }

        private string _totalQuotaInput = "0";
        public string TotalQuotaInput
        {
            get => _totalQuotaInput;
            set
            {
                if (SetProperty(ref _totalQuotaInput, value))
                {
                    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                    {
                        Model.TotalQuota = parsed;
                    }
                    else if (string.IsNullOrWhiteSpace(value))
                    {
                        Model.TotalQuota = 0;
                    }
                    UpdateLivePreview();
                }
            }
        }

        private string _maxUsagePerCustomerInput = "0";
        public string MaxUsagePerCustomerInput
        {
            get => _maxUsagePerCustomerInput;
            set
            {
                if (SetProperty(ref _maxUsagePerCustomerInput, value))
                {
                    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                    {
                        Model.MaxUsagePerCustomer = parsed;
                    }
                    else if (string.IsNullOrWhiteSpace(value))
                    {
                        Model.MaxUsagePerCustomer = 0;
                    }
                    UpdateLivePreview();
                }
            }
        }

        public void NormalizeEmptyNumericInputs()
        {
            if (string.IsNullOrWhiteSpace(MinSpendInput))
            {
                MinSpendInput = "0";
            }
            if (string.IsNullOrWhiteSpace(MaxDiscountAmountInput))
            {
                Model.MaxDiscountAmount = null;
                MaxDiscountAmountInput = string.Empty;
            }
            if (string.IsNullOrWhiteSpace(BuyQuantityInput))
            {
                BuyQuantityInput = "0";
            }
            if (string.IsNullOrWhiteSpace(FreeQuantityInput))
            {
                FreeQuantityInput = "0";
            }
            if (string.IsNullOrWhiteSpace(PointsRequiredInput))
            {
                PointsRequiredInput = "0";
            }
            if (string.IsNullOrWhiteSpace(RedeemDiscountInput))
            {
                RedeemDiscountInput = "0";
            }
            if (string.IsNullOrWhiteSpace(TotalQuotaInput))
            {
                TotalQuotaInput = "0";
            }
            if (string.IsNullOrWhiteSpace(MaxUsagePerCustomerInput))
            {
                MaxUsagePerCustomerInput = "0";
            }
        }

        #endregion

        public bool ValidateDiscountValue(bool isStrict = false)
        {
            if (Model == null || Model.PromoType != "Discount")
            {
                HasDiscountValueError = false;
                DiscountErrorMessage = string.Empty;
                return true;
            }

            // ถ้าค่ายังเป็นค่าว่าง:
            if (string.IsNullOrWhiteSpace(DiscountValue))
            {
                if (isStrict)
                {
                    HasDiscountValueError = true;
                    DiscountErrorMessage = "⚠️ กรุณาระบุมูลค่าส่วนลด";
                    return false;
                }

                // ขณะพิมพ์หรือสลับประเภท หากยังเป็นค่าว่าง ให้มองเป็นสถานะรอกรอก ไม่แสดงเตือนสีแดง
                HasDiscountValueError = false;
                DiscountErrorMessage = string.Empty;
                return true;
            }

            // เมื่อมีการพิมพ์ตัวเลขเข้ามา ตรวจสอบว่าแปลงเป็นตัวเลขได้หรือไม่
            if (!decimal.TryParse(DiscountValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) &&
                !decimal.TryParse(DiscountValue, NumberStyles.Any, CultureInfo.CurrentCulture, out val))
            {
                HasDiscountValueError = true;
                DiscountErrorMessage = "⚠️ กรุณาระบุมูลค่าส่วนลดเป็นตัวเลขที่ถูกต้อง";
                return false;
            }

            if (Model.DiscountType == "Percentage")
            {
                if (val < 1)
                {
                    HasDiscountValueError = true;
                    DiscountErrorMessage = "⚠️ กรณีเลือกเปอร์เซ็นต์ มูลค่าส่วนลดต้องไม่ต่ำกว่า 1%";
                    return false;
                }
                if (val > 100)
                {
                    HasDiscountValueError = true;
                    DiscountErrorMessage = "⚠️ มูลค่าส่วนลดต้องไม่เกิน 100%";
                    return false;
                }
            }
            else // Cash
            {
                if (val <= 0)
                {
                    HasDiscountValueError = true;
                    DiscountErrorMessage = "⚠️ มูลค่าส่วนลดต้องมากกว่า 0 บาท";
                    return false;
                }
            }

            HasDiscountValueError = false;
            DiscountErrorMessage = string.Empty;
            return true;
        }

        private void OnModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PromotionModel.DiscountType))
            {
                OnPropertyChanged(nameof(DiscountPlaceholderText));
                ValidateDiscountValue(isStrict: false);
                UpdateLivePreview();
            }
            else if (e.PropertyName == nameof(PromotionModel.PromoType))
            {
                OnPropertyChanged(nameof(DiscountPlaceholderText));
                ValidateDiscountValue(isStrict: false);
                UpdateLivePreview();
            }
            else if (e.PropertyName == nameof(PromotionModel.DiscountAmount) ||
                e.PropertyName == nameof(PromotionModel.MaxDiscountAmount) ||
                e.PropertyName == nameof(PromotionModel.MinSpend) ||
                e.PropertyName == nameof(PromotionModel.BuyQuantity) ||
                e.PropertyName == nameof(PromotionModel.FreeQuantity) ||
                e.PropertyName == nameof(PromotionModel.PointsRequired) ||
                e.PropertyName == nameof(PromotionModel.PromoName))
            {
                UpdateLivePreview();
            }
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
                    Model.StartDate = ThaiDateHelper.FormatToDbStorage(value.Value);
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
                    Model.EndDate = ThaiDateHelper.FormatToDbStorage(value.Value);
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
                StartDate = ThaiDateHelper.FormatToDbStorage(DateTime.Today),
                EndDate = ThaiDateHelper.FormatToDbStorage(DateTime.Today.AddMonths(1)),
                IsActive = true,
                IsDraft = false,
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
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

            MinSpendInput = "0";
            MaxDiscountAmountInput = string.Empty;
            BuyQuantityInput = "2";
            FreeQuantityInput = "1";
            PointsRequiredInput = "0";
            RedeemDiscountInput = "0";
            TotalQuotaInput = "0";
            MaxUsagePerCustomerInput = "0";
            DiscountValue = string.Empty;
            HasDiscountValueError = false;
            DiscountErrorMessage = string.Empty;

            NotifyAllStepChanges();
            UpdateLivePreview();
        }

        public void LoadForEdit(PromotionModel source)
        {
            IsEditMode = true;
            HeaderTitle = $"แก้ไขโปรโมชั่น: {source.PromoID}";

            Model = source.Clone();

            SelectedStartDate = ThaiDateHelper.ParseToUniversalDate(Model.StartDate);
            SelectedEndDate = ThaiDateHelper.ParseToUniversalDate(Model.EndDate);

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

            MinSpendInput = Model.MinSpend > 0 ? Model.MinSpend.ToString("0.##", CultureInfo.InvariantCulture) : "0";
            MaxDiscountAmountInput = Model.MaxDiscountAmount.HasValue && Model.MaxDiscountAmount.Value > 0
                ? Model.MaxDiscountAmount.Value.ToString("0.##", CultureInfo.InvariantCulture)
                : string.Empty;
            BuyQuantityInput = Model.BuyQuantity.ToString();
            FreeQuantityInput = Model.FreeQuantity.ToString();
            PointsRequiredInput = Model.PointsRequired.ToString();
            RedeemDiscountInput = Model.DiscountAmount > 0 ? Model.DiscountAmount.ToString("0.##", CultureInfo.InvariantCulture) : "0";
            TotalQuotaInput = Model.TotalQuota.ToString();
            MaxUsagePerCustomerInput = Model.MaxUsagePerCustomer.ToString();

            if (Model.PromoType == "Discount" && Model.DiscountAmount > 0)
            {
                DiscountValue = Model.DiscountAmount.ToString("0.##", CultureInfo.InvariantCulture);
            }
            else
            {
                DiscountValue = string.Empty;
            }
            HasDiscountValueError = false;
            DiscountErrorMessage = string.Empty;

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
            OnPropertyChanged(nameof(DiscountPlaceholderText));
            OnPropertyChanged(nameof(MinSpendInput));
            OnPropertyChanged(nameof(MaxDiscountAmountInput));
            OnPropertyChanged(nameof(BuyQuantityInput));
            OnPropertyChanged(nameof(FreeQuantityInput));
            OnPropertyChanged(nameof(PointsRequiredInput));
            OnPropertyChanged(nameof(RedeemDiscountInput));
            OnPropertyChanged(nameof(TotalQuotaInput));
            OnPropertyChanged(nameof(MaxUsagePerCustomerInput));
        }

        public async Task SaveAsync(bool isActive, bool isDraft)
        {
            NormalizeEmptyNumericInputs();
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

            if (Model.PromoType == "Discount")
            {
                if (string.IsNullOrWhiteSpace(DiscountValue))
                {
                    HasDiscountValueError = true;
                    DiscountErrorMessage = "⚠️ กรุณาระบุมูลค่าส่วนลด";
                    _dialog.ShowWarning("ข้อมูลไม่ครบถ้วน", "กรุณาระบุมูลค่าส่วนลดก่อนทำการบันทึก");
                    return;
                }

                if (!ValidateDiscountValue(isStrict: true))
                {
                    string cleanMsg = DiscountErrorMessage.StartsWith("⚠️ ")
                        ? DiscountErrorMessage.Substring(3)
                        : DiscountErrorMessage;
                    _dialog.ShowWarning("ข้อมูลไม่ถูกต้อง", cleanMsg);
                    return;
                }
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
                Model.StartDate = ThaiDateHelper.FormatToDbStorage(SelectedStartDate.Value);
            if (SelectedEndDate.HasValue)
                Model.EndDate = ThaiDateHelper.FormatToDbStorage(SelectedEndDate.Value);

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
