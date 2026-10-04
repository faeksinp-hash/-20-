using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Dapper;
using Porjai20.Models;
using Porjai20.Services;
using Porjai20.Views;

namespace Porjai20.ViewModels
{
    /// <summary>
    /// Partial ViewModel class for Staff Management in ProductViewModel.
    /// Handles DataGrid selection, dynamic modal title, edit/add state, single in-page modal CRUD operations, and ICollectionView real-time filtering.
    /// </summary>
    public partial class ProductViewModel
    {
        private ICollectionView? _filteredStaffs;
        public ICollectionView FilteredStaffs
        {
            get
            {
                if (_filteredStaffs == null && UsersList != null)
                {
                    _filteredStaffs = CollectionViewSource.GetDefaultView(UsersList);
                    _filteredStaffs.Filter = item =>
                    {
                        string query = (SearchText ?? StaffSearchKeyword ?? string.Empty).Trim().ToLower();
                        if (string.IsNullOrWhiteSpace(query)) return true;
                        if (item is User model)
                        {
                            bool matchCode = !string.IsNullOrEmpty(model.Username) && model.Username.ToLower().Contains(query);
                            bool matchName = !string.IsNullOrEmpty(model.Name) && model.Name.ToLower().Contains(query);
                            return matchCode || matchName;
                        }
                        return false;
                    };
                }
                return _filteredStaffs;
            }
        }

        public ICollectionView StaffsView => FilteredStaffs;

        public ObservableCollection<User> FilteredStaffList
        {
            get
            {
                if (FilteredStaffs is ICollectionView cv)
                {
                    return new ObservableCollection<User>(cv.Cast<User>());
                }
                return UsersList ?? new ObservableCollection<User>();
            }
        }

        private string _staffModalTitle = "⚙️ แก้ไขข้อมูลพนักงาน";
        public string StaffModalTitle
        {
            get => _staffModalTitle;
            set => SetProperty(ref _staffModalTitle, value);
        }

        private string _staffValidationMessage = string.Empty;
        public string StaffValidationMessage
        {
            get => _staffValidationMessage;
            set
            {
                if (SetProperty(ref _staffValidationMessage, value))
                {
                    OnPropertyChanged(nameof(HasStaffValidationMessage));
                }
            }
        }

        public bool HasStaffValidationMessage => !string.IsNullOrWhiteSpace(StaffValidationMessage);

        private string _staffUsername = string.Empty;
        public string StaffUsername
        {
            get => _staffUsername;
            set => SetProperty(ref _staffUsername, value);
        }

        private string _staffPassword = string.Empty;
        public string StaffPassword
        {
            get => _staffPassword;
            set => SetProperty(ref _staffPassword, value);
        }

        private string _staffName = string.Empty;
        public string StaffName
        {
            get => _staffName;
            set => SetProperty(ref _staffName, value);
        }

        private string _staffPhone = string.Empty;
        public string StaffPhone
        {
            get => _staffPhone;
            set => SetProperty(ref _staffPhone, value);
        }

        private string _staffRole = "พนักงานทั่วไป";
        public string StaffRole
        {
            get => _staffRole;
            set => SetProperty(ref _staffRole, value);
        }

        public User? SelectedStaff
        {
            get => SelectedUser;
            set
            {
                SelectedUser = value;
                OnPropertyChanged(nameof(SelectedStaff));
                if (value != null)
                {
                    StaffUsername = value.Username ?? string.Empty;
                    StaffPassword = value.Password ?? string.Empty;
                    StaffName = value.Name ?? string.Empty;
                    StaffPhone = value.Phone ?? string.Empty;
                    StaffRole = !string.IsNullOrWhiteSpace(value.Role) ? value.Role : "พนักงานทั่วไป";
                }
            }
        }

