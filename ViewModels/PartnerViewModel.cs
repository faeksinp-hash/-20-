using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Porjai20.Models;
using Porjai20.Services;
using Porjai20.Views;

namespace Porjai20.ViewModels
{
    /// <summary>
    /// Partial ViewModel class for Partner Management in ProductViewModel.
    /// Handles DataGrid selection, dynamic modal title, edit/add state, single in-page modal CRUD operations, auto-generated partner code, and ICollectionView real-time filtering.
    /// </summary>
    public partial class ProductViewModel
    {
        private ICollectionView? _filteredPartners;
        public ICollectionView FilteredPartners
        {
            get
            {
                if (_filteredPartners == null && Partners != null)
                {
                    _filteredPartners = CollectionViewSource.GetDefaultView(Partners);
                    _filteredPartners.Filter = item =>
                    {
                        string query = (SearchText ?? PartnerSearchKeyword ?? string.Empty).Trim().ToLower();
                        if (string.IsNullOrWhiteSpace(query)) return true;
                        if (item is Partner model)
                        {
                            bool matchCode = !string.IsNullOrEmpty(model.Code) && model.Code.ToLower().Contains(query);
                            bool matchName = !string.IsNullOrEmpty(model.Name) && model.Name.ToLower().Contains(query);
                            return matchCode || matchName;
                        }
                        return false;
                    };
                }
                return _filteredPartners;
            }
        }

        public ICollectionView PartnersView => FilteredPartners;

        private string _partnerModalTitle = "ข้อมูลบริษัทคู่ค้า";
        public string PartnerModalTitle
        {
            get => _partnerModalTitle;
            set => SetProperty(ref _partnerModalTitle, value);
        }

        private bool _isPartnerAddMode;
        public bool IsPartnerAddMode
        {
            get => _isPartnerAddMode;
            set => SetProperty(ref _isPartnerAddMode, value);
        }

        private string _originalPartnerCode = string.Empty;
        private string _originalPartnerName = string.Empty;
        private string _originalPartnerContactPerson = string.Empty;
        private string _originalPartnerPhone = string.Empty;
        private string _originalPartnerEmail = string.Empty;
        private string _originalPartnerAddress = string.Empty;
        private string _originalPartnerNote = string.Empty;

        public ICommand EnterPartnerEditModeCommand { get; set; }
        public ICommand CancelPartnerEditCommand { get; set; }

        private string _partnerValidationMessage = string.Empty;
        public string PartnerValidationMessage
        {
            get => _partnerValidationMessage;
            set
            {
                if (SetProperty(ref _partnerValidationMessage, value))
                {
                    OnPropertyChanged(nameof(HasPartnerValidationMessage));
                }
            }
        }

        public bool HasPartnerValidationMessage => !string.IsNullOrWhiteSpace(PartnerValidationMessage);

        public void InitializePartnerViewModelCommands()
        {
            OpenPartnerManageModalCommand = new RelayCommand(_ => ExecuteOpenPartnerManageModal());
            OpenAddPartnerModalCommand = new RelayCommand(_ => ExecuteOpenAddPartnerModal());
            EnterPartnerEditModeCommand = new RelayCommand(_ => ExecuteEnterPartnerEditMode());
            CancelPartnerEditCommand = new RelayCommand(_ => ExecuteCancelPartnerEdit());
            
            SavePartnerCommand = new RelayCommand(_ => ExecuteSavePartner());
            DeletePartnerCommand = new RelayCommand(_ => ExecuteDeletePartner());
            
            ClosePartnerModalCommand = new RelayCommand(_ => ExecuteClosePartnerModal());
            ClearPartnerCommand = new RelayCommand(_ => ClearPartnerForm());
            
            SearchPartnerCommand = new RelayCommand(_ =>
            {
                FilteredPartners?.Refresh();
                OnPropertyChanged(nameof(FilteredPartners));
            });
            ClearPartnerFilterCommand = new RelayCommand(_ =>
            {
                PartnerSearchKeyword = string.Empty;
                SearchText = string.Empty;
                FilteredPartners?.Refresh();
                OnPropertyChanged(nameof(FilteredPartners));
            });
        }

