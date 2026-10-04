using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Porjai20.Models;
using Porjai20.Services;

namespace Porjai20.ViewModels
{
    /// <summary>
    /// ViewModel สำหรับหน้าจอหลักจัดการโปรโมชั่น (PromotionView.xaml)
    /// ควบคุมรายการโปรโมชั่น, ฟิลเตอร์ 4 ประเภท (ลด, แลก, แจก, แถม), ตัวกรองหมดอายุ,
    /// สรุปสถิติ 3 แคปซูลมน, การทำสำเนา, และการสลับไปหน้าสร้าง/แก้ไข
    /// </summary>
    public class PromotionViewModel : ViewModelBase
    {
        private readonly PromotionService _promoService;
        private readonly IDialogService _dialog;

        private ObservableCollection<PromotionModel> _allPromotions = new();
        private ObservableCollection<PromotionModel> _filteredPromotions = new();
        private PromotionModel? _selectedPromotion;

        private string _searchKeyword = string.Empty;
        private string _selectedFilter = "ALL"; // ALL, Discount, Redeem, Gift, Freebie, Expired

        private int _allCount;
        private int _activeCount;
        private int _draftCount;
        private int _expiredCount;

        private int _discountCount;
        private int _redeemCount;
        private int _giftCount;
        private int _freebieCount;

        // Subview Switcher
        private bool _isCreateEditMode;
        private PromotionCreateEditViewModel _createEditVM;

        public PromotionViewModel() : this(PromotionService.Instance, DialogService.Instance)
        {
        }

        public PromotionViewModel(PromotionService promoService, IDialogService dialog)
        {
            _promoService = promoService;
            _dialog = dialog;

            _createEditVM = new PromotionCreateEditViewModel(_promoService, _dialog);
            _createEditVM.Saved += OnCreateEditSaved;
            _createEditVM.Cancelled += OnCreateEditCancelled;

            // Commands
            SelectPromotionCommand = new RelayCommand(param =>
            {
                if (param is PromotionModel promo)
                {
                    SelectPromotion(promo);
                }
            });

            StartCreateNewCommand = new RelayCommand(_ => StartCreateNew());
            StartEditCommand = new RelayCommand(param =>
            {
                var target = param as PromotionModel ?? SelectedPromotion;
                if (target != null)
                {
                    StartEdit(target);
                }
            });

            CloseDetailsCommand = new RelayCommand(_ => CloseDetails());

            DuplicatePromotionCommand = new RelayCommand(async param =>
            {
                var target = param as PromotionModel ?? SelectedPromotion;
                if (target != null)
                {
                    await DuplicatePromotionAsync(target);
                }
            });

            ToggleStatusCommand = new RelayCommand(async param =>
            {
                if (param is PromotionModel promo)
                {
                    await ToggleStatusAsync(promo);
                }
            });

            DeletePromotionCommand = new RelayCommand(async _ =>
            {
                if (SelectedPromotion != null)
                {
                    await DeletePromotionAsync(SelectedPromotion);
                }
            });

            SetFilterCommand = new RelayCommand(param =>
            {
                if (param is string filterKey)
                {
                    SelectedFilter = filterKey;
                }
            });

            ClearSearchCommand = new RelayCommand(_ => SearchKeyword = string.Empty);
            BackToCatalogCommand = new RelayCommand(_ => IsCreateEditMode = false);

            _ = LoadPromotionsAsync();
        }

        #region Properties

        public ObservableCollection<PromotionModel> FilteredPromotions
        {
            get => _filteredPromotions;
            set => SetProperty(ref _filteredPromotions, value);
        }

        public PromotionModel? SelectedPromotion
        {
            get => _selectedPromotion;
            set
            {
                if (_selectedPromotion != null)
                {
                    _selectedPromotion.IsSelected = false;
                }
                if (SetProperty(ref _selectedPromotion, value))
                {
                    if (value != null)
                    {
                        value.IsSelected = true;
                    }
                    OnPropertyChanged(nameof(HasSelectedPromotion));
                }
            }
        }

        public bool HasSelectedPromotion => SelectedPromotion != null;

        public bool IsCreateEditMode
        {
            get => _isCreateEditMode;
            set
            {
                if (SetProperty(ref _isCreateEditMode, value))
                {
                    OnPropertyChanged(nameof(IsCatalogMode));
                }
            }
        }

        public bool IsCatalogMode => !IsCreateEditMode;