        private bool _isStaffAddMode;
        public bool IsStaffAddMode
        {
            get => _isStaffAddMode;
            set
            {
                if (SetProperty(ref _isStaffAddMode, value))
                {
                    OnPropertyChanged(nameof(CanEditStaffUsername));
                }
            }
        }
        public bool CanEditStaffUsername => IsStaffAddMode;

        private string _originalStaffUsername = string.Empty;
        private string _originalStaffPassword = string.Empty;
        private string _originalStaffName = string.Empty;
        private string _originalStaffPhone = string.Empty;
        private string _originalStaffRole = "พนักงานทั่วไป";

        public ICommand OpenAddStaffModalCommand { get; set; }
        public ICommand EnterStaffEditModeCommand { get; set; }
        public ICommand CancelStaffEditCommand { get; set; }
        public ICommand SaveStaffCommand { get; set; }
        public ICommand DeleteStaffCommand { get; set; }
        public ICommand ClearStaffCommand { get; set; }

        public void InitializeStaffViewModelCommands()
        {
            OpenStaffManageModalCommand = new RelayCommand(_ => ExecuteOpenStaffManageModal());
            OpenAddStaffModalCommand = new RelayCommand(_ => ExecuteOpenAddStaffModal());
            EnterStaffEditModeCommand = new RelayCommand(_ => ExecuteEnterStaffEditMode());
            CancelStaffEditCommand = new RelayCommand(_ => ExecuteCancelStaffEdit());
            
            SaveStaffCommand = new RelayCommand(_ => ExecuteSaveStaff());
            DeleteStaffCommand = new RelayCommand(_ => ExecuteDeleteStaff());
            
            CloseStaffModalCommand = new RelayCommand(_ => ExecuteCloseStaffModal());
            ClearStaffCommand = new RelayCommand(_ => ClearStaffForm());
            
            SearchStaffCommand = new RelayCommand(_ =>
            {
                FilteredStaffs?.Refresh();
                OnPropertyChanged(nameof(FilteredStaffList));
            });
            ClearStaffFilterCommand = new RelayCommand(_ =>
            {
                StaffSearchKeyword = string.Empty;
                SearchText = string.Empty;
                FilteredStaffs?.Refresh();
                OnPropertyChanged(nameof(FilteredStaffList));
            });
        }

        public void ExecuteOpenStaffManageModal()
        {
            if (SelectedStaff == null || SelectedStaff.Id <= 0)
            {
                ShowAlert("กรุณาคลิกเลือกรายการพนักงานในตารางก่อนดำเนินการ", "แจ้งเตือน", "⚠️");
                return;
            }

            IsStaffAddMode = false;
            IsEditMode = false; // เริ่มต้นในสถานะดูข้อมูลอย่างเดียว (Read-Only)
            StaffModalTitle = "ข้อมูลพนักงาน";
            ModalTitle = StaffModalTitle;
            StaffValidationMessage = string.Empty;
            
            StaffUsername = SelectedStaff.Username ?? string.Empty;
            StaffPassword = SelectedStaff.Password ?? string.Empty;
            StaffName = SelectedStaff.Name ?? string.Empty;
            StaffPhone = SelectedStaff.Phone ?? string.Empty;
            StaffRole = !string.IsNullOrWhiteSpace(SelectedStaff.Role) ? SelectedStaff.Role : "พนักงานทั่วไป";

            _originalStaffUsername = StaffUsername;
            _originalStaffPassword = StaffPassword;
            _originalStaffName = StaffName;
            _originalStaffPhone = StaffPhone;
            _originalStaffRole = StaffRole;

            IsModalOpen = true;
            IsStaffModalOpen = true;
        }

        public void ExecuteEnterStaffEditMode()
        {
            _originalStaffUsername = StaffUsername;
            _originalStaffPassword = StaffPassword;
            _originalStaffName = StaffName;
            _originalStaffPhone = StaffPhone;
            _originalStaffRole = StaffRole;

            IsEditMode = true;
            StaffModalTitle = "แก้ไขข้อมูลพนักงาน";
            ModalTitle = StaffModalTitle;
            StaffValidationMessage = string.Empty;
        }