        private string GenerateNextPartnerCode()
        {
            int maxId = 0;
            if (Partners != null && Partners.Count > 0)
            {
                maxId = Partners.Max(p => p.Id);
            }
            return $"SUP-{(maxId + 1):D3}";
        }

        public void ExecuteOpenPartnerManageModal()
        {
            if (SelectedPartner == null || SelectedPartner.Id <= 0)
            {
                ShowAlert("กรุณาคลิกเลือกรายการบริษัทคู่ค้าในตารางก่อนดำเนินการ", "แจ้งเตือน", "⚠️");
                return;
            }

            IsPartnerAddMode = false;
            IsEditMode = false; // Start in Read-Only Mode!
            PartnerModalTitle = "ข้อมูลบริษัทคู่ค้า";
            ModalTitle = PartnerModalTitle;
            PartnerValidationMessage = string.Empty;

            _originalPartnerCode = SelectedPartner.Code ?? string.Empty;
            _originalPartnerName = SelectedPartner.Name ?? string.Empty;
            _originalPartnerContactPerson = SelectedPartner.ContactPerson ?? string.Empty;
            _originalPartnerPhone = SelectedPartner.Phone ?? string.Empty;
            _originalPartnerEmail = SelectedPartner.Email ?? string.Empty;
            _originalPartnerAddress = SelectedPartner.Address ?? string.Empty;
            _originalPartnerNote = SelectedPartner.Note ?? string.Empty;

            PartnerCode = _originalPartnerCode;
            PartnerName = _originalPartnerName;
            PartnerContactPerson = _originalPartnerContactPerson;
            PartnerPhone = _originalPartnerPhone;
            PartnerEmail = _originalPartnerEmail;
            PartnerAddress = _originalPartnerAddress;
            PartnerNote = _originalPartnerNote;

            IsModalOpen = true;
            IsPartnerModalOpen = true;
        }

        public void ExecuteEnterPartnerEditMode()
        {
            IsEditMode = true;
            PartnerModalTitle = "แก้ไขข้อมูลบริษัทคู่ค้า";
            ModalTitle = PartnerModalTitle;
        }

        public void ExecuteCancelPartnerEdit()
        {
            if (IsPartnerAddMode)
            {
                ExecuteClosePartnerModal();
                return;
            }

            PartnerCode = _originalPartnerCode;
            PartnerName = _originalPartnerName;
            PartnerContactPerson = _originalPartnerContactPerson;
            PartnerPhone = _originalPartnerPhone;
            PartnerEmail = _originalPartnerEmail;
            PartnerAddress = _originalPartnerAddress;
            PartnerNote = _originalPartnerNote;

            PartnerValidationMessage = string.Empty;
            IsEditMode = false;
            PartnerModalTitle = "ข้อมูลบริษัทคู่ค้า";
            ModalTitle = PartnerModalTitle;
        }

        public void ExecuteOpenAddPartnerModal()
        {
            ClearPartnerForm();
            IsPartnerAddMode = true;
            IsEditMode = true;
            PartnerModalTitle = "เพิ่มข้อมูลบริษัทคู่ค้า";
            ModalTitle = PartnerModalTitle;
            PartnerValidationMessage = string.Empty;
            PartnerCode = GenerateNextPartnerCode();
            IsModalOpen = true;
            IsPartnerModalOpen = true;
        }

