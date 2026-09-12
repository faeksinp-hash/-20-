using Dapper;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Porjai20.Models;
using Porjai20.Services;
using Porjai20.Views;

namespace Porjai20.ViewModels
{
    /// <summary>
    /// Partial ViewModel class for Customer Management in ProductViewModel.
    /// Extends ProductViewModel with inline validation properties and modal handlers.
    /// </summary>
    public partial class ProductViewModel
    {
        private string _customerModalTitle = "⚙️ แก้ไข/จัดการข้อมูลลูกค้า";
        public string CustomerModalTitle
        {
            get => _customerModalTitle;
            set => SetProperty(ref _customerModalTitle, value);
        }

        private string _modalTitle = "⚙️ แก้ไข/จัดการข้อมูลลูกค้า";
        public string ModalTitle
        {
            get => _modalTitle;
            set => SetProperty(ref _modalTitle, value);
        }

        private string _customerValidationMessage = string.Empty;
        public string CustomerValidationMessage
        {
            get => _customerValidationMessage;
            set
            {
                if (SetProperty(ref _customerValidationMessage, value))
                {
                    OnPropertyChanged(nameof(HasCustomerValidationMessage));
                }
            }
        }

        public bool HasCustomerValidationMessage => !string.IsNullOrWhiteSpace(CustomerValidationMessage);

        public void InitializeCustomerViewModelCommands()
        {
            OpenManageModalCommand = new RelayCommand(_ => ExecuteOpenManageModal());
            OpenCustomerManageModalCommand = OpenManageModalCommand;
            
            OpenAddCustomerModalCommand = new RelayCommand(_ => ExecuteOpenAddModal());
            
            SaveCustomerCommand = new RelayCommand(_ => ExecuteSaveCustomer());
            AddCustomerCommand = SaveCustomerCommand;
            UpdateCustomerCommand = SaveCustomerCommand;
            
            DeleteCustomerCommand = new RelayCommand(_ => ExecuteDeleteCustomer());
            
            CloseModalCommand = new RelayCommand(_ => ExecuteCloseModal());
            CloseCustomerModalCommand = CloseModalCommand;
            CloseEditCustomerModalCommand = CloseModalCommand;
            
            ClearCustomerCommand = new RelayCommand(_ => ClearCustomerForm());
            SearchCustomerCommand = new RelayCommand(_ => _ = LoadCustomers());
            ClearCustomerFilterCommand = new RelayCommand(_ =>
            {
                CustomerSearchKeyword = string.Empty;
                _ = LoadCustomers();
            });
        }

        /// <summary>
        /// Executes command when clicking '⚙️ จัดการข้อมูล' button.
        /// </summary>
        public void ExecuteOpenManageModal()
        {
            if (SelectedCustomer == null || SelectedCustomer.Id <= 0)
            {
                ShowAlert("กรุณาคลิกเลือกรายการลูกค้าในตารางก่อนดำเนินการ", "แจ้งเตือน", "⚠️");
                return;
            }

            IsEditMode = true;
            CustomerModalTitle = "⚙️ แก้ไข/จัดการข้อมูลลูกค้า";
            ModalTitle = CustomerModalTitle;
            CustomerValidationMessage = string.Empty;
            
            CustomerCode = SelectedCustomer.Code ?? string.Empty;
            CustomerName = SelectedCustomer.Name ?? string.Empty;
            CustomerPhone = SelectedCustomer.Phone ?? string.Empty;
            CustomerAddress = SelectedCustomer.Address ?? string.Empty;

            IsModalOpen = true;
            IsManageModalOpen = true;
            IsCustomerModalOpen = true;
        }

        /// <summary>
        /// Executes command when clicking '➕ เพิ่มข้อมูลลูกค้า' button.
        /// </summary>
        public void ExecuteOpenAddModal()
        {
            ClearCustomerForm();
            IsEditMode = false;
            CustomerModalTitle = "➕ เพิ่มข้อมูลลูกค้า";
            ModalTitle = CustomerModalTitle;
            CustomerCode = GenerateCustomerCode();
            CustomerValidationMessage = string.Empty;
            IsModalOpen = true;
            IsManageModalOpen = true;
            IsCustomerModalOpen = true;
        }