        public void ExecuteCancelStaffEdit()
        {
            if (IsStaffAddMode)
            {
                ExecuteCloseStaffModal();
                return;
            }

            // คืนค่าเดิมก่อนแก้ไข
            StaffUsername = _originalStaffUsername;
            StaffPassword = _originalStaffPassword;
            StaffName = _originalStaffName;
            StaffPhone = _originalStaffPhone;
            StaffRole = _originalStaffRole;
            StaffValidationMessage = string.Empty;

            // สลับกลับสู่โหมดดูข้อมูล
            IsEditMode = false;
            StaffModalTitle = "ข้อมูลพนักงาน";
            ModalTitle = StaffModalTitle;
        }

        public void ExecuteOpenAddStaffModal()
        {
            ClearStaffForm();
            IsStaffAddMode = true;
            IsEditMode = true; // โหมดเพิ่มข้อมูลปลดล็อกให้กรอกได้
            StaffModalTitle = "➕ เพิ่มข้อมูลพนักงานใหม่";
            ModalTitle = StaffModalTitle;
            StaffValidationMessage = string.Empty;
            IsModalOpen = true;
            IsStaffModalOpen = true;
        }

        public void ExecuteSaveStaff()
        {
            if (string.IsNullOrWhiteSpace(StaffName))
            {
                StaffValidationMessage = "กรุณากรอกชื่อ-นามสกุลของพนักงาน";
                return;
            }

            if (string.IsNullOrWhiteSpace(StaffUsername))
            {
                StaffValidationMessage = "กรุณากรอกชื่อผู้ใช้ / รหัสพนักงาน";
                return;
            }

            if (string.IsNullOrWhiteSpace(StaffPassword))
            {
                StaffValidationMessage = "กรุณากรอกรหัสผ่าน";
                return;
            }

            if (!IsStaffAddMode && SelectedStaff != null && SelectedStaff.Id > 0)
            {
                SelectedStaff.Username = StaffUsername;
                SelectedStaff.Password = StaffPassword;
                SelectedStaff.Name = StaffName;
                SelectedStaff.Phone = StaffPhone;
                SelectedStaff.Role = StaffRole;

                _ = UpdateUser();
                ClearStaffForm();
                SetStaffModalsClosed();
                FilteredStaffs?.Refresh();
                OnPropertyChanged(nameof(TotalStaffCount));
                OnPropertyChanged(nameof(ActiveStaffCount));
                OnPropertyChanged(nameof(AdminStaffCount));
                ShowAlert("อัปเดตข้อมูลพนักงานเรียบร้อยแล้ว", "สำเร็จ", "🎉");
            }
            else
            {
                // Check if username exists
                if (UsersList.Any(u => string.Equals(u.Username, StaffUsername, StringComparison.OrdinalIgnoreCase)))
                {
                    StaffValidationMessage = "ชื่อผู้ใช้/รหัสพนักงานนี้มีอยู่แล้วในระบบ";
                    return;
                }

                var newUser = new User
                {
                    Username = StaffUsername,
                    Password = StaffPassword,
                    Name = StaffName,
                    Phone = StaffPhone,
                    Role = StaffRole
                };
                SelectedUser = newUser;

                _ = AddUser();
                ClearStaffForm();
                SetStaffModalsClosed();
                FilteredStaffs?.Refresh();
                OnPropertyChanged(nameof(TotalStaffCount));
                OnPropertyChanged(nameof(ActiveStaffCount));
                OnPropertyChanged(nameof(AdminStaffCount));
                ShowAlert("บันทึกข้อมูลพนักงานเรียบร้อยแล้ว", "สำเร็จ", "🎉");
            }
        }