        public void ExecuteSavePartner()
        {
            if (string.IsNullOrWhiteSpace(PartnerName))
            {
                PartnerValidationMessage = "กรุณากรอกชื่อบริษัท/คู่ค้า";
                ShowAlert("กรุณากรอกชื่อบริษัท/คู่ค้า", "แจ้งเตือน", "⚠️");
                return;
            }

            if (!IsPartnerAddMode && SelectedPartner != null && SelectedPartner.Id > 0)
            {
                SelectedPartner.Code = PartnerCode;
                SelectedPartner.Name = PartnerName?.Trim() ?? string.Empty;
                SelectedPartner.ContactPerson = PartnerContactPerson?.Trim() ?? string.Empty;
                SelectedPartner.Phone = PartnerPhone?.Trim() ?? string.Empty;
                SelectedPartner.Email = PartnerEmail?.Trim() ?? string.Empty;
                SelectedPartner.Address = PartnerAddress?.Trim() ?? string.Empty;
                SelectedPartner.Note = PartnerNote?.Trim() ?? string.Empty;

                _databaseService.UpdatePartner(SelectedPartner);
                ClearPartnerForm();
                SetPartnerModalsClosed();
                _ = LoadPartners();
                FilteredPartners?.Refresh();
                OnPropertyChanged(nameof(TotalPartnersCount));
                ShowAlert("อัปเดตข้อมูลบริษัทคู่ค้าเรียบร้อยแล้ว", "สำเร็จ", "🎉");
            }
            else
            {
                string autoCode = string.IsNullOrWhiteSpace(PartnerCode) ? GenerateNextPartnerCode() : PartnerCode;
                var newPartner = new Partner
                {
                    Code = autoCode,
                    Name = PartnerName?.Trim() ?? string.Empty,
                    ContactPerson = PartnerContactPerson?.Trim() ?? string.Empty,
                    Phone = PartnerPhone?.Trim() ?? string.Empty,
                    Email = PartnerEmail?.Trim() ?? string.Empty,
                    Address = PartnerAddress?.Trim() ?? string.Empty,
                    Note = PartnerNote?.Trim() ?? string.Empty,
                    RegisteredDate = DateTime.Now
                };

                _databaseService.SavePartner(newPartner);
                ClearPartnerForm();
                SetPartnerModalsClosed();
                _ = LoadPartners();
                FilteredPartners?.Refresh();
                OnPropertyChanged(nameof(TotalPartnersCount));
                ShowAlert("บันทึกข้อมูลบริษัทคู่ค้าเรียบร้อยแล้ว", "สำเร็จ", "🎉");
            }
        }

        public void ExecuteDeletePartner()
        {
            if (SelectedPartner == null || SelectedPartner.Id <= 0)
            {
                PartnerValidationMessage = "กรุณาคลิกเลือกรายการบริษัทคู่ค้าในตารางก่อนดำเนินการ";
                ShowAlert("กรุณาคลิกเลือกรายการบริษัทคู่ค้าในตารางก่อนดำเนินการ", "แจ้งเตือน", "⚠️");
                return;
            }

            ShowConfirm(
                $"คุณต้องการลบข้อมูลบริษัทคู่ค้า '{SelectedPartner.Name}' ใช่หรือไม่?",
                () =>
                {
                    _databaseService.DeletePartner(SelectedPartner.Id);
                    ClearPartnerForm();
                    SetPartnerModalsClosed();
                    _ = LoadPartners();
                    FilteredPartners?.Refresh();
                    OnPropertyChanged(nameof(TotalPartnersCount));
                    ShowAlert("ลบข้อมูลบริษัทคู่ค้าเรียบร้อยแล้ว", "สำเร็จ", "🗑️");
                },
                "ยืนยันการลบข้อมูลคู่ค้า");
        }

        public void ExecuteClosePartnerModal()
        {
            ClearPartnerForm();
            SetPartnerModalsClosed();
        }

        private void SetPartnerModalsClosed()
        {
            IsModalOpen = false;
            IsPartnerModalOpen = false;
            PartnerValidationMessage = string.Empty;
        }

        public void ClearPartnerForm()
        {
            SelectedPartner = new Partner();
            PartnerCode = string.Empty;
            PartnerName = string.Empty;
            PartnerContactPerson = string.Empty;
            PartnerPhone = string.Empty;
            PartnerEmail = string.Empty;
            PartnerAddress = string.Empty;
            PartnerNote = string.Empty;
            PartnerValidationMessage = string.Empty;
        }
    }

    /// <summary>
    /// Standalone ViewModel class for Partner Management, inheriting from ViewModelBase.
    /// Provides full compatibility if PartnerView is bound directly to PartnerViewModel.
    /// </summary>
    public class PartnerViewModel : ViewModelBase
    {
        private readonly DatabaseService _databaseService;

        public ObservableCollection<Partner> Partners { get; } = new ObservableCollection<Partner>();

        public ICollectionView FilteredPartners { get; private set; }
        public ICollectionView PartnersView => FilteredPartners;

