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
        private string _originalCustomerName = string.Empty;
        private string _originalCustomerPhone = string.Empty;
        private string _originalCustomerAddress = string.Empty;
        private bool _isCustomerAddMode;

        public bool IsCustomerAddMode
        {
            get => _isCustomerAddMode;
            set => SetProperty(ref _isCustomerAddMode, value);
        }

        private string _customerModalTitle = "ข้อมูลลูกค้า";
        public string CustomerModalTitle
        {
            get => _customerModalTitle;
            set => SetProperty(ref _customerModalTitle, value);
        }

        private string _modalTitle = "ข้อมูลลูกค้า";
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

        public ICommand EnterCustomerEditModeCommand { get; set; }
        public ICommand SwitchToCustomerEditModeCommand => EnterCustomerEditModeCommand;
        public ICommand CancelCustomerEditCommand { get; set; }

        public void InitializeCustomerViewModelCommands()
        {
            OpenManageModalCommand = new RelayCommand(_ => ExecuteOpenManageModal());
            OpenCustomerManageModalCommand = OpenManageModalCommand;
            
            OpenAddCustomerModalCommand = new RelayCommand(_ => ExecuteOpenAddModal());
            
            EnterCustomerEditModeCommand = new RelayCommand(_ => ExecuteEnterCustomerEditMode());
            CancelCustomerEditCommand = new RelayCommand(_ => ExecuteCancelCustomerEdit());

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
        /// Executes command when double-clicking customer row or clicking 'จัดการข้อมูล'.
        /// Opens modal in Read-Only View mode (IsEditMode = false) initially.
        /// </summary>
        public void ExecuteOpenManageModal()
        {
            if (SelectedCustomer == null || SelectedCustomer.Id <= 0)
            {
                ShowAlert("กรุณาคลิกเลือกรายการลูกค้าในตารางก่อนดำเนินการ", "แจ้งเตือน", "⚠️");
                return;
            }

            IsCustomerAddMode = false;
            IsEditMode = false; // เริ่มต้นในสถานะดูข้อมูลอย่างเดียว
            CustomerModalTitle = "ข้อมูลลูกค้า";
            ModalTitle = CustomerModalTitle;
            CustomerValidationMessage = string.Empty;
            
            CustomerCode = SelectedCustomer.Code ?? string.Empty;
            CustomerName = SelectedCustomer.Name ?? string.Empty;
            CustomerPhone = SelectedCustomer.Phone ?? string.Empty;
            CustomerAddress = SelectedCustomer.Address ?? string.Empty;

            // สำรองค่าเดิมไว้สำหรับกดยกเลิก
            _originalCustomerName = CustomerName;
            _originalCustomerPhone = CustomerPhone;
            _originalCustomerAddress = CustomerAddress;

            IsModalOpen = true;
            IsManageModalOpen = true;
            IsCustomerModalOpen = true;
        }

        /// <summary>
        /// Switches customer modal to Edit Mode (IsEditMode = true).
        /// </summary>
        public void ExecuteEnterCustomerEditMode()
        {
            if (RolePermissions.IsReadOnly(CurrentUser?.Role, "customer"))
            {
                ShowAlert("ไม่มีสิทธิ์ในการดำเนินการนี้ (สิทธิ์ดูอย่างเดียว)", "ไม่มีสิทธิ์", "⚠️");
                return;
            }

            _originalCustomerName = CustomerName ?? string.Empty;
            _originalCustomerPhone = CustomerPhone ?? string.Empty;
            _originalCustomerAddress = CustomerAddress ?? string.Empty;

            IsEditMode = true;
            CustomerModalTitle = "แก้ไขข้อมูลลูกค้า";
            ModalTitle = CustomerModalTitle;
            CustomerValidationMessage = string.Empty;
        }

        /// <summary>
        /// Cancels customer edit mode: restores original values and closes the modal immediately,
        /// leaving the Flyout Drawer open.
        /// </summary>
        public void ExecuteCancelCustomerEdit()
        {
            // คืนค่าข้อมูลในช่องกรอกกลับเป็นค่าเดิมก่อนแก้ไข
            CustomerName = _originalCustomerName;
            CustomerPhone = _originalCustomerPhone;
            CustomerAddress = _originalCustomerAddress;
            CustomerValidationMessage = string.Empty;

            // ปิดหน้าต่าง Modal ทันที และคงสถานะของแผงรายละเอียดลูกค้าฝั่งขวา (Drawer) ไว้ตามเดิม
            SetCustomerModalsClosed();
        }

        /// <summary>
        /// Executes command when clicking '➕ เพิ่มข้อมูลลูกค้า' button.
        /// </summary>
        public void ExecuteOpenAddModal()
        {
            ClearCustomerForm();
            IsCustomerAddMode = true;
            IsEditMode = true; // โหมดเพิ่มข้อมูลปลดล็อกให้พิมพ์ได้
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

            if (!IsCustomerAddMode && SelectedCustomer != null && SelectedCustomer.Id > 0)
            {
                string codeToUse = !string.IsNullOrWhiteSpace(CustomerCode)
                    ? CustomerCode
                    : (!string.IsNullOrWhiteSpace(SelectedCustomer.Code) ? SelectedCustomer.Code : GenerateCustomerCode());

                SelectedCustomer.Code = codeToUse;
                SelectedCustomer.Name = CustomerName;
                SelectedCustomer.Phone = CustomerPhone;
                SelectedCustomer.Address = CustomerAddress;

                _databaseService.UpdateCustomer(SelectedCustomer);

                // อัปเดตข้อมูลบนแผงรายละเอียดฝั่งขวา (Flyout Drawer) ให้เปลี่ยนเป็นข้อมูลใหม่ทันที
                if (DrawerCustomer != null && (DrawerCustomer.Id == SelectedCustomer.Id || (!string.IsNullOrEmpty(DrawerCustomer.Code) && DrawerCustomer.Code == SelectedCustomer.Code)))
                {
                    DrawerCustomer.Code = SelectedCustomer.Code;
                    DrawerCustomer.Name = SelectedCustomer.Name;
                    DrawerCustomer.Phone = SelectedCustomer.Phone;
                    DrawerCustomer.Address = SelectedCustomer.Address;
                    OnPropertyChanged(nameof(DrawerCustomer));
                    OnPropertyChanged(nameof(DrawerCustomerInitials));
                    OnPropertyChanged(nameof(DrawerCustomerSubInfo));
                }

                ClearCustomerForm();
                SetCustomerModalsClosed();
                _ = LoadCustomers();
                _ = LoadDeliveries();
                ShowAlert("บันทึกข้อมูลเรียบร้อยแล้ว", "สำเร็จ", "🎉");
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
                ShowAlert("บันทึกข้อมูลเรียบร้อยแล้ว", "สำเร็จ", "🎉");
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
                    DrawerCustomer = null;
                    IsCustomerDrawerOpen = false;
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
            IsCustomerAddMode = false;
            IsEditMode = false;
            CustomerValidationMessage = string.Empty;
        }

        // ── Drawer Properties (Feature 2: Customer History & Points Drawer) ──
        private bool _isCustomerDrawerOpen;
        public bool IsCustomerDrawerOpen
        {
            get => _isCustomerDrawerOpen;
            set => SetProperty(ref _isCustomerDrawerOpen, value);
        }

        private Customer? _drawerCustomer;
        public Customer? DrawerCustomer
        {
            get => _drawerCustomer;
            set
            {
                if (SetProperty(ref _drawerCustomer, value))
                {
                    OnPropertyChanged(nameof(DrawerCustomerInitials));
                    OnPropertyChanged(nameof(DrawerCustomerSubInfo));
                }
            }
        }

        public string DrawerCustomerInitials
        {
            get
            {
                if (DrawerCustomer == null || string.IsNullOrWhiteSpace(DrawerCustomer.Name)) return "ล";
                return DrawerCustomer.Name.Trim().Substring(0, 1);
            }
        }

        public string DrawerCustomerSubInfo
        {
            get
            {
                if (DrawerCustomer == null) return "-";
                string code = !string.IsNullOrWhiteSpace(DrawerCustomer.Code) ? DrawerCustomer.Code : $"CUS-{DrawerCustomer.Id:D4}";
                string dateStr = Common.DateTimeHelper.ToBuddhistEra(DrawerCustomer.RegistrationDate, "dd/MM/yyyy");
                string phone = !string.IsNullOrWhiteSpace(DrawerCustomer.Phone) ? DrawerCustomer.Phone : "-";
                return $"{code} · {dateStr} · {phone}";
            }
        }

        // Points Card
        private int _drawerPointsBalance;
        public int DrawerPointsBalance
        {
            get => _drawerPointsBalance;
            set => SetProperty(ref _drawerPointsBalance, value);
        }

        private int _drawerEarnedPointsTotal;
        public int DrawerEarnedPointsTotal
        {
            get => _drawerEarnedPointsTotal;
            set => SetProperty(ref _drawerEarnedPointsTotal, value);
        }

        private int _drawerRedeemedPointsTotal;
        public int DrawerRedeemedPointsTotal
        {
            get => _drawerRedeemedPointsTotal;
            set => SetProperty(ref _drawerRedeemedPointsTotal, value);
        }

        private string _totalEarnedPointsText = "0 แต้ม";
        public string TotalEarnedPointsText
        {
            get => _totalEarnedPointsText;
            set => SetProperty(ref _totalEarnedPointsText, value);
        }

        private string _totalUsedPointsText = "0 แต้ม";
        public string TotalUsedPointsText
        {
            get => _totalUsedPointsText;
            set => SetProperty(ref _totalUsedPointsText, value);
        }

        public string DrawerEarnedPointsTotalText => TotalEarnedPointsText;
        public string DrawerRedeemedPointsTotalText => TotalUsedPointsText;
        public int TotalEarnedPoints => DrawerEarnedPointsTotal;
        public int TotalUsedPoints => DrawerRedeemedPointsTotal;

        public ObservableCollection<string> DrawerActivePromotions { get; } = new ObservableCollection<string>();
        public bool HasActivePromotions => DrawerActivePromotions.Count > 0;

        // Summary Metric Box
        private string _drawerTotalBills = "0 บิล";
        public string DrawerTotalBills
        {
            get => _drawerTotalBills;
            set => SetProperty(ref _drawerTotalBills, value);
        }

        private string _drawerTotalSpent = "0.00 บาท";
        public string DrawerTotalSpent
        {
            get => _drawerTotalSpent;
            set => SetProperty(ref _drawerTotalSpent, value);
        }

        private string _drawerLatestPurchase = "-";
        public string DrawerLatestPurchase
        {
            get => _drawerLatestPurchase;
            set => SetProperty(ref _drawerLatestPurchase, value);
        }

        // Purchase History
        public ObservableCollection<CustomerPurchaseHistoryDto> DrawerPurchases { get; } = new ObservableCollection<CustomerPurchaseHistoryDto>();
        public bool HasDrawerPurchases => DrawerPurchases.Count > 0;

        private string _drawerPurchaseFilter = "all"; // "all", "1month", "3months"
        public string DrawerPurchaseFilter
        {
            get => _drawerPurchaseFilter;
            set
            {
                if (SetProperty(ref _drawerPurchaseFilter, value))
                {
                    OnPropertyChanged(nameof(IsFilterAll));
                    OnPropertyChanged(nameof(IsFilter30Days));
                    OnPropertyChanged(nameof(IsFilter1Month));
                    OnPropertyChanged(nameof(IsFilter3Months));
                }
            }
        }

        public bool IsFilterAll => DrawerPurchaseFilter == "all";
        public bool IsFilter30Days => DrawerPurchaseFilter == "30days" || DrawerPurchaseFilter == "1month";
        public bool IsFilter1Month => DrawerPurchaseFilter == "30days" || DrawerPurchaseFilter == "1month";
        public bool IsFilter3Months => DrawerPurchaseFilter == "3months";

        private int _drawerCurrentPage = 1;
        public int DrawerCurrentPage
        {
            get => _drawerCurrentPage;
            set
            {
                if (SetProperty(ref _drawerCurrentPage, value))
                {
                    UpdateDrawerPaginationState();
                }
            }
        }

        private int _drawerTotalCount = 0;
        public int DrawerTotalCount
        {
            get => _drawerTotalCount;
            set
            {
                if (SetProperty(ref _drawerTotalCount, value))
                {
                    UpdateDrawerPaginationState();
                }
            }
        }

        private int _drawerTotalPages = 1;
        public int DrawerTotalPages
        {
            get => _drawerTotalPages;
            set
            {
                if (SetProperty(ref _drawerTotalPages, value))
                {
                    UpdateDrawerPaginationState();
                }
            }
        }

        private string _drawerPaginationInfo = "แสดง 0 จาก 0 บิล";
        public string DrawerPaginationInfo
        {
            get => _drawerPaginationInfo;
            set => SetProperty(ref _drawerPaginationInfo, value);
        }

        private bool _canPrevPage;
        public bool CanPrevPage
        {
            get => _canPrevPage;
            set => SetProperty(ref _canPrevPage, value);
        }

        private bool _canNextPage;
        public bool CanNextPage
        {
            get => _canNextPage;
            set => SetProperty(ref _canNextPage, value);
        }

        // Drawer Commands
        private ICommand? _openCustomerDrawerCommand;
        public ICommand OpenCustomerDrawerCommand => _openCustomerDrawerCommand ??= new RelayCommand(p => ExecuteOpenCustomerDrawer(p as Customer));
        public ICommand OpenCustomerDetailsCommand => OpenCustomerDrawerCommand;

        private ICommand? _closeCustomerDrawerCommand;
        public ICommand CloseCustomerDrawerCommand => _closeCustomerDrawerCommand ??= new RelayCommand(_ => ExecuteCloseCustomerDrawer());

        private ICommand? _setPurchaseFilterCommand;
        public ICommand SetPurchaseFilterCommand => _setPurchaseFilterCommand ??= new RelayCommand(p => ExecuteSetPurchaseFilter(p?.ToString() ?? "all"));

        private ICommand? _prevPurchasePageCommand;
        public ICommand PrevPurchasePageCommand => _prevPurchasePageCommand ??= new RelayCommand(_ => ExecutePrevPurchasePage());

        private ICommand? _nextPurchasePageCommand;
        public ICommand NextPurchasePageCommand => _nextPurchasePageCommand ??= new RelayCommand(_ => ExecuteNextPurchasePage());

        private ICommand? _viewAllPurchasesCommand;
        public ICommand ViewAllPurchasesCommand => _viewAllPurchasesCommand ??= new RelayCommand(_ => ExecuteViewAllPurchases());

        private ICommand? _openReceiptDetailCommand;
        public ICommand OpenReceiptDetailCommand => _openReceiptDetailCommand ??= new RelayCommand(p => ExecuteOpenReceiptDetail(p));

        private ICommand? _openEditCustomerFromDrawerCommand;
        public ICommand OpenEditCustomerFromDrawerCommand => _openEditCustomerFromDrawerCommand ??= new RelayCommand(_ => ExecuteOpenEditCustomerFromDrawer());

        public void ExecuteOpenCustomerDrawer(Customer? customer = null)
        {
            var target = customer ?? SelectedCustomer;
            if (target == null || target.Id <= 0) return;

            SelectedCustomer = target;
            DrawerCustomer = target;
            DrawerPurchaseFilter = "all";
            DrawerCurrentPage = 1;

            // รีเซ็ตค่าเริ่มต้นของแต้มป้องกันจังหวะที่เป็นค่าว่าง
            TotalEarnedPointsText = "0 แต้ม";
            TotalUsedPointsText = "0 แต้ม";
            OnPropertyChanged(nameof(TotalEarnedPointsText));
            OnPropertyChanged(nameof(TotalUsedPointsText));

            LoadDrawerData(target);
            IsCustomerDrawerOpen = true;
        }

        public void ExecuteCloseCustomerDrawer()
        {
            IsCustomerDrawerOpen = false;
        }

        public void ExecuteSetPurchaseFilter(string period)
        {
            DrawerPurchaseFilter = period;
            DrawerCurrentPage = 1;
            LoadDrawerPurchasesPage();
        }

        public void ExecutePrevPurchasePage()
        {
            if (DrawerCurrentPage > 1)
            {
                DrawerCurrentPage--;
                LoadDrawerPurchasesPage();
            }
        }

        public void ExecuteNextPurchasePage()
        {
            if (DrawerCurrentPage < DrawerTotalPages)
            {
                DrawerCurrentPage++;
                LoadDrawerPurchasesPage();
            }
        }

        public void ExecuteViewAllPurchases()
        {
            DrawerPurchaseFilter = "all";
            DrawerCurrentPage = 1;
            LoadDrawerPurchasesPage();
        }

        public void ExecuteOpenReceiptDetail(object? param)
        {
            int salesId = 0;
            string refNo = "";

            if (param is CustomerPurchaseHistoryDto dto)
            {
                salesId = dto.SalesId;
                refNo = dto.RefNo;
            }
            else if (param is int id)
            {
                salesId = id;
            }
            else if (param is string s)
            {
                refNo = s;
            }

            SalesOrder? order = null;
            if (salesId > 0)
            {
                order = _databaseService.GetSalesOrderById(salesId);
            }
            else if (!string.IsNullOrWhiteSpace(refNo))
            {
                order = _databaseService.GetSalesOrderByRef(refNo);
            }

            if (order == null)
            {
                Views.CustomMessageBox.Show("ไม่พบข้อมูลใบเสร็จที่ต้องการ", "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var items = _databaseService.GetSalesOrderItems(order.Sales_ID);
            var dialog = new ReceiptDetailDialog(order, items)
            {
                Owner = Application.Current?.MainWindow
            };
            dialog.ShowDialog();
        }

        public void ExecuteOpenEditCustomerFromDrawer()
        {
            if (DrawerCustomer == null) return;
            SelectedCustomer = DrawerCustomer;

            CustomerCode = DrawerCustomer.Code ?? string.Empty;
            CustomerName = DrawerCustomer.Name ?? string.Empty;
            CustomerPhone = DrawerCustomer.Phone ?? string.Empty;
            CustomerAddress = DrawerCustomer.Address ?? string.Empty;

            _originalCustomerName = CustomerName;
            _originalCustomerPhone = CustomerPhone;
            _originalCustomerAddress = CustomerAddress;

            IsCustomerAddMode = false;
            IsEditMode = true;
            CustomerModalTitle = "แก้ไขข้อมูลลูกค้า";
            ModalTitle = CustomerModalTitle;
            CustomerValidationMessage = string.Empty;

            // คงสถานะแผงฝั่งขวาไว้ (ไม่ปิดแผงขวา) และเปิด Modal โหมดแก้ไขโดยตรงทันที
            IsModalOpen = true;
            IsManageModalOpen = true;
            IsCustomerModalOpen = true;
        }

        public void LoadDrawerData(Customer customer)
        {
            try
            {
                string cusCode = customer.Code ?? string.Empty;
                string cusIdStr = customer.Id > 0 ? customer.Id.ToString() : string.Empty;
                var summary = PointsService.Instance.GetPointsSummary(cusCode, _databaseService.GetConnection(), null, cusIdStr);

                // ป้องกันการเขียนทับด้วยค่า 0 หากใน tblCustomer มีแต้มเดิมอยู่
                int customerPoints = Math.Max(customer.Points, customer.TotalPoints);
                int effectiveBalance = Math.Max(summary.Balance, customerPoints);
                int effectiveEarned = Math.Max(summary.TotalEarned, effectiveBalance);
                if (effectiveEarned == 0 && customerPoints > 0)
                {
                    effectiveEarned = customerPoints;
                }
                int usedPoints = summary.TotalRedeemed;

                DrawerPointsBalance = effectiveBalance;
                DrawerEarnedPointsTotal = effectiveEarned;
                DrawerRedeemedPointsTotal = usedPoints;

                // จัดรูปแบบข้อความแต้มให้ลงท้ายด้วย 'แต้ม' เสมอ
                TotalEarnedPointsText = effectiveEarned > 0 ? $"+{effectiveEarned} แต้ม" : "0 แต้ม";
                TotalUsedPointsText = usedPoints > 0 ? $"-{usedPoints} แต้ม" : "0 แต้ม";

                // อัปเดตค่าแต้มของลูกค้าให้ตรงกัน ป้องกันแถวในตารางหลักกลายเป็น 0
                customer.Points = effectiveBalance;
                OnPropertyChanged(nameof(DrawerPointsBalance));
                OnPropertyChanged(nameof(DrawerEarnedPointsTotal));
                OnPropertyChanged(nameof(DrawerRedeemedPointsTotal));
                OnPropertyChanged(nameof(TotalEarnedPointsText));
                OnPropertyChanged(nameof(TotalUsedPointsText));
                OnPropertyChanged(nameof(TotalEarnedPoints));
                OnPropertyChanged(nameof(TotalUsedPoints));
                OnPropertyChanged(nameof(DrawerEarnedPointsTotalText));
                OnPropertyChanged(nameof(DrawerRedeemedPointsTotalText));
                OnPropertyChanged(nameof(DrawerCustomer));

                DrawerActivePromotions.Clear();
                var promos = _databaseService.GetActivePointRedeemPromotions();
                if (promos != null && promos.Any())
                {
                    foreach (var p in promos)
                    {
                        DrawerActivePromotions.Add($"• {p.PromoID} (ใช้ {p.PointsRequired} แต้ม ลด {p.DiscountAmount:N0} บาท)");
                    }
                }
                OnPropertyChanged(nameof(HasActivePromotions));

                var purchaseSummary = _databaseService.GetCustomerPurchaseSummary(customer.Id);
                DrawerTotalBills = $"{purchaseSummary.TotalBills} บิล";
                DrawerTotalSpent = $"{purchaseSummary.TotalSpent:N2} บาท";
                DrawerLatestPurchase = purchaseSummary.LatestPurchaseDate;

                // รีเซ็ตตัวกรองเป็น 'all' และหน้า 1 เสมอ
                DrawerPurchaseFilter = "all";
                DrawerCurrentPage = 1;

                LoadDrawerPurchasesPage();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadDrawerData error: {ex.Message}");
            }
        }

        public void LoadDrawerPurchasesPage()
        {
            if (DrawerCustomer == null) return;

            try
            {
                var result = _databaseService.GetCustomerPurchaseHistory(DrawerCustomer.Id, DrawerPurchaseFilter, DrawerCurrentPage, 5);
                DrawerPurchases.Clear();
                foreach (var item in result.Items)
                {
                    DrawerPurchases.Add(item);
                }
                OnPropertyChanged(nameof(HasDrawerPurchases));

                DrawerTotalCount = result.TotalCount;
                DrawerTotalPages = Math.Max(1, (int)Math.Ceiling((double)DrawerTotalCount / 5));

                UpdateDrawerPaginationState();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadDrawerPurchasesPage error: {ex.Message}");
            }
        }

        private void UpdateDrawerPaginationState()
        {
            if (DrawerTotalCount == 0)
            {
                DrawerPaginationInfo = "แสดง 0 จาก 0 บิล";
                CanPrevPage = false;
                CanNextPage = false;
            }
            else
            {
                int start = Math.Min(DrawerTotalCount, (DrawerCurrentPage - 1) * 5 + 1);
                int end = Math.Min(DrawerTotalCount, DrawerCurrentPage * 5);
                DrawerPaginationInfo = $"แสดง {start}-{end} จาก {DrawerTotalCount} บิล";
                CanPrevPage = DrawerCurrentPage > 1;
                CanNextPage = DrawerCurrentPage < DrawerTotalPages;
            }
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
        public ObservableCollection<Customer> FilteredCustomers => Customers;

        private string _originalCustomerName = string.Empty;
        private string _originalCustomerPhone = string.Empty;
        private string _originalCustomerAddress = string.Empty;

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

        private bool _isCustomerAddMode;
        public bool IsCustomerAddMode
        {
            get => _isCustomerAddMode;
            set => SetProperty(ref _isCustomerAddMode, value);
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        private string _customerModalTitle = "ข้อมูลลูกค้า";
        public string CustomerModalTitle
        {
            get => _customerModalTitle;
            set => SetProperty(ref _customerModalTitle, value);
        }

        private string _modalTitle = "ข้อมูลลูกค้า";
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
        public ICommand EnterCustomerEditModeCommand { get; }
        public ICommand SwitchToCustomerEditModeCommand => EnterCustomerEditModeCommand;
        public ICommand CancelCustomerEditCommand { get; }
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
            EnterCustomerEditModeCommand = new RelayCommand(_ => ExecuteEnterCustomerEditMode());
            CancelCustomerEditCommand = new RelayCommand(_ => ExecuteCancelCustomerEdit());
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

            IsCustomerAddMode = false;
            IsEditMode = false; // เปิดในสถานะดูข้อมูลอย่างเดียว
            CustomerModalTitle = "ข้อมูลลูกค้า";
            ModalTitle = CustomerModalTitle;
            CustomerValidationMessage = string.Empty;

            CustomerCode = SelectedCustomer.Code ?? string.Empty;
            CustomerName = SelectedCustomer.Name ?? string.Empty;
            CustomerPhone = SelectedCustomer.Phone ?? string.Empty;
            CustomerAddress = SelectedCustomer.Address ?? string.Empty;

            _originalCustomerName = CustomerName;
            _originalCustomerPhone = CustomerPhone;
            _originalCustomerAddress = CustomerAddress;

            IsModalOpen = true;
        }

        public void ExecuteEnterCustomerEditMode()
        {
            _originalCustomerName = CustomerName ?? string.Empty;
            _originalCustomerPhone = CustomerPhone ?? string.Empty;
            _originalCustomerAddress = CustomerAddress ?? string.Empty;

            IsEditMode = true;
            CustomerModalTitle = "แก้ไขข้อมูลลูกค้า";
            ModalTitle = CustomerModalTitle;
            CustomerValidationMessage = string.Empty;
        }

        public void ExecuteCancelCustomerEdit()
        {
            CustomerName = _originalCustomerName;
            CustomerPhone = _originalCustomerPhone;
            CustomerAddress = _originalCustomerAddress;
            CustomerValidationMessage = string.Empty;

            IsModalOpen = false;
            IsEditMode = false;
            IsCustomerAddMode = false;
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
            IsCustomerAddMode = true;
            IsEditMode = true;
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

            if (!IsCustomerAddMode && SelectedCustomer != null && SelectedCustomer.Id > 0)
            {
                SelectedCustomer.Code = !string.IsNullOrWhiteSpace(SelectedCustomer.Code)
                    ? SelectedCustomer.Code
                    : (!string.IsNullOrWhiteSpace(CustomerCode) ? CustomerCode : GenerateCustomerCode());

                SelectedCustomer.Name = CustomerName;
                SelectedCustomer.Phone = CustomerPhone;
                SelectedCustomer.Address = CustomerAddress;

                _databaseService.UpdateCustomer(SelectedCustomer);

                // อัปเดตข้อมูลบนแผงรายละเอียดฝั่งขวา (Flyout Drawer) ให้เปลี่ยนเป็นข้อมูลใหม่ทันที
                if (DrawerCustomer != null && (DrawerCustomer.Id == SelectedCustomer.Id || (!string.IsNullOrEmpty(DrawerCustomer.Code) && DrawerCustomer.Code == SelectedCustomer.Code)))
                {
                    DrawerCustomer.Code = SelectedCustomer.Code;
                    DrawerCustomer.Name = SelectedCustomer.Name;
                    DrawerCustomer.Phone = SelectedCustomer.Phone;
                    DrawerCustomer.Address = SelectedCustomer.Address;
                    OnPropertyChanged(nameof(DrawerCustomer));
                    OnPropertyChanged(nameof(DrawerCustomerInitials));
                    OnPropertyChanged(nameof(DrawerCustomerSubInfo));
                }

                ClearCustomerForm();
                IsModalOpen = false;
                IsEditMode = false;
                IsCustomerAddMode = false;
                LoadCustomers();
                Views.CustomMessageBox.Show("บันทึกข้อมูลเรียบร้อยแล้ว", "สำเร็จ", MessageBoxButton.OK, MessageBoxImage.Information);
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
                IsEditMode = false;
                IsCustomerAddMode = false;
                LoadCustomers();
                Views.CustomMessageBox.Show("บันทึกข้อมูลเรียบร้อยแล้ว", "สำเร็จ", MessageBoxButton.OK, MessageBoxImage.Information);
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
            IsEditMode = false;
            IsCustomerAddMode = false;
            IsCustomerDrawerOpen = false;
            DrawerCustomer = null;
            LoadCustomers();
        }

        public void ExecuteCloseModal()
        {
            ClearCustomerForm();
            IsModalOpen = false;
            IsEditMode = false;
            IsCustomerAddMode = false;
        }

        public void ClearCustomerForm()
        {
            CustomerCode = GenerateCustomerCode();
            CustomerName = string.Empty;
            CustomerPhone = string.Empty;
            CustomerAddress = string.Empty;
            CustomerValidationMessage = string.Empty;
        }

        // ── Drawer Properties (Feature 2: Customer History & Points Drawer) ──
        private bool _isCustomerDrawerOpen;
        public bool IsCustomerDrawerOpen
        {
            get => _isCustomerDrawerOpen;
            set => SetProperty(ref _isCustomerDrawerOpen, value);
        }

        private Customer? _drawerCustomer;
        public Customer? DrawerCustomer
        {
            get => _drawerCustomer;
            set
            {
                if (SetProperty(ref _drawerCustomer, value))
                {
                    OnPropertyChanged(nameof(DrawerCustomerInitials));
                    OnPropertyChanged(nameof(DrawerCustomerSubInfo));
                }
            }
        }

        public string DrawerCustomerInitials
        {
            get
            {
                if (DrawerCustomer == null || string.IsNullOrWhiteSpace(DrawerCustomer.Name)) return "ล";
                return DrawerCustomer.Name.Trim().Substring(0, 1);
            }
        }

        public string DrawerCustomerSubInfo
        {
            get
            {
                if (DrawerCustomer == null) return "-";
                string code = !string.IsNullOrWhiteSpace(DrawerCustomer.Code) ? DrawerCustomer.Code : $"CUS-{DrawerCustomer.Id:D4}";
                string dateStr = Common.DateTimeHelper.ToBuddhistEra(DrawerCustomer.RegistrationDate, "dd/MM/yyyy");
                string phone = !string.IsNullOrWhiteSpace(DrawerCustomer.Phone) ? DrawerCustomer.Phone : "-";
                return $"{code} · {dateStr} · {phone}";
            }
        }

        // Points Card
        private int _drawerPointsBalance;
        public int DrawerPointsBalance
        {
            get => _drawerPointsBalance;
            set => SetProperty(ref _drawerPointsBalance, value);
        }

        private int _drawerEarnedPointsTotal;
        public int DrawerEarnedPointsTotal
        {
            get => _drawerEarnedPointsTotal;
            set => SetProperty(ref _drawerEarnedPointsTotal, value);
        }

        private int _drawerRedeemedPointsTotal;
        public int DrawerRedeemedPointsTotal
        {
            get => _drawerRedeemedPointsTotal;
            set => SetProperty(ref _drawerRedeemedPointsTotal, value);
        }

        private string _totalEarnedPointsText = "0 แต้ม";
        public string TotalEarnedPointsText
        {
            get => _totalEarnedPointsText;
            set => SetProperty(ref _totalEarnedPointsText, value);
        }

        private string _totalUsedPointsText = "0 แต้ม";
        public string TotalUsedPointsText
        {
            get => _totalUsedPointsText;
            set => SetProperty(ref _totalUsedPointsText, value);
        }

        public string DrawerEarnedPointsTotalText => TotalEarnedPointsText;
        public string DrawerRedeemedPointsTotalText => TotalUsedPointsText;
        public int TotalEarnedPoints => DrawerEarnedPointsTotal;
        public int TotalUsedPoints => DrawerRedeemedPointsTotal;

        public ObservableCollection<string> DrawerActivePromotions { get; } = new ObservableCollection<string>();
        public bool HasActivePromotions => DrawerActivePromotions.Count > 0;

        // Summary Metric Box
        private string _drawerTotalBills = "0 บิล";
        public string DrawerTotalBills
        {
            get => _drawerTotalBills;
            set => SetProperty(ref _drawerTotalBills, value);
        }

        private string _drawerTotalSpent = "0.00 บาท";
        public string DrawerTotalSpent
        {
            get => _drawerTotalSpent;
            set => SetProperty(ref _drawerTotalSpent, value);
        }

        private string _drawerLatestPurchase = "-";
        public string DrawerLatestPurchase
        {
            get => _drawerLatestPurchase;
            set => SetProperty(ref _drawerLatestPurchase, value);
        }

        // Purchase History
        public ObservableCollection<CustomerPurchaseHistoryDto> DrawerPurchases { get; } = new ObservableCollection<CustomerPurchaseHistoryDto>();
        public bool HasDrawerPurchases => DrawerPurchases.Count > 0;

        private string _drawerPurchaseFilter = "all"; // "all", "1month", "3months"
        public string DrawerPurchaseFilter
        {
            get => _drawerPurchaseFilter;
            set
            {
                if (SetProperty(ref _drawerPurchaseFilter, value))
                {
                    OnPropertyChanged(nameof(IsFilterAll));
                    OnPropertyChanged(nameof(IsFilter30Days));
                    OnPropertyChanged(nameof(IsFilter1Month));
                    OnPropertyChanged(nameof(IsFilter3Months));
                }
            }
        }

        public bool IsFilterAll => DrawerPurchaseFilter == "all";
        public bool IsFilter30Days => DrawerPurchaseFilter == "30days" || DrawerPurchaseFilter == "1month";
        public bool IsFilter1Month => DrawerPurchaseFilter == "30days" || DrawerPurchaseFilter == "1month";
        public bool IsFilter3Months => DrawerPurchaseFilter == "3months";

        private int _drawerCurrentPage = 1;
        public int DrawerCurrentPage
        {
            get => _drawerCurrentPage;
            set
            {
                if (SetProperty(ref _drawerCurrentPage, value))
                {
                    UpdateDrawerPaginationState();
                }
            }
        }

        private int _drawerTotalCount = 0;
        public int DrawerTotalCount
        {
            get => _drawerTotalCount;
            set
            {
                if (SetProperty(ref _drawerTotalCount, value))
                {
                    UpdateDrawerPaginationState();
                }
            }
        }

        private int _drawerTotalPages = 1;
        public int DrawerTotalPages
        {
            get => _drawerTotalPages;
            set
            {
                if (SetProperty(ref _drawerTotalPages, value))
                {
                    UpdateDrawerPaginationState();
                }
            }
        }

        private string _drawerPaginationInfo = "แสดง 0 จาก 0 บิล";
        public string DrawerPaginationInfo
        {
            get => _drawerPaginationInfo;
            set => SetProperty(ref _drawerPaginationInfo, value);
        }

        private bool _canPrevPage;
        public bool CanPrevPage
        {
            get => _canPrevPage;
            set => SetProperty(ref _canPrevPage, value);
        }

        private bool _canNextPage;
        public bool CanNextPage
        {
            get => _canNextPage;
            set => SetProperty(ref _canNextPage, value);
        }

        // Drawer Commands
        private ICommand? _openCustomerDrawerCommand;
        public ICommand OpenCustomerDrawerCommand => _openCustomerDrawerCommand ??= new RelayCommand(p => ExecuteOpenCustomerDrawer(p as Customer));
        public ICommand OpenCustomerDetailsCommand => OpenCustomerDrawerCommand;

        private ICommand? _closeCustomerDrawerCommand;
        public ICommand CloseCustomerDrawerCommand => _closeCustomerDrawerCommand ??= new RelayCommand(_ => ExecuteCloseCustomerDrawer());

        private ICommand? _setPurchaseFilterCommand;
        public ICommand SetPurchaseFilterCommand => _setPurchaseFilterCommand ??= new RelayCommand(p => ExecuteSetPurchaseFilter(p?.ToString() ?? "all"));

        private ICommand? _prevPurchasePageCommand;
        public ICommand PrevPurchasePageCommand => _prevPurchasePageCommand ??= new RelayCommand(_ => ExecutePrevPurchasePage());

        private ICommand? _nextPurchasePageCommand;
        public ICommand NextPurchasePageCommand => _nextPurchasePageCommand ??= new RelayCommand(_ => ExecuteNextPurchasePage());

        private ICommand? _viewAllPurchasesCommand;
        public ICommand ViewAllPurchasesCommand => _viewAllPurchasesCommand ??= new RelayCommand(_ => ExecuteViewAllPurchases());

        private ICommand? _openReceiptDetailCommand;
        public ICommand OpenReceiptDetailCommand => _openReceiptDetailCommand ??= new RelayCommand(p => ExecuteOpenReceiptDetail(p));

        private ICommand? _openEditCustomerFromDrawerCommand;
        public ICommand OpenEditCustomerFromDrawerCommand => _openEditCustomerFromDrawerCommand ??= new RelayCommand(_ => ExecuteOpenEditCustomerFromDrawer());

        public void ExecuteOpenCustomerDrawer(Customer? customer = null)
        {
            var target = customer ?? SelectedCustomer;
            if (target == null || target.Id <= 0) return;

            SelectedCustomer = target;
            DrawerCustomer = target;
            DrawerPurchaseFilter = "all";
            DrawerCurrentPage = 1;

            // รีเซ็ตค่าเริ่มต้นของแต้มป้องกันจังหวะที่เป็นค่าว่าง
            TotalEarnedPointsText = "0 แต้ม";
            TotalUsedPointsText = "0 แต้ม";
            OnPropertyChanged(nameof(TotalEarnedPointsText));
            OnPropertyChanged(nameof(TotalUsedPointsText));

            LoadDrawerData(target);
            IsCustomerDrawerOpen = true;
        }

        public void ExecuteCloseCustomerDrawer()
        {
            IsCustomerDrawerOpen = false;
        }

        public void ExecuteSetPurchaseFilter(string period)
        {
            DrawerPurchaseFilter = period;
            DrawerCurrentPage = 1;
            LoadDrawerPurchasesPage();
        }

        public void ExecutePrevPurchasePage()
        {
            if (DrawerCurrentPage > 1)
            {
                DrawerCurrentPage--;
                LoadDrawerPurchasesPage();
            }
        }

        public void ExecuteNextPurchasePage()
        {
            if (DrawerCurrentPage < DrawerTotalPages)
            {
                DrawerCurrentPage++;
                LoadDrawerPurchasesPage();
            }
        }

        public void ExecuteViewAllPurchases()
        {
            DrawerPurchaseFilter = "all";
            DrawerCurrentPage = 1;
            LoadDrawerPurchasesPage();
        }

        public void ExecuteOpenReceiptDetail(object? param)
        {
            int salesId = 0;
            string refNo = "";

            if (param is CustomerPurchaseHistoryDto dto)
            {
                salesId = dto.SalesId;
                refNo = dto.RefNo;
            }
            else if (param is int id)
            {
                salesId = id;
            }
            else if (param is string s)
            {
                refNo = s;
            }

            SalesOrder? order = null;
            if (salesId > 0)
            {
                order = _databaseService.GetSalesOrderById(salesId);
            }
            else if (!string.IsNullOrWhiteSpace(refNo))
            {
                order = _databaseService.GetSalesOrderByRef(refNo);
            }

            if (order == null)
            {
                Views.CustomMessageBox.Show("ไม่พบข้อมูลใบเสร็จที่ต้องการ", "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var items = _databaseService.GetSalesOrderItems(order.Sales_ID);
            var dialog = new ReceiptDetailDialog(order, items)
            {
                Owner = Application.Current?.MainWindow
            };
            dialog.ShowDialog();
        }

        public void ExecuteOpenEditCustomerFromDrawer()
        {
            if (DrawerCustomer == null) return;
            SelectedCustomer = DrawerCustomer;

            CustomerCode = DrawerCustomer.Code ?? string.Empty;
            CustomerName = DrawerCustomer.Name ?? string.Empty;
            CustomerPhone = DrawerCustomer.Phone ?? string.Empty;
            CustomerAddress = DrawerCustomer.Address ?? string.Empty;

            _originalCustomerName = CustomerName;
            _originalCustomerPhone = CustomerPhone;
            _originalCustomerAddress = CustomerAddress;

            IsCustomerAddMode = false;
            IsEditMode = true;
            CustomerModalTitle = "แก้ไขข้อมูลลูกค้า";
            ModalTitle = CustomerModalTitle;
            CustomerValidationMessage = string.Empty;

            // คงสถานะแผงฝั่งขวาไว้ (ไม่ปิดแผงขวา) และเปิด Modal โหมดแก้ไขโดยตรงทันที
            IsModalOpen = true;
        }

        public void LoadDrawerData(Customer customer)
        {
            try
            {
                string cusCode = customer.Code ?? string.Empty;
                string cusIdStr = customer.Id > 0 ? customer.Id.ToString() : string.Empty;
                var summary = PointsService.Instance.GetPointsSummary(cusCode, _databaseService.GetConnection(), null, cusIdStr);

                // ป้องกันการเขียนทับด้วยค่า 0 หากใน tblCustomer มีแต้มเดิมอยู่
                int customerPoints = Math.Max(customer.Points, customer.TotalPoints);
                int effectiveBalance = Math.Max(summary.Balance, customerPoints);
                int effectiveEarned = Math.Max(summary.TotalEarned, effectiveBalance);
                if (effectiveEarned == 0 && customerPoints > 0)
                {
                    effectiveEarned = customerPoints;
                }
                int usedPoints = summary.TotalRedeemed;

                DrawerPointsBalance = effectiveBalance;
                DrawerEarnedPointsTotal = effectiveEarned;
                DrawerRedeemedPointsTotal = usedPoints;

                // จัดรูปแบบข้อความแต้มให้ลงท้ายด้วย 'แต้ม' เสมอ
                TotalEarnedPointsText = effectiveEarned > 0 ? $"+{effectiveEarned} แต้ม" : "0 แต้ม";
                TotalUsedPointsText = usedPoints > 0 ? $"-{usedPoints} แต้ม" : "0 แต้ม";

                // อัปเดตค่าแต้มของลูกค้าให้ตรงกัน ป้องกันแถวในตารางหลักกลายเป็น 0
                customer.Points = effectiveBalance;
                OnPropertyChanged(nameof(DrawerPointsBalance));
                OnPropertyChanged(nameof(DrawerEarnedPointsTotal));
                OnPropertyChanged(nameof(DrawerRedeemedPointsTotal));
                OnPropertyChanged(nameof(TotalEarnedPointsText));
                OnPropertyChanged(nameof(TotalUsedPointsText));
                OnPropertyChanged(nameof(TotalEarnedPoints));
                OnPropertyChanged(nameof(TotalUsedPoints));
                OnPropertyChanged(nameof(DrawerEarnedPointsTotalText));
                OnPropertyChanged(nameof(DrawerRedeemedPointsTotalText));
                OnPropertyChanged(nameof(DrawerCustomer));

                DrawerActivePromotions.Clear();
                var promos = _databaseService.GetActivePointRedeemPromotions();
                if (promos != null && promos.Any())
                {
                    foreach (var p in promos)
                    {
                        DrawerActivePromotions.Add($"• {p.PromoID} (ใช้ {p.PointsRequired} แต้ม ลด {p.DiscountAmount:N0} บาท)");
                    }
                }
                OnPropertyChanged(nameof(HasActivePromotions));

                var purchaseSummary = _databaseService.GetCustomerPurchaseSummary(customer.Id);
                DrawerTotalBills = $"{purchaseSummary.TotalBills} บิล";
                DrawerTotalSpent = $"{purchaseSummary.TotalSpent:N2} บาท";
                DrawerLatestPurchase = purchaseSummary.LatestPurchaseDate;

                // รีเซ็ตตัวกรองเป็น 'all' และหน้า 1 เสมอ
                DrawerPurchaseFilter = "all";
                DrawerCurrentPage = 1;

                LoadDrawerPurchasesPage();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadDrawerData error: {ex.Message}");
            }
        }

        public void LoadDrawerPurchasesPage()
        {
            if (DrawerCustomer == null) return;

            try
            {
                var result = _databaseService.GetCustomerPurchaseHistory(DrawerCustomer.Id, DrawerPurchaseFilter, DrawerCurrentPage, 5);
                DrawerPurchases.Clear();
                foreach (var item in result.Items)
                {
                    DrawerPurchases.Add(item);
                }
                OnPropertyChanged(nameof(HasDrawerPurchases));

                DrawerTotalCount = result.TotalCount;
                DrawerTotalPages = Math.Max(1, (int)Math.Ceiling((double)DrawerTotalCount / 5));

                UpdateDrawerPaginationState();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadDrawerPurchasesPage error: {ex.Message}");
            }
        }

        private void UpdateDrawerPaginationState()
        {
            if (DrawerTotalCount == 0)
            {
                DrawerPaginationInfo = "แสดง 0 จาก 0 บิล";
                CanPrevPage = false;
                CanNextPage = false;
            }
            else
            {
                int start = Math.Min(DrawerTotalCount, (DrawerCurrentPage - 1) * 5 + 1);
                int end = Math.Min(DrawerTotalCount, DrawerCurrentPage * 5);
                DrawerPaginationInfo = $"แสดง {start}-{end} จาก {DrawerTotalCount} บิล";
                CanPrevPage = DrawerCurrentPage > 1;
                CanNextPage = DrawerCurrentPage < DrawerTotalPages;
            }
        }
    }
}

