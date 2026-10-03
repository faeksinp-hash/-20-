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
    /// ViewModel สำหรับหน้าจอจัดการโปรโมชั่น (PromotionView.xaml)
    /// รองรับ MVVM, SQLite CRUD, Real-time Filter, Badge Counters, และ Toggle Status
    /// </summary>
    public class PromotionViewModel : ViewModelBase
    {
        private readonly DatabaseService _db;
        private readonly IDialogService _dialog;

        private ObservableCollection<PromotionModel> _allPromotions = new();
        private ObservableCollection<PromotionModel> _filteredPromotions = new();
        private PromotionModel? _selectedPromotion;
        private PromotionModel _editModel = new();
        private bool _isCreatingNew;
        private string _searchKeyword = string.Empty;
        private string _selectedFilter = "ALL"; // ALL, CashDiscount, PointRedeem, FreeDelivery, Expired

        private DateTime? _editStartDate;
        private DateTime? _editEndDate;

        private int _allCount;
        private int _cashDiscountCount;
        private int _pointRedeemCount;
        private int _freeDeliveryCount;
        private int _expiredCount;
        private int _activeCount;

        public PromotionViewModel() : this(new DatabaseService(), DialogService.Instance)
        {
        }

        public PromotionViewModel(DatabaseService db, IDialogService dialog)
        {
            _db = db;
            _dialog = dialog;

            // Initialize Commands
            SelectPromotionCommand = new RelayCommand(param =>
            {
                if (param is PromotionModel promo)
                {
                    SelectPromotion(promo);
                }
            });

            StartCreateNewCommand = new RelayCommand(_ => StartCreateNew());
            SavePromotionCommand = new RelayCommand(async _ => await SavePromotionAsync());
            CancelEditCommand = new RelayCommand(_ => CancelEdit());
            DeletePromotionCommand = new RelayCommand(async _ => await DeletePromotionAsync());
            DuplicatePromotionCommand = new RelayCommand(async param =>
            {
                if (param is PromotionModel promo)
                {
                    await DuplicatePromotionAsync(promo);
                }
                else if (SelectedPromotion != null)
                {
                    await DuplicatePromotionAsync(SelectedPromotion);
                }
            });

            ToggleStatusCommand = new RelayCommand(async param =>
            {
                if (param is PromotionModel promo)
                {
                    await ToggleStatusAsync(promo);
                }
            });

            SetFilterCommand = new RelayCommand(param =>
            {
                if (param is string filterKey)
                {
                    SelectedFilter = filterKey;
                }
            });

            // Initial load
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
                    OnPropertyChanged(nameof(IsFormVisible));
                    if (value != null && !_isCreatingNew)
                    {
                        LoadToEditForm(value);
                    }
                }
            }
        }

        public PromotionModel EditModel
        {
            get => _editModel;
            set => SetProperty(ref _editModel, value);
        }

        public bool IsCreatingNew
        {
            get => _isCreatingNew;
            set
            {
                if (SetProperty(ref _isCreatingNew, value))
                {
                    OnPropertyChanged(nameof(FormHeaderTitle));
                    OnPropertyChanged(nameof(IsPromoIdReadOnly));
                }
            }
        }

        public bool IsPromoIdReadOnly => !IsCreatingNew;
        public bool HasSelectedPromotion => SelectedPromotion != null || IsCreatingNew;
        public bool IsFormVisible => true;

        public string FormHeaderTitle => IsCreatingNew
            ? "สร้างโปรโมชั่นใหม่"
            : $"กำลังแก้ไข: {EditModel.PromoID}";

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
                    OnPropertyChanged(nameof(IsFilterCashDiscountSelected));
                    OnPropertyChanged(nameof(IsFilterPointRedeemSelected));
                    OnPropertyChanged(nameof(IsFilterFreeDeliverySelected));
                    OnPropertyChanged(nameof(IsFilterExpiredSelected));
                }
            }
        }

        public bool IsFilterAllSelected => SelectedFilter == "ALL";
        public bool IsFilterCashDiscountSelected => SelectedFilter == "CashDiscount";
        public bool IsFilterPointRedeemSelected => SelectedFilter == "PointRedeem";
        public bool IsFilterFreeDeliverySelected => SelectedFilter == "FreeDelivery";
        public bool IsFilterExpiredSelected => SelectedFilter == "Expired";

        public DateTime? EditStartDate
        {
            get => _editStartDate;
            set
            {
                if (SetProperty(ref _editStartDate, value) && value.HasValue)
                {
                    EditModel.StartDate = value.Value.ToString("yyyy-MM-dd");
                }
            }
        }

        public DateTime? EditEndDate
        {
            get => _editEndDate;
            set
            {
                if (SetProperty(ref _editEndDate, value) && value.HasValue)
                {
                    EditModel.EndDate = value.Value.ToString("yyyy-MM-dd");
                }
            }
        }

        // Summary Badges & Counters
        public int AllCount
        {
            get => _allCount;
            set => SetProperty(ref _allCount, value);
        }

        public int CashDiscountCount
        {
            get => _cashDiscountCount;
            set => SetProperty(ref _cashDiscountCount, value);
        }

        public int PointRedeemCount
        {
            get => _pointRedeemCount;
            set => SetProperty(ref _pointRedeemCount, value);
        }

        public int FreeDeliveryCount
        {
            get => _freeDeliveryCount;
            set => SetProperty(ref _freeDeliveryCount, value);
        }

        public int ExpiredCount
        {
            get => _expiredCount;
            set => SetProperty(ref _expiredCount, value);
        }

        public int ActiveCount
        {
            get => _activeCount;
            set => SetProperty(ref _activeCount, value);
        }

        #endregion

        #region Commands

        public ICommand SelectPromotionCommand { get; }
        public ICommand StartCreateNewCommand { get; }
        public ICommand SavePromotionCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand DeletePromotionCommand { get; }
        public ICommand DuplicatePromotionCommand { get; }
        public ICommand ToggleStatusCommand { get; }
        public ICommand SetFilterCommand { get; }

        #endregion

        #region Methods

        public async Task LoadPromotionsAsync()
        {
            var list = await _db.GetAllPromotionsAsync();
            _allPromotions = new ObservableCollection<PromotionModel>(list);

            UpdateCounters();
            ApplyFilters();

            if (SelectedPromotion != null)
            {
                var match = _allPromotions.FirstOrDefault(p => p.PromoID == SelectedPromotion.PromoID);
                SelectedPromotion = match ?? _allPromotions.FirstOrDefault();
            }
            else
            {
                SelectedPromotion = _allPromotions.FirstOrDefault();
            }

            if (SelectedPromotion != null && !_isCreatingNew)
            {
                LoadToEditForm(SelectedPromotion);
            }
        }

        private void UpdateCounters()
        {
            AllCount = _allPromotions.Count;
            CashDiscountCount = _allPromotions.Count(p => p.PromoType == "CashDiscount");
            PointRedeemCount = _allPromotions.Count(p => p.PromoType == "PointRedeem");
            FreeDeliveryCount = _allPromotions.Count(p => p.PromoType == "FreeDelivery");
            ExpiredCount = _allPromotions.Count(p => p.IsExpired);
            ActiveCount = _allPromotions.Count(p => p.IsActive && !p.IsExpired);
        }

        private void ApplyFilters()
        {
            var query = _allPromotions.AsEnumerable();

            // Filter chips
            if (SelectedFilter == "CashDiscount")
            {
                query = query.Where(p => p.PromoType == "CashDiscount");
            }
            else if (SelectedFilter == "PointRedeem")
            {
                query = query.Where(p => p.PromoType == "PointRedeem");
            }
            else if (SelectedFilter == "FreeDelivery")
            {
                query = query.Where(p => p.PromoType == "FreeDelivery");
            }
            else if (SelectedFilter == "Expired")
            {
                query = query.Where(p => p.IsExpired);
            }

            // Keyword Search (Exact Code & Case-Sensitive Only)
            if (!string.IsNullOrEmpty(SearchKeyword))
            {
                query = query.Where(p => p.PromoID != null && p.PromoID.Contains(SearchKeyword, StringComparison.Ordinal));
            }

            FilteredPromotions = new ObservableCollection<PromotionModel>(query.ToList());
        }

        public void SelectPromotion(PromotionModel promo)
        {
            IsCreatingNew = false;
            SelectedPromotion = promo;
            LoadToEditForm(promo);
        }

        public void StartCreateNew()
        {
            IsCreatingNew = true;
            SelectedPromotion = null;

            // Generate an automatic suggested ID
            int nextIndex = _allPromotions.Count + 1;
            string autoId = $"PROMO-{nextIndex:D2}";

            EditModel = new PromotionModel
            {
                PromoID = autoId,
                PromoName = "โปรโมชั่นใหม่",
                PromoType = "CashDiscount",
                DiscountAmount = 10,
                MinSpend = 50,
                PointsRequired = 0,
                StartDate = DateTime.Today.ToString("yyyy-MM-dd"),
                EndDate = DateTime.Today.AddMonths(1).ToString("yyyy-MM-dd"),
                IsActive = true,
                UsageCount = 0,
                TotalDiscountGiven = 0
            };

            EditStartDate = DateTime.Today;
            EditEndDate = DateTime.Today.AddMonths(1);

            OnPropertyChanged(nameof(FormHeaderTitle));
        }

        private void LoadToEditForm(PromotionModel source)
        {
            EditModel = source.Clone();

            if (DateTime.TryParse(source.StartDate, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var start))
                EditStartDate = start;
            else if (DateTime.TryParse(source.StartDate, out var startFallback))
                EditStartDate = startFallback;
            else
                EditStartDate = DateTime.Today;

            if (DateTime.TryParse(source.EndDate, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var end))
                EditEndDate = end;
            else if (DateTime.TryParse(source.EndDate, out var endFallback))
                EditEndDate = endFallback;
            else
                EditEndDate = DateTime.Today.AddMonths(1);

            OnPropertyChanged(nameof(FormHeaderTitle));
        }

        public async Task SavePromotionAsync()
        {
            if (string.IsNullOrWhiteSpace(EditModel.PromoID))
            {
                _dialog.ShowWarning("ข้อมูลไม่ครบถ้วน", "กรุณาระบุรหัสโปรโมชั่น เช่น PROMO-10B");
                return;
            }

            if (string.IsNullOrWhiteSpace(EditModel.PromoName))
            {
                _dialog.ShowWarning("ข้อมูลไม่ครบถ้วน", "กรุณาระบุชื่อโปรโมชั่น");
                return;
            }

            if (EditModel.DiscountAmount <= 0)
            {
                _dialog.ShowWarning("ข้อมูลไม่ถูกต้อง", "กรุณาระบุมูลค่าส่วนลดที่มากกว่า 0");
                return;
            }

            if (EditModel.PromoType == "PointRedeem" && EditModel.PointsRequired <= 0)
            {
                _dialog.ShowWarning("ข้อมูลไม่ถูกต้อง", "สำหรับสิทธิ์แลกแต้ม ต้องระบุแต้มสะสมที่ต้องใช้มากกว่า 0");
                return;
            }

            // Sync dates
            if (EditStartDate.HasValue)
                EditModel.StartDate = EditStartDate.Value.ToString("yyyy-MM-dd");
            if (EditEndDate.HasValue)
                EditModel.EndDate = EditEndDate.Value.ToString("yyyy-MM-dd");

            // Check duplicate promo ID if creating new
            if (IsCreatingNew)
            {
                var existing = await _db.GetPromotionByIdAsync(EditModel.PromoID.Trim());
                if (existing != null)
                {
                    _dialog.ShowWarning("รหัสซ้ำ", $"มีรหัสโปรโมชั่น '{EditModel.PromoID}' อยู่ในระบบแล้ว กรุณาใช้รหัสอื่น");
                    return;
                }
            }

            bool success = await _db.SavePromotionAsync(EditModel);
            if (success)
            {
                _dialog.ShowSuccess("บันทึกสำเร็จ", $"บันทึกข้อมูลโปรโมชั่น '{EditModel.PromoName}' เรียบร้อยแล้ว");
                string savedId = EditModel.PromoID;
                IsCreatingNew = false;
                await LoadPromotionsAsync();
                SelectedPromotion = _allPromotions.FirstOrDefault(p => p.PromoID == savedId);
            }
            else
            {
                _dialog.ShowError("เกิดข้อผิดพลาด", "ไม่สามารถบันทึกข้อมูลโปรโมชั่นลงฐานข้อมูลได้");
            }
        }

        public void CancelEdit()
        {
            if (IsCreatingNew)
            {
                IsCreatingNew = false;
                SelectedPromotion = _allPromotions.FirstOrDefault();
                if (SelectedPromotion != null)
                {
                    LoadToEditForm(SelectedPromotion);
                }
            }
            else if (SelectedPromotion != null)
            {
                LoadToEditForm(SelectedPromotion);
            }
        }

        public async Task DeletePromotionAsync()
        {
            if (EditModel == null || string.IsNullOrWhiteSpace(EditModel.PromoID))
            {
                _dialog.ShowWarning("แจ้งเตือน", "กรุณาเลือกโปรโมชั่นที่ต้องการลบ");
                return;
            }

            bool confirmed = _dialog.ShowConfirm(
                "ยืนยันการลบ",
                $"คุณแน่ใจหรือไม่ว่าต้องการลบหรือเก็บถาวรโปรโมชั่น '{EditModel.PromoName}' ({EditModel.PromoID}) ?");

            if (!confirmed) return;

            bool success = await _db.DeletePromotionAsync(EditModel.PromoID);
            if (success)
            {
                _dialog.ShowSuccess("ลบเรียบร้อย", $"ลบโปรโมชั่น '{EditModel.PromoID}' สำเร็จแล้ว");
                IsCreatingNew = false;
                await LoadPromotionsAsync();
            }
            else
            {
                _dialog.ShowError("ข้อผิดพลาด", "ไม่สามารถลบโปรโมชั่นได้");
            }
        }

        public async Task DuplicatePromotionAsync(PromotionModel source)
        {
            string newId = $"{source.PromoID}-COPY";
            int copyIdx = 1;
            while (_allPromotions.Any(p => p.PromoID.Equals(newId, StringComparison.OrdinalIgnoreCase)))
            {
                copyIdx++;
                newId = $"{source.PromoID}-CP{copyIdx}";
            }

            var duplicate = source.Clone();
            duplicate.PromoID = newId;
            duplicate.PromoName = $"{source.PromoName} (สำเนา)";
            duplicate.UsageCount = 0;
            duplicate.TotalDiscountGiven = 0;
            duplicate.IsActive = true;

            bool success = await _db.SavePromotionAsync(duplicate);
            if (success)
            {
                _dialog.ShowSuccess("ทำสำเนาสำเร็จ", $"สร้างโปรโมชั่นสำเนา '{duplicate.PromoID}' เรียบร้อยแล้ว");
                await LoadPromotionsAsync();
                var created = _allPromotions.FirstOrDefault(p => p.PromoID == duplicate.PromoID);
                if (created != null)
                {
                    SelectPromotion(created);
                }
            }
        }

        public async Task ToggleStatusAsync(PromotionModel promo)
        {
            // Note: ToggleButton's two-way binding already inverted promo.IsActive
            bool success = await _db.UpdatePromotionStatusAsync(promo.PromoID, promo.IsActive);
            if (success)
            {
                UpdateCounters();
            }
            else
            {
                // Revert on failure
                promo.IsActive = !promo.IsActive;
                _dialog.ShowError("ข้อผิดพลาด", "ไม่สามารถอัปเดตสถานะของโปรโมชั่นได้");
            }
        }

        #endregion
    }
}