        /// <summary>
        /// Executes save/update operation for customer with inline validation.
        /// </summary>
        public void ExecuteSaveCustomer()
        {
            if (RolePermissions.IsReadOnly(CurrentUser?.Role, "customer"))
            {
                ShowAlert("ไม่มีสิทธิ์ในการดำเนินการนี้ (สิทธิ์ดูอย่างเดียว)", "ไม่มีสิทธิ์", "⚠️");
                return;
            }

            if (string.IsNullOrWhiteSpace(CustomerName))
            {
                CustomerValidationMessage = "กรุณากรอกชื่อ-นามสกุลของลูกค้า";
                return;
            }

            if (string.IsNullOrWhiteSpace(CustomerPhone))
            {
                CustomerValidationMessage = "กรุณากรอกเบอร์โทรศัพท์ของลูกค้า";
                return;
            }

            if (IsEditMode && SelectedCustomer != null && SelectedCustomer.Id > 0)
            {
                string codeToUse = !string.IsNullOrWhiteSpace(CustomerCode)
                    ? CustomerCode
                    : (!string.IsNullOrWhiteSpace(SelectedCustomer.Code) ? SelectedCustomer.Code : GenerateCustomerCode());

                SelectedCustomer.Code = codeToUse;
                SelectedCustomer.Name = CustomerName;
                SelectedCustomer.Phone = CustomerPhone;
                SelectedCustomer.Address = CustomerAddress;

                _databaseService.UpdateCustomer(SelectedCustomer);
                
                // Also update any delivery records for this customer
                try
                {
                    using (var conn = _databaseService.GetConnection())
                    {
                        conn.Execute("UPDATE tblDelivery SET Recipient_Name = @Name, Recipient_Tel = @Phone, Recipient_Address = @Address WHERE Sales_ID IN (SELECT Sales_ID FROM tblSales_H WHERE Cus_ID = @CusId)",
                            new { Name = SelectedCustomer.Name, Phone = SelectedCustomer.Phone, Address = SelectedCustomer.Address, CusId = SelectedCustomer.Id });
                    }
                }
                catch { }

                ClearCustomerForm();
                SetCustomerModalsClosed();
                _ = LoadCustomers();
                _ = LoadDeliveries();
                ShowAlert("อัปเดตข้อมูลลูกค้าเรียบร้อยแล้ว", "สำเร็จ", "🎉");
            }
            else
            {
                string generatedCode = string.IsNullOrWhiteSpace(CustomerCode) ? GenerateCustomerCode() : CustomerCode;

                var customer = new Customer
                {
                    Code = generatedCode,
                    Name = CustomerName,
                    Phone = CustomerPhone,
                    Address = CustomerAddress,
                    RegistrationDate = DateTime.Now
                };

                _databaseService.SaveCustomer(customer);

                ClearCustomerForm();
                SetCustomerModalsClosed();
                _ = LoadCustomers();
                ShowAlert("บันทึกข้อมูลลูกค้าเรียบร้อยแล้ว", "สำเร็จ", "🎉");
            }
        }

        /// <summary>
        /// Triggers deletion of customer record using universal confirmation popup.
        /// </summary>
        public void ExecuteDeleteCustomer()
        {
            if (RolePermissions.IsReadOnly(CurrentUser?.Role, "customer"))
            {
                ShowAlert("ไม่มีสิทธิ์ในการดำเนินการนี้ (สิทธิ์ดูอย่างเดียว)", "ไม่มีสิทธิ์", "⚠️");
                return;
            }

            if (SelectedCustomer == null || SelectedCustomer.Id <= 0)
            {
                CustomerValidationMessage = "กรุณาคลิกเลือกรายการลูกค้าในตารางก่อนดำเนินการ";
                return;
            }

            ShowConfirm(
                $"คุณต้องการลบข้อมูลลูกค้า '{SelectedCustomer.Name}' ใช่หรือไม่?\nประวัติการสั่งซื้อของลูกค้าจะไม่ถูกลบ",
                () =>
                {
                    _databaseService.DeleteCustomer(SelectedCustomer.Id);
                    SelectedCustomer = new Customer();
                    ClearCustomerForm();
                    SetCustomerModalsClosed();
                    _ = LoadCustomers();
                    ShowAlert("ลบข้อมูลลูกค้าเรียบร้อยแล้ว", "สำเร็จ", "🗑️");
                },
                "ยืนยันการลบข้อมูลลูกค้า");
        }

        /// <summary>
        /// Resets form inputs and closes modal.
        /// </summary>
        public void ExecuteCloseModal()
        {
            ClearCustomerForm();
            SetCustomerModalsClosed();
        }