        public void ExecuteDeleteStaff()
        {
            if (SelectedStaff == null || SelectedStaff.Id <= 0)
            {
                StaffValidationMessage = "กรุณาคลิกเลือกรายการพนักงานในตารางก่อนดำเนินการ";
                return;
            }

            if (string.Equals(SelectedStaff.Username, "admin", StringComparison.OrdinalIgnoreCase))
            {
                StaffValidationMessage = "ไม่สามารถลบบัญชีผู้ดูแลระบบหลัก (admin) ได้";
                ShowAlert("ไม่สามารถลบบัญชีผู้ดูแลระบบหลัก (admin) ได้", "แจ้งเตือน", "⚠️");
                return;
            }

            if (CurrentUser != null && string.Equals(SelectedStaff.Username, CurrentUser.Username, StringComparison.OrdinalIgnoreCase))
            {
                StaffValidationMessage = "ไม่สามารถลบบัญชีพนักงานที่กำลังเข้าสู่ระบบอยู่ได้";
                ShowAlert("ไม่สามารถลบบัญชีพนักงานที่กำลังเข้าสู่ระบบอยู่ได้", "แจ้งเตือน", "⚠️");
                return;
            }

            ShowConfirm(
                $"คุณต้องการลบบัญชีพนักงาน '{SelectedStaff.Name}' ใช่หรือไม่?",
                async () =>
                {
                    try
                    {
                        using (var conn = _databaseService.GetConnection())
                        {
                            string sql = "DELETE FROM tblEmployee WHERE Emp_ID = @Id";
                            await conn.ExecuteAsync(sql, new { Id = SelectedStaff.Id });
                        }
                        await LoadUsers();
                        ClearStaffForm();
                        SetStaffModalsClosed();
                        FilteredStaffs?.Refresh();
                        OnPropertyChanged(nameof(TotalStaffCount));
                        OnPropertyChanged(nameof(ActiveStaffCount));
                        OnPropertyChanged(nameof(AdminStaffCount));
                        ShowAlert("ลบข้อมูลพนักงานเรียบร้อยแล้ว", "สำเร็จ", "🗑️");
                    }
                    catch (Exception ex)
                    {
                        ShowAlert($"เกิดข้อผิดพลาดในการลบข้อมูล: {ex.Message}", "ข้อผิดพลาด", "❌");
                    }
                },
                "ยืนยันการลบข้อมูลพนักงาน");
        }

        public void ExecuteCloseStaffModal()
        {
            ClearStaffForm();
            SetStaffModalsClosed();
        }

        private void SetStaffModalsClosed()
        {
            IsModalOpen = false;
            IsStaffModalOpen = false;
            IsStaffAddMode = false;
            StaffValidationMessage = string.Empty;
        }

        public void ClearStaffForm()
        {
            SelectedStaff = null;
            IsStaffAddMode = false;
            StaffUsername = string.Empty;
            StaffPassword = string.Empty;
            StaffName = string.Empty;
            StaffPhone = string.Empty;
            StaffRole = "พนักงานทั่วไป";
            StaffValidationMessage = string.Empty;
        }
    }

    /// <summary>
    /// Standalone ViewModel class for Staff Management, inheriting from ViewModelBase.
    /// Provides full compatibility if StaffView is bound directly to StaffViewModel.
    /// </summary>
    public class StaffViewModel : ViewModelBase
    {
        private readonly DatabaseService _databaseService;

        public ObservableCollection<User> UsersList { get; } = new ObservableCollection<User>();
        public ObservableCollection<User> Staffs => UsersList;

        public ICollectionView FilteredStaffs { get; private set; }
        public ICollectionView StaffsView => FilteredStaffs;

        public ObservableCollection<User> FilteredStaffList
        {
            get
            {
                if (FilteredStaffs is ICollectionView cv)
                {
                    return new ObservableCollection<User>(cv.Cast<User>());
                }
                return UsersList ?? new ObservableCollection<User>();
            }
        }