        private Partner? _selectedPartner;
        public Partner? SelectedPartner
        {
            get => _selectedPartner;
            set
            {
                if (SetProperty(ref _selectedPartner, value) && value != null)
                {
                    PartnerCode = value.Code;
                    PartnerName = value.Name;
                    PartnerContactPerson = value.ContactPerson;
                    PartnerPhone = value.Phone;
                    PartnerEmail = value.Email;
                    PartnerAddress = value.Address;
                    PartnerNote = value.Note;
                }
            }
        }

        private bool _isPartnerAddMode;
        public bool IsPartnerAddMode
        {
            get => _isPartnerAddMode;
            set => SetProperty(ref _isPartnerAddMode, value);
        }

        private string _originalPartnerCode = string.Empty;
        private string _originalPartnerName = string.Empty;
        private string _originalPartnerContactPerson = string.Empty;
        private string _originalPartnerPhone = string.Empty;
        private string _originalPartnerEmail = string.Empty;
        private string _originalPartnerAddress = string.Empty;
        private string _originalPartnerNote = string.Empty;

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        private string _modalTitle = "ข้อมูลบริษัทคู่ค้า";
        public string ModalTitle
        {
            get => _modalTitle;
            set => SetProperty(ref _modalTitle, value);
        }

        public string PartnerModalTitle
        {
            get => ModalTitle;
            set => ModalTitle = value;
        }

        private bool _isModalOpen;
        public bool IsModalOpen
        {
            get => _isModalOpen;
            set => SetProperty(ref _isModalOpen, value);
        }

        public bool IsPartnerModalOpen
        {
            get => IsModalOpen;
            set => IsModalOpen = value;
        }

        private string _partnerValidationMessage = string.Empty;
        public string PartnerValidationMessage
        {
            get => _partnerValidationMessage;
            set
            {
                if (SetProperty(ref _partnerValidationMessage, value))
                {
                    OnPropertyChanged(nameof(HasPartnerValidationMessage));
                }
            }
        }

        public bool HasPartnerValidationMessage => !string.IsNullOrWhiteSpace(PartnerValidationMessage);

        private string _partnerCode = string.Empty;
        public string PartnerCode
        {
            get => _partnerCode;
            set => SetProperty(ref _partnerCode, value);
        }

        private string _partnerName = string.Empty;
        public string PartnerName
        {
            get => _partnerName;
            set => SetProperty(ref _partnerName, value);
        }

        private string _partnerContactPerson = string.Empty;
        public string PartnerContactPerson
        {
            get => _partnerContactPerson;
            set => SetProperty(ref _partnerContactPerson, value);
        }

        private string _partnerPhone = string.Empty;
        public string PartnerPhone
        {
            get => _partnerPhone;
            set => SetProperty(ref _partnerPhone, value);
        }

        private string _partnerEmail = string.Empty;
        public string PartnerEmail
        {
            get => _partnerEmail;
            set => SetProperty(ref _partnerEmail, value);
        }

        private string _partnerAddress = string.Empty;
        public string PartnerAddress
        {
            get => _partnerAddress;
            set => SetProperty(ref _partnerAddress, value);
        }

        private string _partnerNote = string.Empty;
        public string PartnerNote
        {
            get => _partnerNote;
            set => SetProperty(ref _partnerNote, value);
        }