        public PromotionCreateEditViewModel CreateEditVM
        {
            get => _createEditVM;
            set => SetProperty(ref _createEditVM, value);
        }

        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (SetProperty(ref _searchKeyword, value))
                {
                    ApplyFilters();
                }
            }
        }

        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (SetProperty(ref _selectedFilter, value))
                {
                    ApplyFilters();
                    OnPropertyChanged(nameof(IsFilterAllSelected));
                    OnPropertyChanged(nameof(IsFilterDiscountSelected));
                    OnPropertyChanged(nameof(IsFilterRedeemSelected));
                    OnPropertyChanged(nameof(IsFilterGiftSelected));
                    OnPropertyChanged(nameof(IsFilterFreebieSelected));
                    OnPropertyChanged(nameof(IsFilterExpiredSelected));
                }
            }
        }

        public bool IsFilterAllSelected => SelectedFilter == "ALL";
        public bool IsFilterDiscountSelected => SelectedFilter == "Discount";
        public bool IsFilterRedeemSelected => SelectedFilter == "Redeem";
        public bool IsFilterGiftSelected => SelectedFilter == "Gift";
        public bool IsFilterFreebieSelected => SelectedFilter == "Freebie";
        public bool IsFilterExpiredSelected => SelectedFilter == "Expired";

        // Badges: ใช้งานอยู่ X รายการ | ฉบับร่าง Y รายการ | หมดอายุ Z รายการ
        public int AllCount
        {
            get => _allCount;
            set => SetProperty(ref _allCount, value);
        }

        public int ActiveCount
        {
            get => _activeCount;
            set => SetProperty(ref _activeCount, value);
        }

        public int DraftCount
        {
            get => _draftCount;
            set => SetProperty(ref _draftCount, value);
        }

        public int ExpiredCount
        {
            get => _expiredCount;
            set => SetProperty(ref _expiredCount, value);
        }

        public int DiscountCount
        {
            get => _discountCount;
            set => SetProperty(ref _discountCount, value);
        }

        public int RedeemCount
        {
            get => _redeemCount;
            set => SetProperty(ref _redeemCount, value);
        }

        public int GiftCount
        {
            get => _giftCount;
            set => SetProperty(ref _giftCount, value);
        }

        public int FreebieCount
        {
            get => _freebieCount;
            set => SetProperty(ref _freebieCount, value);
        }

        #endregion

        #region Commands

        public ICommand SelectPromotionCommand { get; }
        public ICommand StartCreateNewCommand { get; }
        public ICommand StartEditCommand { get; }
        public ICommand CloseDetailsCommand { get; }
        public ICommand DuplicatePromotionCommand { get; }
        public ICommand ToggleStatusCommand { get; }
        public ICommand DeletePromotionCommand { get; }
        public ICommand SetFilterCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand BackToCatalogCommand { get; }

        #endregion

        #region Methods

        public async Task LoadPromotionsAsync()
        {
            var list = await _promoService.GetAllPromotionsAsync();
            _allPromotions = new ObservableCollection<PromotionModel>(list);

            UpdateCounters();
            ApplyFilters();

            if (SelectedPromotion != null)
            {
                var match = _allPromotions.FirstOrDefault(p => p.PromoID == SelectedPromotion.PromoID);
                SelectedPromotion = match;
            }
        }

        private void UpdateCounters()
        {
            AllCount = _allPromotions.Count;
            ActiveCount = _allPromotions.Count(p => p.IsActive && !p.IsDraft && !p.IsExpired);
            DraftCount = _allPromotions.Count(p => p.IsDraft);
            ExpiredCount = _allPromotions.Count(p => p.IsExpired);

            DiscountCount = _allPromotions.Count(p => p.PromoType == "Discount");
            RedeemCount = _allPromotions.Count(p => p.PromoType == "Redeem");
            GiftCount = _allPromotions.Count(p => p.PromoType == "Gift");
            FreebieCount = _allPromotions.Count(p => p.PromoType == "Freebie");
        }

        private void ApplyFilters()
        {
            var query = _allPromotions.AsEnumerable();

            // Filter Chips: [ ทั้งหมด (N) ], [ ลด ], [ แลก ], [ แจก ], [ แถม ], [ หมดอายุ (N) ]
            if (SelectedFilter == "Discount")
            {
                query = query.Where(p => p.PromoType == "Discount");
            }
            else if (SelectedFilter == "Redeem")
            {
                query = query.Where(p => p.PromoType == "Redeem");
            }
            else if (SelectedFilter == "Gift")
            {
                query = query.Where(p => p.PromoType == "Gift");
            }
            else if (SelectedFilter == "Freebie")
            {
                query = query.Where(p => p.PromoType == "Freebie");
            }
            else if (SelectedFilter == "Expired")
            {
                query = query.Where(p => p.IsExpired);
            }

            // Keyword Search (ชื่อ หรือ รหัสโปรโมชั่น)
            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                string kw = SearchKeyword.Trim();
                query = query.Where(p =>
                    (p.PromoName != null && p.PromoName.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                    (p.PromoID != null && p.PromoID.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                    (p.PromoCode != null && p.PromoCode.Contains(kw, StringComparison.OrdinalIgnoreCase)));
            }

            FilteredPromotions = new ObservableCollection<PromotionModel>(query.ToList());
        }

        public void SelectPromotion(PromotionModel promo)
        {
            SelectedPromotion = promo;
        }

        public void CloseDetails()
        {
            SelectedPromotion = null;
        }

        public void StartCreateNew()
        {
            CreateEditVM.LoadForNew();
            IsCreateEditMode = true;
        }

        public void StartEdit(PromotionModel promo)
        {
            CreateEditVM.LoadForEdit(promo);
            IsCreateEditMode = true;
        }

        private async void OnCreateEditSaved()
        {
            IsCreateEditMode = false;
            string savedId = CreateEditVM.Model.PromoID;
            await LoadPromotionsAsync();
            SelectedPromotion = _allPromotions.FirstOrDefault(p => p.PromoID == savedId);
        }

        private void OnCreateEditCancelled()
        {
            IsCreateEditMode = false;
        }

        public async Task DuplicatePromotionAsync(PromotionModel source)
        {
            var duplicate = await _promoService.DuplicatePromotionAsync(source);
            if (duplicate != null)
            {
                _dialog.ShowSuccess("ทำสำเนาสำเร็จ", $"คัดลอกโปรโมชั่นเป็นฉบับร่าง '{duplicate.PromoID}' เรียบร้อยแล้ว (ปิดใช้งานอยู่)");
                await LoadPromotionsAsync();
                var created = _allPromotions.FirstOrDefault(p => p.PromoID == duplicate.PromoID);
                if (created != null)
                {
                    SelectPromotion(created);
                }
            }
            else
            {
                _dialog.ShowError("ข้อผิดพลาด", "ไม่สามารถทำสำเนาโปรโมชั่นได้");
            }
        }

        public async Task ToggleStatusAsync(PromotionModel promo)
        {
            if (promo.IsExpired)
            {
                _dialog.ShowWarning("โปรโมชั่นหมดอายุ", "โปรโมชั่นนี้หมดอายุแล้ว ไม่สามารถเปิดใช้งานได้ กรุณาแก้ไขวันสิ้นสุดก่อน");
                promo.IsActive = false;
                return;
            }

            if (promo.IsActive && promo.PromoType == "Discount" && promo.DiscountType == "Percentage" && promo.DiscountAmount < 1)
            {
                _dialog.ShowWarning("ข้อมูลไม่ถูกต้อง", "สำหรับส่วนลดเป็นเปอร์เซ็นต์ มูลค่าส่วนลดต้องไม่ต่ำกว่า 1% จึงจะสามารถเปิดใช้งานได้");
                promo.IsActive = false;
                return;
            }

            if (promo.IsDraft)
            {
                // ถ้าเป็นฉบับร่าง การเปิดใช้งานจะปลดสถานะฉบับร่าง
                promo.IsDraft = false;
            }

            bool success = await _promoService.UpdatePromotionStatusAsync(promo.PromoID, promo.IsActive);
            if (success)
            {
                UpdateCounters();
            }
            else
            {
                // Revert
                promo.IsActive = !promo.IsActive;
                _dialog.ShowError("ข้อผิดพลาด", "ไม่สามารถอัปเดตสถานะของโปรโมชั่นได้");
            }
        }

        public async Task DeletePromotionAsync(PromotionModel promo)
        {
            bool confirmed = _dialog.ShowConfirm(
                "ยืนยันการลบ",
                $"คุณแน่ใจหรือไม่ว่าต้องการลบโปรโมชั่น '{promo.PromoName}' ({promo.PromoID})?");

            if (!confirmed) return;

            bool success = await _promoService.DeletePromotionAsync(promo.PromoID);
            if (success)
            {
                _dialog.ShowSuccess("ลบเรียบร้อย", $"ลบโปรโมชั่น '{promo.PromoID}' สำเร็จแล้ว");
                SelectedPromotion = null;
                await LoadPromotionsAsync();
            }
            else
            {
                _dialog.ShowError("ข้อผิดพลาด", "ไม่สามารถลบโปรโมชั่นได้");
            }
        }

        #endregion
    }
}