        private User? _selectedStaff;
        public User? SelectedStaff
        {
            get => _selectedStaff;
            set
            {
                if (SetProperty(ref _selectedStaff, value) && value != null)
                {
                    StaffUsername = value.Username ?? string.Empty;
                    StaffPassword = value.Password ?? string.Empty;
                    StaffName = value.Name ?? string.Empty;
                    StaffPhone = value.Phone ?? string.Empty;
                    StaffRole = !string.IsNullOrWhiteSpace(value.Role) ? value.Role : "พนักงานทั่วไป (User)";
                }
            }
        }

        public User? SelectedUser
        {
            get => SelectedStaff;
            set => SelectedStaff = value;
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        private string _modalTitle = "⚙️ แก้ไขข้อมูลพนักงาน";
        public string ModalTitle
        {
            get => _modalTitle;
            set => SetProperty(ref _modalTitle, value);
        }

        public string StaffModalTitle
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

        public bool IsStaffModalOpen
        {
            get => IsModalOpen;
            set => IsModalOpen = value;
        }

        private string _staffValidationMessage = string.Empty;
        public string StaffValidationMessage
        {
            get => _staffValidationMessage;
            set
            {
                if (SetProperty(ref _staffValidationMessage, value))
                {
                    OnPropertyChanged(nameof(HasStaffValidationMessage));
                }
            }
        }

        public bool HasStaffValidationMessage => !string.IsNullOrWhiteSpace(StaffValidationMessage);

        private string _staffUsername = string.Empty;
        public string StaffUsername
        {
            get => _staffUsername;
            set => SetProperty(ref _staffUsername, value);
        }

        private string _staffPassword = string.Empty;
        public string StaffPassword
        {
            get => _staffPassword;
            set => SetProperty(ref _staffPassword, value);
        }

        private string _staffName = string.Empty;
        public string StaffName
        {
            get => _staffName;
            set => SetProperty(ref _staffName, value);
        }

        private string _staffPhone = string.Empty;
        public string StaffPhone
        {
            get => _staffPhone;
            set => SetProperty(ref _staffPhone, value);
        }

        private string _staffRole = "พนักงานทั่วไป";
        public string StaffRole
        {
            get => _staffRole;
            set => SetProperty(ref _staffRole, value);
        }

        private string _staffSearchKeyword = string.Empty;
        public string StaffSearchKeyword
        {
            get => _staffSearchKeyword;
            set
            {
                if (SetProperty(ref _staffSearchKeyword, value))
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    FilteredStaffs?.Refresh();
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
                    _staffSearchKeyword = value;
                    OnPropertyChanged(nameof(StaffSearchKeyword));
                    FilteredStaffs?.Refresh();
                }
            }
        }

        public int TotalStaffCount => UsersList.Count;
        public int ActiveStaffCount => UsersList.Count(u => u != null);
        public int AdminStaffCount => UsersList.Count(u => u.Role == "Admin" || u.Role == "ผู้ดูแลระบบ" || u.Role == "ผู้ดูแลระบบ (Admin)" || u.Role == "เจ้าของร้าน");

        private bool _isStaffAddMode;
        public bool IsStaffAddMode
        {
            get => _isStaffAddMode;
            set
            {
                if (SetProperty(ref _isStaffAddMode, value))
                {
                    OnPropertyChanged(nameof(CanEditStaffUsername));
                }
            }
        }
        public bool CanEditStaffUsername => IsStaffAddMode;

        private string _originalStaffUsername = string.Empty;
        private string _originalStaffPassword = string.Empty;
        private string _originalStaffName = string.Empty;
        private string _originalStaffPhone = string.Empty;
        private string _originalStaffRole = "พนักงานทั่วไป";