        private string _partnerSearchKeyword = string.Empty;
        public string PartnerSearchKeyword
        {
            get => _partnerSearchKeyword;
            set
            {
                if (SetProperty(ref _partnerSearchKeyword, value))
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    FilteredPartners?.Refresh();
                }
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    _partnerSearchKeyword = value;
                    OnPropertyChanged(nameof(PartnerSearchKeyword));
                    FilteredPartners?.Refresh();
                }
            }
        }

        public int TotalPartnersCount => Partners.Count;
        public int MonthlyPurchaseOrdersCount { get; private set; } = 0;
        public decimal TotalSupplierPurchaseValue { get; private set; } = 0;

        public ICommand OpenManageModalCommand { get; }
        public ICommand OpenPartnerManageModalCommand => OpenManageModalCommand;
        public ICommand OpenAddPartnerModalCommand { get; }
        public ICommand EnterPartnerEditModeCommand { get; }
        public ICommand CancelPartnerEditCommand { get; }
        public ICommand SavePartnerCommand { get; }
        public ICommand DeletePartnerCommand { get; }
        public ICommand CloseModalCommand { get; }
        public ICommand ClosePartnerModalCommand => CloseModalCommand;
        public ICommand ClearPartnerCommand { get; }
        public ICommand SearchPartnerCommand { get; }
        public ICommand ClearPartnerFilterCommand { get; }

        public PartnerViewModel()
        {
            _databaseService = new DatabaseService();

            FilteredPartners = CollectionViewSource.GetDefaultView(Partners);
            FilteredPartners.Filter = item =>
            {
                if (string.IsNullOrWhiteSpace(SearchText)) return true;
                if (item is Partner model)
                {
                    string query = SearchText.Trim().ToLower();
                    bool matchCode = !string.IsNullOrEmpty(model.Code) && model.Code.ToLower().Contains(query);
                    bool matchName = !string.IsNullOrEmpty(model.Name) && model.Name.ToLower().Contains(query);
                    return matchCode || matchName;
                }
                return false;
            };

            OpenManageModalCommand = new RelayCommand(_ => ExecuteOpenManageModal());
            OpenAddPartnerModalCommand = new RelayCommand(_ => ExecuteOpenAddModal());
            EnterPartnerEditModeCommand = new RelayCommand(_ => ExecuteEnterEditMode());
            CancelPartnerEditCommand = new RelayCommand(_ => ExecuteCancelEdit());
            SavePartnerCommand = new RelayCommand(_ => ExecuteSavePartner());
            DeletePartnerCommand = new RelayCommand(_ => ExecuteDeletePartner());
            CloseModalCommand = new RelayCommand(_ => ExecuteCloseModal());
            ClearPartnerCommand = new RelayCommand(_ => ClearPartnerForm());
            SearchPartnerCommand = new RelayCommand(_ =>
            {
                FilteredPartners?.Refresh();
                OnPropertyChanged(nameof(FilteredPartners));
            });
            ClearPartnerFilterCommand = new RelayCommand(_ =>
            {
                SearchText = string.Empty;
                FilteredPartners?.Refresh();
                OnPropertyChanged(nameof(FilteredPartners));
            });

            LoadPartners();
        }

        private string GenerateNextPartnerCode()
        {
            int maxId = 0;
            if (Partners != null && Partners.Count > 0)
            {
                maxId = Partners.Max(p => p.Id);
            }
            return $"SUP-{(maxId + 1):D3}";
        }

        public void LoadPartners()
        {
            Partners.Clear();
            var partners = _databaseService.GetPartners();
            foreach (var p in partners)
            {
                Partners.Add(p);
            }
            FilteredPartners?.Refresh();

            OnPropertyChanged(nameof(TotalPartnersCount));
        }

        public void ExecuteOpenManageModal()
        {
            if (SelectedPartner == null || SelectedPartner.Id <= 0)
            {
                return;
            }

            IsPartnerAddMode = false;
            IsEditMode = false; // Start in Read-Only Mode!
            ModalTitle = "ข้อมูลบริษัทคู่ค้า";
            PartnerValidationMessage = string.Empty;

            _originalPartnerCode = SelectedPartner.Code ?? string.Empty;
            _originalPartnerName = SelectedPartner.Name ?? string.Empty;
            _originalPartnerContactPerson = SelectedPartner.ContactPerson ?? string.Empty;
            _originalPartnerPhone = SelectedPartner.Phone ?? string.Empty;
            _originalPartnerEmail = SelectedPartner.Email ?? string.Empty;
            _originalPartnerAddress = SelectedPartner.Address ?? string.Empty;
            _originalPartnerNote = SelectedPartner.Note ?? string.Empty;

            PartnerCode = _originalPartnerCode;
            PartnerName = _originalPartnerName;
            PartnerContactPerson = _originalPartnerContactPerson;
            PartnerPhone = _originalPartnerPhone;
            PartnerEmail = _originalPartnerEmail;
            PartnerAddress = _originalPartnerAddress;
            PartnerNote = _originalPartnerNote;

            IsModalOpen = true;
        }

        public void ExecuteEnterEditMode()
        {
            IsEditMode = true;
            ModalTitle = "แก้ไขข้อมูลบริษัทคู่ค้า";
        }

        public void ExecuteCancelEdit()
        {
            if (IsPartnerAddMode)
            {
                ExecuteCloseModal();
                return;
            }

            PartnerCode = _originalPartnerCode;
            PartnerName = _originalPartnerName;
            PartnerContactPerson = _originalPartnerContactPerson;
            PartnerPhone = _originalPartnerPhone;
            PartnerEmail = _originalPartnerEmail;
            PartnerAddress = _originalPartnerAddress;
            PartnerNote = _originalPartnerNote;

            PartnerValidationMessage = string.Empty;
            IsEditMode = false;
            ModalTitle = "ข้อมูลบริษัทคู่ค้า";
        }

        public void ExecuteOpenAddModal()
        {
            ClearPartnerForm();
            IsPartnerAddMode = true;
            IsEditMode = true;
            ModalTitle = "เพิ่มข้อมูลบริษัทคู่ค้า";
            PartnerValidationMessage = string.Empty;
            PartnerCode = GenerateNextPartnerCode();
            IsModalOpen = true;
        }

        public void ExecuteSavePartner()
        {
            if (string.IsNullOrWhiteSpace(PartnerName))
            {
                PartnerValidationMessage = "กรุณากรอกชื่อบริษัท/คู่ค้า";
                return;
            }

            if (!IsPartnerAddMode && SelectedPartner != null && SelectedPartner.Id > 0)
            {
                SelectedPartner.Code = PartnerCode;
                SelectedPartner.Name = PartnerName?.Trim() ?? string.Empty;
                SelectedPartner.ContactPerson = PartnerContactPerson?.Trim() ?? string.Empty;
                SelectedPartner.Phone = PartnerPhone?.Trim() ?? string.Empty;
                SelectedPartner.Email = PartnerEmail?.Trim() ?? string.Empty;
                SelectedPartner.Address = PartnerAddress?.Trim() ?? string.Empty;
                SelectedPartner.Note = PartnerNote?.Trim() ?? string.Empty;

                _databaseService.UpdatePartner(SelectedPartner);
                ClearPartnerForm();
                IsModalOpen = false;
                LoadPartners();
            }
            else
            {
                string autoCode = string.IsNullOrWhiteSpace(PartnerCode) ? GenerateNextPartnerCode() : PartnerCode;
                var partner = new Partner
                {
                    Code = autoCode,
                    Name = PartnerName?.Trim() ?? string.Empty,
                    ContactPerson = PartnerContactPerson?.Trim() ?? string.Empty,
                    Phone = PartnerPhone?.Trim() ?? string.Empty,
                    Email = PartnerEmail?.Trim() ?? string.Empty,
                    Address = PartnerAddress?.Trim() ?? string.Empty,
                    Note = PartnerNote?.Trim() ?? string.Empty,
                    RegisteredDate = DateTime.Now
                };

                _databaseService.SavePartner(partner);
                ClearPartnerForm();
                IsModalOpen = false;
                LoadPartners();
            }
        }

        public void ExecuteDeletePartner()
        {
            if (SelectedPartner == null || SelectedPartner.Id <= 0)
            {
                PartnerValidationMessage = "กรุณาคลิกเลือกรายการบริษัทคู่ค้าในตารางก่อนดำเนินการ";
                return;
            }

            _databaseService.DeletePartner(SelectedPartner.Id);
            ClearPartnerForm();
            IsModalOpen = false;
            LoadPartners();
        }

        public void ExecuteCloseModal()
        {
            ClearPartnerForm();
            IsModalOpen = false;
        }

        public void ClearPartnerForm()
        {
            SelectedPartner = null;
            PartnerCode = string.Empty;
            PartnerName = string.Empty;
            PartnerContactPerson = string.Empty;
            PartnerPhone = string.Empty;
            PartnerEmail = string.Empty;
            PartnerAddress = string.Empty;
            PartnerNote = string.Empty;
            PartnerValidationMessage = string.Empty;
        }
    }
}