        private void SetCustomerModalsClosed()
        {
            IsModalOpen = false;
            IsManageModalOpen = false;
            IsCustomerModalOpen = false;
            CustomerValidationMessage = string.Empty;
        }
    }

    /// <summary>
    /// Standalone ViewModel class for Customer Management, inheriting from ViewModelBase.
    /// Provides full compatibility if CustomerView is bound directly to CustomerViewModel.
    /// </summary>
    public class CustomerViewModel : ViewModelBase
    {
        private readonly DatabaseService _databaseService;

        public ObservableCollection<Customer> Customers { get; } = new ObservableCollection<Customer>();

        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value) && value != null)
                {
                    CustomerCode = value.Code ?? string.Empty;
                    CustomerName = value.Name ?? string.Empty;
                    CustomerPhone = value.Phone ?? string.Empty;
                    CustomerAddress = value.Address ?? string.Empty;
                }
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        private string _customerModalTitle = "⚙️ แก้ไข/จัดการข้อมูลลูกค้า";
        public string CustomerModalTitle
        {
            get => _customerModalTitle;
            set => SetProperty(ref _customerModalTitle, value);
        }

        private string _modalTitle = "⚙️ แก้ไข/จัดการข้อมูลลูกค้า";
        public string ModalTitle
        {
            get => _modalTitle;
            set => SetProperty(ref _modalTitle, value);
        }

        private bool _isModalOpen;
        public bool IsModalOpen
        {
            get => _isModalOpen;
            set => SetProperty(ref _isModalOpen, value);
        }

        public bool IsCustomerModalOpen
        {
            get => IsModalOpen;
            set => IsModalOpen = value;
        }

        private string _customerValidationMessage = string.Empty;
        public string CustomerValidationMessage
        {
            get => _customerValidationMessage;
            set
            {
                if (SetProperty(ref _customerValidationMessage, value))
                {
                    OnPropertyChanged(nameof(HasCustomerValidationMessage));
                }
            }
        }

        public bool HasCustomerValidationMessage => !string.IsNullOrWhiteSpace(CustomerValidationMessage);

        private string _customerCode = string.Empty;
        public string CustomerCode
        {
            get => _customerCode;
            set => SetProperty(ref _customerCode, value);
        }

        private string _customerName = string.Empty;
        public string CustomerName
        {
            get => _customerName;
            set => SetProperty(ref _customerName, value);
        }

        private string _customerPhone = string.Empty;
        public string CustomerPhone
        {
            get => _customerPhone;
            set => SetProperty(ref _customerPhone, value);
        }

        private string _customerAddress = string.Empty;
        public string CustomerAddress
        {
            get => _customerAddress;
            set => SetProperty(ref _customerAddress, value);
        }

        private string _customerSearchKeyword = string.Empty;
        public string CustomerSearchKeyword
        {
            get => _customerSearchKeyword;
            set
            {
                if (SetProperty(ref _customerSearchKeyword, value))
                {
                    OnPropertyChanged(nameof(SearchText));
                    LoadCustomers();
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
                    _customerSearchKeyword = value;
                    OnPropertyChanged(nameof(CustomerSearchKeyword));
                    LoadCustomers();
                }
            }
        }

        public ICommand OpenManageModalCommand { get; }
        public ICommand OpenCustomerManageModalCommand => OpenManageModalCommand;
        public ICommand OpenAddCustomerModalCommand { get; }
        public ICommand SaveCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }
        public ICommand CloseModalCommand { get; }
        public ICommand CloseCustomerModalCommand => CloseModalCommand;
        public ICommand ClearCustomerCommand { get; }
        public ICommand SearchCustomerCommand { get; }
        public ICommand ClearCustomerFilterCommand { get; }

        public CustomerViewModel()
        {
            _databaseService = new DatabaseService();

            OpenManageModalCommand = new RelayCommand(_ => ExecuteOpenManageModal());
            OpenAddCustomerModalCommand = new RelayCommand(_ => ExecuteOpenAddModal());
            SaveCustomerCommand = new RelayCommand(_ => ExecuteSaveCustomer());
            DeleteCustomerCommand = new RelayCommand(_ => ExecuteDeleteCustomer());
            CloseModalCommand = new RelayCommand(_ => ExecuteCloseModal());
            ClearCustomerCommand = new RelayCommand(_ => ClearCustomerForm());
            SearchCustomerCommand = new RelayCommand(_ => LoadCustomers());
            ClearCustomerFilterCommand = new RelayCommand(_ =>
            {
                SearchText = string.Empty;
                LoadCustomers();
            });

            LoadCustomers();
        }

        public void LoadCustomers()
        {
            Customers.Clear();
            var list = _databaseService.GetCustomers();
            var kw = (SearchText ?? CustomerSearchKeyword ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(kw))
            {
                list = list.Where(c =>
                    (c.Code != null && c.Code.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                    (c.Name != null && c.Name.Contains(kw, StringComparison.OrdinalIgnoreCase)));
            }

            foreach (var c in list)
            {
                Customers.Add(c);
            }
        }

        public void ExecuteOpenManageModal()
        {
            if (SelectedCustomer == null || SelectedCustomer.Id <= 0)
            {
                return;
            }

            IsEditMode = true;
            CustomerModalTitle = "⚙️ แก้ไข/จัดการข้อมูลลูกค้า";
            ModalTitle = CustomerModalTitle;
            CustomerValidationMessage = string.Empty;

            CustomerCode = SelectedCustomer.Code ?? string.Empty;
            CustomerName = SelectedCustomer.Name ?? string.Empty;
            CustomerPhone = SelectedCustomer.Phone ?? string.Empty;
            CustomerAddress = SelectedCustomer.Address ?? string.Empty;

            IsModalOpen = true;
        }

        private string GenerateCustomerCode()
        {
            try
            {
                var list = _databaseService.GetCustomers();
                int nextId = (list != null && list.Any()) ? list.Select(c => c.Id).DefaultIfEmpty(0).Max() + 1 : 1;
                return $"CUS-{nextId:D4}";
            }
            catch
            {
                return $"CUS-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
            }
        }

        public void ExecuteOpenAddModal()
        {
            ClearCustomerForm();
            IsEditMode = false;
            CustomerModalTitle = "➕ เพิ่มข้อมูลลูกค้า";
            ModalTitle = CustomerModalTitle;
            CustomerValidationMessage = string.Empty;
            CustomerCode = GenerateCustomerCode();
            IsModalOpen = true;
        }

        public void ExecuteSaveCustomer()
        {
            if (string.IsNullOrWhiteSpace(CustomerName))
            {
                CustomerValidationMessage = "กรุณากรอกชื่อ-นามสกุลของลูกค้า";
                return;
            }

            if (string.IsNullOrWhiteSpace(CustomerPhone))
            {
                CustomerValidationMessage = "กรุณากรอกเบอร์โทรศัพท์ของลูกค้า";
                return;
            }

            if (IsEditMode && SelectedCustomer != null && SelectedCustomer.Id > 0)
            {
                SelectedCustomer.Code = !string.IsNullOrWhiteSpace(SelectedCustomer.Code)
                    ? SelectedCustomer.Code
                    : (!string.IsNullOrWhiteSpace(CustomerCode) ? CustomerCode : GenerateCustomerCode());

                SelectedCustomer.Name = CustomerName;
                SelectedCustomer.Phone = CustomerPhone;
                SelectedCustomer.Address = CustomerAddress;

                _databaseService.UpdateCustomer(SelectedCustomer);
                ClearCustomerForm();
                IsModalOpen = false;
                LoadCustomers();
            }
            else
            {
                string codeToUse = !string.IsNullOrWhiteSpace(CustomerCode) ? CustomerCode : GenerateCustomerCode();

                var customer = new Customer
                {
                    Code = codeToUse,
                    Name = CustomerName,
                    Phone = CustomerPhone,
                    Address = CustomerAddress,
                    RegistrationDate = DateTime.Now
                };

                _databaseService.SaveCustomer(customer);
                ClearCustomerForm();
                IsModalOpen = false;
                LoadCustomers();
            }
        }

        public void ExecuteDeleteCustomer()
        {
            if (SelectedCustomer == null || SelectedCustomer.Id <= 0)
            {
                CustomerValidationMessage = "กรุณาคลิกเลือกรายการลูกค้าในตารางก่อนดำเนินการ";
                return;
            }

            _databaseService.DeleteCustomer(SelectedCustomer.Id);
            ClearCustomerForm();
            IsModalOpen = false;
            LoadCustomers();
        }

        public void ExecuteCloseModal()
        {
            ClearCustomerForm();
            IsModalOpen = false;
        }

        public void ClearCustomerForm()
        {
            CustomerCode = GenerateCustomerCode();
            CustomerName = string.Empty;
            CustomerPhone = string.Empty;
            CustomerAddress = string.Empty;
            CustomerValidationMessage = string.Empty;
        }
    }
}