        public ICommand OpenManageModalCommand { get; }
        public ICommand OpenStaffManageModalCommand => OpenManageModalCommand;
        public ICommand OpenAddStaffModalCommand { get; }
        public ICommand EnterStaffEditModeCommand { get; }
        public ICommand CancelStaffEditCommand { get; }
        public ICommand SaveStaffCommand { get; }
        public ICommand SaveUserCommand => SaveStaffCommand;
        public ICommand DeleteStaffCommand { get; }
        public ICommand DeleteUserCommand => DeleteStaffCommand;
        public ICommand CloseModalCommand { get; }
        public ICommand CloseStaffModalCommand => CloseModalCommand;
        public ICommand ClearStaffCommand { get; }
        public ICommand ClearUserCommand => ClearStaffCommand;
        public ICommand SearchStaffCommand { get; }
        public ICommand ClearStaffFilterCommand { get; }

        public StaffViewModel()
        {
            _databaseService = new DatabaseService();

            FilteredStaffs = CollectionViewSource.GetDefaultView(UsersList);
            FilteredStaffs.Filter = item =>
            {
                if (string.IsNullOrWhiteSpace(SearchText)) return true;
                if (item is User model)
                {
                    string query = SearchText.Trim().ToLower();
                    bool matchCode = !string.IsNullOrEmpty(model.Username) && model.Username.ToLower().Contains(query);
                    bool matchName = !string.IsNullOrEmpty(model.Name) && model.Name.ToLower().Contains(query);
                    return matchCode || matchName;
                }
                return false;
            };

            OpenManageModalCommand = new RelayCommand(_ => ExecuteOpenManageModal());
            OpenAddStaffModalCommand = new RelayCommand(_ => ExecuteOpenAddModal());
            EnterStaffEditModeCommand = new RelayCommand(_ => ExecuteEnterStaffEditMode());
            CancelStaffEditCommand = new RelayCommand(_ => ExecuteCancelStaffEdit());
            SaveStaffCommand = new RelayCommand(_ => ExecuteSaveStaff());
            DeleteStaffCommand = new RelayCommand(_ => ExecuteDeleteStaff());
            CloseModalCommand = new RelayCommand(_ => ExecuteCloseModal());
            ClearStaffCommand = new RelayCommand(_ => ClearStaffForm());
            SearchStaffCommand = new RelayCommand(_ => FilteredStaffs?.Refresh());
            ClearStaffFilterCommand = new RelayCommand(_ =>
            {
                SearchText = string.Empty;
                FilteredStaffs?.Refresh();
            });

            LoadStaffs();
        }

        public void LoadStaffs()
        {
            UsersList.Clear();
            var users = _databaseService.GetEmployees();
            foreach (var u in users) UsersList.Add(u);
            FilteredStaffs?.Refresh();

            OnPropertyChanged(nameof(TotalStaffCount));
            OnPropertyChanged(nameof(ActiveStaffCount));
            OnPropertyChanged(nameof(AdminStaffCount));
        }

        public void ExecuteOpenManageModal()
        {
            if (SelectedStaff == null || SelectedStaff.Id <= 0)
            {
                return;
            }

            IsStaffAddMode = false;
            IsEditMode = false;
            ModalTitle = "ข้อมูลพนักงาน";
            StaffValidationMessage = string.Empty;

            StaffUsername = SelectedStaff.Username ?? string.Empty;
            StaffPassword = SelectedStaff.Password ?? string.Empty;
            StaffName = SelectedStaff.Name ?? string.Empty;
            StaffPhone = SelectedStaff.Phone ?? string.Empty;
            StaffRole = !string.IsNullOrWhiteSpace(SelectedStaff.Role) ? SelectedStaff.Role : "พนักงานทั่วไป";

            _originalStaffUsername = StaffUsername;
            _originalStaffPassword = StaffPassword;
            _originalStaffName = StaffName;
            _originalStaffPhone = StaffPhone;
            _originalStaffRole = StaffRole;

            IsModalOpen = true;
        }

        public void ExecuteEnterStaffEditMode()
        {
            _originalStaffUsername = StaffUsername;
            _originalStaffPassword = StaffPassword;
            _originalStaffName = StaffName;
            _originalStaffPhone = StaffPhone;
            _originalStaffRole = StaffRole;

            IsEditMode = true;
            ModalTitle = "แก้ไขข้อมูลพนักงาน";
            StaffValidationMessage = string.Empty;
        }

        public void ExecuteCancelStaffEdit()
        {
            if (IsStaffAddMode)
            {
                ExecuteCloseModal();
                return;
            }

            StaffUsername = _originalStaffUsername;
            StaffPassword = _originalStaffPassword;
            StaffName = _originalStaffName;
            StaffPhone = _originalStaffPhone;
            StaffRole = _originalStaffRole;
            StaffValidationMessage = string.Empty;

            IsEditMode = false;
            ModalTitle = "ข้อมูลพนักงาน";
        }

        public void ExecuteOpenAddModal()
        {
            ClearStaffForm();
            IsStaffAddMode = true;
            IsEditMode = true;
            ModalTitle = "➕ เพิ่มข้อมูลพนักงานใหม่";
            StaffValidationMessage = string.Empty;
            IsModalOpen = true;
        }

        public void ExecuteSaveStaff()
        {
            if (string.IsNullOrWhiteSpace(StaffName))
            {
                StaffValidationMessage = "กรุณากรอกชื่อ-นามสกุลของพนักงาน";
                return;
            }

            if (string.IsNullOrWhiteSpace(StaffUsername))
            {
                StaffValidationMessage = "กรุณากรอกชื่อผู้ใช้ / รหัสพนักงาน";
                return;
            }

            if (string.IsNullOrWhiteSpace(StaffPassword))
            {
                StaffValidationMessage = "กรุณากรอกรหัสผ่าน";
                return;
            }

            if (!IsStaffAddMode && SelectedStaff != null && SelectedStaff.Id > 0)
            {
                SelectedStaff.Username = StaffUsername;
                SelectedStaff.Password = StaffPassword;
                SelectedStaff.Name = StaffName;
                SelectedStaff.Phone = StaffPhone;
                SelectedStaff.Role = StaffRole;

                _databaseService.UpdateEmployee(SelectedStaff);
                ClearStaffForm();
                IsModalOpen = false;
                LoadStaffs();
            }
            else
            {
                // Check duplicate username
                if (UsersList.Any(u => string.Equals(u.Username, StaffUsername, StringComparison.OrdinalIgnoreCase)))
                {
                    StaffValidationMessage = "ชื่อผู้ใช้/รหัสพนักงานนี้มีอยู่แล้วในระบบ";
                    return;
                }

                var user = new User
                {
                    Username = StaffUsername,
                    Password = StaffPassword,
                    Name = StaffName,
                    Phone = StaffPhone,
                    Role = StaffRole
                };

                _databaseService.SaveEmployee(user);
                ClearStaffForm();
                IsModalOpen = false;
                LoadStaffs();
            }
        }

        public void ExecuteDeleteStaff()
        {
            if (SelectedStaff == null || SelectedStaff.Id <= 0)
            {
                StaffValidationMessage = "กรุณาคลิกเลือกรายการพนักงานในตารางก่อนดำเนินการ";
                return;
            }

            if (string.Equals(SelectedStaff.Username, "admin", StringComparison.OrdinalIgnoreCase))
            {
                StaffValidationMessage = "ไม่สามารถลบบัญชีผู้ดูแลระบบหลัก (admin) ได้";
                return;
            }

            _databaseService.DeleteEmployee(SelectedStaff.Id);
            ClearStaffForm();
            IsModalOpen = false;
            LoadStaffs();
        }

        public void ExecuteCloseModal()
        {
            ClearStaffForm();
            IsModalOpen = false;
        }

        public void ClearStaffForm()
        {
            SelectedStaff = null;
            IsStaffAddMode = false;
            StaffUsername = string.Empty;
            StaffPassword = string.Empty;
            StaffName = string.Empty;
            StaffPhone = string.Empty;
            StaffRole = "พนักงานทั่วไป";
            StaffValidationMessage = string.Empty;
        }
    }
}
