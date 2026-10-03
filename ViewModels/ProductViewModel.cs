using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ClosedXML.Excel;
using Dapper;
using Porjai20.Common;
using Porjai20.Models;
using Porjai20.Services;
using Porjai20.Views;

namespace Porjai20.ViewModels
{
    public partial class ProductViewModel : ViewModelBase
    {
        private readonly DatabaseService _databaseService;
        private Product _selectedProduct;
        private string _code;
        private string _name;
        private string _category;
        private string _unit = string.Empty;
        private decimal _price;
        private decimal _cost;
        private int _stock;
        private int _reorderPoint;
        private string _imagePath;
        private string _description;
        private string _searchText;

        public decimal Cost
        {
            get => _cost;
            set => SetProperty(ref _cost, value);
        }
        
        // Stock In Fields
        private string _stockInRefNo;
        private string _stockInSupplier;
        private int _stockInQuantity;
        private decimal _stockInCost;

        // Stock Adjustment Fields
        // Stock Adjustment Fields
        private bool _isStockAdjustmentMode;
        public bool IsStockAdjustmentMode
        {
            get => _isStockAdjustmentMode;
            set => SetProperty(ref _isStockAdjustmentMode, value);
        }

        private string _selectedAdjustmentReason;
        public string SelectedAdjustmentReason
        {
            get => _selectedAdjustmentReason;
            set => SetProperty(ref _selectedAdjustmentReason, value);
        }

        public ObservableCollection<string> AdjustmentReasons { get; } = new ObservableCollection<string>
        {
            "สินค้าชำรุด",
            "สินค้าสูญหาย",
            "ข้อผิดพลาดจากการนับสต็อก",
            "อื่นๆ"
        };

        private int _adjustmentQuantity;
        public int AdjustmentQuantity
        {
            get => _adjustmentQuantity;
            set => SetProperty(ref _adjustmentQuantity, value);
        }

        private string _adjustmentNote;
        public string AdjustmentNote
        {
            get => _adjustmentNote;
            set => SetProperty(ref _adjustmentNote, value);
        }

        public bool IsStaffMode
        {
            get => _isStaffMode;
            set
            {
                if (SetProperty(ref _isStaffMode, value))
                {
                    if (value) { _currentView = "staff"; OnPropertyChanged(nameof(CurrentView)); }
                    OnPropertyChanged(nameof(IsSearchMode));
                }
            }
        }

        // --- User Management ---
        private User? _currentUser;
        public User? CurrentUser
        {
            get => _currentUser;
            set
            {
                SetProperty(ref _currentUser, value);
                IsAdmin = RolePermissions.IsAdminOrOwner(_currentUser?.Role);
                OnPropertyChanged(nameof(IsLoggedIn));
                OnPropertyChanged(nameof(ClaimSenderPhone));
                RefreshHomeMenu();
                NotifyReadOnlyProperties();
            }
        }

        // --- Role-Based Read-Only Mode (RBAC) ---
        public bool IsReadOnlyMode => RolePermissions.IsReadOnly(CurrentUser?.Role, CurrentView);
        public bool CanEditCurrentView => !IsReadOnlyMode;

        public bool IsStockManageReadOnly => RolePermissions.IsReadOnly(CurrentUser?.Role, "stock_manage");
        public bool CanEditStockManage => !IsStockManageReadOnly;

        public bool IsStockInReadOnly => RolePermissions.IsReadOnly(CurrentUser?.Role, "stock_in");
        public bool CanEditStockIn => !IsStockInReadOnly;

        public bool IsClaimReadOnly => RolePermissions.IsReadOnly(CurrentUser?.Role, "claim");
        public bool CanEditClaim => !IsClaimReadOnly;

        public bool IsCustomerReadOnly => RolePermissions.IsReadOnly(CurrentUser?.Role, "customer");
        public bool CanEditCustomer => !IsCustomerReadOnly;

        public void NotifyReadOnlyProperties()
        {
            OnPropertyChanged(nameof(IsReadOnlyMode));
            OnPropertyChanged(nameof(CanEditCurrentView));
            OnPropertyChanged(nameof(IsStockManageReadOnly));
            OnPropertyChanged(nameof(CanEditStockManage));
            OnPropertyChanged(nameof(IsStockInReadOnly));
            OnPropertyChanged(nameof(CanEditStockIn));
            OnPropertyChanged(nameof(CanConfirmGoodsReceipt));
            OnPropertyChanged(nameof(IsReceiptDeliveryNoteValid));
            OnPropertyChanged(nameof(IsReceiptDeliveryNoteInvalid));
            OnPropertyChanged(nameof(IsClaimReadOnly));
            OnPropertyChanged(nameof(CanEditClaim));
            OnPropertyChanged(nameof(IsCustomerReadOnly));
            OnPropertyChanged(nameof(CanEditCustomer));
        }

        private bool _isNavigating = false;
        private async Task NavigateAsync(System.Func<Task> navigateAction)
        {
            if (_isNavigating) return;
            _isNavigating = true;
            try
            {
                await navigateAction();
            }
            finally
            {
                _isNavigating = false;
            }
        }

        private string _currentView = "home";
        public string CurrentView
        {
            get => _currentView;
            set
            {
                if (SetProperty(ref _currentView, value))
                {
                    IsHomeMode = (value == "home");
                    IsManagementMode = (value == "stock_manage");
                    IsStockInMode = (value == "stock_in");
                    IsReportsMode = (value == "reports");
                    IsHistoryMode = (value == "history");
                    IsDeliveryMode = (value == "delivery");
                    IsCustomerMode = (value == "customer");
                    IsPartnerMode = (value == "partner");
                    IsClaimMode = (value == "claim");
                    IsPurchaseOrderMode = (value == "purchase_order");
                    IsExpenseMode = (value == "expense");
                    IsStaffMode = (value == "staff");
                    NotifyReadOnlyProperties();
                }
            }
        }

        // --- Home Dashboard Menu ---
        private ObservableCollection<MenuItemModel> _homeMenuItems = new();
        public ObservableCollection<MenuItemModel> HomeMenuItems
        {
            get => _homeMenuItems;
            private set => SetProperty(ref _homeMenuItems, value);
        }

        private ICollectionView? _homeGroupedMenuItems;
        public ICollectionView? HomeGroupedMenuItems
        {
            get => _homeGroupedMenuItems;
            private set => SetProperty(ref _homeGroupedMenuItems, value);
        }

        public bool IsLoggedIn => CurrentUser != null;

        private User? _selectedUser;
        public User? SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public ObservableCollection<User> UsersList { get; set; } = new ObservableCollection<User>();
        
        
        
        private System.Collections.Generic.List<Product> _allProducts = new System.Collections.Generic.List<Product>();

        // Mode Controls
        private bool _isHomeMode = true;
        private bool _isManagementMode;
        private bool _isStockInMode;
        private bool _isReportsMode;

        private bool _isHistoryMode;
        private bool _isStaffMode = false; 
        private bool _isAdmin = false;
        private bool _isLoginDialogVisible = false; 
        private string _loginUsername = string.Empty;
        private string _loginPassword = string.Empty;
        private string _loginErrorMessage = string.Empty;

        public bool IsAdmin
        {
            get => _isAdmin;
            set => SetProperty(ref _isAdmin, value);
        }

        public bool IsLoginDialogVisible
        {
            get => _isLoginDialogVisible;
            set => SetProperty(ref _isLoginDialogVisible, value);
        }

        public string LoginUsername
        {
            get => _loginUsername;
            set => SetProperty(ref _loginUsername, value);
        }

        public string LoginPassword
        {
            get => _loginPassword;
            set => SetProperty(ref _loginPassword, value);
        }

        public string LoginErrorMessage
        {
            get => _loginErrorMessage;
            set => SetProperty(ref _loginErrorMessage, value);
        }

        public bool IsHistoryMode 
        { 
            get => _isHistoryMode; 
            set 
            {
                if (SetProperty(ref _isHistoryMode, value))
                {
                    if (value) { _currentView = "history"; OnPropertyChanged(nameof(CurrentView)); }
                    OnPropertyChanged(nameof(IsSearchMode));
                    OnPropertyChanged(nameof(IsHomeMode));
                    OnPropertyChanged(nameof(IsManagementMode));
                    OnPropertyChanged(nameof(IsStockInMode));
                    OnPropertyChanged(nameof(IsReportsMode));
                    OnPropertyChanged(nameof(IsDeliveryMode));
                }
            } 
        }

        private bool _isDeliveryMode;
        public bool IsDeliveryMode
        {
            get => _isDeliveryMode;
            set
            {
                if (SetProperty(ref _isDeliveryMode, value))
                {
                    if (value) { _currentView = "delivery"; OnPropertyChanged(nameof(CurrentView)); }
                    OnPropertyChanged(nameof(IsSearchMode));
                    OnPropertyChanged(nameof(IsHomeMode));
                    OnPropertyChanged(nameof(IsManagementMode));
                    OnPropertyChanged(nameof(IsStockInMode));
                    OnPropertyChanged(nameof(IsReportsMode));
                    OnPropertyChanged(nameof(IsHistoryMode));
                    OnPropertyChanged(nameof(IsCustomerMode));
                }
            }
        }

        private bool _isCustomerMode;
        public bool IsCustomerMode
        {
            get => _isCustomerMode;
            set
            {
                if (SetProperty(ref _isCustomerMode, value))
                {
                    if (value) { _currentView = "customer"; OnPropertyChanged(nameof(CurrentView)); }
                    OnPropertyChanged(nameof(IsSearchMode));
                    OnPropertyChanged(nameof(IsHomeMode));
                    OnPropertyChanged(nameof(IsManagementMode));
                    OnPropertyChanged(nameof(IsStockInMode));
                    OnPropertyChanged(nameof(IsReportsMode));
                    OnPropertyChanged(nameof(IsHistoryMode));
                    OnPropertyChanged(nameof(IsDeliveryMode));
                    OnPropertyChanged(nameof(IsPartnerMode));
                }
            }
        }

        private bool _isPartnerMode;
        public bool IsPartnerMode
        {
            get => _isPartnerMode;
            set
            {
                if (SetProperty(ref _isPartnerMode, value))
                {
                    if (value) { _currentView = "partner"; OnPropertyChanged(nameof(CurrentView)); }
                    OnPropertyChanged(nameof(IsSearchMode));
                    OnPropertyChanged(nameof(IsHomeMode));
                    OnPropertyChanged(nameof(IsManagementMode));
                    OnPropertyChanged(nameof(IsStockInMode));
                    OnPropertyChanged(nameof(IsReportsMode));
                    OnPropertyChanged(nameof(IsHistoryMode));
                    OnPropertyChanged(nameof(IsDeliveryMode));
                    OnPropertyChanged(nameof(IsCustomerMode));
                    OnPropertyChanged(nameof(IsClaimMode));
                }
            }
        }

        private bool _isClaimMode;
        public bool IsClaimMode
        {
            get => _isClaimMode;
            set
            {
                if (SetProperty(ref _isClaimMode, value))
                {
                    if (value) { _currentView = "claim"; OnPropertyChanged(nameof(CurrentView)); }
                    OnPropertyChanged(nameof(IsSearchMode));
                    OnPropertyChanged(nameof(IsHomeMode));
                    OnPropertyChanged(nameof(IsManagementMode));
                    OnPropertyChanged(nameof(IsStockInMode));
                    OnPropertyChanged(nameof(IsReportsMode));
                    OnPropertyChanged(nameof(IsHistoryMode));
                    OnPropertyChanged(nameof(IsDeliveryMode));
                    OnPropertyChanged(nameof(IsCustomerMode));
                    OnPropertyChanged(nameof(IsPartnerMode));
                    OnPropertyChanged(nameof(IsPurchaseOrderMode));
                }
            }
        }

        private bool _isPurchaseOrderMode;
        public bool IsPurchaseOrderMode
        {
            get => _isPurchaseOrderMode;
            set
            {
                if (SetProperty(ref _isPurchaseOrderMode, value))
                {
                    if (value) { _currentView = "purchase_order"; OnPropertyChanged(nameof(CurrentView)); }
                    OnPropertyChanged(nameof(IsSearchMode));
                    OnPropertyChanged(nameof(IsHomeMode));
                    OnPropertyChanged(nameof(IsManagementMode));
                    OnPropertyChanged(nameof(IsStockInMode));
                    OnPropertyChanged(nameof(IsReportsMode));
                    OnPropertyChanged(nameof(IsHistoryMode));
                    OnPropertyChanged(nameof(IsDeliveryMode));
                    OnPropertyChanged(nameof(IsCustomerMode));
                    OnPropertyChanged(nameof(IsPartnerMode));
                    OnPropertyChanged(nameof(IsClaimMode));
                    OnPropertyChanged(nameof(IsExpenseMode));
                }
            }
        }

        private bool _isExpenseMode;
        public bool IsExpenseMode
        {
            get => _isExpenseMode;
            set
            {
                if (SetProperty(ref _isExpenseMode, value))
                {
                    if (value) { _currentView = "expense"; OnPropertyChanged(nameof(CurrentView)); }
                    OnPropertyChanged(nameof(IsSearchMode));
                    OnPropertyChanged(nameof(IsHomeMode));
                    OnPropertyChanged(nameof(IsManagementMode));
                    OnPropertyChanged(nameof(IsStockInMode));
                    OnPropertyChanged(nameof(IsReportsMode));
                    OnPropertyChanged(nameof(IsHistoryMode));
                    OnPropertyChanged(nameof(IsDeliveryMode));
                    OnPropertyChanged(nameof(IsCustomerMode));
                    OnPropertyChanged(nameof(IsPartnerMode));
                    OnPropertyChanged(nameof(IsClaimMode));
                    OnPropertyChanged(nameof(IsPurchaseOrderMode));
                }
            }
        }

        // Data Collections
        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();
        // Dashboard Properties
        private decimal _todaySalesTotal;
        public decimal TodaySalesTotal
        {
            get => _todaySalesTotal;
            set => SetProperty(ref _todaySalesTotal, value);
        }

        private int _todayOrdersCount;
        public int TodayOrdersCount
        {
            get => _todayOrdersCount;
            set => SetProperty(ref _todayOrdersCount, value);
        }

        private int _totalProductsInStock;
        public int TotalProductsInStock
        {
            get => _totalProductsInStock;
            set => SetProperty(ref _totalProductsInStock, value);
        }

        public ObservableCollection<SalesOrder> RecentSalesOrders { get; set; } = new ObservableCollection<SalesOrder>();

        public ObservableCollection<PurchaseOrder> PurchaseOrders { get; set; } = new ObservableCollection<PurchaseOrder>();
        public ObservableCollection<PurchaseOrderItem> CurrentPOItems { get; set; } = new ObservableCollection<PurchaseOrderItem>();
        public ObservableCollection<Product> LowStockProducts { get; set; } = new ObservableCollection<Product>();
        public ObservableCollection<CartItem> CartItems { get; set; } = new ObservableCollection<CartItem>();
        public ObservableCollection<SalesOrder> SalesOrders { get; set; } = new ObservableCollection<SalesOrder>();
        public ObservableCollection<SalesOrderItem> SelectedOrderItems { get; set; } = new ObservableCollection<SalesOrderItem>();
        public ObservableCollection<SalesOrder> Deliveries { get; set; } = new ObservableCollection<SalesOrder>();
        public ObservableCollection<Customer> Customers { get; set; } = new ObservableCollection<Customer>();
        public ObservableCollection<Customer> FilteredCustomers => Customers;
        public ObservableCollection<Partner> Partners { get; set; } = new ObservableCollection<Partner>();
        public ObservableCollection<Claim> Claims { get; set; } = new ObservableCollection<Claim>();
        public System.Collections.ObjectModel.ObservableCollection<string> ClaimStatusOptions { get; } = new System.Collections.ObjectModel.ObservableCollection<string> { "ทั้งหมด", "รอดำเนินการ", "เคลมสำเร็จ", "ปฏิเสธ" };
        public System.Collections.ObjectModel.ObservableCollection<string> ClaimTypeOptions { get; } = new System.Collections.ObjectModel.ObservableCollection<string> { "ลูกค้า", "บริษัทคู่ค้า" };
        public System.Collections.ObjectModel.ObservableCollection<string> ClaimNewStatusOptions { get; } = new System.Collections.ObjectModel.ObservableCollection<string> { "รอดำเนินการ", "เคลมสำเร็จ", "ปฏิเสธ" };
        public System.Collections.ObjectModel.ObservableCollection<string> ClaimFormStatusOptions { get; } = new System.Collections.ObjectModel.ObservableCollection<string> { "รอดำเนินการ", "เคลมสำเร็จ", "ปฏิเสธ" };
        public System.Collections.ObjectModel.ObservableCollection<string> ClaimActionOptions { get; } = new System.Collections.ObjectModel.ObservableCollection<string> { "เปลี่ยนสินค้าใหม่", "คืนเงิน" };
        
        // Expenses handled in ExpenseViewModel.cs partial
        
        // Checkout Delivery Fields
        private bool _isDeliverySelected;
        public bool IsDeliverySelected
        {
            get => _isDeliverySelected;
            set
            {
                if (SetProperty(ref _isDeliverySelected, value))
                {
                    if (value)
                    {
                        if (string.IsNullOrWhiteSpace(DeliveryCustomerName)) 
                            DeliveryCustomerName = !string.IsNullOrWhiteSpace(CustomerName) ? CustomerName : (SelectedCustomer?.Name ?? "");
                        if (string.IsNullOrWhiteSpace(DeliveryCustomerPhone)) 
                            DeliveryCustomerPhone = !string.IsNullOrWhiteSpace(CustomerPhone) ? CustomerPhone : (SelectedCustomer?.Phone ?? "");
                        if (string.IsNullOrWhiteSpace(DeliveryCustomerAddress)) 
                            DeliveryCustomerAddress = !string.IsNullOrWhiteSpace(CustomerAddress) ? CustomerAddress : (SelectedCustomer?.Address ?? "");
                    }
                    OnPropertyChanged(nameof(IsPickupSelected));
                    OnPropertyChanged(nameof(CheckoutDeliveryMethod));
                    OnPropertyChanged(nameof(ShippingFee));
                    OnPropertyChanged(nameof(CheckoutGrandTotal));
                }
            }
        }

        public bool IsPickupSelected
        {
            get => !_isDeliverySelected;
            set
            {
                if (value)
                {
                    IsDeliverySelected = false;
                    // IsDeliverySelected setter already fires these, but call explicitly
                    // to guarantee UI updates even if value didn't change
                    OnPropertyChanged(nameof(IsPickupSelected));
                    OnPropertyChanged(nameof(ShippingFee));
                    OnPropertyChanged(nameof(CheckoutGrandTotal));
                }
            }
        }

        private string _deliveryCustomerName = "";
        public string DeliveryCustomerName
        {
            get => _deliveryCustomerName;
            set
            {
                if (SetProperty(ref _deliveryCustomerName, value))
                {
                    OnPropertyChanged(nameof(RecipientName));
                }
            }
        }
        public string RecipientName { get => DeliveryCustomerName; set => DeliveryCustomerName = value; }

        private string _deliveryCustomerPhone = "";
        public string DeliveryCustomerPhone
        {
            get => _deliveryCustomerPhone;
            set
            {
                if (SetProperty(ref _deliveryCustomerPhone, value))
                {
                    OnPropertyChanged(nameof(RecipientPhone));
                }
            }
        }
        public string RecipientPhone { get => DeliveryCustomerPhone; set => DeliveryCustomerPhone = value; }

        private string _deliveryCustomerAddress = "";
        public string DeliveryCustomerAddress
        {
            get => _deliveryCustomerAddress;
            set
            {
                if (SetProperty(ref _deliveryCustomerAddress, value))
                {
                    OnPropertyChanged(nameof(ShippingAddress));
                }
            }
        }
        public string ShippingAddress { get => DeliveryCustomerAddress; set => DeliveryCustomerAddress = value; }

        private string _deliveryNote = "";
        public string DeliveryNote
        {
            get => _deliveryNote;
            set => SetProperty(ref _deliveryNote, value);
        }

        // Error Modal Properties
        private bool _isErrorModalOpen;
        public bool IsErrorModalOpen
        {
            get => _isErrorModalOpen;
            set { _isErrorModalOpen = value; OnPropertyChanged(nameof(IsErrorModalOpen)); }
        }

        private string _errorModalMessage = string.Empty;
        public string ErrorModalMessage
        {
            get => _errorModalMessage;
            set { _errorModalMessage = value; OnPropertyChanged(nameof(ErrorModalMessage)); }
        }

        // ── Universal In-Page Alert Modal Properties ──────────────────────────
        private bool _isMainAlertOpen;
        public bool IsMainAlertOpen
        {
            get => _isMainAlertOpen;
            set => SetProperty(ref _isMainAlertOpen, value);
        }

        private string _mainAlertTitle = "แจ้งเตือน";
        public string MainAlertTitle
        {
            get => _mainAlertTitle;
            set => SetProperty(ref _mainAlertTitle, value);
        }

        private string _mainAlertMessage = string.Empty;
        public string MainAlertMessage
        {
            get => _mainAlertMessage;
            set => SetProperty(ref _mainAlertMessage, value);
        }

        private string _mainAlertIcon = "⚠️";
        public string MainAlertIcon
        {
            get => _mainAlertIcon;
            set => SetProperty(ref _mainAlertIcon, value);
        }

        public ICommand CloseMainAlertCommand { get; set; }

        public void ShowAlert(string message, string title = "แจ้งเตือน", string icon = "⚠️")
        {
            MainAlertTitle = title;
            MainAlertMessage = message;
            MainAlertIcon = icon;
            IsMainAlertOpen = true;
        }

        // ── Universal In-Page Confirm Modal Properties ────────────────────────
        private bool _isMainConfirmOpen;
        public bool IsMainConfirmOpen
        {
            get => _isMainConfirmOpen;
            set => SetProperty(ref _isMainConfirmOpen, value);
        }

        private string _mainConfirmTitle = "ยืนยันการทำรายการ";
        public string MainConfirmTitle
        {
            get => _mainConfirmTitle;
            set => SetProperty(ref _mainConfirmTitle, value);
        }

        private string _mainConfirmMessage = string.Empty;
        public string MainConfirmMessage
        {
            get => _mainConfirmMessage;
            set => SetProperty(ref _mainConfirmMessage, value);
        }

        private System.Action? _pendingConfirmAction;

        public ICommand ConfirmMainActionCommand { get; set; }
        public ICommand CancelMainActionCommand { get; set; }

        public void ShowConfirm(string message, System.Action onConfirm, string title = "ยืนยันการทำรายการ")
        {
            MainConfirmTitle = title;
            MainConfirmMessage = message;
            _pendingConfirmAction = onConfirm;
            IsMainConfirmOpen = true;
        }

        // Delivery / Customer Manage Modal
        private bool _isManageModalOpen;
        public bool IsManageModalOpen
        {
            get => _isManageModalOpen;
            set
            {
                if (SetProperty(ref _isManageModalOpen, value))
                {
                    OnPropertyChanged(nameof(IsEditCustomerModalOpen));
                }
            }
        }

        // ── Stock Management Modal ───────────────────────────────────────────────
        private bool _isStockManageModalOpen;
        public bool IsStockManageModalOpen
        {
            get => _isStockManageModalOpen;
            set { _isStockManageModalOpen = value; OnPropertyChanged(nameof(IsStockManageModalOpen)); }
        }

        private bool _isProductFormOpen;
        public bool IsProductFormOpen
        {
            get => _isProductFormOpen;
            set { _isProductFormOpen = value; OnPropertyChanged(nameof(IsProductFormOpen)); }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set { _isEditMode = value; OnPropertyChanged(nameof(IsEditMode)); OnPropertyChanged(nameof(ProductFormTitle)); }
        }

        private bool _isProductAddMode;
        public bool IsProductAddMode
        {
            get => _isProductAddMode;
            set { _isProductAddMode = value; OnPropertyChanged(nameof(IsProductAddMode)); }
        }

        private string _originalProductName = string.Empty;
        private string _originalProductCategory = string.Empty;
        private int _originalProductCategoryId = 0;
        private string _originalProductUnit = string.Empty;
        private decimal _originalProductCost = 0;
        private decimal _originalProductPrice = 0;
        private int _originalProductStock = 0;
        private int _originalProductReorderPoint = 0;
        private string _originalProductImage = string.Empty;

        public string ProductFormTitle => IsProductAddMode ? "➕ เพิ่มข้อมูลสินค้า" : (IsEditMode ? "✏️ แก้ไขข้อมูลสินค้า" : "📦 ข้อมูลสินค้า");

        public bool HasSelectedProduct => SelectedProduct != null;
        // ── End Stock Management Modal ───────────────────────────────────────────

        private string _selectedDeliveryStatus;
        public string SelectedDeliveryStatus
        {
            get => _selectedDeliveryStatus;
            set { _selectedDeliveryStatus = value; OnPropertyChanged(nameof(SelectedDeliveryStatus)); }
        }
        
        private string _deliverySearchKeyword = string.Empty;
        public string DeliverySearchKeyword
        {
            get => _deliverySearchKeyword;
            set
            {
                if (SetProperty(ref _deliverySearchKeyword, value))
                {
                    _ = LoadDeliveries();
                }
            }
        }

        private SalesOrder _selectedDeliveryOrder;
        public SalesOrder SelectedDeliveryOrder
        {
            get => _selectedDeliveryOrder;
            set
            {
                // ป้องกันไม่ให้ Selection หลุดเป็น null ขณะที่เปิดป๊อปอัปรายละเอียดคำสั่งซื้อ
                if (value == null && IsDeliveryDetailModalOpen && _selectedDeliveryOrder != null)
                {
                    return;
                }

                if (SetProperty(ref _selectedDeliveryOrder, value))
                {
                    if (_selectedDeliveryOrder != null)
                    {
                        string currentStatus = _selectedDeliveryOrder.DeliveryStatus;
                        if (currentStatus == "ยกเลิกรายการ") currentStatus = "ยกเลิก";
                        if (currentStatus == "รอดำเนินการ") currentStatus = "รอจัดส่ง";
                        _selectedDeliveryOrderStatus = currentStatus ?? "รอจัดส่ง";
                    }
                    OnPropertyChanged(nameof(IsDeliveryOrderSelected));
                    OnPropertyChanged(nameof(SelectedDeliveryOrderStatus));
                    OnPropertyChanged(nameof(SelectedDeliverySubtotal));
                    OnPropertyChanged(nameof(SelectedDeliveryShippingFee));
                    OnPropertyChanged(nameof(SelectedDeliveryGrandTotal));
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                    if (_selectedDeliveryOrder != null && _selectedDeliveryOrder.Id > 0)
                    {
                        _ = LoadSelectedDeliveryItems(_selectedDeliveryOrder.Id);
                    }
                }
            }
        }

        public decimal SelectedDeliverySubtotal => SelectedOrderItems?.Sum(i => i.Total) ?? 0m;

        public decimal SelectedDeliveryShippingFee
        {
            get
            {
                if (SelectedDeliveryOrder == null) return 0m;
                decimal subtotal = SelectedDeliverySubtotal;
                if (SelectedDeliveryOrder.TotalAmount > subtotal && subtotal > 0)
                {
                    return SelectedDeliveryOrder.TotalAmount - subtotal;
                }
                return 0m;
            }
        }

        public decimal SelectedDeliveryGrandTotal => SelectedDeliveryOrder?.TotalAmount ?? SelectedDeliverySubtotal;

        private string _selectedDeliveryOrderStatus = "รอจัดส่ง";
        public string SelectedDeliveryOrderStatus
        {
            get => _selectedDeliveryOrderStatus ?? "รอจัดส่ง";
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    string cleanVal = value.Replace("⌛", "").Replace("🚚", "").Replace("☑", "").Replace("✖", "").Trim();
                    if (_selectedDeliveryOrderStatus != cleanVal)
                    {
                        _selectedDeliveryOrderStatus = cleanVal;
                        OnPropertyChanged(nameof(SelectedDeliveryOrderStatus));
                    }
                }
            }
        }

        public bool IsDeliveryOrderSelected => SelectedDeliveryOrder != null && SelectedDeliveryOrder.Id != 0;

        private bool _isDeliveryDetailModalOpen;
        public bool IsDeliveryDetailModalOpen
        {
            get => _isDeliveryDetailModalOpen;
            set => SetProperty(ref _isDeliveryDetailModalOpen, value);
        }

        private string _selectedDeliveryStatusFilter = "ทั้งหมด";
        public string SelectedDeliveryStatusFilter
        {
            get => _selectedDeliveryStatusFilter;
            set { if (SetProperty(ref _selectedDeliveryStatusFilter, value)) _ = LoadDeliveries(); }
        }

        private DateTime _deliveryStartDate = DateTime.Today.AddMonths(-1);
        public DateTime DeliveryStartDate
        {
            get => _deliveryStartDate;
            set { if (SetProperty(ref _deliveryStartDate, value)) _ = LoadDeliveries(); }
        }

        private DateTime _deliveryEndDate = DateTime.Today;
        public DateTime DeliveryEndDate
        {
            get => _deliveryEndDate;
            set { if (SetProperty(ref _deliveryEndDate, value)) _ = LoadDeliveries(); }
        }

        public ObservableCollection<string> DeliveryStatusOptions { get; } = new ObservableCollection<string>
        {
            "ทั้งหมด", "รอจัดส่ง", "กำลังจัดส่ง", "จัดส่งสำเร็จ", "ยกเลิกรายการ"
        };

        public int PendingDeliveriesCount => Deliveries.Count(d => d.DeliveryStatus == "รอจัดส่ง" || d.DeliveryStatus == "รอดำเนินการ");
        public int InDeliveryCount => Deliveries.Count(d => d.DeliveryStatus == "กำลังจัดส่ง");
        public int DeliveredTodayCount => Deliveries.Count(d => d.DeliveryStatus == "จัดส่งสำเร็จ" && d.Timestamp.Date == DateTime.Today);

        // Customer Management Fields
        private string _customerSearchKeyword = string.Empty;
        public string CustomerSearchKeyword
        {
            get => _customerSearchKeyword;
            set
            {
                if (SetProperty(ref _customerSearchKeyword, value))
                {
                    _ = LoadCustomers();
                }
            }
        }

        private Customer _selectedCustomer = new Customer();
        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value) && value != null)
                {
                    CustomerCode = value.Code;
                    CustomerName = value.Name;
                    CustomerPhone = value.Phone;
                    CustomerAddress = value.Address;

                    if (string.IsNullOrWhiteSpace(DeliveryCustomerName)) DeliveryCustomerName = value.Name ?? "";
                    if (string.IsNullOrWhiteSpace(DeliveryCustomerPhone)) DeliveryCustomerPhone = value.Phone ?? "";
                    if (string.IsNullOrWhiteSpace(DeliveryCustomerAddress)) DeliveryCustomerAddress = value.Address ?? "";
                }
            }
        }

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

        // --- Customer History Modal Fields ---
        private bool _isCustomerHistoryModalOpen;
        public bool IsCustomerHistoryModalOpen
        {
            get => _isCustomerHistoryModalOpen;
            set => SetProperty(ref _isCustomerHistoryModalOpen, value);
        }

        private ObservableCollection<SalesOrder> _customerHistoryOrders = new ObservableCollection<SalesOrder>();
        public ObservableCollection<SalesOrder> CustomerHistoryOrders
        {
            get => _customerHistoryOrders;
            set => SetProperty(ref _customerHistoryOrders, value);
        }

        private string _customerHistoryName = string.Empty;
        public string CustomerHistoryName
        {
            get => _customerHistoryName;
            set => SetProperty(ref _customerHistoryName, value);
        }

        private string _customerHistoryPhone = string.Empty;
        public string CustomerHistoryPhone
        {
            get => _customerHistoryPhone;
            set => SetProperty(ref _customerHistoryPhone, value);
        }

        private int _customerHistoryTotalOrders;
        public int CustomerHistoryTotalOrders
        {
            get => _customerHistoryTotalOrders;
            set => SetProperty(ref _customerHistoryTotalOrders, value);
        }

        private decimal _customerHistoryTotalSpent;
        public decimal CustomerHistoryTotalSpent
        {
            get => _customerHistoryTotalSpent;
            set => SetProperty(ref _customerHistoryTotalSpent, value);
        }

        public ICommand CloseCustomerHistoryModalCommand { get; set; }

        // --- Customer Add/Edit Modal Fields & KPI Counts ---
        private bool _isCustomerModalOpen;
        public bool IsCustomerModalOpen
        {
            get => _isCustomerModalOpen;
            set => SetProperty(ref _isCustomerModalOpen, value);
        }

        public bool IsEditCustomerModalOpen
        {
            get => IsManageModalOpen;
            set => IsManageModalOpen = value;
        }

        public int TotalCustomersCount => Customers?.Count ?? 0;
        public int NewCustomersThisMonthCount => Customers?.Count(c => c.RegistrationDate.Month == DateTime.Today.Month && c.RegistrationDate.Year == DateTime.Today.Year) ?? 0;
        public decimal TotalCumulativeSpent => Customers?.Sum(c => (decimal)c.TotalSpent) ?? 0;

        public ICommand OpenAddCustomerModalCommand { get; set; }
        public ICommand OpenEditCustomerModalCommand { get; set; }
        public ICommand OpenCustomerManageModalCommand { get; set; }
        public ICommand CloseCustomerModalCommand { get; set; }
        public ICommand CloseEditCustomerModalCommand { get; set; }
        public ICommand ClearCustomerFilterCommand { get; set; }

        // --- Staff / Employee Management Fields & KPI Counts ---
        private bool _isStaffModalOpen;
        public bool IsStaffModalOpen
        {
            get => _isStaffModalOpen;
            set => SetProperty(ref _isStaffModalOpen, value);
        }

        private string _staffSearchKeyword = string.Empty;
        public string StaffSearchKeyword
        {
            get => _staffSearchKeyword;
            set
            {
                if (SetProperty(ref _staffSearchKeyword, value))
                {
                    OnPropertyChanged(nameof(SearchText));
                    FilteredStaffs?.Refresh();
                }
            }
        }

        public int TotalStaffCount => UsersList?.Count ?? 0;
        public int ActiveStaffCount => UsersList?.Count(u => u != null) ?? 0;
        public int AdminStaffCount => UsersList?.Count(u => RolePermissions.IsAdminOrOwner(u.Role)) ?? 0;

        public ICommand OpenStaffManageModalCommand { get; set; }
        public ICommand CloseStaffModalCommand { get; set; }
        public ICommand SearchStaffCommand { get; set; }
        public ICommand ClearStaffFilterCommand { get; set; }

        // --- Partner Management Fields ---
        private string _partnerSearchKeyword = string.Empty;
        public string PartnerSearchKeyword
        {
            get => _partnerSearchKeyword;
            set => SetProperty(ref _partnerSearchKeyword, value);
        }

        private Partner _selectedPartner = new Partner();
        public Partner SelectedPartner
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

        private string _partnerCode = string.Empty;
        public string PartnerCode { get => _partnerCode; set => SetProperty(ref _partnerCode, value); }

        private string _partnerName = string.Empty;
        public string PartnerName { get => _partnerName; set => SetProperty(ref _partnerName, value); }

        private string _partnerContactPerson = string.Empty;
        public string PartnerContactPerson { get => _partnerContactPerson; set => SetProperty(ref _partnerContactPerson, value); }

        private string _partnerPhone = string.Empty;
        public string PartnerPhone { get => _partnerPhone; set => SetProperty(ref _partnerPhone, value); }

        private string _partnerEmail = string.Empty;
        public string PartnerEmail { get => _partnerEmail; set => SetProperty(ref _partnerEmail, value); }

        private string _partnerAddress = string.Empty;
        public string PartnerAddress { get => _partnerAddress; set => SetProperty(ref _partnerAddress, value); }

        private string _partnerNote = string.Empty;
        public string PartnerNote { get => _partnerNote; set => SetProperty(ref _partnerNote, value); }

        // --- Partner / Supplier Modal Fields & KPI Counts ---
        private bool _isPartnerModalOpen;
        public bool IsPartnerModalOpen
        {
            get => _isPartnerModalOpen;
            set => SetProperty(ref _isPartnerModalOpen, value);
        }

        public int TotalPartnersCount => Partners?.Count ?? 0;
        public int MonthlyPurchaseOrdersCount => PurchaseOrders?.Count(p => p.OrderDate.Month == DateTime.Today.Month && p.OrderDate.Year == DateTime.Today.Year) ?? 0;
        public decimal TotalSupplierPurchaseValue => PurchaseOrders?.Sum(p => p.TotalAmount) ?? 0;

        public ICommand OpenPartnerManageModalCommand { get; set; }
        public ICommand OpenAddPartnerModalCommand { get; set; }
        public ICommand OpenManageSupplierModalCommand { get; set; }
        public ICommand SavePartnerCommand { get; set; }
        public ICommand ClosePartnerModalCommand { get; set; }
        public ICommand ClearPartnerFilterCommand { get; set; }

        // --- Claim Management Fields ---
        private string _claimSearchKeyword = string.Empty;
        public string ClaimSearchKeyword { get => _claimSearchKeyword; set => SetProperty(ref _claimSearchKeyword, value); }



        private Claim _selectedClaim = new Claim();
        public Claim SelectedClaim
        {
            get => _selectedClaim;
            set
            {
                if (SetProperty(ref _selectedClaim, value) && value != null)
                {
                    ClaimSalesId = value.Sales_ID;
                    ClaimProId = value.Pro_ID;
                    ClaimNo = value.ClaimNo;
                    ClaimType = value.ClaimType;
                    ClaimSalesOrderRefNo = value.SalesOrderRefNo;
                    ClaimSaleDate = value.SaleDate;
                    ClaimCustomerName = value.CustomerName;
                    ClaimCustomerPhone = value.CustomerPhone;
                    ClaimStockInRefNo = value.StockInRefNo;
                    ClaimStockInDate = value.StockInDate;
                    ClaimProductName = value.ProductName;
                    ClaimProductCode = value.ProductCode;
                    ClaimQuantity = value.Quantity;
                    ClaimReason = value.Reason;
                    ClaimNote = value.Note;
                    ClaimStatus = value.Status;
                    ClaimAction = string.IsNullOrWhiteSpace(value.ClaimAction) ? "เปลี่ยนสินค้าใหม่" : value.ClaimAction;
                    CurrentClaimItems.Clear();
                    if (!string.IsNullOrWhiteSpace(value.ProductName) || value.Pro_ID > 0)
                    {
                        CurrentClaimItems.Add(new ClaimItemLine
                        {
                            Pro_ID = value.Pro_ID,
                            ProductCode = value.ProductCode,
                            ProductName = value.ProductName,
                            ReceiptQty = value.Quantity,
                            ClaimQty = value.Quantity
                        });
                    }
                    RefreshClaimValidation();
                    OnPropertyChanged(nameof(IsClaimSelected));
                }
            }
        }

        private int _claimSalesId;
        public int ClaimSalesId
        {
            get => _claimSalesId;
            set => SetProperty(ref _claimSalesId, value);
        }

        private int _claimProId;
        public int ClaimProId
        {
            get => _claimProId;
            set => SetProperty(ref _claimProId, value);
        }

        private string _claimNo = string.Empty;
        public string ClaimNo { get => _claimNo; set => SetProperty(ref _claimNo, value); }

        private string _claimType = "ลูกค้า";
        public string ClaimType
        {
            get => _claimType;
            set
            {
                if (SetProperty(ref _claimType, value))
                {
                    RefreshClaimValidation();
                    OnPropertyChanged(nameof(SearchReceiptText));
                    UpdateGhostTextSuggestion(SearchReceiptText);
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                    (AddClaimCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (UpdateClaimCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _claimSalesOrderRefNo = string.Empty;
        public string ClaimSalesOrderRefNo
        {
            get => _claimSalesOrderRefNo;
            set
            {
                if (SetProperty(ref _claimSalesOrderRefNo, value))
                {
                    OnPropertyChanged(nameof(SearchReceiptText));
                    LookupSalesOrderForClaim();
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                    (AddClaimCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (UpdateClaimCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private DateTime? _claimSaleDate;
        public DateTime? ClaimSaleDate { get => _claimSaleDate; set { if (SetProperty(ref _claimSaleDate, value)) RefreshClaimValidation(); } }

        private string _claimCustomerName = string.Empty;
        public string ClaimCustomerName { get => _claimCustomerName; set => SetProperty(ref _claimCustomerName, value); }

        private string _claimCustomerPhone = string.Empty;
        public string ClaimCustomerPhone { get => _claimCustomerPhone; set => SetProperty(ref _claimCustomerPhone, value); }

        public string ClaimSenderName => "ร้านพอใจ ทุกอย่าง 20 บาท";
        public string ClaimSenderPhone => !string.IsNullOrWhiteSpace(CurrentUser?.Phone) ? CurrentUser.Phone : "02-111-2222";

        private string _ghostTextPrefix = string.Empty;
        public string GhostTextPrefix
        {
            get => _ghostTextPrefix;
            set => SetProperty(ref _ghostTextPrefix, value);
        }

        private string _ghostTextSuggestion = string.Empty;
        public string GhostTextSuggestion
        {
            get => _ghostTextSuggestion;
            set => SetProperty(ref _ghostTextSuggestion, value);
        }

        public void ClearGhostText()
        {
            GhostTextPrefix = string.Empty;
            GhostTextSuggestion = string.Empty;
        }

        public string SearchReceiptText
        {
            get => ClaimType == "บริษัทคู่ค้า" ? ClaimStockInRefNo : ClaimSalesOrderRefNo;
            set
            {
                string norm = value ?? string.Empty;
                if (ClaimType == "บริษัทคู่ค้า")
                {
                    if (ClaimStockInRefNo != norm)
                    {
                        ClaimStockInRefNo = norm;
                    }
                }
                else
                {
                    if (ClaimSalesOrderRefNo != norm)
                    {
                        ClaimSalesOrderRefNo = norm;
                    }
                }
                OnPropertyChanged(nameof(SearchReceiptText));
                UpdateGhostTextSuggestion(norm);
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                (AddClaimCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (UpdateClaimCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private System.Collections.Generic.List<string> _cachedDocumentNumbers = new System.Collections.Generic.List<string>();

        public async System.Threading.Tasks.Task RefreshDocumentNumbersCacheAsync()
        {
            try
            {
                var list = await System.Threading.Tasks.Task.Run(() => _databaseService.GetCachedDocumentNumbers());
                if (list != null)
                {
                    _cachedDocumentNumbers = list;
                }
            }
            catch { }
        }

        public void UpdateGhostTextSuggestion(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                ClearGhostText();
                return;
            }

            var trimmed = input.Trim();

            // เงื่อนไข Case-Sensitive อย่างเคร่งครัด:
            // หากผู้ใช้พิมพ์ตัวพิมพ์เล็ก เช่น "sa" หรือ "sale" -> ต้อง "ไม่แสดง" ข้อความแนะนำใดๆ ทั้งสิ้น
            // ฝั่งเลขที่ใบรับสินค้าของคู่ค้า: ต้องพิมพ์ "RC-" ตัวพิมพ์ใหญ่เท่านั้น หากพิมพ์ "rc" จะต้องไม่ขึ้นแนะนำ
            if (ClaimType == "บริษัทคู่ค้า")
            {
                if (!trimmed.StartsWith("R", System.StringComparison.Ordinal))
                {
                    ClearGhostText();
                    return;
                }
            }
            else
            {
                if (!trimmed.StartsWith("S", System.StringComparison.Ordinal))
                {
                    ClearGhostText();
                    return;
                }
            }

            if (_cachedDocumentNumbers.Count == 0)
            {
                try
                {
                    _cachedDocumentNumbers = _databaseService.GetCachedDocumentNumbers();
                }
                catch { }
            }

            string? match = null;
            if (ClaimType == "บริษัทคู่ค้า")
            {
                match = _cachedDocumentNumbers.FirstOrDefault(doc =>
                    doc.StartsWith("RC-", System.StringComparison.Ordinal) &&
                    doc.StartsWith(trimmed, System.StringComparison.Ordinal));

                if (string.IsNullOrEmpty(match))
                {
                    match = _databaseService.GetStockInSuggestion(trimmed);
                    if (!string.IsNullOrEmpty(match) && !_cachedDocumentNumbers.Contains(match))
                    {
                        _cachedDocumentNumbers.Add(match);
                    }
                }
            }
            else
            {
                // ตรวจสอบเลขที่ใบเสร็จจริงในรูปแบบ SALE-XXXXXXXXXXXXXX (14 หลัก รวม 19 ตัวอักษร)
                match = _cachedDocumentNumbers.FirstOrDefault(doc =>
                    ValidationHelper.IsSaleReference(doc) &&
                    doc.StartsWith(trimmed, System.StringComparison.Ordinal));

                if (string.IsNullOrEmpty(match))
                {
                    match = _databaseService.GetReceiptSuggestion(trimmed);
                    if (!string.IsNullOrEmpty(match) && !_cachedDocumentNumbers.Contains(match))
                    {
                        _cachedDocumentNumbers.Add(match);
                    }
                }
            }

            // หากข้อความที่พิมพ์ตรงกับส่วนหน้าของรหัสบิลแบบ Case-Sensitive และยังพิมพ์ไม่ครบ:
            // ให้แสดงเฉพาะส่วนที่เหลือต่อท้ายเคอร์เซอร์ เช่น พิมพ์ "SALE-2026" ส่วน Ghost Text คือ "0914024806"
            if (!string.IsNullOrEmpty(match) && 
                !string.Equals(match, trimmed, System.StringComparison.Ordinal) && 
                match.StartsWith(trimmed, System.StringComparison.Ordinal))
            {
                GhostTextPrefix = trimmed;
                GhostTextSuggestion = match.Substring(trimmed.Length);
            }
            else
            {
                ClearGhostText();
            }
        }

        public string? GetCanonicalDocumentNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            var trimmed = input.Trim();

            if (ClaimType == "บริษัทคู่ค้า")
            {
                if (!trimmed.StartsWith("R", System.StringComparison.Ordinal)) return null;
                var match = _cachedDocumentNumbers.FirstOrDefault(doc =>
                    doc.StartsWith("RC-", System.StringComparison.Ordinal) &&
                    doc.StartsWith(trimmed, System.StringComparison.Ordinal));
                if (!string.IsNullOrEmpty(match)) return match;
                return _databaseService.GetStockInSuggestion(trimmed);
            }
            else
            {
                if (!trimmed.StartsWith("S", System.StringComparison.Ordinal)) return null;
                var match = _cachedDocumentNumbers.FirstOrDefault(doc =>
                    ValidationHelper.IsSaleReference(doc) &&
                    doc.StartsWith(trimmed, System.StringComparison.Ordinal));
                if (!string.IsNullOrEmpty(match)) return match;
                return _databaseService.GetReceiptSuggestion(trimmed);
            }
        }

        private string _claimStockInRefNo = string.Empty;
        public string ClaimStockInRefNo
        {
            get => _claimStockInRefNo;
            set
            {
                if (SetProperty(ref _claimStockInRefNo, value))
                {
                    OnPropertyChanged(nameof(SearchReceiptText));
                    LookupStockInForClaim();
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                    (AddClaimCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (UpdateClaimCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private DateTime? _claimStockInDate;
        public DateTime? ClaimStockInDate { get => _claimStockInDate; set { if (SetProperty(ref _claimStockInDate, value)) RefreshClaimValidation(); } }

        private string _claimProductName = string.Empty;
        public string ClaimProductName { get => _claimProductName; set => SetProperty(ref _claimProductName, value); }

        private string _claimProductCode = string.Empty;
        public string ClaimProductCode { get => _claimProductCode; set => SetProperty(ref _claimProductCode, value); }

        private int _claimQuantity = 1;
        public int ClaimQuantity { get => _claimQuantity; set => SetProperty(ref _claimQuantity, value); }

        private string _claimReason = string.Empty;
        public string ClaimReason { get => _claimReason; set => SetProperty(ref _claimReason, value); }

        private string _claimNote = string.Empty;
        public string ClaimNote { get => _claimNote; set => SetProperty(ref _claimNote, value); }

        private string _claimStatus = "รอดำเนินการ";
        public string ClaimStatus { get => _claimStatus; set => SetProperty(ref _claimStatus, value); }

        private string _claimAction = "เปลี่ยนสินค้าใหม่";
        public string ClaimAction { get => _claimAction; set => SetProperty(ref _claimAction, value); }

        private string _claimValidationMessage = string.Empty;
        public string ClaimValidationMessage { get => _claimValidationMessage; set => SetProperty(ref _claimValidationMessage, value); }

        private bool _claimValidationOk = true;
        public bool ClaimValidationOk { get => _claimValidationOk; set => SetProperty(ref _claimValidationOk, value); }

        private SalesOrder _selectedOrder;
        public SalesOrder SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (SetProperty(ref _selectedOrder, value))
                {
                    LoadOrderItems(_selectedOrder?.Id ?? 0);
                }
            }
        }

        private DateTime _selectedHistoryDate = DateTime.Today;
        public DateTime SelectedHistoryDate
        {
            get => _selectedHistoryDate;
            set
            {
                if (SetProperty(ref _selectedHistoryDate, value))
                {
                    _ = LoadSalesHistory();
                }
            }
        }

        // Report Data
        private decimal _reportTotalValue;
        private decimal _reportTotalCost;
        public ObservableCollection<BestSellerItem> ReportBestSellers { get; set; } = new ObservableCollection<BestSellerItem>();
        public ObservableCollection<Product> ReportDeadStock { get; set; } = new ObservableCollection<Product>();

        // --- Report Management System ---
        private string _selectedReportType = "รายงานข้อมูลลูกค้า";
        public string SelectedReportType { get => _selectedReportType; set { if (SetProperty(ref _selectedReportType, value)) { UpdateReportHeaders(); _ = GenerateReport(); } } }

        private string _selectedReportPeriod = "ทั้งหมด";
        public string SelectedReportPeriod { get => _selectedReportPeriod; set { if (SetProperty(ref _selectedReportPeriod, value)) { ApplyPeriodFilter(); _ = GenerateReport(); } } }

        private DateTime _reportStartDate = DateTime.Today.AddMonths(-1);
        public DateTime ReportStartDate { get => _reportStartDate; set => SetProperty(ref _reportStartDate, value); }

        private DateTime _reportEndDate = DateTime.Today;
        public DateTime ReportEndDate { get => _reportEndDate; set => SetProperty(ref _reportEndDate, value); }

        private bool _isGeneratingReport = false;
        public bool IsGeneratingReport { get => _isGeneratingReport; set => SetProperty(ref _isGeneratingReport, value); }

        private string _reportSummaryText = string.Empty;
        public string ReportSummaryText { get => _reportSummaryText; set => SetProperty(ref _reportSummaryText, value); }

        // Dynamic column headers
        public string ColHeader1 { get => _colHeaders[0]; }
        public string ColHeader2 { get => _colHeaders[1]; }
        public string ColHeader3 { get => _colHeaders[2]; }
        public string ColHeader4 { get => _colHeaders[3]; }
        public string ColHeader5 { get => _colHeaders[4]; }

        private string[] _colHeaders = new string[] { "คอลัมน์ 1", "คอลัมน์ 2", "คอลัมน์ 3", "คอลัมน์ 4", "คอลัมน์ 5" };

        private void UpdateReportHeaders()
        {
            _colHeaders = SelectedReportType switch
            {
                "รายงานข้อมูลพนักงาน"           => new[] { "ID", "ชื่อผู้ใช้งาน", "บทบาท", "เบอร์โทร", "-" },
                "รายงานข้อมูลลูกค้า"             => new[] { "ชื่อลูกค้า", "เบอร์โทร", "ที่อยู่", "-", "-" },
                "รายงานข้อมูลสินค้า"             => new[] { "ชื่อสินค้า", "ราคา (บาท)", "คงเหลือ", "ประเภท", "-" },
                "รายงานข้อมูลบริษัทคู่ค้า"       => new[] { "ชื่อบริษัท", "ผู้ติดต่อ", "เบอร์โทรศัพท์", "ที่อยู่", "-" },
                "รายงานข้อมูลประเภทสินค้า"       => new[] { "รหัสสินค้า", "ชื่อประเภท", "ชื่อสินค้า", "รูปภาพประกอบ", "-" },
                "รายงานข้อมูลการสั่งซื้อสินค้า"  => new[] { "เลขที่ใบสั่งซื้อ", "ซัพพลายเออร์", "ยอดรวม (บาท)", "สถานะ", "วันที่สั่งซื้อ" },
                "รายงานข้อมูลการรับเข้าสินค้า"   => new[] { "เลขที่อ้างอิง", "ชื่อสินค้า", "จำนวนรับเข้า", "ซัพพลายเออร์", "วันที่รับเข้า" },
                "รายงานข้อมูลการขายหน้าร้าน"     => new[] { "เลขที่บิล", "ยอดรวม (บาท)", "วิธีชำระเงิน", "สถานะ", "วันที่ขาย" },
                "รายงานข้อมูลรายรับ-รายจ่าย"     => new[] { "ประเภท", "เลขอ้างอิง / รายละเอียด", "จำนวนเงิน (บาท)", "หมวดหมู่ / วิธีชำระ", "วันที่" },
                "รายงานข้อมูลการจัดส่งสินค้า"    => new[] { "เลขที่บิล", "ชื่อลูกค้า", "เบอร์โทร", "สถานะจัดส่ง", "วันที่สั่ง" },
                "รายงานข้อมูลการเคลม"          => new[] { "เลขที่เคลม", "สินค้า / รายละเอียด", "สถานะเคลม", "รูปแบบการเคลม", "วันที่เคลม" },
                _                                 => new[] { "คอลัมน์ 1", "คอลัมน์ 2", "คอลัมน์ 3", "คอลัมน์ 4", "คอลัมน์ 5" },
            };
            OnPropertyChanged(nameof(ColHeader1));
            OnPropertyChanged(nameof(ColHeader2));
            OnPropertyChanged(nameof(ColHeader3));
            OnPropertyChanged(nameof(ColHeader4));
            OnPropertyChanged(nameof(ColHeader5));
        }

        public ObservableCollection<string> ReportTypes { get; } = new ObservableCollection<string>
        {
            "รายงานข้อมูลลูกค้า",
            "รายงานข้อมูลพนักงาน",
            "รายงานข้อมูลสินค้า",
            "รายงานข้อมูลบริษัทคู่ค้า",
            "รายงานข้อมูลประเภทสินค้า",
            "รายงานข้อมูลการสั่งซื้อสินค้า",
            "รายงานข้อมูลการรับเข้าสินค้า",
            "รายงานข้อมูลการขายหน้าร้าน",
            "รายงานข้อมูลการเคลม",
            "รายงานข้อมูลการจัดส่งสินค้า",
            "รายงานข้อมูลรายรับ-รายจ่าย",
        };

        public ObservableCollection<string> ReportPeriods { get; } = new ObservableCollection<string>
        {
            "ทั้งหมด", "วันนี้", "7 วันล่าสุด", "เดือนนี้", "ปีนี้"
        };

        public ObservableCollection<dynamic> ReportResults { get; set; } = new ObservableCollection<dynamic>();
        
                // --- Reports Dynamic Visualization & Split Summary Properties ---
        private SeriesCollection _reportCartesianSeries = new SeriesCollection();
        public SeriesCollection ReportCartesianSeries
        {
            get => _reportCartesianSeries;
            set => SetProperty(ref _reportCartesianSeries, value);
        }
        private SeriesCollection _reportPieSeries = new SeriesCollection();
        public SeriesCollection ReportPieSeries
        {
            get => _reportPieSeries;
            set => SetProperty(ref _reportPieSeries, value);
        }

        private SeriesCollection _reportChartSeries = new SeriesCollection();
        public SeriesCollection ReportChartSeries
        {
            get => _reportChartSeries;
            set => SetProperty(ref _reportChartSeries, value);
        }

        private ObservableCollection<string> _reportChartLabels = new ObservableCollection<string>();
        public ObservableCollection<string> ReportChartLabels
        {
            get => _reportChartLabels;
            set => SetProperty(ref _reportChartLabels, value);
        }

        private Func<double, string> _reportChartFormatter = value => value.ToString("N0");
        public Func<double, string> ReportChartFormatter
        {
            get => _reportChartFormatter;
            set => SetProperty(ref _reportChartFormatter, value);
        }

        private string _reportChartTitle = "กราฟสรุปข้อมูล";
        public string ReportChartTitle
        {
            get => _reportChartTitle;
            set => SetProperty(ref _reportChartTitle, value);
        }

        private int _reportChartDisplayMode = 0; // 0: Cartesian (Bar/Line), 1: Pie (Doughnut)
        public int ReportChartDisplayMode
        {
            get => _reportChartDisplayMode;
            set => SetProperty(ref _reportChartDisplayMode, value);
        }

        private ObservableCollection<ReportSummaryItem> _reportSummaryList = new ObservableCollection<ReportSummaryItem>();
        public ObservableCollection<ReportSummaryItem> ReportSummaryList
        {
            get => _reportSummaryList;
            set => SetProperty(ref _reportSummaryList, value);
        }

        private string _reportSummaryHeaderTitle = "🏆 สรุปรายการสำคัญ";
        public string ReportSummaryHeaderTitle
        {
            get => _reportSummaryHeaderTitle;
            set => SetProperty(ref _reportSummaryHeaderTitle, value);
        }

        private string _reportSummaryTotalCountText = "รวมทั้งหมด 0 รายการ";
        public string ReportSummaryTotalCountText
        {
            get => _reportSummaryTotalCountText;
            set => SetProperty(ref _reportSummaryTotalCountText, value);
        }

        public ObservableCollection<ReportRow> ReportRows { get; set; } = new ObservableCollection<ReportRow>();
        public bool HasReportData => (ReportRows != null && ReportRows.Any()) || (ReportSummaryList != null && ReportSummaryList.Any());
        public ICommand GenerateReportCommand { get; }
        public ICommand ProcessReportCommand { get; }
        public ICommand ExportExcelCommand { get; set; }

        public ProductViewModel()
        {
            _databaseService = new DatabaseService();

            // Load product categories into CategoriesList on application startup
            LoadProductCategories();

            // Explicitly initialize all modal state flags to false on startup to prevent frozen screen overlays
            IsModalOpen = false;
            IsAddModalOpen = false;
            IsEditModalOpen = false;
            IsConfirmDialogOpen = false;
            IsCategoryModalOpen = false;

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentView) || e.PropertyName == nameof(CurrentUser))
                {
                    NotifyReadOnlyProperties();
                }
            };

            CloseMainAlertCommand = new RelayCommand(_ => IsMainAlertOpen = false);
            ConfirmMainActionCommand = new RelayCommand(_ =>
            {
                IsMainConfirmOpen = false;
                _pendingConfirmAction?.Invoke();
                _pendingConfirmAction = null;
            });
            CancelMainActionCommand = new RelayCommand(_ =>
            {
                IsMainConfirmOpen = false;
                _pendingConfirmAction = null;
            });

            LoadProductsCommand = new RelayCommand(async _ => await LoadProducts());
            AddCommand = new RelayCommand(async _ => await AddProduct(), _ => CanExecuteSubmit());
            UpdateCommand = new RelayCommand(async _ => await UpdateProduct(), _ => SelectedProduct != null && CanExecuteSubmit());
            SaveAdjustmentCommand = new RelayCommand(async _ => await SaveStockAdjustment(), _ => SelectedProduct != null && AdjustmentQuantity != 0 && !string.IsNullOrEmpty(SelectedAdjustmentReason));
            DeleteCommand = new RelayCommand(async _ => await DeleteProduct(), _ => SelectedProduct != null);
            ClearCommand = new RelayCommand(_ => ClearForm());
            BrowseImageCommand = new RelayCommand(_ => BrowseImage());
            RemoveImageCommand = new RelayCommand(_ => ImagePath = string.Empty);
            SearchCommand = new RelayCommand(_ => FilterProducts());
            OpenAddCategoryModalCommand = new RelayCommand(_ => ExecuteOpenAddCategoryModal());
            OpenManageCategoryModalCommand = new RelayCommand(p => ExecuteOpenManageCategoryModal(p as ProductCategory));
            OpenEditCategoryModalCommand = OpenManageCategoryModalCommand;
            SaveCategoryCommand = new RelayCommand(_ => ExecuteSaveCategory());
            DeleteCategoryCommand = new RelayCommand(_ => ExecuteDeleteCategory());
            ClearCategoryCommand = new RelayCommand(_ => ClearCategoryForm());
            CloseCategoryModalCommand = new RelayCommand(_ => { IsCategoryModalOpen = false; IsModalOpen = false; CurrentModalContent = string.Empty; });
            
            SwitchToPosCommand = new RelayCommand(async _ => await NavigateAsync(async () => { CurrentView = "pos"; await Task.CompletedTask; }));
            SwitchToAdminCommand = new RelayCommand(async _ => await NavigateAsync(async () => { CurrentView = "stock_manage"; await LoadProducts(); }));
            SwitchToStockInCommand = new RelayCommand(async _ => await NavigateAsync(async () => { CurrentView = "stock_in"; await LoadPendingPurchaseOrders(); await LoadStockTransactions(); }));
            SwitchToReportsCommand = new RelayCommand(async _ => await NavigateAsync(async () => { CurrentView = "reports"; await SwitchToReports(); }));
            GenerateReportCommand = new RelayCommand(async _ => await GenerateReport());
            ProcessReportCommand = GenerateReportCommand;
            ExportExcelCommand = new RelayCommand(async _ => await ExecuteExportExcel(), _ => HasReportData);
            ExportReportCommand = ExportExcelCommand;
            PrintReportCommand = new RelayCommand(_ =>
            {
                ShowAlert("ส่งคำสั่งพิมพ์รายงานไปยังเครื่องพิมพ์เรียบร้อยแล้ว", "พิมพ์รายงาน", "🖨️");
            });
            ClearReportFilterCommand = new RelayCommand(async _ =>
            {
                SelectedReportType = ReportTypes.FirstOrDefault() ?? "รายงานข้อมูลลูกค้า";
                SelectedReportPeriod = "ทั้งหมด";
                ReportStartDate = DateTime.Today;
                ReportEndDate = DateTime.Today;
                ReportRows.Clear();
                await GenerateReport();
                OnPropertyChanged(nameof(HasReportData));
                CommandManager.InvalidateRequerySuggested();
            });
            OpenReportDetailModalCommand = new RelayCommand(param =>
            {
                if (param is ReportRow row) SelectedReportRow = row;
                IsReportModalOpen = true;
            });
            CloseReportModalCommand = new RelayCommand(_ => { IsReportModalOpen = false; });
            OpenHistoryCommand = new RelayCommand(async _ => await NavigateAsync(async () => { CurrentView = "sales_history"; await Task.CompletedTask; }));
            OpenSalesHistoryCommand = new RelayCommand(async _ => await NavigateAsync(async () => { CurrentView = "sales_history"; await Task.CompletedTask; }));
            PrintOrderCommand = new RelayCommand(_ => PrintSelectedOrder());
            SwitchToHomeCommand = new RelayCommand(async _ => await NavigateAsync(async () => { CurrentView = "home"; await LoadDashboardData(); }));
            OpenDeliveryCommand = new RelayCommand(async _ => await NavigateAsync(async () => { CurrentView = "delivery"; await OpenDeliveryMode(); }));
            OpenCustomerCommand = new RelayCommand(async _ => await NavigateAsync(async () => { CurrentView = "customer"; await OpenCustomerMode(); }));
            OpenPartnerCommand = new RelayCommand(async _ => await NavigateAsync(async () => { CurrentView = "partner"; await OpenPartnerMode(); }));
            OpenClaimCommand = new RelayCommand(async _ => await NavigateAsync(async () => { CurrentView = "claim"; await OpenClaimMode(); }));
            
            // Purchase Order Commands
            OpenPurchaseOrderCommand = new RelayCommand(async _ => await NavigateAsync(async () => { CurrentView = "purchase_order"; await OpenPurchaseOrderMode(); }));
            SearchPOCommand = new RelayCommand(_ => _ = LoadPurchaseOrders());
            AddPOCommand = new RelayCommand(_ => SavePurchaseOrder(), _ => CurrentPOItems.Count > 0 && !string.IsNullOrWhiteSpace(POSupplierName));
            DeletePOCommand = new RelayCommand(_ => DeletePurchaseOrder(), _ => SelectedPurchaseOrder != null && SelectedPurchaseOrder.Id > 0);
            ClearPOCommand = new RelayCommand(_ => ClearPurchaseOrderForm());
            ReceivePOCommand = new RelayCommand(async param =>
            {
                if (param is PurchaseOrder po)
                {
                    OpenGoodsReceiptModal(po);
                    await NavigateAsync(async () => { CurrentView = "stock_in"; await Task.CompletedTask; });
                }
            });
            AddPOItemCommand = new RelayCommand(_ => AddPOItem(), _ => !string.IsNullOrWhiteSpace(POItemProductName) && POItemQuantity > 0);
            SelectPOSuggestionCommand = new RelayCommand(param => SelectPOSuggestion(param as Product));
            ClosePOSuggestionsCommand = new RelayCommand(_ => { _poSuggestedProducts.Clear(); IsPOSuggestionsOpen = false; });
            OpenPOItemModalCommand = new RelayCommand(_ => { ClearPOItemForm(); IsItemModalOpen = true; });
            ClosePOItemModalCommand = new RelayCommand(_ => { ClearPOItemForm(); IsItemModalOpen = false; });
            RemovePOItemCommand = new RelayCommand(param => RemovePOItem(param as PurchaseOrderItem));
            PrintPOCommand = new RelayCommand(_ => PrintPurchaseOrder(), _ => SelectedPurchaseOrder != null && SelectedPurchaseOrder.Id > 0);
            ScanPOLowStockProductCommand = new RelayCommand(param => AddLowStockToPO(param as Product));
            OpenAddPOModalCommand = new RelayCommand(_ =>
            {
                ClearPurchaseOrderForm();
                IsNewPOMode = true;
                IsPOReadOnly = false;
                POModalTitle = "สร้างใบสั่งซื้อสินค้าใหม่";
                IsPOModalOpen = true;
                _ = LoadSuppliersAsync();
                _ = LoadProducts();
            });
            OpenEditPOModalCommand = new RelayCommand(async param =>
            {
                var po = (param as PurchaseOrder) ?? SelectedPurchaseOrder;
                if (po != null && po.Id > 0)
                {
                    await LoadPODetailsAsync(po, isReadOnly: true);
                }
                else
                {
                    ShowAlert("กรุณาคลิกเลือกรายการใบสั่งซื้อในตารางก่อนกดปุ่มจัดการ", "แจ้งเตือน", "⚠️");
                }
            });
            ClosePOModalCommand = new RelayCommand(_ => { IsPOModalOpen = false; });
            ClearPOFilterCommand = new RelayCommand(_ => ExecuteClearPOFilter());
            AutoFillReorderItemsCommand = new RelayCommand(async _ => await AutoFillReorderItems());
            CreatePOFromSelectedLowStockCommand = new RelayCommand(async _ => await CreatePOFromSelectedLowStock());
            SelectPOSuggestionItemCommand = new RelayCommand(param => SelectPOSuggestionItem(param as PurchaseOrder));

            // Goods Receipt Commands
            OpenGoodsReceiptModalCommand = new RelayCommand(param => OpenGoodsReceiptModal(param as PurchaseOrder));
            CloseGoodsReceiptModalCommand = new RelayCommand(_ => { IsGoodsReceiptModalOpen = false; });
            ConfirmGoodsReceiptCommand = new RelayCommand(_ => ConfirmGoodsReceipt(), _ => CanConfirmGoodsReceipt);
            SearchPendingPOCommand = new RelayCommand(_ => _ = LoadPendingPurchaseOrders());
            ClearStockInFilterCommand = new RelayCommand(_ => ExecuteClearStockInFilter());
            ClearStockSearchCommand = new RelayCommand(_ => 
            { 
                SearchText = string.Empty; 
                SelectedStockStatusFilter = "ทั้งหมด";
            });


            SearchDeliveriesCommand = new RelayCommand(_ => _ = LoadDeliveries());
            PrintDeliverySlipCommand = new RelayCommand(param => PrintDeliverySlip((param as SalesOrder) ?? SelectedDeliveryOrder));
            UpdateDeliveryStatusCommand = new RelayCommand(_ =>
            {
                if (SelectedDeliveryOrder != null)
                {
                    string statusToSave = SelectedDeliveryOrderStatus ?? SelectedDeliveryOrder.DeliveryStatus ?? "รอจัดส่ง";
                    string cleanStatus = statusToSave.Replace("⌛", "").Replace("🚚", "").Replace("☑", "").Replace("✖", "").Trim();
                    UpdateDeliveryStatus(SelectedDeliveryOrder, cleanStatus);
                    IsDeliveryDetailModalOpen = false;
                    _ = LoadDeliveries();
                    ShowAlert("อัปเดตสถานะจัดส่งเรียบร้อยแล้ว", "สำเร็จ", "🎉");
                }
            });
            OpenManageModalCommand = new RelayCommand(_ =>
            {
                if (SelectedDeliveryOrder != null)
                {
                    string statusToSave = SelectedDeliveryOrderStatus ?? SelectedDeliveryOrder.DeliveryStatus ?? "รอจัดส่ง";
                    string cleanStatus = statusToSave.Replace("⌛", "").Replace("🚚", "").Replace("☑", "").Replace("✖", "").Trim();
                    UpdateDeliveryStatus(SelectedDeliveryOrder, cleanStatus);
                    IsManageModalOpen = false;
                    _ = LoadDeliveries();
                    ShowAlert("อัปเดตสถานะจัดส่งเรียบร้อยแล้ว", "สำเร็จ", "🎉");
                }
            });
            CloseManageModalCommand = new RelayCommand(_ => IsManageModalOpen = false);
            SaveDeliveryStatusCommand = new RelayCommand(_ => SaveDeliveryStatus());
            FilterDeliveryStatusCommand = new RelayCommand(param =>
            {
                _selectedDeliveryStatusFilter = param?.ToString() ?? "ทั้งหมด";
                OnPropertyChanged(nameof(SelectedDeliveryStatusFilter));
                _ = LoadDeliveries();
            });
            ClearDeliveryFilterCommand = new RelayCommand(_ =>
            {
                DeliverySearchKeyword = string.Empty;
                _selectedDeliveryStatusFilter = "ทั้งหมด";
                OnPropertyChanged(nameof(SelectedDeliveryStatusFilter));
                _deliveryStartDate = DateTime.Today.AddMonths(-1);
                OnPropertyChanged(nameof(DeliveryStartDate));
                _deliveryEndDate = DateTime.Today;
                OnPropertyChanged(nameof(DeliveryEndDate));
                _ = LoadDeliveries();
            });
            CloseDeliveryDetailCommand = new RelayCommand(_ =>
            {
                IsDeliveryDetailModalOpen = false;
                _ = LoadDeliveries();
            });
            ViewDeliveryDetailsCommand = new RelayCommand(param =>
            {
                if (param is SalesOrder order)
                {
                    SelectedDeliveryOrder = order;
                    string currentStatus = order.DeliveryStatus;
                    if (currentStatus == "ยกเลิกรายการ") currentStatus = "ยกเลิก";
                    if (currentStatus == "รอดำเนินการ") currentStatus = "รอจัดส่ง";
                    SelectedDeliveryOrderStatus = currentStatus ?? "รอจัดส่ง";
                    IsDeliveryDetailModalOpen = true;
                }
            });

            // ── Stock Management Modal Commands ──────────────────────────────────
            OpenAddModalCommand = new RelayCommand(_ =>
            {
                ExecuteOpenAddProductModal();
                SelectedProduct = new Product { SellingPrice = 20, CostPrice = 0, StockQuantity = 0, ReorderPoint = 10, Unit = string.Empty };
                SellingPrice = 20; // Default store price 20 baht
                CostPrice = 0;
                StockQuantity = 0;
                ReorderPoint = 10;
                Unit = string.Empty;
                IsProductModalOpen = true;
                IsAddModalOpen = true; // Trigger Modal
            });
            OpenAddProductCommand = OpenAddModalCommand;
            CloseAddModalCommand = new RelayCommand(_ =>
            {
                IsProductModalOpen = false;
                IsAddModalOpen = false;
                ExecuteCloseProductModal();
            });
            CloseProductModalCommand = CloseAddModalCommand;
            SaveProductCommand = new RelayCommand(async param =>
            {
                await ExecuteSaveProduct();
            });
            ClearFormCommand = new RelayCommand(_ => ClearForm());
            CloseModalCommand = new RelayCommand(_ => ExecuteCloseProductModal());
            DeleteProductCommand = new RelayCommand(async _ => await ExecuteDeleteProduct());
            ToggleManagementModalCommand = new RelayCommand(_ => ExecuteOpenAddProductModal());
            CloseStockManageModalCommand = new RelayCommand(_ => ExecuteCloseProductModal());
            StockAddProductCommand = new RelayCommand(_ => ExecuteOpenAddProductModal());
            StockEditProductCommand = new RelayCommand(_ => ExecuteOpenEditProductModal());
            StockDeleteProductCommand = new RelayCommand(async _ => await ExecuteDeleteProduct());
            SaveProductFormCommand = new RelayCommand(async _ => await ExecuteSaveProduct());
            CancelProductFormCommand = new RelayCommand(_ => ExecuteCloseProductModal());
            EnterProductEditModeCommand = new RelayCommand(_ => ExecuteEnterProductEditMode());
            CancelProductEditCommand = new RelayCommand(_ => ExecuteCancelProductEdit());
            // ── End Stock Management Modal Commands ──────────────────────────────
            
            SwitchToStaffCommand = new RelayCommand(async _ => await NavigateAsync(async () => { CurrentView = "staff"; await OpenStaffMode(); }));
            PerformLoginCommand = new RelayCommand(async _ => await PerformLogin(), _ => !string.IsNullOrEmpty(LoginUsername) && !string.IsNullOrEmpty(LoginPassword));
            CancelLoginCommand = new RelayCommand(_ => { IsLoginDialogVisible = false; });
            ShowLoginCommand = new RelayCommand(_ => { IsLoginDialogVisible = true; });
            LogoutCommand = new RelayCommand(_ => Logout());

            AddUserCommand = new RelayCommand(async _ => await AddUser(), _ => SelectedUser != null && !string.IsNullOrEmpty(SelectedUser.Username));
            UpdateUserCommand = new RelayCommand(async _ => await UpdateUser(), _ => SelectedUser != null && SelectedUser.Id > 0);
            DeleteUserCommand = new RelayCommand(async _ => await DeleteUser(), _ => SelectedUser != null && SelectedUser.Id > 0 && SelectedUser.Username != "admin");
            ClearUserCommand = new RelayCommand(_ => SelectedUser = new User());
            
            OpenStaffManageModalCommand = new RelayCommand(_ => { IsStaffModalOpen = true; });
            CloseStaffModalCommand = new RelayCommand(_ => { IsStaffModalOpen = false; });
            SearchStaffCommand = new RelayCommand(async _ => await LoadUsers());
            ClearStaffFilterCommand = new RelayCommand(async _ => { StaffSearchKeyword = string.Empty; await LoadUsers(); });
            
            // Staff Commands
            InitializeStaffViewModelCommands();

            // Customer Commands
            InitializeCustomerViewModelCommands();

            // Partner Commands
            InitializePartnerViewModelCommands();

            // Expense Commands
            InitializeExpenseViewModelCommands();
            
            BackupDatabaseCommand = new RelayCommand(_ => BackupDatabase());
            RestoreDatabaseCommand = new RelayCommand(_ => RestoreDatabase());

            // Claim Commands
            SearchClaimCommand = new RelayCommand(_ => _ = LoadClaims());
            AddClaimCommand = new RelayCommand(_ => AddClaim(), _ => CanSaveClaim());
            UpdateClaimCommand = new RelayCommand(_ => UpdateClaimRecord(), _ => CanUpdateClaim());
            DeleteClaimCommand = new RelayCommand(_ => DeleteClaimRecord(), _ => SelectedClaim != null && SelectedClaim.Id > 0);
            ClearClaimCommand = new RelayCommand(_ => ClearClaimForm());
            UpdateClaimStatusCommand = new RelayCommand(param => 
            { 
                if (RolePermissions.IsReadOnly(CurrentUser?.Role, "claim"))
                {
                    ShowAlert("ไม่มีสิทธิ์ในการดำเนินการนี้ (สิทธิ์ดูอย่างเดียว)", "ไม่มีสิทธิ์", "⚠️");
                    return;
                }
                if (SelectedClaim != null && SelectedClaim.Id > 0) 
                { 
                    _databaseService.UpdateClaimStatus(SelectedClaim.Id, ClaimStatus); 
                    _ = LoadClaims(); 
                    ShowAlert("อัปเดตสถานะสำเร็จ", "สำเร็จ", "🎉"); 
                } 
            }, _ => SelectedClaim != null && SelectedClaim.Id > 0);
            FilterClaimStatusCommand = new RelayCommand(param =>
            {
                _claimStatusFilter = param?.ToString() ?? "ทั้งหมด";
                OnPropertyChanged(nameof(ClaimStatusFilter));
                _ = LoadClaims();
            });
            ClearClaimFilterCommand = new RelayCommand(_ =>
            {
                ClaimSearchKeyword = string.Empty;
                _claimStatusFilter = "ทั้งหมด";
                OnPropertyChanged(nameof(ClaimStatusFilter));
                _claimStartDate = DateTime.Today.AddMonths(-1);
                OnPropertyChanged(nameof(ClaimStartDate));
                _claimEndDate = DateTime.Today;
                OnPropertyChanged(nameof(ClaimEndDate));
                _ = LoadClaims();
            });
            OpenAddModalCommand = new RelayCommand(_ =>
            {
                ClearClaimForm();
                IsModalOpen = true;
            });
            OpenEditModalCommand = new RelayCommand(_ =>
            {
                if (SelectedClaim != null && SelectedClaim.Id != 0)
                {
                    IsModalOpen = true;
                }
            }, _ => SelectedClaim != null && SelectedClaim.Id != 0);
            CloseModalCommand = new RelayCommand(_ => ClearClaimForm());
            LookupReceiptCommand = new RelayCommand(_ =>
            {
                if (ClaimType == "ลูกค้า") LookupSalesOrderForClaim();
                else LookupStockInForClaim();
            });
            CloseClaimDetailCommand = new RelayCommand(_ => SelectedClaim = null);
            ViewClaimDetailsCommand = new RelayCommand(param =>
            {
                if (param is Claim claim)
                {
                    SelectedClaim = claim;
                }
                if (SelectedClaim != null && SelectedClaim.Id > 0)
                {
                    IsClaimDetailModalOpen = true;
                }
            });
            PrintClaimSlipCommand = new RelayCommand(_ =>
            {
                if (SelectedClaim != null && SelectedClaim.Id != 0)
                {
                    ShowAlert($"พิมพ์ใบเคลมเลขที่: {SelectedClaim.ClaimNo}", "พิมพ์ใบเคลม", "🖨️");
                }
            });
            InitializeClaimModal();
            
            // Expense commands initialized via InitializeExpenseViewModelCommands()

            ConfirmStockInCommand = new RelayCommand(async _ => await ConfirmStockIn(), _ => SelectedProduct != null && StockInQuantity > 0);
            OpenStockInModalCommand = new RelayCommand(_ => { IsStockInModalOpen = true; });
            CloseStockInModalCommand = new RelayCommand(_ => { IsStockInModalOpen = false; });
            SearchStockInCommand = new RelayCommand(_ => _ = LoadStockTransactions());
            ScanBarcodeCommand = new RelayCommand(_ => ScanBarcode());

            // Cart Commands
            AddToCartCommand = new RelayCommand(param => AddToCart(param));
            RemoveFromCartCommand = new RelayCommand(param => RemoveFromCart(param));
            ClearCartCommand = new RelayCommand(_ => ClearCart());
            FilterCategoryCommand = new RelayCommand(param => { SelectedPosCategory = param?.ToString() ?? "ทั้งหมด"; FilterProducts(); });
            
            CheckoutCommand = new RelayCommand(_ => OpenCheckoutWizard(), _ => CartItems.Count > 0);
            AddCashCommand = new RelayCommand(param => AddCash(param));
            NumpadKeyCommand = new RelayCommand(param => NumpadKey(param?.ToString()));
            CloseCheckoutCommand = new RelayCommand(_ =>
            {
                ClearCart();
                IsCheckoutWizardOpen = false;
            });
            NextStepCommand = new RelayCommand(async _ => await NextStep());
            PrevStepCommand = new RelayCommand(_ => PrevStep());
            CloseErrorModalCommand = new RelayCommand(_ => IsErrorModalOpen = false);
            PrintReceiptCommand = new RelayCommand(_ => PrintCurrentReceipt());
            
            IncreaseQuantityCommand = new RelayCommand(param => IncreaseQuantity(param as CartItem));
            DecreaseQuantityCommand = new RelayCommand(param => DecreaseQuantity(param as CartItem));

            // Collection Logic
            CartItems.CollectionChanged += CartItems_CollectionChanged;

            // Initial Load
            _ = LoadProducts();
            _ = LoadDashboardData();

            // Build initial menu (ยังไม่ล็อกอิน)
            RefreshHomeMenu();
        }

        /// <summary>
        /// สร้าง/อัปเดตรายการเมนูหน้าแรกตาม Role ของผู้ใช้ปัจจุบัน
        /// เรียกเมื่อ CurrentUser เปลี่ยน
        /// </summary>
        private void RefreshHomeMenu()
        {
            var allItems = MenuConfigService.BuildMenuItems(this);
            var userRole = CurrentUser?.Role;

            var filtered = allItems
                .Where(m => RolePermissions.CanAccessMenu(userRole, m.Id))
                .ToList();

            HomeMenuItems = new ObservableCollection<MenuItemModel>(filtered);

            var view = CollectionViewSource.GetDefaultView(HomeMenuItems);
            view.GroupDescriptions.Clear();
            view.GroupDescriptions.Add(
                new PropertyGroupDescription(nameof(MenuItemModel.CategoryDisplayName)));
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(
                new SortDescription(nameof(MenuItemModel.Category), ListSortDirection.Ascending));
            view.SortDescriptions.Add(
                new SortDescription(nameof(MenuItemModel.SortOrder), ListSortDirection.Ascending));

            HomeGroupedMenuItems = view;
        }

        private void CartItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CartItem item in e.NewItems)
                {
                    item.PropertyChanged += CartItem_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (CartItem item in e.OldItems)
                {
                    item.PropertyChanged -= CartItem_PropertyChanged;
                }
            }
            
            CalculateTotal();
        }

        private void CartItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CartItem.Quantity))
            {
                CalculateTotal();
            }
        }

        private void IncreaseQuantity(CartItem item)
        {
            if (item != null)
            {
                item.Quantity++;
            }
        }

        private void DecreaseQuantity(CartItem item)
        {
            if (item != null && item.Quantity > 1)
            {
                item.Quantity--;
            }
        }

        private void AddCash(object parameter)
        {
            if (decimal.TryParse(parameter?.ToString(), out decimal amount))
            {
                CashReceived += amount;
            }
        }

        private void NumpadKey(string key)
        {
            if (key == null) return;
            if (key == "Exact")
            {
                _cashInputBuffer = ((int)Math.Ceiling(CheckoutGrandTotal)).ToString();
            }
            else if (key == "20" || key == "50" || key == "100" || key == "500" || key == "1000")
            {
                _cashInputBuffer = key;
            }
            else if (key == "CLEAR")
            {
                _cashInputBuffer = "";
            }
            else if (key == "BACK")
            {
                if (_cashInputBuffer.Length > 0)
                    _cashInputBuffer = _cashInputBuffer.Substring(0, _cashInputBuffer.Length - 1);
            }
            else if (key == "00")
            {
                if (!string.IsNullOrEmpty(_cashInputBuffer))
                    _cashInputBuffer += "00";
            }
            else if (key == ".")
            {
                if (!_cashInputBuffer.Contains("."))
                    _cashInputBuffer += string.IsNullOrEmpty(_cashInputBuffer) ? "0." : ".";
            }
            else
            {
                _cashInputBuffer += key;
            }

            if (!_cashInputBuffer.Contains("."))
            {
                _cashInputBuffer = _cashInputBuffer.TrimStart('0');
            }

            if (string.IsNullOrEmpty(_cashInputBuffer)) _cashInputBuffer = "";

            CashReceived = decimal.TryParse(
                string.IsNullOrEmpty(_cashInputBuffer) ? "0" : _cashInputBuffer,
                out decimal result) ? result : 0;
        }

        // Form Properties
        public bool IsHomeMode
        {
            get => _isHomeMode;
            set
            {
                if (SetProperty(ref _isHomeMode, value))
                {
                    if (value) { _currentView = "home"; OnPropertyChanged(nameof(CurrentView)); }
                    OnPropertyChanged(nameof(IsSearchMode));
                    OnPropertyChanged(nameof(IsManagementMode));
                    OnPropertyChanged(nameof(IsStockInMode));
                    OnPropertyChanged(nameof(IsReportsMode));
                }
            }
        }

        public bool IsManagementMode 
        { 
            get => _isManagementMode; 
            set 
            { 
                if (SetProperty(ref _isManagementMode, value))
                {
                    if (value) { _currentView = "stock_manage"; OnPropertyChanged(nameof(CurrentView)); }
                    OnPropertyChanged(nameof(IsSearchMode));
                    OnPropertyChanged(nameof(IsHomeMode));
                    OnPropertyChanged(nameof(IsReportsMode));
                }
            } 
        }

        public bool IsSearchMode => !IsHomeMode && !IsManagementMode && !IsStockInMode && !IsReportsMode && !IsHistoryMode && !IsDeliveryMode && !IsCustomerMode && !IsPartnerMode && !IsClaimMode && !IsPurchaseOrderMode && !IsExpenseMode && !IsStaffMode;

        public bool IsStockInMode
        {
            get => _isStockInMode;
            set
            {
                if (SetProperty(ref _isStockInMode, value))
                {
                    if (value) { _currentView = "stock_in"; OnPropertyChanged(nameof(CurrentView)); }
                    OnPropertyChanged(nameof(IsSearchMode));
                    OnPropertyChanged(nameof(IsManagementMode));
                    OnPropertyChanged(nameof(IsHomeMode));
                    OnPropertyChanged(nameof(IsReportsMode));
                }
            }
        }
        
        public string Code
        {
            get => _code;
            set
            {
                if (SetProperty(ref _code, value))
                {
                    OnPropertyChanged(nameof(AutoProductCode));
                    OnPropertyChanged(nameof(ProductCode));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        public string AutoProductCode => string.IsNullOrWhiteSpace(_code) ? GenerateProductCode() : _code;
        public string Name { get => _name; set { SetProperty(ref _name, value); CommandManager.InvalidateRequerySuggested(); } }

        private int _categoryId;
        public int CategoryId
        {
            get => _categoryId;
            set
            {
                if (SetProperty(ref _categoryId, value))
                {
                    OnPropertyChanged(nameof(CategoryID));
                    if (ProductCategories != null)
                    {
                        var matched = ProductCategories.FirstOrDefault(c => c.CategoryID == value || c.CategoryId == value || c.Id == value);
                        if (matched != null)
                        {
                            if (_selectedProductCategory != matched)
                            {
                                _selectedProductCategory = matched;
                                OnPropertyChanged(nameof(SelectedProductCategory));
                            }
                            if (!string.IsNullOrWhiteSpace(matched.CategoryName))
                            {
                                _category = matched.CategoryName;
                                OnPropertyChanged(nameof(Category));
                            }
                        }
                    }
                }
            }
        }
        public int CategoryID
        {
            get => CategoryId;
            set => CategoryId = value;
        }

        public string Category
        {
            get => _category;
            set
            {
                if (SetProperty(ref _category, value))
                {
                    if (!string.IsNullOrWhiteSpace(value) && ProductCategories != null)
                    {
                        var matched = ProductCategories.FirstOrDefault(c => string.Equals(c.CategoryName, value, StringComparison.OrdinalIgnoreCase));
                        if (matched != null)
                        {
                            if (_selectedProductCategory != matched)
                            {
                                _selectedProductCategory = matched;
                                OnPropertyChanged(nameof(SelectedProductCategory));
                            }
                            if (_categoryId != matched.CategoryID)
                            {
                                _categoryId = matched.CategoryID;
                                OnPropertyChanged(nameof(CategoryId));
                                OnPropertyChanged(nameof(CategoryID));
                            }
                        }
                    }
                }
            }
        }

        public string Unit { get => _unit ?? string.Empty; set { SetProperty(ref _unit, value); } }
        public decimal Price { get => _price; set { SetProperty(ref _price, value); } }
        public int Stock { get => _stock; set { SetProperty(ref _stock, value); } }
        public int ReorderPoint { get => _reorderPoint; set { SetProperty(ref _reorderPoint, value); } }
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                if (SetProperty(ref _imagePath, value))
                {
                    OnPropertyChanged(nameof(HasImagePath));
                }
            }
        }
        public bool HasImagePath => !string.IsNullOrWhiteSpace(ImagePath);
        public string Description { get => _description; set { SetProperty(ref _description, value); } }
        
        // Stock In Properties
        private bool _isStockInModalOpen;
        public bool IsStockInModalOpen
        {
            get => _isStockInModalOpen;
            set { if (SetProperty(ref _isStockInModalOpen, value)) OnPropertyChanged(nameof(IsStockInModalOpen)); }
        }

        public ObservableCollection<StockTransaction> StockTransactions { get; } = new ObservableCollection<StockTransaction>();

        private int _totalStockInEntriesCount;
        public int TotalStockInEntriesCount => _totalStockInEntriesCount;

        private int _totalStockInQtyCount;
        public int TotalStockInQtyCount => _totalStockInQtyCount;

        private decimal _totalStockInCostSum;
        public decimal TotalStockInCostSum => _totalStockInCostSum;

        public ICommand OpenStockInModalCommand { get; private set; }
        public ICommand CloseStockInModalCommand { get; private set; }
        public ICommand SearchStockInCommand { get; private set; }

        private async Task LoadStockTransactions()
        {
            StockTransactions.Clear();
            var list = await Task.Run(() => _databaseService.GetStockTransactions(SearchText));
            foreach (var t in list) StockTransactions.Add(t);

            var kpi = await Task.Run(() => _databaseService.GetStockInKPIs());
            _totalStockInEntriesCount = kpi.TotalEntries;
            _totalStockInQtyCount = kpi.TotalQty;
            _totalStockInCostSum = kpi.TotalCost;

            OnPropertyChanged(nameof(TotalStockInEntriesCount));
            OnPropertyChanged(nameof(TotalStockInQtyCount));
            OnPropertyChanged(nameof(TotalStockInCostSum));
        }

        public string StockInRefNo { get => _stockInRefNo; set { SetProperty(ref _stockInRefNo, value); } }
        public string StockInSupplier { get => _stockInSupplier; set { SetProperty(ref _stockInSupplier, value); } }
        public int StockInQuantity { get => _stockInQuantity; set { SetProperty(ref _stockInQuantity, value); } }
        public decimal StockInCost { get => _stockInCost; set { SetProperty(ref _stockInCost, value); } }
        
        public string SearchText 
        { 
            get => _searchText; 
            set 
            { 
                if (SetProperty(ref _searchText, value))
                {
                    FilterStockView();
                    FilteredStaffs?.Refresh();
                    OnPropertyChanged(nameof(FilteredStaffList));
                    FilteredPartners?.Refresh();
                    OnPropertyChanged(nameof(FilteredPartners));
                    FilteredExpenses?.Refresh();
                    OnPropertyChanged(nameof(FilteredExpenses));
                }
            } 
        }

        // --- Stock Management View & Product Categories Properties ---
        public ObservableCollection<ProductCategory> ProductCategories { get; } = new ObservableCollection<ProductCategory>();
        public ObservableCollection<string> CategoriesList { get; } = new ObservableCollection<string>();

        private ProductCategory? _selectedProductCategory;
        public ProductCategory? SelectedProductCategory
        {
            get => _selectedProductCategory;
            set
            {
                if (SetProperty(ref _selectedProductCategory, value))
                {
                    if (value != null)
                    {
                        if (_categoryId != value.CategoryID)
                        {
                            _categoryId = value.CategoryID;
                            OnPropertyChanged(nameof(CategoryId));
                            OnPropertyChanged(nameof(CategoryID));
                        }
                        if (_category != value.CategoryName)
                        {
                            _category = value.CategoryName;
                            OnPropertyChanged(nameof(Category));
                        }
                    }
                }
            }
        }

        private string _selectedStockViewMode = "products";
        public string SelectedStockViewMode
        {
            get => _selectedStockViewMode;
            set
            {
                if (SetProperty(ref _selectedStockViewMode, value))
                {
                    OnPropertyChanged(nameof(IsProductViewActive));
                    OnPropertyChanged(nameof(IsCategoryViewActive));
                    OnPropertyChanged(nameof(StockViewSearchPlaceholder));
                    FilterStockView();
                }
            }
        }

        public bool IsProductViewActive => SelectedStockViewMode == "products";
        public bool IsCategoryViewActive => SelectedStockViewMode == "categories";

        public string StockViewSearchPlaceholder => IsProductViewActive
            ? "ค้นหารหัสสินค้า, ชื่อสินค้า..."
            : "ค้นหารหัสหมวดหมู่, ชื่อหมวดหมู่...";

        // --- Stock Status Filter Options & Selection ---
        public ObservableCollection<string> StockStatusFilterOptions { get; } = new ObservableCollection<string>
        {
            "ทั้งหมด",
            "ปกติ",
            "ใกล้หมด",
            "หมดสต็อก"
        };

        private string _selectedStockStatusFilter = "ทั้งหมด";
        public string SelectedStockStatusFilter
        {
            get => _selectedStockStatusFilter;
            set
            {
                if (SetProperty(ref _selectedStockStatusFilter, value ?? "ทั้งหมด"))
                {
                    FilterStockView();
                }
            }
        }

        private ProductCategory? _selectedCategory;
        public ProductCategory? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value) && value != null)
                {
                    CategoryCode = value.Code ?? string.Empty;
                    CategoryName = value.Name ?? string.Empty;
                    CategoryDescription = value.Description ?? string.Empty;
                }
            }
        }

        private string _categoryCode = string.Empty;
        public string CategoryCode
        {
            get => _categoryCode;
            set => SetProperty(ref _categoryCode, value);
        }

        private string _categoryName = string.Empty;
        public string CategoryName
        {
            get => _categoryName;
            set => SetProperty(ref _categoryName, value);
        }

        private string _categoryDescription = string.Empty;
        public string CategoryDescription
        {
            get => _categoryDescription;
            set => SetProperty(ref _categoryDescription, value);
        }

        private bool _isCategoryModalOpen;
        public bool IsCategoryModalOpen
        {
            get => _isCategoryModalOpen;
            set
            {
                if (SetProperty(ref _isCategoryModalOpen, value))
                {
                    IsModalOpen = value;
                }
            }
        }

        private bool _isCategoryEditMode;
        public bool IsCategoryEditMode
        {
            get => _isCategoryEditMode;
            set => SetProperty(ref _isCategoryEditMode, value);
        }

        private string _categoryModalTitle = "➕ เพิ่มประเภทสินค้า";
        public string CategoryModalTitle
        {
            get => _categoryModalTitle;
            set => SetProperty(ref _categoryModalTitle, value);
        }

        private string _categoryValidationMessage = string.Empty;
        public string CategoryValidationMessage
        {
            get => _categoryValidationMessage;
            set
            {
                if (SetProperty(ref _categoryValidationMessage, value))
                {
                    OnPropertyChanged(nameof(HasCategoryValidationMessage));
                }
            }
        }

        public bool HasCategoryValidationMessage => !string.IsNullOrWhiteSpace(CategoryValidationMessage);

        public ICommand OpenAddCategoryModalCommand { get; set; }
        public ICommand OpenManageCategoryModalCommand { get; set; }
        public ICommand OpenEditCategoryModalCommand { get; set; }
        public ICommand SaveCategoryCommand { get; set; }
        public ICommand DeleteCategoryCommand { get; set; }
        public ICommand ClearCategoryCommand { get; set; }
        public ICommand CloseCategoryModalCommand { get; set; }

        public void FilterStockView()
        {
            if (IsProductViewActive)
            {
                FilterProducts();
            }
            else
            {
                LoadProductCategories(filterBySearch: true);
            }
        }

        public void LoadProductCategories(bool filterBySearch = false)
        {
            ProductCategories.Clear();
            CategoriesList.Clear();
            var allCategories = _databaseService.GetProductCategories() ?? Enumerable.Empty<ProductCategory>();

            foreach (var c in allCategories)
            {
                if (!string.IsNullOrWhiteSpace(c.Name) && !CategoriesList.Contains(c.Name))
                {
                    CategoriesList.Add(c.Name);
                }
            }

            var list = allCategories;
            if (filterBySearch && IsCategoryViewActive)
            {
                var kw = (SearchText ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(kw))
                {
                    list = list.Where(c =>
                        (c.Code != null && c.Code.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                        (c.Name != null && c.Name.Contains(kw, StringComparison.OrdinalIgnoreCase)));
                }
            }

            foreach (var c in list)
            {
                ProductCategories.Add(c);
            }
        }

        private string GenerateCategoryCode()
        {
            try
            {
                var list = _databaseService.GetProductCategories();
                int nextId = (list != null && list.Any()) ? list.Count() + 1 : 1;
                return $"CAT-{nextId:D3}";
            }
            catch
            {
                return $"CAT-{new Random().Next(100, 999)}";
            }
        }

        public void ExecuteOpenAddCategoryModal()
        {
            ClearCategoryForm();
            IsCategoryEditMode = false;
            CategoryModalTitle = "➕ เพิ่มประเภทสินค้า";
            CategoryValidationMessage = string.Empty;
            CategoryCode = GenerateCategoryCode();
            CurrentModalContent = "category";
            IsCategoryModalOpen = true;
            IsModalOpen = true;
        }

        public void ExecuteOpenManageCategoryModal(ProductCategory category = null)
        {
            if (category != null)
            {
                SelectedCategory = category;
            }

            if (SelectedCategory == null || SelectedCategory.Id <= 0)
            {
                ShowAlert("กรุณาคลิกเลือกรายการประเภทสินค้าในตารางก่อนดำเนินการ", "แจ้งเตือน", "⚠️");
                return;
            }

            IsCategoryEditMode = true;
            CategoryModalTitle = "⚙️ แก้ไข/จัดการประเภทสินค้า";
            CategoryValidationMessage = string.Empty;
            CategoryCode = SelectedCategory.Code ?? string.Empty;
            CategoryName = SelectedCategory.Name ?? string.Empty;
            CategoryDescription = SelectedCategory.Description ?? string.Empty;
            CurrentModalContent = "category";
            IsCategoryModalOpen = true;
            IsModalOpen = true;
        }

        public void ExecuteSaveCategory()
        {
            if (RolePermissions.IsReadOnly(CurrentUser?.Role, "stock_manage"))
            {
                ShowAlert("ไม่มีสิทธิ์ในการดำเนินการนี้ (สิทธิ์ดูอย่างเดียว)", "ไม่มีสิทธิ์", "⚠️");
                return;
            }

            if (string.IsNullOrWhiteSpace(CategoryName))
            {
                CategoryValidationMessage = "กรุณากรอกชื่อประเภทสินค้า";
                return;
            }

            if (IsCategoryEditMode && SelectedCategory != null && SelectedCategory.Id > 0)
            {
                SelectedCategory.Code = !string.IsNullOrWhiteSpace(CategoryCode) ? CategoryCode : SelectedCategory.Code;
                SelectedCategory.Name = CategoryName;
                SelectedCategory.Description = CategoryDescription;

                _databaseService.UpdateProductCategory(SelectedCategory);
                ClearCategoryForm();
                IsCategoryModalOpen = false;
                IsModalOpen = false;
                LoadProductCategories();
                ShowAlert("อัปเดตข้อมูลประเภทสินค้าเรียบร้อยแล้ว", "สำเร็จ", "🎉");
            }
            else
            {
                string codeToUse = !string.IsNullOrWhiteSpace(CategoryCode) ? CategoryCode : GenerateCategoryCode();
                var cat = new ProductCategory
                {
                    Code = codeToUse,
                    Name = CategoryName,
                    Description = CategoryDescription
                };

                _databaseService.SaveProductCategory(cat);
                ClearCategoryForm();
                IsCategoryModalOpen = false;
                IsModalOpen = false;
                LoadProductCategories();
                ShowAlert("บันทึกข้อมูลประเภทสินค้าเรียบร้อยแล้ว", "สำเร็จ", "🎉");
            }
        }

        private string _currentModalContent = string.Empty;
        public string CurrentModalContent
        {
            get => _currentModalContent;
            set => SetProperty(ref _currentModalContent, value);
        }

        public void ExecuteDeleteCategory()
        {
            if (RolePermissions.IsReadOnly(CurrentUser?.Role, "stock_manage"))
            {
                ShowAlert("ไม่มีสิทธิ์ในการดำเนินการนี้ (สิทธิ์ดูอย่างเดียว)", "ไม่มีสิทธิ์", "⚠️");
                return;
            }

            if (SelectedCategory == null || SelectedCategory.Id <= 0)
            {
                CategoryValidationMessage = "กรุณาคลิกเลือกรายการประเภทสินค้าในตารางก่อนดำเนินการ";
                return;
            }

            ShowConfirm($"คุณต้องการลบประเภทสินค้า '{SelectedCategory.Name}' ใช่หรือไม่?", () =>
            {
                _databaseService.DeleteProductCategory(SelectedCategory.Id);
                ClearCategoryForm();
                IsCategoryModalOpen = false;
                IsModalOpen = false;
                LoadProductCategories();
                ShowAlert("ลบประเภทสินค้าเรียบร้อยแล้ว", "สำเร็จ", "🗑️");
            }, "ยืนยันการลบประเภทสินค้า");
        }

        public void ClearCategoryForm()
        {
            SelectedCategory = null;
            CategoryCode = GenerateCategoryCode();
            CategoryName = string.Empty;
            CategoryDescription = string.Empty;
            CategoryValidationMessage = string.Empty;
        }

        private string _productModalTitle = "➕ เพิ่มข้อมูลสินค้า";
        public string ProductModalTitle
        {
            get => _productModalTitle;
            set => SetProperty(ref _productModalTitle, value);
        }

        private string _productValidationMessage = string.Empty;
        public string ProductValidationMessage
        {
            get => _productValidationMessage;
            set
            {
                if (SetProperty(ref _productValidationMessage, value))
                {
                    OnPropertyChanged(nameof(HasProductValidationMessage));
                }
            }
        }

        public bool HasProductValidationMessage => !string.IsNullOrWhiteSpace(ProductValidationMessage);

        private bool _isProductModalOpen;
        public bool IsProductModalOpen
        {
            get => _isProductModalOpen || _isAddModalOpen;
            set
            {
                _isProductModalOpen = value;
                _isAddModalOpen = value;
                OnPropertyChanged(nameof(IsAddModalOpen));
                OnPropertyChanged(nameof(IsProductModalOpen));
                IsModalOpen = value;
                if (value) CurrentModalContent = "product";
                else if (CurrentModalContent == "product") CurrentModalContent = string.Empty;
            }
        }

        private bool _isAddModalOpen;
        public bool IsAddModalOpen
        {
            get => IsProductModalOpen;
            set => IsProductModalOpen = value;
        }

        private bool _isEditModalOpen;
        public bool IsEditModalOpen
        {
            get => _isEditModalOpen;
            set => SetProperty(ref _isEditModalOpen, value);
        }

        private bool _isConfirmDialogOpen;
        public bool IsConfirmDialogOpen
        {
            get => _isConfirmDialogOpen;
            set => SetProperty(ref _isConfirmDialogOpen, value);
        }

        public string GenerateNextProductCode()
        {
            return GenerateProductCode();
        }

        private string _productCode = string.Empty;
        public string ProductCode
        {
            get => string.IsNullOrWhiteSpace(_productCode) ? (string.IsNullOrWhiteSpace(Code) ? AutoProductCode : Code) : _productCode;
            set
            {
                _productCode = value;
                _code = value;
                OnPropertyChanged(nameof(ProductCode));
                OnPropertyChanged(nameof(Code));
                OnPropertyChanged(nameof(AutoProductCode));
            }
        }

        public string ProductName
        {
            get => Name;
            set { Name = value; OnPropertyChanged(); }
        }

        public decimal CostPrice
        {
            get => Cost;
            set { Cost = value; OnPropertyChanged(); }
        }

        public decimal SellingPrice
        {
            get => Price;
            set { Price = value; OnPropertyChanged(); }
        }

        public int StockQuantity
        {
            get => Stock;
            set { Stock = value; OnPropertyChanged(); }
        }

        public void ExecuteOpenAddProductModal()
        {
            if (RolePermissions.IsReadOnly(CurrentUser?.Role, "stock_manage"))
            {
                ShowAlert("ไม่มีสิทธิ์ในการดำเนินการนี้ (สิทธิ์ดูอย่างเดียว)", "ไม่มีสิทธิ์", "⚠️");
                return;
            }

            // Load Categories first before clearing and setting defaults
            LoadProductCategories();

            ClearForm();
            IsProductAddMode = true;
            IsEditMode = true;
            ProductModalTitle = "เพิ่มข้อมูลสินค้า";
            ProductValidationMessage = string.Empty;

            string generatedCode = GenerateNextProductCode();
            Code = generatedCode;
            ProductCode = generatedCode;
            OnPropertyChanged(nameof(ProductCode));
            OnPropertyChanged(nameof(Code));
            OnPropertyChanged(nameof(AutoProductCode));
            ProductName = string.Empty;
            Name = string.Empty;
            CostPrice = 0;
            Cost = 0;
            SellingPrice = 20; // Default store price 20 baht
            Price = 20;
            StockQuantity = 0;
            Stock = 0;
            ReorderPoint = 10;
            Unit = string.Empty;
            ImagePath = string.Empty;

            if (ProductCategories.Any())
            {
                var firstCat = ProductCategories.First();
                SelectedProductCategory = firstCat;
                CategoryID = firstCat.CategoryID;
                Category = firstCat.CategoryName;
            }
            else
            {
                SelectedProductCategory = null;
                Category = "เบ็ดเตล็ด";
            }

            IsAddModalOpen = true;
            IsProductFormOpen = true;
            IsModalOpen = true;
            CurrentModalContent = "product";
        }

        public void ExecuteOpenViewProductModal(Product product = null)
        {
            if (product != null)
            {
                SelectedProduct = product;
            }

            if (SelectedProduct == null || (SelectedProduct.Id <= 0 && SelectedProduct.Pro_ID <= 0))
            {
                ShowAlert("กรุณาคลิกเลือกรายการสินค้าในตารางก่อนดำเนินการ", "แจ้งเตือน", "⚠️");
                return;
            }

            int targetId = SelectedProduct.Id > 0 ? SelectedProduct.Id : SelectedProduct.Pro_ID;

            // Load Categories first before binding so ComboBox items are populated
            LoadProductCategories();

            // Refresh fresh product record from SQLite to guarantee we have the latest CategoryId & data
            using (var conn = _databaseService.GetConnection())
            {
                var freshProduct = conn.QueryFirstOrDefault<Product>(@"
                    SELECT 
                        Pro_ID AS Pro_ID, 
                        Pro_ID AS Id, 
                        Pro_Barcode AS Pro_Barcode, 
                        Pro_Barcode AS Barcode, 
                        Pro_Barcode AS Code, 
                        Pro_Name AS Pro_Name, 
                        Pro_Name AS Name, 
                        COALESCE(Pro_Category, '') AS Pro_Category, 
                        COALESCE(Pro_Category, '') AS Category, 
                        COALESCE(Pro_Unit, 'ชิ้น') AS Pro_Unit, 
                        COALESCE(Pro_Unit, 'ชิ้น') AS Unit, 
                        COALESCE(CategoryId, 0) AS CategoryId,
                        COALESCE(CategoryId, 0) AS CategoryID,
                        Pro_Price AS Pro_Price, 
                        Pro_Price AS Price, 
                        COALESCE(Pro_Cost, 0) AS Pro_Cost, 
                        COALESCE(Pro_Cost, 0) AS Cost, 
                        COALESCE(Pro_Cost, 0) AS CostPrice, 
                        Pro_Qty AS Pro_Qty, 
                        Pro_Qty AS Stock, 
                        Pro_MinQty AS Pro_MinQty, 
                        Pro_MinQty AS ReorderPoint, 
                        Pro_Image AS Pro_Image, 
                        Pro_Image AS ImagePath, 
                        '' AS Description 
                    FROM tblProduct 
                    WHERE Pro_ID = @Id", new { Id = targetId });

                if (freshProduct != null)
                {
                    SelectedProduct = freshProduct;
                }
            }

            IsProductAddMode = false;
            IsEditMode = false; // Start in Read-Only mode!
            ProductModalTitle = "ข้อมูลสินค้า";
            ProductValidationMessage = string.Empty;

            Code = SelectedProduct.Code;
            ProductCode = SelectedProduct.Code;
            OnPropertyChanged(nameof(ProductCode));
            OnPropertyChanged(nameof(Code));
            Name = SelectedProduct.Name;

            // Set Category and CategoryID with explicit match to ProductCategories
            ProductCategory? matchedCat = null;
            if (SelectedProduct.CategoryID > 0)
            {
                matchedCat = ProductCategories.FirstOrDefault(c => c.CategoryID == SelectedProduct.CategoryID || c.CategoryId == SelectedProduct.CategoryID || c.Id == SelectedProduct.CategoryID);
            }
            if (matchedCat == null && SelectedProduct.CategoryId > 0)
            {
                matchedCat = ProductCategories.FirstOrDefault(c => c.CategoryID == SelectedProduct.CategoryId || c.CategoryId == SelectedProduct.CategoryId || c.Id == SelectedProduct.CategoryId);
            }
            if (matchedCat == null && !string.IsNullOrWhiteSpace(SelectedProduct.Category))
            {
                matchedCat = ProductCategories.FirstOrDefault(c => string.Equals(c.CategoryName, SelectedProduct.Category, StringComparison.OrdinalIgnoreCase));
            }

            if (matchedCat != null)
            {
                SelectedProductCategory = matchedCat;
                CategoryID = matchedCat.CategoryID;
                Category = matchedCat.CategoryName;
            }
            else
            {
                SelectedProductCategory = ProductCategories.FirstOrDefault();
                if (SelectedProductCategory != null)
                {
                    CategoryID = SelectedProductCategory.CategoryID;
                    Category = SelectedProductCategory.CategoryName;
                }
                else
                {
                    Category = SelectedProduct.Category ?? "";
                    CategoryID = SelectedProduct.CategoryID;
                }
            }

            Unit = SelectedProduct.Unit ?? string.Empty;
            Price = SelectedProduct.Price;
            Cost = SelectedProduct.Cost;
            CostPrice = SelectedProduct.Cost;
            Stock = SelectedProduct.Stock;
            ReorderPoint = SelectedProduct.ReorderPoint;
            ImagePath = SelectedProduct.ImagePath;
            Description = SelectedProduct.Description;

            SaveProductOriginalState();

            CurrentModalContent = "product";
            IsProductFormOpen = true;
            IsModalOpen = true;
        }

        public void ExecuteOpenEditProductModal(Product product = null)
        {
            ExecuteOpenViewProductModal(product);
        }

        private void SaveProductOriginalState()
        {
            _originalProductName = Name ?? string.Empty;
            _originalProductCategory = Category ?? string.Empty;
            _originalProductCategoryId = CategoryID;
            _originalProductUnit = Unit ?? string.Empty;
            _originalProductCost = CostPrice;
            _originalProductPrice = Price;
            _originalProductStock = Stock;
            _originalProductReorderPoint = ReorderPoint;
            _originalProductImage = ImagePath ?? string.Empty;
        }

        private void RestoreProductOriginalState()
        {
            Name = _originalProductName;
            CategoryID = _originalProductCategoryId;
            Category = _originalProductCategory;
            SelectedProductCategory = ProductCategories.FirstOrDefault(c => c.CategoryID == _originalProductCategoryId);
            Unit = _originalProductUnit;
            CostPrice = _originalProductCost;
            Cost = _originalProductCost;
            Price = _originalProductPrice;
            Stock = _originalProductStock;
            ReorderPoint = _originalProductReorderPoint;
            ImagePath = _originalProductImage;
        }

        public void ExecuteEnterProductEditMode()
        {
            if (RolePermissions.IsReadOnly(CurrentUser?.Role, "stock_manage"))
            {
                ShowAlert("ไม่มีสิทธิ์ในการดำเนินการนี้ (สิทธิ์ดูอย่างเดียว)", "ไม่มีสิทธิ์", "⚠️");
                return;
            }

            SaveProductOriginalState();
            IsEditMode = true;
            ProductModalTitle = "แก้ไขข้อมูลสินค้า";
            ProductValidationMessage = string.Empty;
        }

        public void ExecuteCancelProductEdit()
        {
            if (IsProductAddMode)
            {
                ExecuteCloseProductModal();
                return;
            }

            RestoreProductOriginalState();
            IsEditMode = false;
            ProductModalTitle = "ข้อมูลสินค้า";
            ProductValidationMessage = string.Empty;
        }

        public async Task ExecuteSaveProduct()
        {
            if (RolePermissions.IsReadOnly(CurrentUser?.Role, "stock_manage"))
            {
                ShowAlert("ไม่มีสิทธิ์ในการดำเนินการนี้ (สิทธิ์ดูอย่างเดียว)", "ไม่มีสิทธิ์", "⚠️");
                return;
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                ProductValidationMessage = "กรุณากรอกชื่อสินค้า";
                return;
            }

            if (Price <= 0)
            {
                ProductValidationMessage = "กรุณากรอกราคาขายสินค้า (ต้องมากกว่า 0)";
                return;
            }

            bool wasEdit = !IsProductAddMode;
            if (wasEdit)
            {
                await UpdateProduct();
            }
            else
            {
                await AddProduct();
            }

            ExecuteCloseProductModal();
            await LoadProducts();
            ShowAlert(wasEdit ? "บันทึกการแก้ไขข้อมูลสินค้าเรียบร้อยแล้ว" : "เพิ่มข้อมูลสินค้าเรียบร้อยแล้ว", "สำเร็จ", "🎉");
        }

        public async Task ExecuteDeleteProduct()
        {
            await Task.CompletedTask;
            if (RolePermissions.IsReadOnly(CurrentUser?.Role, "stock_manage"))
            {
                ShowAlert("ไม่มีสิทธิ์ในการดำเนินการนี้ (สิทธิ์ดูอย่างเดียว)", "ไม่มีสิทธิ์", "⚠️");
                return;
            }

            if (SelectedProduct == null || SelectedProduct.Id <= 0)
            {
                ProductValidationMessage = "กรุณาคลิกเลือกรายการสินค้าในตารางก่อนดำเนินการ";
                return;
            }

            ShowConfirm($"คุณต้องการลบสินค้า '{SelectedProduct.Name}' ใช่หรือไม่?", async () =>
            {
                await DeleteProduct();
                ExecuteCloseProductModal();
                await LoadProducts();
                ShowAlert("ลบข้อมูลสินค้าเรียบร้อยแล้ว", "สำเร็จ", "🗑️");
            }, "ยืนยันการลบสินค้า");
        }

        public void ExecuteCloseProductModal()
        {
            _isAddModalOpen = false;
            OnPropertyChanged(nameof(IsAddModalOpen));
            IsProductFormOpen = false;
            IsModalOpen = false;
            CurrentModalContent = string.Empty;
            ProductValidationMessage = string.Empty;
        }

        public string GenerateProductCode()
        {
            try
            {
                int nextId = (_allProducts != null && _allProducts.Any()) ? _allProducts.Count + 1 : 1;
                return $"P-{nextId:D4}";
            }
            catch
            {
                return $"P-{new Random().Next(1000, 9999)}";
            }
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value))
                {
                    OnPropertyChanged(nameof(HasSelectedProduct));
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                    if (value != null)
                    {
                        // Populate Form
                        Code = value.Code;
                        Name = value.Name;
                        Category = value.Category;

                        ProductCategory? matched = null;
                        if (value.CategoryID > 0)
                        {
                            matched = ProductCategories.FirstOrDefault(c => c.CategoryID == value.CategoryID || c.CategoryId == value.CategoryID || c.Id == value.CategoryID);
                        }
                        if (matched == null && value.CategoryId > 0)
                        {
                            matched = ProductCategories.FirstOrDefault(c => c.CategoryID == value.CategoryId || c.CategoryId == value.CategoryId || c.Id == value.CategoryId);
                        }
                        if (matched == null && !string.IsNullOrWhiteSpace(value.Category) && ProductCategories != null)
                        {
                            matched = ProductCategories.FirstOrDefault(c => string.Equals(c.CategoryName, value.Category, StringComparison.OrdinalIgnoreCase));
                        }

                        if (matched != null)
                        {
                            SelectedProductCategory = matched;
                            CategoryID = matched.CategoryID;
                            Category = matched.CategoryName;
                        }
                        else
                        {
                            CategoryID = value.CategoryID > 0 ? value.CategoryID : value.CategoryId;
                        }
                        Unit = value.Unit ?? string.Empty;
                        Price = value.Price;
                        Cost = value.Cost;
                        CostPrice = value.Cost;
                        Stock = value.Stock;
                        ReorderPoint = value.ReorderPoint;
                        StockInCost = value.Cost; // Default cost for stock in to current cost
                        ImagePath = value.ImagePath;
                        Description = value.Description;

                        // Reset Adjustment Fields
                        IsStockAdjustmentMode = false;
                        SelectedAdjustmentReason = AdjustmentReasons.FirstOrDefault();
                        AdjustmentQuantity = 0;
                        AdjustmentNote = string.Empty;
                    }
                }
            }
        }

        // System Commands
        public ICommand LoadProductsCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand SaveAdjustmentCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand BrowseImageCommand { get; }
        public ICommand RemoveImageCommand { get; }
        public ICommand SearchCommand { get; }
        // public ICommand ToggleModeCommand { get; }
        public ICommand SwitchToPosCommand { get; }
        public ICommand SwitchToAdminCommand { get; }
        public ICommand SwitchToStockInCommand { get; }
        public ICommand SwitchToReportsCommand { get; }
        public ICommand SwitchToHomeCommand { get; }
        public ICommand OpenHistoryCommand { get; }
        public ICommand OpenSalesHistoryCommand { get; }
        public ICommand OpenDeliveryCommand { get; }
        public ICommand ConfirmStockInCommand { get; }
        public ICommand ScanBarcodeCommand { get; }
        
        public ICommand SearchDeliveriesCommand { get; }
        public ICommand PrintDeliverySlipCommand { get; }
        public ICommand UpdateDeliveryStatusCommand { get; }
        public ICommand OpenManageModalCommand { get; set; }
        public ICommand CloseManageModalCommand { get; set; }
        public ICommand SaveDeliveryStatusCommand { get; }
        public ICommand FilterDeliveryStatusCommand { get; }
        public ICommand ClearDeliveryFilterCommand { get; }
        public ICommand CloseDeliveryDetailCommand { get; }
        public ICommand ViewDeliveryDetailsCommand { get; }

        // Stock Management Modal Commands
        public ICommand OpenAddProductCommand { get; set; }
        public ICommand CloseProductModalCommand { get; set; }
        public ICommand OpenAddModalCommand { get; }
        public ICommand CloseAddModalCommand { get; }
        public ICommand SaveProductCommand { get; }
        public ICommand ClearFormCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand ToggleManagementModalCommand { get; }
        public ICommand CloseStockManageModalCommand { get; }
        public ICommand StockAddProductCommand { get; }
        public ICommand StockEditProductCommand { get; }
        public ICommand StockDeleteProductCommand { get; }
        public ICommand SaveProductFormCommand { get; }
        public ICommand CancelProductFormCommand { get; }
        public ICommand EnterProductEditModeCommand { get; set; }
        public ICommand CancelProductEditCommand { get; set; }
        public ICommand OpenCustomerCommand { get; }
        public ICommand OpenPartnerCommand { get; }
        
        public ICommand SearchCustomerCommand { get; set; }
        public ICommand SaveCustomerCommand { get; set; }
        public ICommand AddCustomerCommand { get; set; }
        public ICommand UpdateCustomerCommand { get; set; }
        public ICommand DeleteCustomerCommand { get; set; }
        public ICommand ClearCustomerCommand { get; set; }
        public ICommand ViewCustomerHistoryCommand { get; set; }
        public ICommand CloseCustomerAlertCommand { get; set; }

        public ICommand SearchPartnerCommand { get; set; }
        public ICommand AddPartnerCommand { get; set; }
        public ICommand UpdatePartnerCommand { get; set; }
        public ICommand DeletePartnerCommand { get; set; }
        public ICommand ClearPartnerCommand { get; set; }

        public ICommand OpenClaimCommand { get; }
        public ICommand SearchClaimCommand { get; }
        public ICommand AddClaimCommand { get; }
        public ICommand SaveClaimCommand => AddClaimCommand;
        public ICommand UpdateClaimCommand { get; }
        public ICommand DeleteClaimCommand { get; }
        public ICommand ClearClaimCommand { get; }
        public ICommand UpdateClaimStatusCommand { get; }
        public ICommand FilterClaimStatusCommand { get; }
        public ICommand ClearClaimFilterCommand { get; }
        public ICommand CloseClaimDetailCommand { get; }
        public ICommand ViewClaimDetailsCommand { get; }
        public ICommand PrintClaimSlipCommand { get; }

        // Purchase Order Commands
        public ICommand OpenPurchaseOrderCommand { get; }
        public ICommand SearchPOCommand { get; }
        public ICommand AddPOCommand { get; }
        public ICommand SavePOCommand => AddPOCommand;
        public ICommand UpdatePOCommand { get; }
        public ICommand DeletePOCommand { get; }
        public ICommand ClearPOCommand { get; }
        public ICommand ReceivePOCommand { get; private set; }
        public ICommand AddPOItemCommand { get; }
        public ICommand OpenPOItemModalCommand { get; }
        public ICommand ClosePOItemModalCommand { get; }
        public ICommand RemovePOItemCommand { get; }
        public ICommand PrintPOCommand { get; }
        public ICommand ScanPOLowStockProductCommand { get; }
        public ICommand SelectPOSuggestionCommand { get; }
        public ICommand ClosePOSuggestionsCommand { get; }
        public ICommand AutoFillReorderItemsCommand { get; private set; }
        public ICommand ClearStockSearchCommand { get; private set; }

        // Goods Receipt Commands
        public ICommand OpenGoodsReceiptModalCommand { get; private set; }
        public ICommand CloseGoodsReceiptModalCommand { get; private set; }
        public ICommand ConfirmGoodsReceiptCommand { get; private set; }
        public ICommand SearchPendingPOCommand { get; private set; }
        public ICommand ClearStockInFilterCommand { get; private set; }

        // Goods Receipt Properties
        public ObservableCollection<PurchaseOrder> PendingPurchaseOrders { get; set; } = new ObservableCollection<PurchaseOrder>();

        public ObservableCollection<string> StockInStatusOptions { get; } = new ObservableCollection<string>
        {
            "ทั้งหมด",
            "รอดำเนินการ",
            "ตรวจรับแล้ว"
        };

        private string _selectedStockInStatus = "ทั้งหมด";
        public string SelectedStockInStatus
        {
            get => _selectedStockInStatus;
            set
            {
                if (SetProperty(ref _selectedStockInStatus, value))
                {
                    _ = LoadPendingPurchaseOrders();
                }
            }
        }

        private PurchaseOrder? _selectedReceiptPO;
        public PurchaseOrder? SelectedReceiptPO
        {
            get => _selectedReceiptPO;
            set
            {
                if (SetProperty(ref _selectedReceiptPO, value))
                {
                    OnPropertyChanged(nameof(CanReceiveSelectedPO));
                    OnPropertyChanged(nameof(SelectedPOReceiveButtonText));
                    OnPropertyChanged(nameof(HasSelectedReceiptPO));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool HasSelectedReceiptPO => SelectedReceiptPO != null;
        public bool CanReceiveSelectedPO => SelectedReceiptPO != null && SelectedReceiptPO.IsPending && !RolePermissions.IsReadOnly(CurrentUser?.Role, "stock_in");
        public string SelectedPOReceiveButtonText => (SelectedReceiptPO != null && SelectedReceiptPO.IsReceived) ? "🔍 ดูรายละเอียด" : "📥 ตรวจรับสินค้า";

        private bool _isGoodsReceiptModalOpen;
        public bool IsGoodsReceiptModalOpen
        {
            get => _isGoodsReceiptModalOpen;
            set => SetProperty(ref _isGoodsReceiptModalOpen, value);
        }

        private bool _isReceiptViewOnly;
        public bool IsReceiptViewOnly
        {
            get => _isReceiptViewOnly;
            set
            {
                if (SetProperty(ref _isReceiptViewOnly, value))
                {
                    OnPropertyChanged(nameof(CanEditReceiptModal));
                    OnPropertyChanged(nameof(IsReceiptDeliveryNoteValid));
                    OnPropertyChanged(nameof(IsReceiptDeliveryNoteInvalid));
                    OnPropertyChanged(nameof(CanConfirmGoodsReceipt));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool CanEditReceiptModal => !IsReceiptViewOnly && CanEditStockIn;

        public static bool IsValidInvoiceNumber(string? input)
        {
            return Common.ValidationHelper.IsValidDeliveryNote(input);
        }

        public bool IsReceiptDeliveryNoteValid => CanEditReceiptModal && !string.IsNullOrWhiteSpace(ReceiptDeliveryNoteNo);
        public bool IsReceiptDeliveryNoteInvalid => CanEditReceiptModal && string.IsNullOrWhiteSpace(ReceiptDeliveryNoteNo);

        public bool CanConfirmGoodsReceipt =>
            !IsReceiptViewOnly &&
            CanEditStockIn &&
            !string.IsNullOrWhiteSpace(ReceiptDeliveryNoteNo) &&
            ReceiptItems != null &&
            ReceiptItems.Count > 0 &&
            ReceiptItems.Any(i => i.ReceivedQty > 0);

        private int _allPendingPOCount;
        private int _allReceivedPOCount;
        public string StockInSummaryText => $"แสดงทั้งหมด {PendingPurchaseOrders.Count} รายการ (รอดำเนินการ {_allPendingPOCount} รายการ | ตรวจรับแล้ว {_allReceivedPOCount} รายการ)";
        public string DisplaySummaryCountText => StockInSummaryText;

        private string _receiptDeliveryNoteNo = string.Empty;
        public string ReceiptDeliveryNoteNo
        {
            get => _receiptDeliveryNoteNo;
            set
            {
                string upperVal = value != null ? value.ToUpper() : string.Empty;
                if (SetProperty(ref _receiptDeliveryNoteNo, upperVal))
                {
                    OnPropertyChanged(nameof(IsReceiptDeliveryNoteValid));
                    OnPropertyChanged(nameof(IsReceiptDeliveryNoteInvalid));
                    OnPropertyChanged(nameof(CanConfirmGoodsReceipt));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public ObservableCollection<GoodsReceiptItem> ReceiptItems { get; set; } = new ObservableCollection<GoodsReceiptItem>();

        private decimal _receiptTotalAmount;
        public decimal ReceiptTotalAmount
        {
            get => _receiptTotalAmount;
            set => SetProperty(ref _receiptTotalAmount, value);
        }

        private string _pendingPOSearchKeyword = string.Empty;
        public string PendingPOSearchKeyword
        {
            get => _pendingPOSearchKeyword;
            set { if (SetProperty(ref _pendingPOSearchKeyword, value)) _ = LoadPendingPurchaseOrders(); }
        }

        public string SearchKeyword
        {
            get => IsPurchaseOrderMode ? POSearchKeyword : PendingPOSearchKeyword;
            set
            {
                if (IsPurchaseOrderMode)
                {
                    POSearchKeyword = value;
                }
                else
                {
                    PendingPOSearchKeyword = value;
                }
            }
        }

        public string FilteredOrdersCountText => $"แสดงทั้งหมด {PurchaseOrders?.Count ?? 0} รายการ";

        public void ExecuteClearStockInFilter()
        {
            _pendingPOSearchKeyword = string.Empty;
            OnPropertyChanged(nameof(PendingPOSearchKeyword));
            OnPropertyChanged(nameof(SearchKeyword));
            _selectedStockInStatus = "ทั้งหมด";
            OnPropertyChanged(nameof(SelectedStockInStatus));
            _ = LoadPendingPurchaseOrders();
        }

        public void ExecuteClearPOFilter()
        {
            _poSearchKeyword = string.Empty;
            OnPropertyChanged(nameof(POSearchKeyword));
            OnPropertyChanged(nameof(SearchKeyword));
            _filteredPOSuggestions?.Clear();
            IsPOSuggestionsDropdownOpen = false;
            _poStatusFilter = "ทั้งหมด";
            OnPropertyChanged(nameof(POStatusFilter));
            _ = LoadPurchaseOrders();
        }

        // System Data Commands
        public ICommand BackupDatabaseCommand { get; }
        public ICommand RestoreDatabaseCommand { get; }

        // History Commands
        public ICommand PrintOrderCommand { get; }

        // --- Purchase Order Properties ---
        private string _poSearchKeyword = string.Empty;
        public string POSearchKeyword
        {
            get => _poSearchKeyword;
            set
            {
                if (SetProperty(ref _poSearchKeyword, value))
                {
                    FilterPOSuggestions(value);
                    _ = LoadPurchaseOrders();
                    OnPropertyChanged(nameof(SearchKeyword));
                }
            }
        }

        private ObservableCollection<PurchaseOrder> _filteredPOSuggestions = new ObservableCollection<PurchaseOrder>();
        public ObservableCollection<PurchaseOrder> FilteredPOSuggestions
        {
            get => _filteredPOSuggestions;
            set => SetProperty(ref _filteredPOSuggestions, value);
        }

        private bool _isPOSuggestionsDropdownOpen;
        public bool IsPOSuggestionsDropdownOpen
        {
            get => _isPOSuggestionsDropdownOpen;
            set => SetProperty(ref _isPOSuggestionsDropdownOpen, value);
        }

        private void FilterPOSuggestions(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                FilteredPOSuggestions.Clear();
                IsPOSuggestionsDropdownOpen = false;
                return;
            }

            var trimmed = keyword.Trim();
            var allPOs = _databaseService.GetPurchaseOrders(string.Empty);
            var matches = allPOs.Where(p =>
                (!string.IsNullOrWhiteSpace(p.PONumber) && p.PONumber.IndexOf(trimmed, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (!string.IsNullOrWhiteSpace(p.SupplierName) && p.SupplierName.IndexOf(trimmed, StringComparison.OrdinalIgnoreCase) >= 0)
            ).Take(10).ToList();

            FilteredPOSuggestions.Clear();
            foreach (var m in matches)
            {
                FilteredPOSuggestions.Add(m);
            }

            IsPOSuggestionsDropdownOpen = FilteredPOSuggestions.Count > 0;
        }

        public void SelectPOSuggestionItem(PurchaseOrder po)
        {
            if (po != null)
            {
                _poSearchKeyword = po.PONumber;
                OnPropertyChanged(nameof(POSearchKeyword));
                IsPOSuggestionsDropdownOpen = false;
                _ = LoadPurchaseOrders();
            }
        }
        
        private bool _isPOReadOnly;
        public bool IsPOReadOnly
        {
            get => _isPOReadOnly;
            set
            {
                if (SetProperty(ref _isPOReadOnly, value))
                {
                    OnPropertyChanged(nameof(IsPOReadOnlyMode));
                    OnPropertyChanged(nameof(IsPOCreateMode));
                    OnPropertyChanged(nameof(CanEditPOStatus));
                }
            }
        }

        public bool IsPOReadOnlyMode
        {
            get => IsPOReadOnly;
            set => IsPOReadOnly = value;
        }

        public bool IsPOCreateMode
        {
            get => !IsPOReadOnly;
            set => IsPOReadOnly = !value;
        }

        public bool CanEditPOStatus => !IsPOReadOnly;

        public async Task LoadPODetailsAsync(PurchaseOrder po, bool isReadOnly = true)
        {
            if (po == null || po.Id <= 0) return;

            if (_suppliersList.Count == 0)
            {
                await LoadSuppliersAsync();
            }

            var dbPo = _databaseService.GetPurchaseOrderById(po.Id) ?? po;

            _selectedPurchaseOrder = dbPo;
            OnPropertyChanged(nameof(SelectedPurchaseOrder));

            PONumber = dbPo.PONumber;
            POExpectedDate = dbPo.ExpectedDate;
            
            string rawStatus = dbPo.Status ?? string.Empty;
            if (rawStatus.Contains("รับ") || rawStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                POStatus = "ได้รับสินค้าแล้ว";
            }
            else if (rawStatus.Contains("ยกเลิก") || rawStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                POStatus = "ยกเลิก";
            }
            else
            {
                POStatus = "รอดำเนินการ";
            }

            POTotalAmount = dbPo.TotalAmount;

            Partner? matched = null;
            if (dbPo.Partner_ID > 0)
            {
                matched = _suppliersList.FirstOrDefault(s => s.SupplierID == dbPo.Partner_ID || s.Partner_ID == dbPo.Partner_ID);
            }
            if (matched == null && !string.IsNullOrWhiteSpace(dbPo.SupplierName))
            {
                matched = _suppliersList.FirstOrDefault(s => string.Equals(s.SupplierName, dbPo.SupplierName, StringComparison.OrdinalIgnoreCase));
            }

            _selectedSupplier = matched;
            _poSupplierId = matched?.SupplierID ?? (dbPo.Partner_ID > 0 ? dbPo.Partner_ID : (int?)null);
            _poSupplierName = matched?.SupplierName ?? dbPo.SupplierName ?? string.Empty;

            OnPropertyChanged(nameof(SelectedSupplier));
            OnPropertyChanged(nameof(POSupplierId));
            OnPropertyChanged(nameof(POSupplierName));

            CurrentPOItems.Clear();
            var items = _databaseService.GetPurchaseOrderItems(dbPo.Id);
            foreach (var item in items)
            {
                item.ItemChanged = CalculatePOTotal;
                CurrentPOItems.Add(item);
            }

            if (POTotalAmount <= 0 && CurrentPOItems.Count > 0)
            {
                CalculatePOTotal();
            }

            IsNewPOMode = false;
            IsPOReadOnly = isReadOnly;
            POModalTitle = isReadOnly ? $"รายละเอียดใบสั่งซื้อ {dbPo.PONumber}" : "จัดการใบสั่งซื้อสินค้า";
            IsPOModalOpen = true;
        }

        private PurchaseOrder _selectedPurchaseOrder = new PurchaseOrder();
        public PurchaseOrder SelectedPurchaseOrder
        {
            get => _selectedPurchaseOrder;
            set
            {
                if (SetProperty(ref _selectedPurchaseOrder, value) && value != null)
                {
                    PONumber = value.PONumber;
                    POSupplierName = value.SupplierName;
                    Partner? matched = null;
                    if (value.Partner_ID > 0)
                    {
                        matched = _suppliersList.FirstOrDefault(s => s.SupplierID == value.Partner_ID || s.Partner_ID == value.Partner_ID);
                    }
                    if (matched == null && !string.IsNullOrWhiteSpace(value.SupplierName))
                    {
                        matched = _suppliersList.FirstOrDefault(s => string.Equals(s.SupplierName, value.SupplierName, StringComparison.OrdinalIgnoreCase));
                    }
                    SelectedSupplier = matched;
                    POSupplierId = matched?.SupplierID ?? (value.Partner_ID > 0 ? value.Partner_ID : (int?)null);
                    if (matched != null)
                    {
                        POSupplierName = matched.SupplierName;
                    }
                    POExpectedDate = value.ExpectedDate;
                    POTotalAmount = value.TotalAmount;
                    string rawStatus = value.Status ?? string.Empty;
                    if (rawStatus.Contains("รับ") || rawStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                    {
                        POStatus = "ได้รับสินค้าแล้ว";
                    }
                    else if (rawStatus.Contains("ยกเลิก") || rawStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                    {
                        POStatus = "ยกเลิก";
                    }
                    else
                    {
                        POStatus = "รอดำเนินการ";
                    }
                    CurrentPOItems.Clear();
                    if (value.Id > 0)
                    {
                        var items = _databaseService.GetPurchaseOrderItems(value.Id);
                        foreach(var item in items)
                        {
                            item.ItemChanged = CalculatePOTotal;
                            CurrentPOItems.Add(item);
                        }
                    }
                }
            }
        }
        
        private string _poNumber = string.Empty;
        public string PONumber { get => _poNumber; set => SetProperty(ref _poNumber, value); }

        private string _poSupplierName = string.Empty;
        public string POSupplierName
        {
            get => _poSupplierName;
            set
            {
                if (SetProperty(ref _poSupplierName, value))
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        var match = _suppliersList.FirstOrDefault(s => string.Equals(s.SupplierName, value, StringComparison.OrdinalIgnoreCase));
                        if (match != null && _selectedSupplier != match)
                        {
                            _selectedSupplier = match;
                            _poSupplierId = match.SupplierID;
                            OnPropertyChanged(nameof(SelectedSupplier));
                            OnPropertyChanged(nameof(POSupplierId));
                        }
                    }
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private Partner? _selectedSupplier;
        public Partner? SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                if (SetProperty(ref _selectedSupplier, value))
                {
                    if (value != null)
                    {
                        _poSupplierName = value.SupplierName;
                        _poSupplierId = value.SupplierID;
                        OnPropertyChanged(nameof(POSupplierName));
                        OnPropertyChanged(nameof(POSupplierId));
                    }
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private int? _poSupplierId;
        public int? POSupplierId
        {
            get => _poSupplierId;
            set
            {
                if (SetProperty(ref _poSupplierId, value))
                {
                    if (value.HasValue)
                    {
                        var match = _suppliersList.FirstOrDefault(s => s.SupplierID == value.Value);
                        if (match != null && _selectedSupplier != match)
                        {
                            _selectedSupplier = match;
                            _poSupplierName = match.SupplierName;
                            OnPropertyChanged(nameof(SelectedSupplier));
                            OnPropertyChanged(nameof(POSupplierName));
                        }
                    }
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private ObservableCollection<Partner> _suppliersList = new ObservableCollection<Partner>();
        public ObservableCollection<Partner> SuppliersList => _suppliersList;
        public ObservableCollection<Partner> Suppliers => _suppliersList;

        public async Task LoadSuppliersAsync()
        {
            try
            {
                var suppliers = await Task.Run(() => _databaseService.GetSuppliers());
                _suppliersList.Clear();
                foreach (var s in suppliers)
                {
                    _suppliersList.Add(s);
                }
                OnPropertyChanged(nameof(SuppliersList));
                OnPropertyChanged(nameof(Suppliers));

                // Sync current POSupplierName if already set
                if (!string.IsNullOrWhiteSpace(POSupplierName))
                {
                    var matched = _suppliersList.FirstOrDefault(s => string.Equals(s.SupplierName, POSupplierName, StringComparison.OrdinalIgnoreCase));
                    if (matched != null)
                    {
                        _selectedSupplier = matched;
                        _poSupplierId = matched.SupplierID;
                        OnPropertyChanged(nameof(SelectedSupplier));
                        OnPropertyChanged(nameof(POSupplierId));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductViewModel] LoadSuppliersAsync error: {ex.Message}");
            }
        }

        private DateTime _poExpectedDate = DateTime.Now.AddDays(3);
        public DateTime POExpectedDate { get => _poExpectedDate; set => SetProperty(ref _poExpectedDate, value); }

        private decimal _poTotalAmount;
        public decimal POTotalAmount { get => _poTotalAmount; set => SetProperty(ref _poTotalAmount, value); }

        private string _poStatus = "รอดำเนินการ";
        public string POStatus
        {
            get => _poStatus;
            set
            {
                if (SetProperty(ref _poStatus, value))
                {
                    OnPropertyChanged(nameof(OrderStatus));
                }
            }
        }

        public string OrderStatus
        {
            get => POStatus;
            set => POStatus = value;
        }

        public IEnumerable<string> POStatusList => new List<string> { "รอดำเนินการ", "ได้รับสินค้าแล้ว", "ยกเลิก" };


        // PO Suggestions Properties
        private ObservableCollection<Product> _poSuggestedProducts = new ObservableCollection<Product>();
        public ObservableCollection<Product> POSuggestedProducts => _poSuggestedProducts;

        private bool _isPOSuggestionsOpen;
        public bool IsPOSuggestionsOpen
        {
            get => _isPOSuggestionsOpen;
            set => SetProperty(ref _isPOSuggestionsOpen, value);
        }

        private Product? _selectedPOSuggestedProduct;
        public Product? SelectedPOSuggestedProduct
        {
            get => _selectedPOSuggestedProduct;
            set
            {
                if (SetProperty(ref _selectedPOSuggestedProduct, value) && value != null)
                {
                    SelectPOSuggestion(value);
                    _selectedPOSuggestedProduct = null;
                    OnPropertyChanged(nameof(SelectedPOSuggestedProduct));
                }
            }
        }

        private bool _isSelectingPOSuggestion = false;

        private string _poItemProductCode = string.Empty;
        public string POItemProductCode
        {
            get => _poItemProductCode;
            set
            {
                if (SetProperty(ref _poItemProductCode, value))
                {
                    UpdatePOSuggestions(value, isBarcode: true);
                }
            }
        }

        private string _poItemProductName = string.Empty;
        public string POItemProductName
        {
            get => _poItemProductName;
            set
            {
                if (SetProperty(ref _poItemProductName, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                    UpdatePOSuggestions(value, isBarcode: false);
                }
            }
        }

        private Product? _selectedPOProduct;
        public Product? SelectedPOProduct
        {
            get => _selectedPOProduct;
            set
            {
                if (SetProperty(ref _selectedPOProduct, value) && value != null)
                {
                    POItemProductCode = value.Code ?? string.Empty;
                    POItemProductName = value.Name ?? string.Empty;
                    POItemCost = value.Cost > 0 ? value.Cost : value.Price;
                    if (POItemQuantity <= 0) POItemQuantity = 1;
                }
            }
        }

        private int _poItemQuantity = 1;
        public int POItemQuantity { get => _poItemQuantity; set { SetProperty(ref _poItemQuantity, value); CommandManager.InvalidateRequerySuggested(); } }

        private decimal _poItemCost;
        public decimal POItemCost { get => _poItemCost; set => SetProperty(ref _poItemCost, value); }

        private bool _isItemModalOpen;
        public bool IsItemModalOpen { get => _isItemModalOpen; set => SetProperty(ref _isItemModalOpen, value); }

        public System.Collections.ObjectModel.ObservableCollection<string> POStatusOptions { get; } = new System.Collections.ObjectModel.ObservableCollection<string> { "รอดำเนินการ", "ได้รับสินค้าแล้ว", "ยกเลิก" };

        public ICommand SwitchToStaffCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ShowLoginCommand { get; }
        public ICommand PerformLoginCommand { get; }
        public ICommand CancelLoginCommand { get; }

        // User Management Commands
        public ICommand AddUserCommand { get; }
        public ICommand UpdateUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand ClearUserCommand { get; }

        // Cart Commands
        public ICommand AddToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand CheckoutCommand { get; }
        public ICommand AddCashCommand { get; }
        public ICommand NumpadKeyCommand { get; }
        public ICommand FilterCategoryCommand { get; }
        public ICommand CloseCheckoutCommand { get; }
        public ICommand NextStepCommand { get; }
        public ICommand PrevStepCommand { get; }
        public ICommand CloseErrorModalCommand { get; }
        public ICommand PrintReceiptCommand { get; }

        // Cart Properties
        private string _selectedPosCategory = "ทั้งหมด";
        public string SelectedPosCategory
        {
            get => _selectedPosCategory;
            set => SetProperty(ref _selectedPosCategory, value);
        }

        public string SearchQuery
        {
            get => SearchText;
            set
            {
                SearchText = value;
                OnPropertyChanged();
                FilterProducts();
            }
        }

        public ObservableCollection<Product> FilteredProducts => Products;

        public int CartTotalItems => CartItems.Sum(item => item.Quantity);
        public string CartTotalString => TotalAmount.ToString("N2");

        // Checkout Wizard Properties (Phase 5 Prep)
        private bool _isCheckoutWizardOpen;
        public bool IsCheckoutWizardOpen
        {
            get => _isCheckoutWizardOpen;
            set
            {
                if (SetProperty(ref _isCheckoutWizardOpen, value))
                {
                    OnPropertyChanged(nameof(IsCheckoutWizardOpen));
                }
            }
        }

        private int _checkoutStep = 1;
        public int CheckoutStep
        {
            get => _checkoutStep;
            set
            {
                if (SetProperty(ref _checkoutStep, value))
                {
                    OnPropertyChanged(nameof(CheckoutStep));
                    OnPropertyChanged(nameof(CurrentStep));
                }
            }
        }

        public int CurrentStep
        {
            get => CheckoutStep;
            set
            {
                CheckoutStep = value;
                OnPropertyChanged(nameof(CurrentStep));
            }
        }

        public string CheckoutDeliveryMethod
        {
            get => IsDeliverySelected ? "จัดส่งสินค้า" : "รับที่ร้าน";
            set
            {
                IsDeliverySelected = (value == "จัดส่งสินค้า");
            }
        }

        public decimal ShippingFee => 0; // Free delivery — no shipping fee
        public decimal CheckoutGrandTotal => TotalAmount;



        private decimal _totalAmount;
        public decimal TotalAmount { get => _totalAmount; set { SetProperty(ref _totalAmount, value); } }

        private decimal _cashReceived;
        private string _cashInputBuffer = "";
        public decimal CashReceived 
        { 
            get => _cashReceived; 
            set 
            { 
                if (SetProperty(ref _cashReceived, value))
                {
                    CalculateChange();
                    CommandManager.InvalidateRequerySuggested();
                }
            } 
        }

        private decimal _change;
        public decimal Change { get => _change; set { SetProperty(ref _change, value); } }

        // Snapshot properties — hold transaction values for Step 4 success screen
        private decimal _finalCashReceived;
        public decimal FinalCashReceived
        {
            get => _finalCashReceived;
            set { _finalCashReceived = value; OnPropertyChanged(nameof(FinalCashReceived)); }
        }

        private decimal _finalChange;
        public decimal FinalChange
        {
            get => _finalChange;
            set { _finalChange = value; OnPropertyChanged(nameof(FinalChange)); }
        }

        private decimal _finalGrandTotal;
        public decimal FinalGrandTotal
        {
            get => _finalGrandTotal;
            set { _finalGrandTotal = value; OnPropertyChanged(nameof(FinalGrandTotal)); }
        }

        private decimal _discountAmount = 0;
        public decimal DiscountAmount
        {
            get => _discountAmount;
            set
            {
                if (SetProperty(ref _discountAmount, value))
                {
                    OnPropertyChanged(nameof(CheckoutGrandTotal));
                    OnPropertyChanged(nameof(HasDiscount));
                    CalculateChange();
                }
            }
        }

        public bool HasDiscount => DiscountAmount > 0;

        private bool _isCashPayment = true;
        public bool IsCashPayment
        {
            get => _isCashPayment;
            set
            {
                if (SetProperty(ref _isCashPayment, value))
                {
                    if (_isCashPayment) 
                    {
                        _isTransferPayment = false;
                        _isCardPayment = false;
                        _isCouponPayment = false;
                        OnPropertyChanged(nameof(IsTransferPayment));
                        OnPropertyChanged(nameof(IsCardPayment));
                        OnPropertyChanged(nameof(IsCouponPayment));
                        CashReceived = 0;
                        _cashInputBuffer = "";
                    }
                    CalculateChange();
                }
            }
        }

        private bool _isTransferPayment;
        public bool IsTransferPayment
        {
            get => _isTransferPayment;
            set
            {
                if (SetProperty(ref _isTransferPayment, value))
                {
                    if (_isTransferPayment) 
                    {
                        _isCashPayment = false;
                        _isCardPayment = false;
                        _isCouponPayment = false;
                        OnPropertyChanged(nameof(IsCashPayment));
                        OnPropertyChanged(nameof(IsCardPayment));
                        OnPropertyChanged(nameof(IsCouponPayment));
                        CashReceived = CheckoutGrandTotal;
                    }
                    CalculateChange();
                }
            }
        }

        private bool _isCardPayment;
        public bool IsCardPayment
        {
            get => _isCardPayment;
            set
            {
                if (SetProperty(ref _isCardPayment, value))
                {
                    if (_isCardPayment)
                    {
                        _isCashPayment = false;
                        _isTransferPayment = false;
                        _isCouponPayment = false;
                        OnPropertyChanged(nameof(IsCashPayment));
                        OnPropertyChanged(nameof(IsTransferPayment));
                        OnPropertyChanged(nameof(IsCouponPayment));
                        CashReceived = CheckoutGrandTotal;
                    }
                    CalculateChange();
                }
            }
        }

        private bool _isCouponPayment;
        public bool IsCouponPayment
        {
            get => _isCouponPayment;
            set
            {
                if (SetProperty(ref _isCouponPayment, value))
                {
                    if (_isCouponPayment)
                    {
                        _isCashPayment = false;
                        _isTransferPayment = false;
                        _isCardPayment = false;
                        OnPropertyChanged(nameof(IsCashPayment));
                        OnPropertyChanged(nameof(IsTransferPayment));
                        OnPropertyChanged(nameof(IsCardPayment));
                        CashReceived = CheckoutGrandTotal;
                    }
                    CalculateChange();
                }
            }
        }

        public string SelectedPaymentMethod
        {
            get => IsCashPayment ? "เงินสด" : (IsTransferPayment ? "สแกน QR Code" : (IsCardPayment ? "บัตรเครดิต/เดบิต" : "คูปอง/ส่วนลด"));
            set
            {
                if (value == "Cash" || value == "เงินสด")
                {
                    IsCashPayment = true;
                }
                else if (value == "Transfer" || value == "โอนเงิน" || value == "QR")
                {
                    IsTransferPayment = true;
                }
                else if (value == "Card" || value == "บัตรเครดิต")
                {
                    IsCardPayment = true;
                }
                else if (value == "Coupon" || value == "คูปอง")
                {
                    IsCouponPayment = true;
                }
                OnPropertyChanged(nameof(SelectedPaymentMethod));
                OnPropertyChanged(nameof(SelectedPaymentMethodDisplay));
            }
        }

        public string SelectedPaymentMethodDisplay => SelectedPaymentMethod;

        private string _lastRefNo = "";
        public string LastRefNo
        {
            get => _lastRefNo;
            set => SetProperty(ref _lastRefNo, value);
        }

        public ICommand NumpadCommand => NumpadKeyCommand;

        private bool CanExecuteSubmit()
        {
            return !string.IsNullOrWhiteSpace(Code) && !string.IsNullOrWhiteSpace(Name);
        }

        private void BrowseImage()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg;*.webp)|*.png;*.jpeg;*.jpg;*.webp|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string sourceFile = openFileDialog.FileName;
                string destinationFolder = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Images");
                
                if (!System.IO.Directory.Exists(destinationFolder))
                {
                    System.IO.Directory.CreateDirectory(destinationFolder);
                }

                string fileName = System.IO.Path.GetFileName(sourceFile);
                string destinationFile = System.IO.Path.Combine(destinationFolder, fileName);

                try 
                {
                    System.IO.File.Copy(sourceFile, destinationFile, true);
                    ImagePath = destinationFile;
                }
                catch (System.Exception ex)
                {
                    CustomMessageBox.Show($"คัดลอกรูปภาพล้มเหลว: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task LoadProducts()
        {
            using (var conn = _databaseService.GetConnection())
            {
                string sql = @"
                    SELECT 
                        Pro_ID AS Pro_ID, 
                        Pro_ID AS Id, 
                        Pro_Barcode AS Pro_Barcode, 
                        Pro_Barcode AS Barcode, 
                        Pro_Barcode AS Code, 
                        Pro_Name AS Pro_Name, 
                        Pro_Name AS Name, 
                        COALESCE(Pro_Category, '') AS Pro_Category, 
                        COALESCE(Pro_Category, '') AS Category, 
                        COALESCE(Pro_Unit, 'ชิ้น') AS Pro_Unit, 
                        COALESCE(Pro_Unit, 'ชิ้น') AS Unit, 
                        COALESCE(CategoryId, 0) AS CategoryId,
                        COALESCE(CategoryId, 0) AS CategoryID,
                        Pro_Price AS Pro_Price, 
                        Pro_Price AS Price, 
                        COALESCE(Pro_Cost, 0) AS Pro_Cost, 
                        COALESCE(Pro_Cost, 0) AS Cost, 
                        COALESCE(Pro_Cost, 0) AS CostPrice, 
                        Pro_Qty AS Pro_Qty, 
                        Pro_Qty AS Stock, 
                        Pro_MinQty AS Pro_MinQty, 
                        Pro_MinQty AS ReorderPoint, 
                        Pro_Image AS Pro_Image, 
                        Pro_Image AS ImagePath, 
                        '' AS Description 
                    FROM tblProduct
                    ORDER BY 
                        CASE 
                            WHEN Pro_Qty <= 0 THEN 1
                            WHEN Pro_Qty <= Pro_MinQty THEN 2
                            ELSE 3
                        END ASC,
                        Pro_ID ASC";
                var products = await conn.QueryAsync<Product>(sql);
                _allProducts = products.ToList();
                FilterProducts();
            }
        }

        public int TotalStockItemsCount => _allProducts?.Count ?? 0;
        public int LowStockItemsCount => _allProducts?.Count(p => p.Stock <= p.ReorderPoint && p.Stock > 0) ?? 0;
        public int OutOfStockItemsCount => _allProducts?.Count(p => p.Stock <= 0) ?? 0;

        private void FilterProducts()
        {
            Products.Clear();
            
            var query = _allProducts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SelectedPosCategory) && SelectedPosCategory != "ทั้งหมด")
            {
                query = query.Where(p => p.Category != null && p.Category.Equals(SelectedPosCategory, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string s = SearchText.ToLower();
                query = query.Where(p => 
                    (p.Code?.ToLower().Contains(s) ?? false) ||
                    (p.Name?.ToLower().Contains(s) ?? false)
                );
            }

            if (!string.IsNullOrWhiteSpace(SelectedStockStatusFilter) && SelectedStockStatusFilter != "ทั้งหมด")
            {
                switch (SelectedStockStatusFilter)
                {
                    case "ปกติ":
                        query = query.Where(p => p.Stock > p.ReorderPoint);
                        break;
                    case "ใกล้หมด":
                        query = query.Where(p => p.Stock <= p.ReorderPoint && p.Stock > 0);
                        break;
                    case "หมดสต็อก":
                    case "สินค้าหมด":
                        query = query.Where(p => p.Stock <= 0);
                        break;
                }
            }

            query = query
                .OrderBy(p => p.StatusPriority)
                .ThenBy(p => p.ProductID);

            foreach (var p in query)
            {
                Products.Add(p);
            }

            OnPropertyChanged(nameof(TotalStockItemsCount));
            OnPropertyChanged(nameof(LowStockItemsCount));
            OnPropertyChanged(nameof(OutOfStockItemsCount));
        }

        private void ScanBarcode()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return;

            // Find Exact Match by Code
            var product = _allProducts.FirstOrDefault(p => p.Code != null && p.Code.Equals(SearchText, System.StringComparison.OrdinalIgnoreCase));

            if (product != null)
            {
                // Exact match found -> Scan to Add
                AddToCart(product);
                SearchText = string.Empty; // Clear for next scan
                
                // Optional: Play sound or give feedback
                System.Media.SystemSounds.Beep.Play();
            }
            else
            {
                // No exact match, maybe just keep filtering?
                // Or if user pressed Enter and nothing found, maybe show "Not Found"?
                // For now, FilterProducts() already handles the visual filtering. 
                // We'll leave it as is so it acts as a normal search submit if not a barcode.
            }
        }

        private async Task AddProduct()
        {
            decimal parsedCost = CostPrice > 0 ? CostPrice : Cost;
            int catIdToSave = SelectedProductCategory?.CategoryID ?? (CategoryID > 0 ? CategoryID : 0);
            string catNameToSave = SelectedProductCategory?.CategoryName ?? (!string.IsNullOrWhiteSpace(Category) ? Category : "");

            var product = new Product
            {
                Code = Code,
                Name = Name,
                Category = catNameToSave,
                CategoryId = catIdToSave,
                Unit = string.IsNullOrWhiteSpace(Unit) ? "ชิ้น" : Unit.Trim(),
                Price = Price,
                Cost = parsedCost,
                Stock = Stock,
                ReorderPoint = ReorderPoint,
                ImagePath = ImagePath ?? "",
                Description = Description ?? ""
            };

            using (var conn = _databaseService.GetConnection())
            {
                string sql = @"
                    INSERT INTO tblProduct (Pro_Barcode, Pro_Name, Pro_Price, Pro_Cost, Pro_Qty, Pro_MinQty, Pro_Category, Pro_Image, Pro_Unit, CategoryId) 
                    VALUES (@Code, @Name, @Price, @Cost, @Stock, @ReorderPoint, @Category, @ImagePath, @Unit, @CategoryId);
                    SELECT last_insert_rowid();";
                
                var id = await conn.ExecuteScalarAsync<int>(sql, new {
                    Code = product.Code,
                    Name = product.Name,
                    Price = product.Price,
                    Cost = product.Cost,
                    Stock = product.Stock,
                    ReorderPoint = product.ReorderPoint,
                    Category = product.Category,
                    ImagePath = product.ImagePath,
                    Unit = product.Unit,
                    CategoryId = product.CategoryId
                });
                product.Id = id;
                product.Pro_ID = id;

                _allProducts.Add(product);
                FilterProducts();
                _ = LoadDashboardData();
                ClearForm();
            }
        }

        private async Task UpdateProduct()
        {
            if (SelectedProduct == null) return;

            decimal parsedCost = CostPrice > 0 ? CostPrice : Cost;
            int catIdToSave = SelectedProductCategory?.CategoryID ?? (CategoryID > 0 ? CategoryID : (SelectedProduct.CategoryID > 0 ? SelectedProduct.CategoryID : 0));
            string catNameToSave = SelectedProductCategory?.CategoryName ?? (!string.IsNullOrWhiteSpace(Category) ? Category : (SelectedProduct.Category ?? ""));

            var product = SelectedProduct;
            product.Code = Code;
            product.Name = Name;
            product.Category = catNameToSave;
            product.CategoryId = catIdToSave;
            product.Unit = string.IsNullOrWhiteSpace(Unit) ? "ชิ้น" : Unit.Trim();
            product.Price = Price;
            product.Cost = parsedCost;
            product.Stock = Stock;
            product.ReorderPoint = ReorderPoint;
            product.ImagePath = ImagePath ?? "";
            product.Description = Description ?? "";

            using (var conn = _databaseService.GetConnection())
            {
                string sql = @"
                    UPDATE tblProduct 
                    SET Pro_Barcode = @Code, 
                        Pro_Name = @Name, 
                        Pro_Price = @Price, 
                        Pro_Cost = @Cost, 
                        Pro_Qty = @Stock, 
                        Pro_MinQty = @ReorderPoint, 
                        Pro_Category = @Category, 
                        Pro_Image = @ImagePath,
                        Pro_Unit = @Unit,
                        CategoryId = @CategoryId
                    WHERE Pro_ID = @Id";
                
                await conn.ExecuteAsync(sql, new {
                    Code = product.Code,
                    Name = product.Name,
                    Price = product.Price,
                    Cost = product.Cost,
                    Stock = product.Stock,
                    ReorderPoint = product.ReorderPoint,
                    Category = product.Category,
                    ImagePath = product.ImagePath,
                    Unit = product.Unit,
                    CategoryId = product.CategoryId,
                    Id = product.Id > 0 ? product.Id : product.Pro_ID
                });

                // Update in local cache
                var existing = _allProducts.FirstOrDefault(p => p.Id == product.Id || p.Pro_ID == product.Id);
                if (existing != null)
                {
                    int idx = _allProducts.IndexOf(existing);
                    _allProducts[idx] = product;
                }
                
                FilterProducts();
                _ = LoadDashboardData();
            }
        }

        private async Task SaveStockAdjustment()
        {
            if (SelectedProduct == null) return;

            // Calculate new stock
            int newStock = SelectedProduct.Stock + AdjustmentQuantity;
            if (newStock < 0) newStock = 0; // Prevent negative stock

            // Create a comprehensive note - In a real app we would save this to a transaction log
             string fullNote = $"[Adjustment] {SelectedAdjustmentReason}: {AdjustmentQuantity} (Note: {AdjustmentNote})";

            var product = SelectedProduct;
            product.Stock = newStock;
            
            // Re-use Update logic but specifically for stock
            // We need to call the database update manually here since we want to be explicit about the change
            using (var conn = _databaseService.GetConnection())
            {
                 string sql = @"
                    UPDATE tblProduct 
                    SET Pro_Qty = @Stock
                    WHERE Pro_ID = @Id";
                
                await conn.ExecuteAsync(sql, new { Stock = product.Stock, Id = product.Id });
                
                // Update in local cache
                var existing = _allProducts.FirstOrDefault(p => p.Id == product.Id);
                if (existing != null)
                {
                     existing.Stock = product.Stock;
                }
                
                FilterProducts();
                _ = LoadDashboardData();
                ClearForm();
            }
        }

        private async Task DeleteProduct()
        {
            if (SelectedProduct == null) return;

            using (var conn = _databaseService.GetConnection())
            {
                await conn.ExecuteAsync("DELETE FROM tblProduct WHERE Pro_ID = @Id", new { Id = SelectedProduct.Id });
                
                var existing = _allProducts.FirstOrDefault(p => p.Id == SelectedProduct.Id);
                if (existing != null)
                {
                    _allProducts.Remove(existing);
                }

                FilterProducts();
                _ = LoadDashboardData();
                ClearForm();
            }
        }

        private void ClearForm()
        {
            SelectedProduct = null;
            Code = GenerateProductCode();
            Name = string.Empty;
            if (ProductCategories != null && ProductCategories.Any())
            {
                var firstCat = ProductCategories.First();
                SelectedProductCategory = firstCat;
                CategoryID = firstCat.CategoryID;
                Category = firstCat.CategoryName;
            }
            else
            {
                SelectedProductCategory = null;
                Category = CategoriesList?.FirstOrDefault() ?? "เบ็ดเตล็ด";
            }
            Unit = string.Empty;
            Price = 20; // Default for 20 Baht Shop
            Cost = 0;
            CostPrice = 0;
            Stock = 0;
            ReorderPoint = 5;
            ImagePath = string.Empty;
            Description = string.Empty;
            OnPropertyChanged(nameof(Code));
            OnPropertyChanged(nameof(AutoProductCode));
            OnPropertyChanged(nameof(ProductCode));
            OnPropertyChanged(nameof(CostPrice));
        }

        // Cart Logic
        private void AddToCart(object parameter)
        {
            if (parameter is Product product)
            {
                if (product.Stock <= 0) return; // Prevent adding if out of stock

                var existingItem = CartItems.FirstOrDefault(c => c.Product.Id == product.Id);
                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    CartItems.Add(new CartItem { Product = product, Quantity = 1 });
                }
                CalculateTotal();
            }
        }

        private void RemoveFromCart(object parameter)
        {
            if (parameter is CartItem item)
            {
                CartItems.Remove(item);
                CalculateTotal();
            }
        }

        private void ClearCart()
        {
            CartItems.Clear();
            CalculateTotal();
            CashReceived = 0;
            _cashInputBuffer = "";
        }

        private void CalculateTotal()
        {
            TotalAmount = CartItems.Sum(i => i.Total);
            if (IsTransferPayment)
            {
                CashReceived = CheckoutGrandTotal;
            }
            CalculateChange();
            OnPropertyChanged(nameof(CartTotalItems));
            OnPropertyChanged(nameof(CartTotalString));
            OnPropertyChanged(nameof(CheckoutGrandTotal));
            CommandManager.InvalidateRequerySuggested();
        }

        private void CalculateChange()
        {
            if (IsTransferPayment)
            {
                Change = 0;
            }
            else if (CashReceived >= CheckoutGrandTotal && CheckoutGrandTotal > 0)
            {
                Change = CashReceived - CheckoutGrandTotal;
            }
            else
            {
                Change = 0;
            }
        }

        private void OpenCheckoutWizard()
        {
            if (CartItems.Count == 0) return;
            IsCheckoutWizardOpen = true;
            CheckoutStep = 2; // Jump directly to POS Cashier Payment Screen!
            CashReceived = 0;
            _cashInputBuffer = "";
            IsCashPayment = true; // Default to Cash payment
        }

        private async Task NextStep()
        {
            if (CheckoutStep == 1)
            {
                if (IsDeliverySelected)
                {
                    if (string.IsNullOrWhiteSpace(DeliveryCustomerName) || 
                        string.IsNullOrWhiteSpace(DeliveryCustomerPhone) || 
                        string.IsNullOrWhiteSpace(DeliveryCustomerAddress))
                    {
                        CustomMessageBox.Show("กรุณากรอกชื่อลูกค้า / ผู้สั่ง เบอร์โทรศัพท์ และที่อยู่จัดส่งให้ครบถ้วนก่อนไปหน้าชำระเงิน", "ข้อมูลไม่ครบถ้วน", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                CheckoutStep = 2; // Proceed to Payment Modal
            }
            else if (CheckoutStep == 2)
            {
                if (IsCashPayment && CashReceived < CheckoutGrandTotal)
                {
                    CustomMessageBox.Show($"จำนวนเงินที่รับมาไม่เพียงพอ (ยอดที่ต้องชำระ {CheckoutGrandTotal:N2})", "เงินสดไม่เพียงพอ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await Checkout();
            }
        }

        private void PrevStep()
        {
            IsCheckoutWizardOpen = false;
        }

        private void PrintCurrentReceipt()
        {
            try
            {
                var order = new SalesOrder
                {
                    RefNo = string.IsNullOrWhiteSpace(LastRefNo) ? "SALE-" + System.DateTime.Now.ToString("yyyyMMddHHmmss") : LastRefNo,
                    Sales_Date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    TotalAmount = FinalGrandTotal,
                    CashReceived = FinalCashReceived,
                    Change = FinalChange,
                    PaymentMethod = SelectedPaymentMethod,
                    Status = "ชำระเงินแล้ว"
                };

                var items = CartItems.Select(item => new SalesOrderItem
                {
                    Pro_ID = item.Product.Id,
                    ProductName = item.Product.Name,
                    UnitPrice = item.Product.Price,
                    Quantity = item.Quantity,
                    Total = item.Total
                }).ToList();

                Porjai20.Views.ReceiptWindow receiptWindow = new Porjai20.Views.ReceiptWindow(order, items);
                receiptWindow.ShowDialog();
            }
            catch (System.Exception ex)
            {
                CustomMessageBox.Show($"เกิดข้อผิดพลาดในการพิมพ์ใบเสร็จ: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task Checkout()
        {
            if (CartItems.Count == 0) return;

            if (IsCashPayment && CashReceived < CheckoutGrandTotal)
            {
                CustomMessageBox.Show($"จำนวนเงินที่รับมาไม่เพียงพอ (ยอดที่ต้องชำระ {CheckoutGrandTotal:N2})", "เงินสดไม่เพียงพอ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1. Validate Stock
            foreach (var item in CartItems)
            {
                if (item.Product.Stock < item.Quantity)
                {
                    CustomMessageBox.Show($"สินค้า '{item.Product.Name}' มีสินค้าไม่พอ (คงเหลือ {item.Product.Stock})", "สินค้าไม่พอ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            string orderRef = "SALE-" + System.DateTime.Now.ToString("yyyyMMddHHmmss");

            // 2. Deduct Stock & Record Transaction
            using (var conn = _databaseService.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 3. Create Sales Order with matching SQL parameters
                        var salesOrderParam = new
                        {
                            RefNo = orderRef,
                            Sales_Date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            Cus_ID = SelectedCustomer != null && SelectedCustomer.Id > 0 ? (int?)SelectedCustomer.Id : null,
                            Emp_ID = (int?)null,
                            Sales_Total = CheckoutGrandTotal,
                            Sales_Cash = IsCashPayment ? CashReceived : CheckoutGrandTotal,
                            Sales_Change = IsCashPayment ? Change : 0,
                            Sales_PaymentType = SelectedPaymentMethod,
                            Sales_Status = "ชำระเงินแล้ว"
                        };

                        string sqlOrder = @"INSERT INTO tblSales_H (RefNo, Sales_Date, Cus_ID, Emp_ID, Sales_Total, Sales_Cash, Sales_Change, Sales_PaymentType, Sales_Status)
                                            VALUES (@RefNo, @Sales_Date, @Cus_ID, @Emp_ID, @Sales_Total, @Sales_Cash, @Sales_Change, @Sales_PaymentType, @Sales_Status);
                                            SELECT last_insert_rowid();";
                        int orderId = await conn.ExecuteScalarAsync<int>(sqlOrder, salesOrderParam, trans);

                        if (IsDeliverySelected)
                        {
                            string recipientName = !string.IsNullOrWhiteSpace(DeliveryCustomerName)
                                ? DeliveryCustomerName.Trim()
                                : (!string.IsNullOrWhiteSpace(RecipientName) ? RecipientName.Trim() : (!string.IsNullOrWhiteSpace(CustomerName) ? CustomerName.Trim() : (SelectedCustomer?.Name?.Trim() ?? "ลูกค้าทั่วไป")));

                            string recipientTel = !string.IsNullOrWhiteSpace(DeliveryCustomerPhone)
                                ? DeliveryCustomerPhone.Trim()
                                : (!string.IsNullOrWhiteSpace(RecipientPhone) ? RecipientPhone.Trim() : (!string.IsNullOrWhiteSpace(CustomerPhone) ? CustomerPhone.Trim() : (SelectedCustomer?.Phone?.Trim() ?? "-")));

                            string recipientAddress = !string.IsNullOrWhiteSpace(DeliveryCustomerAddress)
                                ? DeliveryCustomerAddress.Trim()
                                : (!string.IsNullOrWhiteSpace(ShippingAddress) ? ShippingAddress.Trim() : (!string.IsNullOrWhiteSpace(CustomerAddress) ? CustomerAddress.Trim() : (SelectedCustomer?.Address?.Trim() ?? "ไม่ระบุที่อยู่")));

                            string sqlDeliv = @"INSERT INTO tblDelivery (Sales_ID, Recipient_Name, Recipient_Tel, Recipient_Address, Tracking_No, Delivery_Status)
                                                VALUES (@Sales_ID, @Recipient_Name, @Recipient_Tel, @Recipient_Address, @Tracking_No, @Delivery_Status);";
                            await conn.ExecuteAsync(sqlDeliv, new 
                            { 
                                Sales_ID = orderId, 
                                Recipient_Name = recipientName, 
                                Recipient_Tel = recipientTel, 
                                Recipient_Address = recipientAddress,
                                Tracking_No = (string?)null,
                                Delivery_Status = "รอจัดส่ง"
                            }, trans);
                        }

                        foreach (var item in CartItems)
                        {
                            string sqlUpdate = "UPDATE tblProduct SET Pro_Qty = Pro_Qty - @Qty WHERE Pro_ID = @Id";
                            await conn.ExecuteAsync(sqlUpdate, new { Qty = item.Quantity, Id = item.Product.Id }, trans);

                            var orderItem = new SalesOrderItem
                            {
                                Sales_ID = orderId,
                                SalesOrderId = orderId,
                                Pro_ID = item.Product.Id,
                                ProductId = item.Product.Id,
                                Pro_Price = item.Product.Price,
                                UnitPrice = item.Product.Price,
                                Sales_Qty = item.Quantity,
                                Quantity = item.Quantity,
                                Sales_Subtotal = item.Total,
                                Total = item.Total
                            };
                            string sqlOrderItem = @"INSERT INTO tblSalesDetail (Sales_ID, Pro_ID, Pro_Price, Sales_Qty, Sales_Subtotal)
                                                    VALUES (@Sales_ID, @Pro_ID, @Pro_Price, @Sales_Qty, @Sales_Subtotal)";
                            await conn.ExecuteAsync(sqlOrderItem, orderItem, trans);

                            // Update Local Product Instance
                            item.Product.Stock -= item.Quantity;
                            var existing = _allProducts.FirstOrDefault(p => p.Id == item.Product.Id);
                            if (existing != null)
                            {
                                existing.Stock = item.Product.Stock;
                            }
                        }

                        trans.Commit();
                    }
                    catch (System.Exception ex)
                    {
                        trans.Rollback();
                        CustomMessageBox.Show($"เกิดข้อผิดพลาดในการทำรายการชำระเงิน: {ex.Message}", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
            
            // Capture snapshot BEFORE clearing so Step 4 can display the correct values
            FinalCashReceived = CashReceived;
            FinalChange = Change;
            FinalGrandTotal = CheckoutGrandTotal;

            // ClearCart() is now deferred to CloseCheckoutCommand so Step 4 retains the summary
            LastRefNo = orderRef;
            CheckoutStep = 4;

            FilterProducts(); // Refresh product list to update stock UI
            _ = LoadDashboardData();
        }

        private void PrintSelectedOrder()
        {
            if (SelectedOrder == null)
            {
                CustomMessageBox.Show("กรุณาเลือกบิลที่ต้องการพิมพ์ใบเสร็จ", "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ReceiptWindow receiptWindow = new ReceiptWindow(SelectedOrder, SelectedOrderItems);
            receiptWindow.ShowDialog();
        }

        private async Task ConfirmStockIn()
        {
            if (RolePermissions.IsReadOnly(CurrentUser?.Role, "stock_in"))
            {
                ShowAlert("ไม่มีสิทธิ์ในการดำเนินการนี้ (สิทธิ์ดูอย่างเดียว)", "ไม่มีสิทธิ์", "⚠️");
                return;
            }

            if (SelectedProduct == null) return;

            var transaction = new StockTransaction
            {
                ProductId = SelectedProduct.Id,
                ProductName = SelectedProduct.Name,
                Quantity = StockInQuantity,
                UnitCost = StockInCost,
                RefNo = StockInRefNo,
                Supplier = StockInSupplier,
                Timestamp = System.DateTime.Now
            };

            // Update Product
            SelectedProduct.Stock += StockInQuantity;
            SelectedProduct.Cost = StockInCost; // Update latest cost
            
            // Database Transaction
            using (var conn = _databaseService.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try 
                    {
                        string sqlStockH = @"INSERT INTO tblStockIn_H (StockIn_Date, Note) VALUES (datetime('now'), 'Stock In'); SELECT last_insert_rowid();";
                        int stockInId = await conn.ExecuteScalarAsync<int>(sqlStockH, null, trans);

                        string sqlTrans = @"INSERT INTO tblStockInDetail (StockIn_ID, Pro_ID, StockIn_Qty) VALUES (@StockIn_ID, @Pro_ID, @StockIn_Qty)";
                        await conn.ExecuteAsync(sqlTrans, new { StockIn_ID = stockInId, Pro_ID = SelectedProduct.Id, StockIn_Qty = StockInQuantity }, trans);

                        string sqlProd = "UPDATE tblProduct SET Pro_Qty = Pro_Qty + @Qty, Pro_Cost = @Cost WHERE Pro_ID = @Id";
                        await conn.ExecuteAsync(sqlProd, new { Qty = StockInQuantity, Cost = StockInCost, Id = SelectedProduct.Id }, trans);

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        CustomMessageBox.Show("เกิดข้อผิดพลาดในการบันทึกรับสินค้าเข้าคลัง", "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
            
            // Refresh UI
            StockInQuantity = 0;
            StockInRefNo = string.Empty;
            // Keep Supplier and Cost for convenience? Maybe clear Cost.
            // StockInCost = 0; 

            // Update Local Cache (Already updated reference, but good to be sure)
            var existing = _allProducts.FirstOrDefault(p => p.Id == SelectedProduct.Id);
            if (existing != null)
            {
                existing.Stock = SelectedProduct.Stock;
                existing.Cost = SelectedProduct.Cost;
            }
            FilterProducts(); // Refresh list to show new stock
            _ = LoadStockTransactions();
            _ = LoadDashboardData();
            IsStockInModalOpen = false;
            CustomMessageBox.Show("บันทึกการรับสินค้าเข้าคลังเรียบร้อยแล้ว!", "สำเร็จ", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task OpenHistory()
        {
            // Switch to History Mode (Integrated View)
            IsHomeMode = false;
            IsManagementMode = false;
            IsStockInMode = false;
            IsReportsMode = false;
            IsHistoryMode = true;
            IsDeliveryMode = false;
            
            SelectedHistoryDate = DateTime.Today;
            await LoadSalesHistory();
        }

        // --- Delivery Management ---
        private async Task OpenDeliveryMode()
        {
            IsDeliveryMode = true;
            IsHomeMode = false;
            IsManagementMode = false;
            IsStockInMode = false;
            IsReportsMode = false;
            IsHistoryMode = false;
            IsStaffMode = false;
            await LoadDeliveries();
        }

        private async Task LoadDeliveries()
        {
            int currentSelectedId = SelectedDeliveryOrder?.Id ?? 0;
            var deliveries = await Task.Run(() => _databaseService.GetDeliveries(DeliverySearchKeyword));

            if (!string.IsNullOrEmpty(SelectedDeliveryStatusFilter) && SelectedDeliveryStatusFilter != "ทั้งหมด")
            {
                deliveries = deliveries.Where(d => d.DeliveryStatus == SelectedDeliveryStatusFilter);
            }

            if (DeliveryStartDate != default && DeliveryEndDate != default)
            {
                var start = DeliveryStartDate.Date;
                var end = DeliveryEndDate.Date.AddDays(1).AddTicks(-1);
                deliveries = deliveries.Where(d => d.Timestamp >= start && d.Timestamp <= end);
            }

            Deliveries.Clear();
            foreach (var delivery in deliveries)
            {
                Deliveries.Add(delivery);
            }

            if (currentSelectedId > 0)
            {
                var match = Deliveries.FirstOrDefault(d => d.Id == currentSelectedId);
                if (match != null)
                {
                    SelectedDeliveryOrder = match;
                }
            }

            OnPropertyChanged(nameof(PendingDeliveriesCount));
            OnPropertyChanged(nameof(InDeliveryCount));
            OnPropertyChanged(nameof(DeliveredTodayCount));
            OnPropertyChanged(nameof(IsDeliveryOrderSelected));
        }

        private async Task LoadSelectedDeliveryItems(int orderId)
        {
            SelectedOrderItems.Clear();
            if (orderId == 0) return;

            using (var conn = _databaseService.GetConnection())
            {
                string sql = @"
                    SELECT 
                        d.Detail_ID AS Detail_ID, 
                        d.Detail_ID AS Id, 
                        d.Sales_ID, 
                        d.Pro_ID, 
                        COALESCE(p.Pro_Name, CAST(d.Pro_ID AS TEXT)) AS ProductName, 
                        d.Pro_Price AS UnitPrice, 
                        d.Sales_Qty AS Quantity, 
                        d.Sales_Subtotal AS Total 
                    FROM tblSalesDetail d
                    LEFT JOIN tblProduct p ON d.Pro_ID = p.Pro_ID
                    WHERE d.Sales_ID = @OrderId";
                var items = await conn.QueryAsync<SalesOrderItem>(sql, new { OrderId = orderId });
                foreach (var item in items)
                {
                    SelectedOrderItems.Add(item);
                }
            }
            OnPropertyChanged(nameof(SelectedDeliverySubtotal));
            OnPropertyChanged(nameof(SelectedDeliveryShippingFee));
            OnPropertyChanged(nameof(SelectedDeliveryGrandTotal));
        }

        public void UpdateDeliveryStatus(SalesOrder order, string status)
        {
            if (order != null)
            {
                string cleanStatus = status.Replace("⌛", "").Replace("🚚", "").Replace("☑", "").Replace("✖", "").Trim();
                _databaseService.UpdateDeliveryStatus(order.Id, cleanStatus);
                order.DeliveryStatus = cleanStatus;

                var existing = Deliveries.FirstOrDefault(d => d.Id == order.Id);
                if (existing != null)
                {
                    existing.DeliveryStatus = cleanStatus;
                }

                _selectedDeliveryOrderStatus = cleanStatus;

                OnPropertyChanged(nameof(PendingDeliveriesCount));
                OnPropertyChanged(nameof(InDeliveryCount));
                OnPropertyChanged(nameof(DeliveredTodayCount));
                OnPropertyChanged(nameof(SelectedDeliveryOrderStatus));
                OnPropertyChanged(nameof(SelectedDeliveryOrder));
            }
        }

        private void SaveDeliveryStatus()
        {
            if (SelectedDeliveryOrder != null && !string.IsNullOrEmpty(SelectedDeliveryStatus))
            {
                UpdateDeliveryStatus(SelectedDeliveryOrder, SelectedDeliveryStatus);
            }
            IsManageModalOpen = false;
        }

        private void PrintDeliverySlip(SalesOrder order)
        {
            var targetOrder = order ?? SelectedDeliveryOrder;
            if (targetOrder == null)
            {
                ShowAlert("กรุณาเลือกรายการคำสั่งซื้อที่ต้องการพิมพ์", "แจ้งเตือน", "⚠️");
                return;
            }

            var items = (SelectedOrderItems != null && SelectedOrderItems.Count > 0 && SelectedDeliveryOrder?.Id == targetOrder.Id)
                ? SelectedOrderItems.ToList()
                : null;

            // Open the print window and pass the selected delivery order
            var printWindow = new DeliverySlipWindow(targetOrder, items);
            if (System.Windows.Application.Current?.MainWindow != null)
            {
                printWindow.Owner = System.Windows.Application.Current.MainWindow;
            }
            printWindow.ShowDialog();
        }

        // --- Customer Management Logics ---
        private async Task OpenCustomerMode()
        {
            IsCustomerMode = true;
            IsManagementMode = false;
            IsStockInMode = false;
            IsReportsMode = false;
            IsHistoryMode = false;
            IsHomeMode = false;
            IsDeliveryMode = false;
            IsStaffMode = false;
            await LoadCustomers();
        }

        private async Task LoadCustomers()
        {
            Customers.Clear();
            var allCustomers = await Task.Run(() => _databaseService.GetCustomers());
            
            if (!string.IsNullOrWhiteSpace(CustomerSearchKeyword))
            {
                var trimmedKeyword = CustomerSearchKeyword.Trim();
                allCustomers = allCustomers.Where(c => 
                    (c.Code != null && c.Code.Contains(trimmedKeyword, StringComparison.OrdinalIgnoreCase)) ||
                    (c.Name != null && c.Name.Contains(trimmedKeyword, StringComparison.OrdinalIgnoreCase)));
            }
            
            foreach (var cust in allCustomers)
            {
                Customers.Add(cust);
            }
            
            OnPropertyChanged(nameof(TotalCustomersCount));
            OnPropertyChanged(nameof(NewCustomersThisMonthCount));
            OnPropertyChanged(nameof(TotalCumulativeSpent));
            OnPropertyChanged(nameof(Customers));
            OnPropertyChanged(nameof(FilteredCustomers));
        }

        private bool _isCustomerAlertOpen;
        public bool IsCustomerAlertOpen
        {
            get => _isCustomerAlertOpen;
            set { _isCustomerAlertOpen = value; OnPropertyChanged(nameof(IsCustomerAlertOpen)); }
        }

        private string _customerAlertTitle = "แจ้งเตือน";
        public string CustomerAlertTitle
        {
            get => _customerAlertTitle;
            set { _customerAlertTitle = value; OnPropertyChanged(nameof(CustomerAlertTitle)); }
        }

        private string _customerAlertMessage = string.Empty;
        public string CustomerAlertMessage
        {
            get => _customerAlertMessage;
            set { _customerAlertMessage = value; OnPropertyChanged(nameof(CustomerAlertMessage)); }
        }

        private string _customerAlertIcon = "✨";
        public string CustomerAlertIcon
        {
            get => _customerAlertIcon;
            set { _customerAlertIcon = value; OnPropertyChanged(nameof(CustomerAlertIcon)); }
        }

        public void ShowCustomerAlert(string message, string title = "สำเร็จ", string icon = "✅")
        {
            CustomerAlertTitle = title;
            CustomerAlertMessage = message;
            CustomerAlertIcon = icon;
            IsCustomerAlertOpen = true;
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

        private void SaveCustomer()
        {
            if (SelectedCustomer != null && SelectedCustomer.Id > 0)
            {
                UpdateCustomer();
            }
            else
            {
                AddCustomer();
            }
        }

        private void AddCustomer()
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
            IsCustomerModalOpen = false;
            _ = LoadCustomers();
            ShowCustomerAlert("บันทึกข้อมูลลูกค้าเรียบร้อยแล้ว", "สำเร็จ", "🎉");
        }

        private void UpdateCustomer()
        {
            if (SelectedCustomer == null || SelectedCustomer.Id == 0) return;
            
            string codeToUse = !string.IsNullOrWhiteSpace(CustomerCode)
                ? CustomerCode
                : (!string.IsNullOrWhiteSpace(SelectedCustomer.Code) ? SelectedCustomer.Code : GenerateCustomerCode());

            SelectedCustomer.Code = codeToUse;
            SelectedCustomer.Name = CustomerName;
            SelectedCustomer.Phone = CustomerPhone;
            SelectedCustomer.Address = CustomerAddress;
            
            _databaseService.UpdateCustomer(SelectedCustomer);
            ClearCustomerForm();
            IsCustomerModalOpen = false;
            IsEditCustomerModalOpen = false;
            _ = LoadCustomers();
            ShowCustomerAlert("อัปเดตข้อมูลลูกค้าเรียบร้อยแล้ว", "สำเร็จ", "✏️");
        }

        private void DeleteCustomer()
        {
            if (SelectedCustomer == null || SelectedCustomer.Id == 0) return;
            
            var result = CustomMessageBox.Show($"คุณต้องการลบลูกค้า '{SelectedCustomer.Name}' ใช่หรือไม่?\nประวัติการสั่งซื้อของลูกค้าจะไม่ถูกลบ", 
                                         "ยืนยันการลบ", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                                         
            if (result == MessageBoxResult.Yes)
            {
                _databaseService.DeleteCustomer(SelectedCustomer.Id);
                ClearCustomerForm();
                IsCustomerModalOpen = false;
                IsEditCustomerModalOpen = false;
                _ = LoadCustomers();
                ShowCustomerAlert("ลบข้อมูลลูกค้าเรียบร้อยแล้ว", "สำเร็จ", "🗑️");
            }
        }

        public void ClearCustomerForm()
        {
            CustomerCode = GenerateCustomerCode();
            CustomerName = string.Empty;
            CustomerPhone = string.Empty;
            CustomerAddress = string.Empty;
            CustomerValidationMessage = string.Empty;
        }

        // --- Partner Management Logics ---
        private async Task OpenPartnerMode()
        {
            IsPartnerMode = true;
            IsCustomerMode = false;
            IsManagementMode = false;
            IsStockInMode = false;
            IsReportsMode = false;
            IsHistoryMode = false;
            IsHomeMode = false;
            IsDeliveryMode = false;
            IsStaffMode = false;
            await LoadPartners();
        }

        private async Task LoadPartners()
        {
            Partners.Clear();
            var partners = await Task.Run(() => _databaseService.GetPartners());
            foreach (var p in partners)
            {
                Partners.Add(p);
            }
            FilteredPartners?.Refresh();

            OnPropertyChanged(nameof(TotalPartnersCount));
            OnPropertyChanged(nameof(MonthlyPurchaseOrdersCount));
            OnPropertyChanged(nameof(TotalSupplierPurchaseValue));
        }

        // --- Claim Management Logic ---
        private async Task OpenClaimMode()
        {
            IsClaimMode = true;
            IsCustomerMode = false;
            IsPartnerMode = false;
            IsManagementMode = false;
            IsStockInMode = false;
            IsReportsMode = false;
            IsHistoryMode = false;
            IsHomeMode = false;
            IsDeliveryMode = false;
            IsStaffMode = false;
            await LoadClaims();
        }


        public bool CanSaveClaim()
        {
            // 1. ต้องมีเลขที่ใบเสร็จ หรือเลขที่ใบรับสินค้า (ห้ามเป็นค่าว่างหรือช่องว่าง)
            bool hasDocumentNo = !string.IsNullOrWhiteSpace(SearchReceiptText);

            // 2. ต้องมีรายการสินค้าที่ต้องการเคลมอย่างน้อย 1 รายการ
            bool hasItems = (CurrentClaimItems != null && CurrentClaimItems.Count > 0) || !string.IsNullOrEmpty(ClaimProductName);

            // 3. สิทธิ์การแก้ไข (ไม่เป็น ReadOnly)
            bool hasPermission = CanEditClaim;

            return hasDocumentNo && hasItems && hasPermission;
        }

        public bool CanUpdateClaim()
        {
            // 1. ต้องมีเลขที่ใบเสร็จ หรือเลขที่ใบรับสินค้า (ห้ามเป็นค่าว่างหรือช่องว่าง)
            bool hasDocumentNo = !string.IsNullOrWhiteSpace(SearchReceiptText);

            // 2. ต้องมีรายการสินค้าที่ต้องการเคลมอย่างน้อย 1 รายการ
            bool hasItems = (CurrentClaimItems != null && CurrentClaimItems.Count > 0) || !string.IsNullOrEmpty(ClaimProductName);

            return SelectedClaim != null && SelectedClaim.Id > 0 && hasDocumentNo && hasItems && CanEditClaim;
        }

        private void AddClaim()
        {
            if (RolePermissions.IsReadOnly(CurrentUser?.Role, "claim"))
            {
                ShowAlert("ไม่มีสิทธิ์ในการดำเนินการนี้ (สิทธิ์ดูอย่างเดียว)", "ไม่มีสิทธิ์", "⚠️");
                return;
            }

            // Guard Clause: ตรวจสอบเลขที่ใบเสร็จ หรือเลขที่ใบรับสินค้าก่อนบันทึก
            if (string.IsNullOrWhiteSpace(SearchReceiptText))
            {
                string docLabel = ClaimType == "บริษัทคู่ค้า" ? "เลขที่ใบรับสินค้า" : "เลขที่ใบเสร็จ";
                ShowAlert($"กรุณาระบุ{docLabel}ให้ถูกต้องก่อนบันทึกข้อมูล", "ข้อมูลไม่ครบถ้วน", "⚠️");
                return;
            }

            // Guard Clause: ตรวจสอบว่ามีรายการสินค้าที่ต้องการเคลมอย่างน้อย 1 รายการ
            if ((CurrentClaimItems == null || CurrentClaimItems.Count == 0) && string.IsNullOrWhiteSpace(ClaimProductName))
            {
                ShowAlert("กรุณาเลือกรายการสินค้าที่ต้องการเคลมอย่างน้อย 1 รายการ", "ข้อมูลไม่ครบถ้วน", "⚠️");
                return;
            }

            // Validate based on type
            if (ClaimType == "ลูกค้า")
            {
                if (!ClaimSaleDate.HasValue)
                {
                    ShowAlert("กรุณากรอกเลขที่ใบเสร็จหรือวันที่ซื้อ", "ไม่สามารถเคลมได้", "⚠️");
                    return;
                }
                int days = (DateTime.Now.Date - ClaimSaleDate.Value.Date).Days;
                if (days > 2)
                {
                    ShowAlert($"ไม่สามารถเคลมได้ เนื่องจากเกิน 2 วันแล้ว ({days} วัน)", "หมดอายุการเคลม", "⚠️");
                    return;
                }
            }
            else // Supplier
            {
                if (!ClaimStockInDate.HasValue)
                {
                    ShowAlert("กรุณากรอกเลขที่ใบรับสินค้าหรือวันที่รับสินค้าเข้า", "ไม่สามารถเคลมได้", "⚠️");
                    return;
                }
                int days = (DateTime.Now.Date - ClaimStockInDate.Value.Date).Days;
                if (days > 7)
                {
                    ShowAlert($"ไม่สามารถส่งคืนซัพพลายเออร์ได้ เนื่องจากเกิน 7 วันแล้ว ({days} วัน)", "หมดอายุการคืน", "⚠️");
                    return;
                }
            }

            // Attempt to resolve Sales_ID and Pro_ID if not set
            int salesId = ClaimSalesId;
            if (salesId <= 0 && !string.IsNullOrWhiteSpace(ClaimSalesOrderRefNo))
            {
                salesId = _databaseService.GetSalesIdByRefNo(ClaimSalesOrderRefNo);
            }

            int proId = ClaimProId;
            if (proId <= 0)
            {
                proId = _databaseService.GetProductIdByBarcodeOrName(ClaimProductCode, ClaimProductName);
            }

            // If CurrentClaimItems has lines, ensure ClaimProductName/ClaimProductCode is set from first item
            if (CurrentClaimItems.Count > 0)
            {
                var first = CurrentClaimItems[0];
                if (proId <= 0) proId = first.Pro_ID;
                if (string.IsNullOrWhiteSpace(ClaimProductCode)) ClaimProductCode = first.ProductCode;
                if (string.IsNullOrWhiteSpace(ClaimProductName)) ClaimProductName = first.ProductName;
                ClaimQuantity = first.ClaimQty;
            }

            // Validation check before executing INSERT INTO tblClaim
            if (salesId <= 0 || (proId <= 0 && CurrentClaimItems.Count == 0))
            {
                ShowAlert("กรุณาเลือกใบเสร็จและสินค้าที่ต้องการเคลมให้ถูกต้อง", "ข้อมูลไม่ถูกต้อง", "⚠️");
                return;
            }

            // If multiple items were selected in CurrentClaimItems, save each item (or at least the first if only one)
            var itemsToSave = CurrentClaimItems.Count > 0 ? CurrentClaimItems.ToList() : new System.Collections.Generic.List<ClaimItemLine>
            {
                new ClaimItemLine { Pro_ID = proId, ProductCode = ClaimProductCode, ProductName = ClaimProductName, ClaimQty = ClaimQuantity }
            };

            bool anySaved = false;
            string lastError = string.Empty;

            foreach (var item in itemsToSave)
            {
                int itemProId = item.Pro_ID > 0 ? item.Pro_ID : _databaseService.GetProductIdByBarcodeOrName(item.ProductCode, item.ProductName);
                if (itemProId <= 0) continue;

                var claim = new Claim
                {
                    ClaimNo = "CLM-" + DateTime.Now.ToString("yyMMddHHmmss") + (itemsToSave.Count > 1 ? $"-{itemProId}" : ""),
                    ClaimType = ClaimType,
                    Sales_ID = salesId,
                    Pro_ID = itemProId,
                    SalesOrderRefNo = ClaimSalesOrderRefNo,
                    SaleDate = ClaimSaleDate,
                    CustomerName = ClaimCustomerName,
                    CustomerPhone = ClaimCustomerPhone,
                    StockInRefNo = ClaimStockInRefNo,
                    StockInDate = ClaimStockInDate,
                    ProductName = item.ProductName,
                    ProductCode = item.ProductCode,
                    Quantity = item.ClaimQty,
                    Reason = ClaimReason,
                    Note = ClaimNote,
                    Status = ClaimStatus,
                    ClaimAction = ClaimAction,
                    CreatedDate = DateTime.Now
                };

                if (_databaseService.SaveClaim(claim, out string errorMsg))
                {
                    anySaved = true;
                }
                else
                {
                    lastError = errorMsg;
                }
            }

            if (!anySaved)
            {
                ShowAlert(string.IsNullOrWhiteSpace(lastError) ? "เกิดข้อผิดพลาดในการบันทึกข้อมูลเคลม" : lastError, "เกิดข้อผิดพลาดในการบันทึก", "❌");
                return;
            }

            ClearClaimForm();
            IsModalOpen = false;
            _ = LoadClaims();
            ShowAlert("บันทึกการเคลมสำเร็จ", "สำเร็จ", "🎉");
        }

        private void UpdateClaimRecord()
        {
            if (RolePermissions.IsReadOnly(CurrentUser?.Role, "claim"))
            {
                ShowAlert("ไม่มีสิทธิ์ในการดำเนินการนี้ (สิทธิ์ดูอย่างเดียว)", "ไม่มีสิทธิ์", "⚠️");
                return;
            }

            // Guard Clause: ตรวจสอบเลขที่เอกสารก่อนบันทึกการแก้ไข
            if (string.IsNullOrWhiteSpace(SearchReceiptText))
            {
                string docLabel = ClaimType == "บริษัทคู่ค้า" ? "เลขที่ใบรับสินค้า" : "เลขที่ใบเสร็จ";
                ShowAlert($"กรุณาระบุ{docLabel}ให้ถูกต้องก่อนบันทึกข้อมูล", "ข้อมูลไม่ครบถ้วน", "⚠️");
                return;
            }

            if ((CurrentClaimItems == null || CurrentClaimItems.Count == 0) && string.IsNullOrWhiteSpace(ClaimProductName))
            {
                ShowAlert("กรุณาเลือกรายการสินค้าที่ต้องการเคลมอย่างน้อย 1 รายการ", "ข้อมูลไม่ครบถ้วน", "⚠️");
                return;
            }

            if (SelectedClaim == null || SelectedClaim.Id == 0) return;

            int salesId = ClaimSalesId > 0 ? ClaimSalesId : SelectedClaim.Sales_ID;
            if (salesId <= 0 && !string.IsNullOrWhiteSpace(ClaimSalesOrderRefNo))
            {
                salesId = _databaseService.GetSalesIdByRefNo(ClaimSalesOrderRefNo);
            }

            int proId = ClaimProId > 0 ? ClaimProId : SelectedClaim.Pro_ID;
            if (CurrentClaimItems.Count > 0)
            {
                proId = CurrentClaimItems[0].Pro_ID;
                ClaimProductName = CurrentClaimItems[0].ProductName;
                ClaimProductCode = CurrentClaimItems[0].ProductCode;
                ClaimQuantity = CurrentClaimItems[0].ClaimQty;
            }
            if (proId <= 0)
            {
                proId = _databaseService.GetProductIdByBarcodeOrName(ClaimProductCode, ClaimProductName);
            }

            if (salesId <= 0 || proId <= 0)
            {
                ShowAlert("กรุณาเลือกใบเสร็จและสินค้าที่ต้องการเคลมให้ถูกต้อง", "ข้อมูลไม่ถูกต้อง", "⚠️");
                return;
            }

            SelectedClaim.Sales_ID = salesId;
            SelectedClaim.Pro_ID = proId;
            SelectedClaim.ClaimType = ClaimType;
            SelectedClaim.SalesOrderRefNo = ClaimSalesOrderRefNo;
            SelectedClaim.SaleDate = ClaimSaleDate;
            SelectedClaim.CustomerName = ClaimCustomerName;
            SelectedClaim.CustomerPhone = ClaimCustomerPhone;
            SelectedClaim.StockInRefNo = ClaimStockInRefNo;
            SelectedClaim.StockInDate = ClaimStockInDate;
            SelectedClaim.ProductName = ClaimProductName;
            SelectedClaim.ProductCode = ClaimProductCode;
            SelectedClaim.Quantity = ClaimQuantity;
            SelectedClaim.Reason = ClaimReason;
            SelectedClaim.Note = ClaimNote;
            SelectedClaim.Status = ClaimStatus;
            SelectedClaim.ClaimAction = ClaimAction;

            try
            {
                _databaseService.UpdateClaim(SelectedClaim);
                ClearClaimForm();
                IsModalOpen = false;
                _ = LoadClaims();
                ShowAlert("อัปเดตข้อมูลการเคลมสำเร็จ", "สำเร็จ", "🎉");
            }
            catch (System.Exception ex)
            {
                ShowAlert($"เกิดข้อผิดพลาดในการอัปเดตข้อมูลการเคลม: {ex.Message}", "ข้อผิดพลาด", "❌");
            }
        }

        private void DeleteClaimRecord()
        {
            if (RolePermissions.IsReadOnly(CurrentUser?.Role, "claim"))
            {
                ShowAlert("ไม่มีสิทธิ์ในการดำเนินการนี้ (สิทธิ์ดูอย่างเดียว)", "ไม่มีสิทธิ์", "⚠️");
                return;
            }

            if (SelectedClaim == null || SelectedClaim.Id == 0) return;

            ShowConfirm($"คุณต้องการลบการเคลม '{SelectedClaim.ClaimNo}' ใช่หรือไม่?", () =>
            {
                _databaseService.DeleteClaim(SelectedClaim.Id);
                ClearClaimForm();
                _ = LoadClaims();
            }, "ยืนยันการลบ");
        }

        private void ClearClaimForm()
        {
            SelectedClaim = new Claim();
            ClaimSalesId = 0;
            ClaimProId = 0;
            ClaimNo = string.Empty;
            ClaimType = "ลูกค้า";
            ClaimSalesOrderRefNo = string.Empty;
            ClaimSaleDate = null;
            ClaimCustomerName = string.Empty;
            ClaimCustomerPhone = string.Empty;
            ClaimStockInRefNo = string.Empty;
            ClaimStockInDate = null;
            ClaimProductName = string.Empty;
            ClaimProductCode = string.Empty;
            ClaimQuantity = 1;
            ClaimReason = string.Empty;
            ClaimNote = string.Empty;
            ClaimStatus = "รอดำเนินการ";
            ClaimAction = "เปลี่ยนสินค้าใหม่";
            ClaimValidationMessage = string.Empty;
            ClaimValidationOk = true;
            IsReceiptProductSelectorOpen = false;
            AvailableReceiptItems.Clear();
            CurrentClaimItems.Clear();
            ClearGhostText();
            OnPropertyChanged(nameof(SearchReceiptText));
            IsModalOpen = false;
        }

        private void LookupSalesOrderForClaim()
        {
            if (string.IsNullOrWhiteSpace(ClaimSalesOrderRefNo)) return;
            var details = _databaseService.LookupReceiptDetails(ClaimSalesOrderRefNo);
            if (details != null)
            {
                ClaimSalesId = details.Sales_ID;
                ClaimProId = details.Pro_ID;
                ClaimSaleDate = details.SaleDate;
                ClaimProductCode = details.ProductCode;
                ClaimQuantity = details.Quantity;
                ClaimProductName = details.ProductName;
                ClaimCustomerName = details.CustomerName ?? string.Empty;
                ClaimCustomerPhone = details.CustomerPhone ?? string.Empty;

                // Load all items in receipt for selection
                AvailableReceiptItems.Clear();
                CurrentClaimItems.Clear();
                var items = _databaseService.GetReceiptProductSelections(ClaimSalesOrderRefNo);
                foreach (var itm in items)
                {
                    AvailableReceiptItems.Add(itm);
                }

                // If items exist, add the first one into CurrentClaimItems by default
                if (items.Count > 0)
                {
                    var f = items[0];
                    CurrentClaimItems.Add(new ClaimItemLine
                    {
                        Pro_ID = f.Pro_ID,
                        ProductCode = f.ProductCode,
                        ProductName = f.ProductName,
                        ReceiptQty = f.ReceiptQty,
                        ClaimQty = 1
                    });
                }

                RefreshClaimValidation();
            }
            else
            {
                ClaimSalesId = 0;
                ClaimProId = 0;
                AvailableReceiptItems.Clear();
                CurrentClaimItems.Clear();
                ClaimValidationMessage = "⚠️ ไม่พบข้อมูลเลขที่ใบเสร็จในระบบ";
                ClaimValidationOk = false;
            }
        }

        private void LookupStockInForClaim()
        {
            if (string.IsNullOrWhiteSpace(ClaimStockInRefNo)) return;
            var details = _databaseService.LookupStockInDetails(ClaimStockInRefNo);
            if (details != null)
            {
                ClaimProId = details.Pro_ID;
                ClaimStockInDate = details.StockInDate;
                ClaimProductCode = details.ProductCode;
                ClaimQuantity = details.Quantity < 0 ? -details.Quantity : details.Quantity;
                ClaimProductName = details.ProductName;
                ClaimCustomerName = !string.IsNullOrWhiteSpace(details.SupplierName) ? details.SupplierName : "บริษัทคู่ค้า / ซัพพลายเออร์";
                ClaimCustomerPhone = !string.IsNullOrWhiteSpace(details.SupplierPhone) ? details.SupplierPhone : "-";

                // Load all items in stock-in for selection
                AvailableReceiptItems.Clear();
                CurrentClaimItems.Clear();
                var items = _databaseService.GetStockInProductSelections(ClaimStockInRefNo);
                foreach (var itm in items)
                {
                    AvailableReceiptItems.Add(itm);
                }

                if (items.Count > 0)
                {
                    var f = items[0];
                    CurrentClaimItems.Add(new ClaimItemLine
                    {
                        Pro_ID = f.Pro_ID,
                        ProductCode = f.ProductCode,
                        ProductName = f.ProductName,
                        ReceiptQty = f.ReceiptQty,
                        ClaimQty = 1
                    });
                }

                RefreshClaimValidation();
            }
            else
            {
                ClaimProId = 0;
                AvailableReceiptItems.Clear();
                CurrentClaimItems.Clear();
                ClaimCustomerName = string.Empty;
                ClaimCustomerPhone = string.Empty;
                ClaimValidationMessage = "⚠️ ไม่พบข้อมูลเลขที่ใบรับสินค้าในระบบ";
                ClaimValidationOk = false;
            }
        }

        private void RefreshClaimValidation()
        {
            if (ClaimType == "ลูกค้า")
            {
                if (!ClaimSaleDate.HasValue)
                {
                    ClaimValidationMessage = string.Empty;
                    ClaimValidationOk = true;
                    return;
                }
                int days = (DateTime.Now.Date - ClaimSaleDate.Value.Date).Days;
                if (days <= 2)
                {
                    ClaimValidationMessage = "✓ อยู่ในสิทธิ์การเคลม (สั่งซื้อไม่เกิน 2 วัน)";
                    ClaimValidationOk = true;
                }
                else
                {
                    ClaimValidationMessage = $"✖ หมดสิทธิ์เคลม (เกิน {days} วัน จากกำหนด 2 วัน)";
                    ClaimValidationOk = false;
                }
            }
            else
            {
                if (!ClaimStockInDate.HasValue)
                {
                    ClaimValidationMessage = string.Empty;
                    ClaimValidationOk = true;
                    return;
                }
                int days = (DateTime.Now.Date - ClaimStockInDate.Value.Date).Days;
                if (days <= 7)
                {
                    ClaimValidationMessage = "✓ อยู่ในสิทธิ์การคืนสินค้า (รับเข้าไม่เกิน 7 วัน)";
                    ClaimValidationOk = true;
                }
                else
                {
                    ClaimValidationMessage = $"✖ หมดสิทธิ์คืน (เกิน {days} วัน จากกำหนด 7 วัน)";
                    ClaimValidationOk = false;
                }
            }
        }

        private void ViewCustomerHistory(Customer customer)
        {
            if (customer == null) return;
            
            CustomerHistoryName = customer.Name;
            CustomerHistoryPhone = customer.Phone;
            
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    string sql = @"
                        SELECT Sales_ID AS Sales_ID, Sales_ID AS Id, * FROM tblSales_H 
                        WHERE RefNo = @Phone OR RefNo = @Name
                        ORDER BY Sales_Date DESC";
                    
                    var history = connection.Query<SalesOrder>(sql, new { Phone = customer.Phone, Name = customer.Name }).ToList();
                    CustomerHistoryOrders = new ObservableCollection<SalesOrder>(history);
                    CustomerHistoryTotalOrders = history.Count;
                    CustomerHistoryTotalSpent = history.Sum(h => h.TotalAmount);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading customer history: {ex.Message}");
            }
            
            IsCustomerHistoryModalOpen = true;
        }

        
        public async Task LoadDashboardData()
        {
            try
            {
                using (var conn = _databaseService.GetConnection())
                {
                    var today = DateTime.Today;

                    var allSales = (await conn.QueryAsync("SELECT Sales_ID, Sales_Date, Sales_Total, Sales_Status FROM tblSales_H WHERE Sales_Status IS NULL OR (Sales_Status != 'ยกเลิก' AND Sales_Status != 'Cancelled')")).ToList();

                    decimal salesToday = 0;
                    int ordersToday = 0;
                    foreach (var s in allSales)
                    {
                        var dt = DashboardViewModel.ParseDateToInvariant(s.Sales_Date != null ? s.Sales_Date.ToString() : null);
                        if (dt.HasValue && dt.Value.Date == today.Date)
                        {
                            salesToday += Convert.ToDecimal(s.Sales_Total);
                            ordersToday++;
                        }
                    }

                    TodaySalesTotal = salesToday;
                    TodayOrdersCount = ordersToday;

                    string stockSql = "SELECT COALESCE(SUM(Pro_Qty), 0) FROM tblProduct";
                    TotalProductsInStock = await conn.ExecuteScalarAsync<int>(stockSql);

                    LowStockProducts.Clear();
                    string lowStockSql = "SELECT Pro_ID AS Pro_ID, Pro_ID AS Id, Pro_Barcode AS Code, Pro_Name AS Name, Pro_Price AS Price, Pro_Cost AS Cost, Pro_Qty AS Stock, Pro_MinQty AS ReorderPoint FROM tblProduct WHERE Pro_Qty <= Pro_MinQty";
                    var lowStocks = await conn.QueryAsync<Product>(lowStockSql);
                    foreach (var p in lowStocks)
                    {
                        LowStockProducts.Add(p);
                    }

                    RecentSalesOrders.Clear();
                    string recentSql = "SELECT Sales_ID AS Sales_ID, Sales_ID AS Id, * FROM tblSales_H ORDER BY Sales_ID DESC LIMIT 5";
                    var recents = await conn.QueryAsync<SalesOrder>(recentSql);
                    foreach (var o in recents)
                    {
                        RecentSalesOrders.Add(o);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard: {ex.Message}");
            }
        }

        private async Task LoadSalesHistory()
        {
            SalesOrders.Clear();
            using (var conn = _databaseService.GetConnection())
            {
                string sql = @"
                    SELECT 
                        s.Sales_ID AS Sales_ID, 
                        s.Sales_ID AS Id, 
                        s.RefNo, 
                        s.Sales_Date, 
                        s.Cus_ID, 
                        s.Sales_Total AS TotalAmount, 
                        s.Sales_Cash AS CashReceived, 
                        s.Sales_Change AS Change, 
                        COALESCE(s.Sales_PaymentType, 'เงินสด') AS PaymentMethod, 
                        COALESCE(s.Sales_Status, 'ชำระเงินแล้ว') AS Status,
                        c.Cus_Name AS CustomerName
                    FROM tblSales_H s
                    LEFT JOIN tblCustomer c ON s.Cus_ID = c.Cus_ID
                    WHERE (date(s.Sales_Date) = date(@Date) OR date(s.Sales_Date) = date(@DateTh))
                    ORDER BY s.Sales_Date DESC";
                var dateStr = SelectedHistoryDate.Date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                var dateThStr = SelectedHistoryDate.Date.AddYears(543).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                var orders = await conn.QueryAsync<SalesOrder>(sql, new { Date = dateStr, DateTh = dateThStr });
                foreach (var order in orders)
                {
                    SalesOrders.Add(order);
                }
            }
        }

        private async void LoadOrderItems(int orderId)
        {
            SelectedOrderItems.Clear();
            if (orderId == 0) return;

            using (var conn = _databaseService.GetConnection())
            {
                string sql = @"
                    SELECT 
                        d.Detail_ID AS Detail_ID, 
                        d.Detail_ID AS Id, 
                        d.Sales_ID, 
                        d.Pro_ID, 
                        COALESCE(p.Pro_Name, CAST(d.Pro_ID AS TEXT)) AS ProductName, 
                        d.Pro_Price AS UnitPrice, 
                        d.Sales_Qty AS Quantity, 
                        d.Sales_Subtotal AS Total 
                    FROM tblSalesDetail d
                    LEFT JOIN tblProduct p ON d.Pro_ID = p.Pro_ID
                    WHERE d.Sales_ID = @OrderId";
                var items = await conn.QueryAsync<SalesOrderItem>(sql, new { OrderId = orderId });
                foreach (var item in items)
                {
                    SelectedOrderItems.Add(item);
                }
            }
            OnPropertyChanged(nameof(SelectedDeliverySubtotal));
            OnPropertyChanged(nameof(SelectedDeliveryShippingFee));
            OnPropertyChanged(nameof(SelectedDeliveryGrandTotal));
        }

        public bool IsReportsMode
        {
            get => _isReportsMode;
            set
            {
                if (SetProperty(ref _isReportsMode, value))
                {
                    if (value) { _currentView = "reports"; OnPropertyChanged(nameof(CurrentView)); }
                    OnPropertyChanged(nameof(IsSearchMode));
                    OnPropertyChanged(nameof(IsManagementMode));
                    OnPropertyChanged(nameof(IsHomeMode));
                    OnPropertyChanged(nameof(IsStockInMode));
                }
            }
        }

        public decimal ReportTotalValue
        {
            get => _reportTotalValue;
            set => SetProperty(ref _reportTotalValue, value);
        }

        public decimal ReportTotalCost
        {
            get => _reportTotalCost;
            set => SetProperty(ref _reportTotalCost, value);
        }

        private decimal _reportTotalRevenue;
        public decimal ReportTotalRevenue
        {
            get => _reportTotalRevenue;
            set => SetProperty(ref _reportTotalRevenue, value);
        }

        private decimal _reportNetProfit;
        public decimal ReportNetProfit
        {
            get => _reportNetProfit;
            set => SetProperty(ref _reportNetProfit, value);
        }

        private decimal _reportTotalExpenses;
        public decimal ReportTotalExpenses
        {
            get => _reportTotalExpenses;
            set => SetProperty(ref _reportTotalExpenses, value);
        }

        private bool _isReportModalOpen;
        public bool IsReportModalOpen
        {
            get => _isReportModalOpen;
            set => SetProperty(ref _isReportModalOpen, value);
        }

        private ReportRow? _selectedReportRow;
        public ReportRow? SelectedReportRow
        {
            get => _selectedReportRow;
            set => SetProperty(ref _selectedReportRow, value);
        }

        public ICommand ExportReportCommand { get; set; }
        public ICommand PrintReportCommand { get; set; }
        public ICommand ClearReportFilterCommand { get; set; }
        public ICommand OpenReportDetailModalCommand { get; set; }
        public ICommand CloseReportModalCommand { get; set; }

        private async Task SwitchToReports()
        {
            IsHomeMode = false;
            IsManagementMode = false;
            IsStockInMode = false;
            IsHistoryMode = false;
            IsReportsMode = true;
            await LoadReports();
        }

        private async Task LoadReports()
        {
            // 1. Total Inventory Value & Cost
            ReportTotalValue = _allProducts.Sum(p => p.Price * p.Stock);
            ReportTotalCost = _allProducts.Sum(p => p.Cost * p.Stock);

            var bestSellers = await _databaseService.GetBestSellersAsync(5);
            ReportBestSellers.Clear();
            foreach (var item in bestSellers) ReportBestSellers.Add(item);

            var deadStock = await _databaseService.GetDeadStockAsync(10);
            ReportDeadStock.Clear();
            foreach (var item in deadStock) ReportDeadStock.Add(item);
        }

        private void ApplyPeriodFilter()
        {
            var today = DateTime.Today;
            int currentYear = today.Year > 2400 ? today.Year - 543 : today.Year;
            switch (SelectedReportPeriod)
            {
                case "วันนี้":
                    ReportStartDate = today;
                    ReportEndDate = today;
                    break;
                case "7 วันล่าสุด":
                    ReportStartDate = today.AddDays(-6);
                    ReportEndDate = today;
                    break;
                case "เดือนนี้":
                    ReportStartDate = new DateTime(currentYear, today.Month, 1);
                    ReportEndDate = today;
                    break;
                case "ปีนี้":
                    ReportStartDate = new DateTime(currentYear, 1, 1, 0, 0, 0);
                    ReportEndDate = new DateTime(currentYear, 12, 31, 23, 59, 59);
                    break;
                case "ทั้งหมด":
                default:
                    ReportStartDate = DateTime.MinValue;
                    ReportEndDate = DateTime.Today;
                    break;
            }
        }

        private async Task RenderReportChartsAsync(string reportType, List<ReportRow> rows, bool isAllPeriod, DateTime start, DateTime end)
        {
            var newCartesianSeries = new SeriesCollection();
            var newPieSeries = new SeriesCollection();
            var newLabels = new ObservableCollection<string>();
            var newSummary = new ObservableCollection<ReportSummaryItem>();
            Func<double, string> newFormatter = val => val.ToString("N0");
            int displayMode = 0; // 0: Cartesian (Column/Line), 1: Pie/Doughnut
            string chartTitle = "กราฟสรุปข้อมูล";
            string summaryTitle = "🏆 สรุปรายการสำคัญ";
            string totalCountText = "รวมทั้งหมด 0 รายการ";

            int startYr = start.Year > 2400 ? start.Year - 543 : start.Year;
            int endYr = end.Year > 2400 ? end.Year - 543 : end.Year;
            string startStr = $"{startYr:D4}-{start.Month:D2}-{start.Day:D2} 00:00:00";
            string endStr = $"{endYr:D4}-{end.Month:D2}-{end.Day:D2} 23:59:59";
            string startStrBE = $"{startYr + 543:D4}-{start.Month:D2}-{start.Day:D2} 00:00:00";
            string endStrBE = $"{endYr + 543:D4}-{end.Month:D2}-{end.Day:D2} 23:59:59";
            var chartParams = new { startStr, endStr, startStrBE, endStrBE };

            try
            {
                switch (reportType)
                {
                    case "รายงานข้อมูลลูกค้า":
                        chartTitle = "📊 ลูกค้าที่มียอดซื้อสะสมสูงสุด (Top Customer Spenders)";
                        displayMode = 0;
                        summaryTitle = "👥 สรุปรายชื่อลูกค้ายอดซื้อสูงสุด";
                        newFormatter = val => val.ToString("N0");

                        var topCustomers = await _databaseService.GetTopCustomerSpendersReportAsync(isAllPeriod, chartParams);
                        if (topCustomers.Any())
                        {
                            var values = new ChartValues<double>();
                            int rank = 1;
                            foreach (var item in topCustomers)
                            {
                                newLabels.Add(item.Name);
                                values.Add((double)item.Total);
                                newSummary.Add(new ReportSummaryItem
                                {
                                    Rank = rank++,
                                    Title = item.Name,
                                    Subtitle = $"ซื้อ {item.Count} บิล | โทร: {item.Tel}",
                                    ValueText = $"{item.Total:N2}",
                                    Tag = "ลูกค้ายอดซื้อสูงสุด",
                                    Color = "#0284C7"
                                });
                            }

                            newCartesianSeries.Add(new ColumnSeries
                            {
                                Title = "ยอดซื้อสะสม (บาท)",
                                Values = values,
                                Fill = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#0284C7"),
                                MaxColumnWidth = 36
                            });
                        }
                        totalCountText = $"ลูกค้ายอดซื้อสูงสุด {newSummary.Count} ราย";
                        break;

                    case "รายงานข้อมูลพนักงาน":
                        chartTitle = "📊 ยอดขายต่อคน เทียบผลงานบุคคล (Staff Performance)";
                        displayMode = 0;
                        summaryTitle = "🧑‍💼 สรุปพนักงานยอดขายสูงสุด";
                        newFormatter = val => val.ToString("N0");

                        var staffSales = await _databaseService.GetStaffPerformanceReportAsync(isAllPeriod, chartParams);
                        if (staffSales.Any())
                        {
                            var values = new ChartValues<double>();
                            int rank = 1;
                            foreach (var item in staffSales)
                            {
                                newLabels.Add(item.Username);
                                values.Add((double)item.Total);
                                newSummary.Add(new ReportSummaryItem
                                {
                                    Rank = rank++,
                                    Title = item.Username,
                                    Subtitle = $"{item.Role} | {item.Count} บิลที่ขายได้",
                                    ValueText = $"{item.Total:N2}",
                                    Tag = "พนักงานขายยอดเยี่ยม",
                                    Color = "#10B981"
                                });
                            }

                            newCartesianSeries.Add(new ColumnSeries
                            {
                                Title = "ยอดขายรวม (บาท)",
                                Values = values,
                                Fill = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#10B981"),
                                MaxColumnWidth = 36
                            });
                        }
                        totalCountText = $"จำนวนพนักงานทั้งหมด {newSummary.Count} คน";
                        break;

                    case "รายงานข้อมูลสินค้า":
                        chartTitle = "📊 สินค้าขายดี 5 อันดับแรก (Top 5 Best Sellers)";
                        displayMode = 0;
                        summaryTitle = "📦 สรุปสินค้าขายดีที่สุด";

                        var bestSelling = await _databaseService.GetBestSellingProductsReportAsync(isAllPeriod, chartParams);
                        if (bestSelling.Any())
                        {
                            var values = new ChartValues<double>();
                            int rank = 1;
                            foreach (var item in bestSelling)
                            {
                                newLabels.Add(item.Name);
                                values.Add(item.Qty);
                                newSummary.Add(new ReportSummaryItem
                                {
                                    Rank = rank++,
                                    Title = item.Name,
                                    Subtitle = $"ยอดขายรวม {item.Total:N2}",
                                    ValueText = $"{item.Qty} ชิ้น",
                                    Tag = $"อันดับที่ {rank - 1}",
                                    Color = "#0EA5E9"
                                });
                            }

                            newCartesianSeries.Add(new ColumnSeries
                            {
                                Title = "จำนวนชิ้นที่ขายได้ (ชิ้น)",
                                Values = values,
                                Fill = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#0EA5E9"),
                                MaxColumnWidth = 36
                            });
                        }
                        totalCountText = $"แสดงสินค้าขายดี {newSummary.Count} อันดับแรก";
                        break;

                    case "รายงานข้อมูลบริษัทคู่ค้า":
                        chartTitle = "📊 มูลค่าการสั่งซื้อต่อคู่ค้า (Supplier Order Volume)";
                        displayMode = 0;
                        summaryTitle = "🏢 สรุปคู่ค้ามูลค่าสูงสุด";
                        newFormatter = val => val.ToString("N0");

                        var supplierPOs = await _databaseService.GetSupplierOrderVolumeReportAsync(isAllPeriod, chartParams);
                        if (supplierPOs.Any())
                        {
                            var values = new ChartValues<double>();
                            int rank = 1;
                            foreach (var item in supplierPOs)
                            {
                                newLabels.Add(item.Name);
                                values.Add((double)item.Total);
                                newSummary.Add(new ReportSummaryItem
                                {
                                    Rank = rank++,
                                    Title = item.Name,
                                    Subtitle = $"สั่งซื้อ {item.Count} ใบสั่งซื้อ",
                                    ValueText = $"{item.Total:N2}",
                                    Tag = "คู่ค้าสำคัญ",
                                    Color = "#0284C7"
                                });
                            }

                            newCartesianSeries.Add(new ColumnSeries
                            {
                                Title = "มูลค่าการสั่งซื้อสะสม (บาท)",
                                Values = values,
                                Fill = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#0284C7"),
                                MaxColumnWidth = 36
                            });
                        }
                        totalCountText = $"จำนวนบริษัทคู่ค้า {newSummary.Count} บริษัท";
                        break;

                    case "รายงานข้อมูลประเภทสินค้า":
                        chartTitle = "🍩 สัดส่วนยอดขายตามประเภทสินค้า (Category Share)";
                        displayMode = 1;
                        summaryTitle = "🏷️ สรุปสัดส่วนตามหมวดหมู่ (%)";

                        var catSales = await _databaseService.GetCategorySalesDistributionReportAsync(isAllPeriod, chartParams);
                        if (catSales.Any())
                        {
                            decimal grandTotal = catSales.Sum(c => c.Total);
                            string[] colors = { "#0284C7", "#38BDF8", "#10B981", "#F59E0B", "#8B5CF6", "#EC4899", "#64748B" };
                            int rank = 1;
                            int cIdx = 0;
                            foreach (var item in catSales)
                            {
                                string c = colors[cIdx % colors.Length];
                                cIdx++;
                                double pct = grandTotal > 0 ? (double)(item.Total / grandTotal * 100) : 0;

                                newPieSeries.Add(new PieSeries
                                {
                                    Title = item.Category,
                                    Values = new ChartValues<double> { (double)(item.Total > 0 ? item.Total : 1) },
                                    DataLabels = true,
                                    Fill = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(c)
                                });

                                newSummary.Add(new ReportSummaryItem
                                {
                                    Rank = rank++,
                                    Title = item.Category,
                                    Subtitle = $"สัดส่วน {pct:0.1f}% ของยอดรวม",
                                    ValueText = $"{item.Total:N2}",
                                    Tag = "หมวดหมู่",
                                    Color = c
                                });
                            }
                        }
                        totalCountText = $"จำนวนหมวดหมู่สินค้า {newSummary.Count} ประเภท";
                        break;

                    case "รายงานข้อมูลการสั่งซื้อสินค้า":
                        chartTitle = "🍩 สถานะใบสั่งซื้อสินค้า (PO Status Breakdown)";
                        displayMode = 1;
                        summaryTitle = "📋 สรุปแยกตามสถานะใบสั่งซื้อ";

                        var poStatus = await _databaseService.GetPurchaseOrderStatusReportAsync(isAllPeriod, chartParams);
                        if (poStatus.Any())
                        {
                            int rank = 1;
                            foreach (var item in poStatus)
                            {
                                string color = item.Status == "ได้รับสินค้าแล้ว" ? "#10B981" : (item.Status == "ยกเลิก" ? "#EF4444" : "#F59E0B");
                                newPieSeries.Add(new PieSeries
                                {
                                    Title = item.Status,
                                    Values = new ChartValues<double> { item.Count },
                                    DataLabels = true,
                                    Fill = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(color)
                                });

                                newSummary.Add(new ReportSummaryItem
                                {
                                    Rank = rank++,
                                    Title = item.Status,
                                    Subtitle = $"มูลค่ารวม {item.Total:N2}",
                                    ValueText = $"{item.Count} ใบ",
                                    Tag = item.Status,
                                    Color = color
                                });
                            }
                        }
                        totalCountText = $"จำนวนสถานะใบสั่งซื้อทั้งหมด {newSummary.Count} กลุ่ม";
                        break;

                    case "รายงานข้อมูลการรับเข้าสินค้า":
                        chartTitle = "📈 ปริมาณรับเข้าสินค้ารายวัน (Daily Stock Inflow)";
                        displayMode = 0;
                        summaryTitle = "📥 สรุปวันที่รับเข้าสินค้าสูงสุด";

                        var stockIns = await _databaseService.GetStockInDailyReportAsync(isAllPeriod, chartParams);
                        if (stockIns.Any())
                        {
                            var chartList = stockIns.OrderBy(x => x.Date).ToList();
                            var values = new ChartValues<double>();
                            foreach (var item in chartList)
                            {
                                newLabels.Add(item.Date);
                                values.Add(item.Qty);
                            }

                            newCartesianSeries.Add(new LineSeries
                            {
                                Title = "จำนวนสินค้าที่รับเข้า (ชิ้น)",
                                Values = values,
                                PointGeometrySize = 10,
                                Stroke = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#0284C7"),
                                Fill = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#200284C7")
                            });

                            var summaryList = stockIns.OrderByDescending(x => x.Qty).ToList();
                            int rank = 1;
                            foreach (var item in summaryList)
                            {
                                newSummary.Add(new ReportSummaryItem
                                {
                                    Rank = rank++,
                                    Title = $"วันที่ {item.Date}",
                                    Subtitle = "ปริมาณรับเข้าสต็อกสินค้า",
                                    ValueText = $"{item.Qty} ชิ้น",
                                    Tag = "รับเข้าสูงสุด",
                                    Color = "#0284C7"
                                });
                            }
                        }
                        totalCountText = $"บันทึกการรับเข้า {newSummary.Count} วันทำการ";
                        break;

                    case "รายงานข้อมูลการขายหน้าร้าน":
                    case "รายงานข้อมูลขายหน้าร้าน":
                        chartTitle = "📈 แนวโน้มยอดขายหน้าร้าน (Sales Trend)";
                        displayMode = 0;
                        summaryTitle = "💳 สรุปวันที่ขายดีที่สุด";
                        newFormatter = val => val.ToString("N0");

                        var dailySales = await _databaseService.GetDailySalesTrendReportAsync(isAllPeriod, chartParams);
                        if (dailySales.Any())
                        {
                            var chartList = dailySales.OrderBy(x => x.Date).ToList();
                            var values = new ChartValues<double>();
                            foreach (var item in chartList)
                            {
                                newLabels.Add(item.Date);
                                values.Add((double)item.Total);
                            }

                            newCartesianSeries.Add(new LineSeries
                            {
                                Title = "ยอดขายรายวัน (บาท)",
                                Values = values,
                                PointGeometrySize = 10,
                                Stroke = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#0284C7"),
                                Fill = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#200284C7")
                            });

                            var summaryList = dailySales.OrderByDescending(x => x.Total).ToList();
                            int rank = 1;
                            foreach (var item in summaryList)
                            {
                                newSummary.Add(new ReportSummaryItem
                                {
                                    Rank = rank++,
                                    Title = $"วันที่ {item.Date}",
                                    Subtitle = $"จำนวน {item.Count} ธุรกรรมบิล",
                                    ValueText = $"{item.Total:N2}",
                                    Tag = "ยอดขายดีเด่น",
                                    Color = "#0284C7"
                                });
                            }
                        }
                        totalCountText = $"สรุปยอดขาย {newSummary.Count} วันทำการ";
                        break;

                    case "รายงานข้อมูลการเคลม":
                        chartTitle = "📊 จำนวนเคลมตามสาเหตุ (Product Claims by Reason)";
                        displayMode = 0;
                        summaryTitle = "⚠️ สรุปสาเหตุที่พบบ่อย";

                        var claimReasons = await _databaseService.GetClaimReasonDistributionReportAsync(isAllPeriod, chartParams);
                        if (claimReasons.Any())
                        {
                            var values = new ChartValues<double>();
                            int rank = 1;
                            foreach (var item in claimReasons)
                            {
                                newLabels.Add(item.Reason);
                                values.Add(item.Count);
                                newSummary.Add(new ReportSummaryItem
                                {
                                    Rank = rank++,
                                    Title = item.Reason,
                                    Subtitle = "สาเหตุการเคลมสินค้า",
                                    ValueText = $"{item.Count} รายการ",
                                    Tag = "สาเหตุเคลม",
                                    Color = "#EF4444"
                                });
                            }

                            newCartesianSeries.Add(new ColumnSeries
                            {
                                Title = "จำนวนการเคลม (รายการ)",
                                Values = values,
                                Fill = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#EF4444"),
                                MaxColumnWidth = 36
                            });
                        }
                        totalCountText = $"สาเหตุการเคลมทั้งหมด {newSummary.Count} ประเภท";
                        break;

                    case "รายงานข้อมูลการจัดส่งสินค้า":
                        chartTitle = "📈 การจัดส่งสำเร็จรายวัน (Daily Successful Deliveries)";
                        displayMode = 0;
                        summaryTitle = "🚚 สรุปวันที่จัดส่งมากที่สุด";

                        var deliveryDays = await _databaseService.GetDailyDeliveryReportAsync(isAllPeriod, chartParams);
                        if (deliveryDays.Any())
                        {
                            var chartList = deliveryDays.OrderBy(x => x.Date).ToList();
                            var values = new ChartValues<double>();
                            foreach (var item in chartList)
                            {
                                newLabels.Add(item.Date);
                                values.Add(item.Count);
                            }

                            newCartesianSeries.Add(new LineSeries
                            {
                                Title = "จำนวนจัดส่งสำเร็จ (รายการ)",
                                Values = values,
                                PointGeometrySize = 10,
                                Stroke = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#0284C7"),
                                Fill = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#200284C7")
                            });

                            var summaryList = deliveryDays.OrderByDescending(x => x.Count).ToList();
                            int rank = 1;
                            foreach (var item in summaryList)
                            {
                                newSummary.Add(new ReportSummaryItem
                                {
                                    Rank = rank++,
                                    Title = $"วันที่ {item.Date}",
                                    Subtitle = "จัดส่งพัสดุสำเร็จ",
                                    ValueText = $"{item.Count} รายการ",
                                    Tag = "จัดส่งสูงสุด",
                                    Color = "#10B981"
                                });
                            }
                        }
                        totalCountText = $"สถิติการจัดส่ง {newSummary.Count} วันทำการ";
                        break;

                    case "รายงานข้อมูลรายรับ-รายจ่าย":
                        chartTitle = "📊 รายรับ vs รายจ่ายรายเดือน (Monthly Income vs Expense)";
                        displayMode = 0;
                        summaryTitle = "💰 สรุปกำไรสุทธิรายเดือน";
                        newFormatter = val => val.ToString("N0");

                        var (revRaw, expRaw) = await _databaseService.GetMonthlyIncomeExpenseReportAsync(isAllPeriod, chartParams);

                        string NormalizeMonth(string m)
                        {
                            if (string.IsNullOrEmpty(m) || m.Length < 7) return m;
                            if (int.TryParse(m.Substring(0, 4), out int y) && y > 2400)
                            {
                                return $"{(y - 543):D4}{m.Substring(4)}";
                            }
                            return m;
                        }

                        var revList = revRaw
                            .GroupBy(x => NormalizeMonth(x.Month))
                            .ToDictionary(g => g.Key, g => g.Sum(x => x.Total));

                        var expList = expRaw
                            .GroupBy(x => NormalizeMonth(x.Month))
                            .ToDictionary(g => g.Key, g => g.Sum(x => x.Total));

                        var allMonths = revList.Keys.Union(expList.Keys).OrderBy(m => m).ToList();
                        if (!allMonths.Any())
                        {
                            allMonths.Add(DateTime.Today.ToString("yyyy-MM"));
                        }

                        var revValues = new ChartValues<double>();
                        var expValues = new ChartValues<double>();

                        foreach (var m in allMonths)
                        {
                            newLabels.Add(m);
                            decimal r = revList.ContainsKey(m) ? revList[m] : 0;
                            decimal e = expList.ContainsKey(m) ? expList[m] : 0;
                            revValues.Add((double)r);
                            expValues.Add((double)e);
                        }

                        newCartesianSeries.Add(new ColumnSeries
                        {
                            Title = "รายรับ (บาท)",
                            Values = revValues,
                            Fill = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#10B981"),
                            MaxColumnWidth = 32
                        });
                        newCartesianSeries.Add(new ColumnSeries
                        {
                            Title = "รายจ่าย (บาท)",
                            Values = expValues,
                            Fill = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#EF4444"),
                            MaxColumnWidth = 32
                        });

                        var descMonths = allMonths.OrderByDescending(m => m).ToList();
                        int pRank = 1;
                        decimal totalNetAll = 0;
                        foreach (var m in descMonths)
                        {
                            decimal r = revList.ContainsKey(m) ? revList[m] : 0;
                            decimal e = expList.ContainsKey(m) ? expList[m] : 0;
                            decimal net = r - e;
                            totalNetAll += net;

                            newSummary.Add(new ReportSummaryItem
                            {
                                Rank = pRank++,
                                Title = $"เดือน {m}",
                                Subtitle = $"รายรับ: {r:N2} | รายจ่าย: {e:N2}",
                                ValueText = $"{net:N2}",
                                Tag = net >= 0 ? "กำไรสุทธิ" : "ขาดทุนสุทธิ",
                                Color = net >= 0 ? "#10B981" : "#EF4444"
                            });
                        }

                        totalCountText = $"กำไรสุทธิรวม: {totalNetAll:N2}";
                        break;
                }
            }
            catch { }

            // Atomic assignment on UI Dispatcher to prevent background thread / updater race condition
            System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
            {
                ReportCartesianSeries = newCartesianSeries;
                ReportPieSeries = newPieSeries;
                ReportChartSeries = displayMode == 1 ? newPieSeries : newCartesianSeries;
                ReportChartLabels = newLabels;
                ReportSummaryList = newSummary;
                ReportChartFormatter = newFormatter;
                ReportChartDisplayMode = displayMode;
                ReportChartTitle = chartTitle;
                ReportSummaryHeaderTitle = summaryTitle;
                ReportSummaryTotalCountText = totalCountText;
            });
        }

        private async Task GenerateReport()
        {
            IsGeneratingReport = true;
            ReportRows.Clear();

            bool isAllPeriod = SelectedReportPeriod == "ทั้งหมด";

            DateTime start = ReportStartDate != default ? ReportStartDate.Date : DateTime.Today;
            DateTime end;

            // Prevent Exception from AddDays(1) on DateTime.MaxValue
            if (ReportEndDate.Date >= DateTime.MaxValue.Date.AddDays(-1))
            {
                end = DateTime.MaxValue;
            }
            else
            {
                end = ReportEndDate.Date.AddDays(1).AddTicks(-1);
            }

            int startYr = start.Year > 2400 ? start.Year - 543 : start.Year;
            int endYr = end.Year > 2400 ? end.Year - 543 : end.Year;
            DateTime startCE = new DateTime(startYr, start.Month, start.Day, start.Hour, start.Minute, start.Second);
            DateTime endCE = end == DateTime.MaxValue ? DateTime.MaxValue : new DateTime(endYr, end.Month, end.Day, end.Hour, end.Minute, end.Second);

            string sCe = startCE.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            string eCe = endCE == DateTime.MaxValue ? "9999-12-31 23:59:59" : endCE.ToString("yyyy-MM-dd 23:59:59", System.Globalization.CultureInfo.InvariantCulture);

            string sBe = (startYr + 543).ToString("D4") + startCE.ToString("-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            string eBe = endCE == DateTime.MaxValue ? "9999-12-31 23:59:59" : (endYr + 543).ToString("D4") + endCE.ToString("-MM-dd 23:59:59", System.Globalization.CultureInfo.InvariantCulture);

            var dateParams = new { S = sCe, E = eCe, S_BE = sBe, E_BE = eBe };

            try
            {
                var (rows, totalCount, totalAmount, summaryText) = await _databaseService.GetReportDataAsync(SelectedReportType, isAllPeriod, dateParams, startCE, endCE);

                foreach (var row in rows)
                    ReportRows.Add(row);

                ReportSummaryText = summaryText;

                ReportTotalRevenue = await _databaseService.GetReportTotalRevenueAsync(isAllPeriod, dateParams);
                ReportTotalExpenses = await _databaseService.GetReportTotalExpensesAsync(isAllPeriod, dateParams);
                ReportNetProfit = ReportTotalRevenue - ReportTotalExpenses;

                await RenderReportChartsAsync(SelectedReportType, ReportRows.ToList(), isAllPeriod, startCE, endCE);

                if (!rows.Any())
                    ReportSummaryText = "ไม่พบข้อมูลในช่วงเวลาที่เลือก";
            }
            catch (Exception ex)
            {
                ReportSummaryText = $"เกิดข้อผิดพลาด: {ex.Message}";
            }
            finally
            {
                IsGeneratingReport = false;
                OnPropertyChanged(nameof(HasReportData));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        #region Excel Export Implementation (ClosedXML)

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "รายงาน";
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(name.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
            return sanitized.Trim();
        }

        private async Task ExecuteExportExcel()
        {
            if (!HasReportData)
            {
                ShowAlert("ไม่พบข้อมูลรายงานที่จะส่งออก กรุณากดปุ่ม 'ประมวลผล' เพื่อสร้างรายงานก่อนดำเนินการ", "แจ้งเตือน", "⚠️");
                return;
            }

            string safeType = SanitizeFileName(SelectedReportType);
            string safePeriod = SanitizeFileName(SelectedReportPeriod);
            string defaultFileName = $"รายงาน_{safeType}_{safePeriod}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = defaultFileName,
                Title = "บันทึกรายงาน Excel"
            };

            if (saveFileDialog.ShowDialog() != true)
            {
                return;
            }

            string filePath = saveFileDialog.FileName;

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    string sheetName = SelectedReportType.Length > 25 ? SelectedReportType.Substring(0, 25) : SelectedReportType;
                    sheetName = System.Text.RegularExpressions.Regex.Replace(sheetName, @"[:\\/?*\[\]]", "_");
                    if (string.IsNullOrWhiteSpace(sheetName)) sheetName = "รายงานสรุปผล";

                    var ws = workbook.Worksheets.Add(sheetName);
                    ws.Style.Font.FontName = "Segoe UI";

                    // 1. ส่วนหัวเอกสาร (Header Section)
                    // แถวที่ 1: ชื่อร้าน "ร้านพอใจ ทุกอย่าง 20 บาท" (ตัวหนา, ขนาด 16pt)
                    ws.Cell(1, 1).Value = "ร้านพอใจ ทุกอย่าง 20 บาท";
                    ws.Cell(1, 1).Style.Font.FontSize = 16;
                    ws.Cell(1, 1).Style.Font.Bold = true;
                    ws.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml("#0F172A");

                    // แถวที่ 2: ชื่อประเภทรายงาน (ตัวหนา, ขนาด 13pt)
                    ws.Cell(2, 1).Value = SelectedReportType;
                    ws.Cell(2, 1).Style.Font.FontSize = 13;
                    ws.Cell(2, 1).Style.Font.Bold = true;
                    ws.Cell(2, 1).Style.Font.FontColor = XLColor.FromHtml("#0284C7");

                    // แถวที่ 3: ช่วงเวลาที่เลือก และวันที่พิมพ์ออกรายงาน
                    string periodInfo = SelectedReportPeriod == "ทั้งหมด"
                        ? "ช่วงเวลา: ทั้งหมด"
                        : $"ช่วงเวลา: {SelectedReportPeriod} ({ReportStartDate:dd/MM/yyyy} - {ReportEndDate:dd/MM/yyyy})";
                    ws.Cell(3, 1).Value = $"{periodInfo}  |  วันที่พิมพ์ออกรายงาน: {DateTime.Now:dd/MM/yyyy HH:mm:ss} น.";
                    ws.Cell(3, 1).Style.Font.FontSize = 10.5;
                    ws.Cell(3, 1).Style.Font.FontColor = XLColor.FromHtml("#64748B");

                    // แถวที่ 4: สรุปยอดรวม 3 ช่อง (ยอดขายรวม | ต้นทุน/รายจ่ายรวม | กำไรสุทธิ)
                    ws.Range(4, 1, 4, 2).Merge();
                    ws.Cell(4, 1).Value = $"ยอดขายรวม: {ReportTotalRevenue:N2}";
                    ws.Cell(4, 1).Style.Font.Bold = true;
                    ws.Cell(4, 1).Style.Font.FontSize = 11;
                    ws.Cell(4, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E0F2FE");
                    ws.Cell(4, 1).Style.Font.FontColor = XLColor.FromHtml("#0369A1");
                    ws.Cell(4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(4, 1, 4, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(4, 1, 4, 2).Style.Border.OutsideBorderColor = XLColor.FromHtml("#BAE6FD");

                    ws.Range(4, 3, 4, 4).Merge();
                    ws.Cell(4, 3).Value = $"ต้นทุน/รายจ่ายรวม: {ReportTotalExpenses:N2}";
                    ws.Cell(4, 3).Style.Font.Bold = true;
                    ws.Cell(4, 3).Style.Font.FontSize = 11;
                    ws.Cell(4, 3).Style.Fill.BackgroundColor = XLColor.FromHtml("#FEF3C7");
                    ws.Cell(4, 3).Style.Font.FontColor = XLColor.FromHtml("#B45309");
                    ws.Cell(4, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(4, 3, 4, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(4, 3, 4, 4).Style.Border.OutsideBorderColor = XLColor.FromHtml("#FDE68A");

                    ws.Range(4, 5, 4, 6).Merge();
                    ws.Cell(4, 5).Value = $"กำไรสุทธิ: {ReportNetProfit:N2}";
                    ws.Cell(4, 5).Style.Font.Bold = true;
                    ws.Cell(4, 5).Style.Font.FontSize = 11;
                    ws.Cell(4, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#DCFCE7");
                    ws.Cell(4, 5).Style.Font.FontColor = XLColor.FromHtml("#15803D");
                    ws.Cell(4, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range(4, 5, 4, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(4, 5, 4, 6).Style.Border.OutsideBorderColor = XLColor.FromHtml("#86EFAC");

                    // แถวที่ 5: เว้นบรรทัดว่าง

                    // 2. ส่วนตารางข้อมูล (Data Table)
                    int headerRow = 6;
                    ws.Row(headerRow).Height = 26;

                    await PopulateExcelReportData(ws, headerRow);

                    // จัดความกว้างคอลัมน์อัตโนมัติ พร้อมเผื่อระยะขอบ
                    ws.Columns().AdjustToContents();
                    foreach (var col in ws.Columns())
                    {
                        col.Width = Math.Max(col.Width + 4, 12);
                    }

                    workbook.SaveAs(filePath);
                }

                ShowConfirm($"บันทึกไฟล์รายงาน Excel เรียบร้อยแล้ว\nที่อยู่ไฟล์: {filePath}\n\nคุณต้องการเปิดไฟล์ขึ้นมาดูทันทีหรือไม่?", () =>
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        ShowAlert($"ไม่สามารถเปิดไฟล์ได้: {ex.Message}", "ข้อผิดพลาด", "⚠️");
                    }
                }, "ส่งออกข้อมูลสำเร็จ");
            }
            catch (IOException)
            {
                ShowAlert("ไม่สามารถบันทึกไฟล์ได้ เนื่องจากไฟล์นี้กำลังถูกเปิดใช้งานอยู่ในโปรแกรมอื่น (เช่น Microsoft Excel)\n\nกรุณาปิดไฟล์ดังกล่าวแล้วลองกดบันทึกใหม่อีกครั้ง", "ไม่สามารถบันทึกไฟล์ได้", "⚠️");
            }
            catch (Exception ex)
            {
                ShowAlert($"เกิดข้อผิดพลาดในการส่งออกไฟล์ Excel: {ex.Message}", "ข้อผิดพลาด", "❌");
            }
        }

        private async Task PopulateExcelReportData(IXLWorksheet ws, int headerRow)
        {
            bool isAllPeriod = SelectedReportPeriod == "ทั้งหมด";
            DateTime start = ReportStartDate != default ? ReportStartDate.Date : DateTime.Today;
            DateTime end = (ReportEndDate.Date >= DateTime.MaxValue.Date.AddDays(-1)) ? DateTime.MaxValue : ReportEndDate.Date.AddDays(1).AddTicks(-1);

            int startYr = start.Year > 2400 ? start.Year - 543 : start.Year;
            int endYr = end.Year > 2400 ? end.Year - 543 : end.Year;
            DateTime startCE = new DateTime(startYr, start.Month, start.Day, start.Hour, start.Minute, start.Second);
            DateTime endCE = end == DateTime.MaxValue ? DateTime.MaxValue : new DateTime(endYr, end.Month, end.Day, end.Hour, end.Minute, end.Second);

            string sCe = startCE.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            string eCe = endCE == DateTime.MaxValue ? "9999-12-31 23:59:59" : endCE.ToString("yyyy-MM-dd 23:59:59", System.Globalization.CultureInfo.InvariantCulture);
            string sBe = (startYr + 543).ToString("D4") + startCE.ToString("-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            string eBe = endCE == DateTime.MaxValue ? "9999-12-31 23:59:59" : (endYr + 543).ToString("D4") + endCE.ToString("-MM-dd 23:59:59", System.Globalization.CultureInfo.InvariantCulture);
            var dateParams = new { S = sCe, E = eCe, S_BE = sBe, E_BE = eBe };

            using (var conn = _databaseService.GetConnection())
            {
                switch (SelectedReportType)
                {
                    case "รายงานข้อมูลลูกค้า":
                    {
                        var headers = new[] { "ลำดับ", "ชื่อลูกค้า", "จำนวนบิลที่ซื้อ", "เบอร์โทรศัพท์", "ที่อยู่", "ยอดซื้อสะสม (บาท)" };
                        ApplyTableHeader(ws, headerRow, headers);

                        string sql = isAllPeriod
                            ? @"SELECT COALESCE(c.Cus_Name, 'ลูกค้าทั่วไป') as Name, 
                                       COUNT(s.Sales_ID) as BillCount, 
                                       COALESCE(c.Cus_Tel, '-') as Tel, 
                                       COALESCE(c.Cus_Address, '-') as Address, 
                                       COALESCE(SUM(s.Sales_Total), 0) as Total
                                FROM tblCustomer c
                                LEFT JOIN tblSales_H s ON c.Cus_ID = s.Cus_ID
                                GROUP BY c.Cus_ID
                                ORDER BY Total DESC, BillCount DESC"
                            : @"SELECT COALESCE(c.Cus_Name, 'ลูกค้าทั่วไป') as Name, 
                                       COUNT(s.Sales_ID) as BillCount, 
                                       COALESCE(c.Cus_Tel, '-') as Tel, 
                                       COALESCE(c.Cus_Address, '-') as Address, 
                                       COALESCE(SUM(s.Sales_Total), 0) as Total
                                FROM tblCustomer c
                                LEFT JOIN tblSales_H s ON c.Cus_ID = s.Cus_ID AND ((s.Sales_Date >= @S AND s.Sales_Date <= @E) OR (s.Sales_Date >= @S_BE AND s.Sales_Date <= @E_BE))
                                GROUP BY c.Cus_ID
                                ORDER BY Total DESC, BillCount DESC";

                        var data = (await conn.QueryAsync(sql, dateParams)).ToList();
                        int rowIdx = headerRow + 1;
                        int seq = 1;
                        decimal sumTotal = 0;
                        long sumBills = 0;

                        foreach (var item in data)
                        {
                            ws.Row(rowIdx).Height = 20;
                            string name = item.Name ?? "-";
                            long bills = Convert.ToInt64(item.BillCount ?? 0);
                            string tel = item.Tel ?? "-";
                            string addr = item.Address ?? "-";
                            decimal total = Convert.ToDecimal(item.Total ?? 0);

                            ws.Cell(rowIdx, 1).Value = seq;
                            ws.Cell(rowIdx, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 2).Value = name;

                            ws.Cell(rowIdx, 3).Value = bills;
                            ws.Cell(rowIdx, 3).Style.NumberFormat.Format = "#,##0";
                            ws.Cell(rowIdx, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            ws.Cell(rowIdx, 4).Value = tel;
                            ws.Cell(rowIdx, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 5).Value = addr;

                            ws.Cell(rowIdx, 6).Value = total;
                            ws.Cell(rowIdx, 6).Style.NumberFormat.Format = "#,##0.00";
                            ws.Cell(rowIdx, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            ApplyDataRowStyle(ws, rowIdx, 6, seq % 2 == 0);
                            sumTotal += total;
                            sumBills += bills;
                            rowIdx++;
                            seq++;
                        }

                        ApplyTableSummaryRow(ws, rowIdx, headers.Length, "รวมทั้งสิ้น", new Dictionary<int, (object val, string fmt)>
                        {
                            { 3, (sumBills, "#,##0") },
                            { 6, (sumTotal, "#,##0.00") }
                        });
                        break;
                    }

                    case "รายงานข้อมูลสินค้า":
                    {
                        var headers = new[] { "ลำดับ", "รหัสสินค้า", "ชื่อสินค้า", "หมวดหมู่", "ราคาขาย (บาท)", "ราคาทุน (บาท)", "คงเหลือในสต็อก", "จำนวนที่ขายได้ (ชิ้น)", "ยอดขายรวม (บาท)" };
                        ApplyTableHeader(ws, headerRow, headers);

                        string sql = isAllPeriod
                            ? @"SELECT COALESCE(p.Pro_Barcode, 'P-' || p.Pro_ID) as Code,
                                       p.Pro_Name as Name,
                                       COALESCE(p.Pro_Category, '-') as Category,
                                       p.Pro_Price as Price,
                                       p.Pro_Cost as Cost,
                                       p.Pro_Qty as Stock,
                                       COALESCE(SUM(d.Sales_Qty), 0) as SoldQty,
                                       COALESCE(SUM(d.Sales_Subtotal), 0) as SoldTotal
                                FROM tblProduct p
                                LEFT JOIN tblSalesDetail d ON p.Pro_ID = d.Pro_ID
                                LEFT JOIN tblSales_H s ON d.Sales_ID = s.Sales_ID
                                GROUP BY p.Pro_ID
                                ORDER BY SoldQty DESC, p.Pro_Qty ASC"
                            : @"SELECT COALESCE(p.Pro_Barcode, 'P-' || p.Pro_ID) as Code,
                                       p.Pro_Name as Name,
                                       COALESCE(p.Pro_Category, '-') as Category,
                                       p.Pro_Price as Price,
                                       p.Pro_Cost as Cost,
                                       p.Pro_Qty as Stock,
                                       COALESCE(SUM(d.Sales_Qty), 0) as SoldQty,
                                       COALESCE(SUM(d.Sales_Subtotal), 0) as SoldTotal
                                FROM tblProduct p
                                LEFT JOIN tblSalesDetail d ON p.Pro_ID = d.Pro_ID
                                LEFT JOIN tblSales_H s ON d.Sales_ID = s.Sales_ID AND ((s.Sales_Date >= @S AND s.Sales_Date <= @E) OR (s.Sales_Date >= @S_BE AND s.Sales_Date <= @E_BE))
                                GROUP BY p.Pro_ID
                                ORDER BY SoldQty DESC, p.Pro_Qty ASC";

                        var data = (await conn.QueryAsync(sql, dateParams)).ToList();
                        int rowIdx = headerRow + 1;
                        int seq = 1;
                        long totalStock = 0;
                        long totalSoldQty = 0;
                        decimal totalSoldAmt = 0;

                        foreach (var item in data)
                        {
                            ws.Row(rowIdx).Height = 20;
                            string code = item.Code ?? "-";
                            string name = item.Name ?? "-";
                            string cat = item.Category ?? "-";
                            decimal price = Convert.ToDecimal(item.Price ?? 0);
                            decimal cost = Convert.ToDecimal(item.Cost ?? 0);
                            int stock = Convert.ToInt32(item.Stock ?? 0);
                            int soldQty = Convert.ToInt32(item.SoldQty ?? 0);
                            decimal soldTotal = Convert.ToDecimal(item.SoldTotal ?? 0);

                            ws.Cell(rowIdx, 1).Value = seq;
                            ws.Cell(rowIdx, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 2).Value = code;
                            ws.Cell(rowIdx, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 3).Value = name;
                            ws.Cell(rowIdx, 4).Value = cat;

                            ws.Cell(rowIdx, 5).Value = price;
                            ws.Cell(rowIdx, 5).Style.NumberFormat.Format = "#,##0.00";
                            ws.Cell(rowIdx, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            ws.Cell(rowIdx, 6).Value = cost;
                            ws.Cell(rowIdx, 6).Style.NumberFormat.Format = "#,##0.00";
                            ws.Cell(rowIdx, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            ws.Cell(rowIdx, 7).Value = stock;
                            ws.Cell(rowIdx, 7).Style.NumberFormat.Format = "#,##0";
                            ws.Cell(rowIdx, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            ws.Cell(rowIdx, 8).Value = soldQty;
                            ws.Cell(rowIdx, 8).Style.NumberFormat.Format = "#,##0";
                            ws.Cell(rowIdx, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            ws.Cell(rowIdx, 9).Value = soldTotal;
                            ws.Cell(rowIdx, 9).Style.NumberFormat.Format = "#,##0.00";
                            ws.Cell(rowIdx, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            ApplyDataRowStyle(ws, rowIdx, 9, seq % 2 == 0);
                            totalStock += stock;
                            totalSoldQty += soldQty;
                            totalSoldAmt += soldTotal;
                            rowIdx++;
                            seq++;
                        }

                        ApplyTableSummaryRow(ws, rowIdx, headers.Length, "รวมทั้งสิ้น", new Dictionary<int, (object val, string fmt)>
                        {
                            { 7, (totalStock, "#,##0") },
                            { 8, (totalSoldQty, "#,##0") },
                            { 9, (totalSoldAmt, "#,##0.00") }
                        });
                        break;
                    }

                    case "รายงานข้อมูลการขายหน้าร้าน":
                    {
                        var headers = new[] { "ลำดับ", "วันที่ทำรายการ", "เลขที่บิล", "ชื่อลูกค้า", "รูปแบบการชำระเงิน", "สถานะ", "ยอดสุทธิ (บาท)" };
                        ApplyTableHeader(ws, headerRow, headers);

                        string sql = isAllPeriod
                            ? @"SELECT s.Sales_Date as SalesDate, 
                                       s.RefNo as RefNo, 
                                       COALESCE(c.Cus_Name, 'ลูกค้าทั่วไป') as CustomerName, 
                                       COALESCE(s.Sales_PaymentType, 'เงินสด') as PaymentType, 
                                       COALESCE(s.Sales_Status, 'ชำระเงินแล้ว') as Status, 
                                       s.Sales_Total as Total
                                FROM tblSales_H s
                                LEFT JOIN tblCustomer c ON s.Cus_ID = c.Cus_ID
                                ORDER BY s.Sales_Date DESC"
                            : @"SELECT s.Sales_Date as SalesDate, 
                                       s.RefNo as RefNo, 
                                       COALESCE(c.Cus_Name, 'ลูกค้าทั่วไป') as CustomerName, 
                                       COALESCE(s.Sales_PaymentType, 'เงินสด') as PaymentType, 
                                       COALESCE(s.Sales_Status, 'ชำระเงินแล้ว') as Status, 
                                       s.Sales_Total as Total
                                FROM tblSales_H s
                                LEFT JOIN tblCustomer c ON s.Cus_ID = c.Cus_ID
                                WHERE (s.Sales_Date >= @S AND s.Sales_Date <= @E) OR (s.Sales_Date >= @S_BE AND s.Sales_Date <= @E_BE)
                                ORDER BY s.Sales_Date DESC";

                        var data = (await conn.QueryAsync(sql, dateParams)).ToList();
                        int rowIdx = headerRow + 1;
                        int seq = 1;
                        decimal grandTotal = 0;

                        foreach (var item in data)
                        {
                            ws.Row(rowIdx).Height = 20;
                            string date = item.SalesDate ?? "-";
                            string refNo = item.RefNo ?? "-";
                            string cusName = item.CustomerName ?? "-";
                            string payType = item.PaymentType ?? "-";
                            string status = item.Status ?? "-";
                            decimal total = Convert.ToDecimal(item.Total ?? 0);

                            ws.Cell(rowIdx, 1).Value = seq;
                            ws.Cell(rowIdx, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 2).Value = date;
                            ws.Cell(rowIdx, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 3).Value = refNo;
                            ws.Cell(rowIdx, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 4).Value = cusName;
                            ws.Cell(rowIdx, 5).Value = payType;
                            ws.Cell(rowIdx, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 6).Value = status;
                            ws.Cell(rowIdx, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 7).Value = total;
                            ws.Cell(rowIdx, 7).Style.NumberFormat.Format = "#,##0.00";
                            ws.Cell(rowIdx, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            ApplyDataRowStyle(ws, rowIdx, 7, seq % 2 == 0);
                            grandTotal += total;
                            rowIdx++;
                            seq++;
                        }

                        ApplyTableSummaryRow(ws, rowIdx, headers.Length, "รวมทั้งสิ้น", new Dictionary<int, (object val, string fmt)>
                        {
                            { 7, (grandTotal, "#,##0.00") }
                        });
                        break;
                    }

                    case "รายงานข้อมูลรายรับ-รายจ่าย":
                    {
                        var headers = new[] { "ลำดับ", "วันที่", "ประเภท", "เลขอ้างอิง / รายละเอียด", "หมวดหมู่ / วิธีชำระ", "จำนวนเงิน (บาท)" };
                        ApplyTableHeader(ws, headerRow, headers);

                        string sqlSales = isAllPeriod
                            ? "SELECT Sales_Date as Dt, 'รายรับ (การขาย)' as Typ, RefNo as Ref, Sales_PaymentType as Cat, Sales_Total as Amt FROM tblSales_H WHERE Sales_Status != 'ยกเลิก' OR Sales_Status IS NULL"
                            : "SELECT Sales_Date as Dt, 'รายรับ (การขาย)' as Typ, RefNo as Ref, Sales_PaymentType as Cat, Sales_Total as Amt FROM tblSales_H WHERE (Sales_Status != 'ยกเลิก' OR Sales_Status IS NULL) AND ((Sales_Date >= @S AND Sales_Date <= @E) OR (Sales_Date >= @S_BE AND Sales_Date <= @E_BE))";

                        string sqlExp = isAllPeriod
                            ? "SELECT Expense_Date as Dt, 'รายจ่าย' as Typ, COALESCE(Expense_Note, '-') as Ref, Expense_Category as Cat, Expense_Amount as Amt FROM tblExpense"
                            : "SELECT Expense_Date as Dt, 'รายจ่าย' as Typ, COALESCE(Expense_Note, '-') as Ref, Expense_Category as Cat, Expense_Amount as Amt FROM tblExpense WHERE (Expense_Date >= @S AND Expense_Date <= @E) OR (Expense_Date >= @S_BE AND Expense_Date <= @E_BE)";

                        var sales = (await conn.QueryAsync(sqlSales, dateParams)).ToList();
                        var exps = (await conn.QueryAsync(sqlExp, dateParams)).ToList();
                        var combined = sales.Concat(exps).OrderByDescending(x => (string)x.Dt).ToList();

                        int rowIdx = headerRow + 1;
                        int seq = 1;
                        decimal totalInc = 0;
                        decimal totalExp = 0;

                        foreach (var item in combined)
                        {
                            ws.Row(rowIdx).Height = 20;
                            string dt = item.Dt ?? "-";
                            string typ = item.Typ ?? "-";
                            string refVal = item.Ref ?? "-";
                            string cat = item.Cat ?? "-";
                            decimal amt = Convert.ToDecimal(item.Amt ?? 0);

                            ws.Cell(rowIdx, 1).Value = seq;
                            ws.Cell(rowIdx, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 2).Value = dt;
                            ws.Cell(rowIdx, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 3).Value = typ;
                            ws.Cell(rowIdx, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            if (typ.Contains("รายรับ"))
                                ws.Cell(rowIdx, 3).Style.Font.FontColor = XLColor.FromHtml("#16A34A");
                            else
                                ws.Cell(rowIdx, 3).Style.Font.FontColor = XLColor.FromHtml("#DC2626");

                            ws.Cell(rowIdx, 4).Value = refVal;
                            ws.Cell(rowIdx, 5).Value = cat;

                            ws.Cell(rowIdx, 6).Value = amt;
                            ws.Cell(rowIdx, 6).Style.NumberFormat.Format = "#,##0.00";
                            ws.Cell(rowIdx, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            ApplyDataRowStyle(ws, rowIdx, 6, seq % 2 == 0);
                            if (typ.Contains("รายรับ")) totalInc += amt;
                            else totalExp += amt;
                            rowIdx++;
                            seq++;
                        }

                        ApplyTableSummaryRow(ws, rowIdx, headers.Length, $"รวมรายรับ: {totalInc:N2} | รวมรายจ่าย: {totalExp:N2} | กำไรสุทธิ: {(totalInc - totalExp):N2}", null);
                        break;
                    }

                    case "รายงานข้อมูลการสั่งซื้อสินค้า":
                    {
                        var headers = new[] { "ลำดับ", "วันที่สั่งซื้อ", "เลขที่ใบสั่งซื้อ", "ซัพพลายเออร์", "สถานะ", "ยอดรวม (บาท)" };
                        ApplyTableHeader(ws, headerRow, headers);

                        string sql = isAllPeriod
                            ? @"SELECT po.PO_Date as Dt, ('PO-' || po.PO_ID) as PoNo, COALESCE(p.Partner_Name, '-') as Sup, COALESCE(po.PO_Status, 'สำเร็จ') as Status, po.PO_Total as Total
                                FROM tblPO_H po
                                LEFT JOIN tblPartner p ON po.Partner_ID = p.Partner_ID
                                ORDER BY po.PO_Date DESC"
                            : @"SELECT po.PO_Date as Dt, ('PO-' || po.PO_ID) as PoNo, COALESCE(p.Partner_Name, '-') as Sup, COALESCE(po.PO_Status, 'สำเร็จ') as Status, po.PO_Total as Total
                                FROM tblPO_H po
                                LEFT JOIN tblPartner p ON po.Partner_ID = p.Partner_ID
                                WHERE (po.PO_Date >= @S AND po.PO_Date <= @E) OR (po.PO_Date >= @S_BE AND po.PO_Date <= @E_BE)
                                ORDER BY po.PO_Date DESC";

                        var data = (await conn.QueryAsync(sql, dateParams)).ToList();
                        int rowIdx = headerRow + 1;
                        int seq = 1;
                        decimal grandTotal = 0;

                        foreach (var item in data)
                        {
                            ws.Row(rowIdx).Height = 20;
                            string dt = item.Dt ?? "-";
                            string poNo = item.PoNo ?? "-";
                            string sup = item.Sup ?? "-";
                            string status = item.Status ?? "-";
                            decimal total = Convert.ToDecimal(item.Total ?? 0);

                            ws.Cell(rowIdx, 1).Value = seq;
                            ws.Cell(rowIdx, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 2).Value = dt;
                            ws.Cell(rowIdx, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 3).Value = poNo;
                            ws.Cell(rowIdx, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 4).Value = sup;

                            ws.Cell(rowIdx, 5).Value = status;
                            ws.Cell(rowIdx, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 6).Value = total;
                            ws.Cell(rowIdx, 6).Style.NumberFormat.Format = "#,##0.00";
                            ws.Cell(rowIdx, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            ApplyDataRowStyle(ws, rowIdx, 6, seq % 2 == 0);
                            grandTotal += total;
                            rowIdx++;
                            seq++;
                        }

                        ApplyTableSummaryRow(ws, rowIdx, headers.Length, "รวมทั้งสิ้น", new Dictionary<int, (object val, string fmt)>
                        {
                            { 6, (grandTotal, "#,##0.00") }
                        });
                        break;
                    }

                    case "รายงานข้อมูลการรับเข้าสินค้า":
                    {
                        var headers = new[] { "ลำดับ", "วันที่รับเข้า", "เลขที่อ้างอิง", "ซัพพลายเออร์ / รายละเอียด", "จำนวนรับเข้ารวม (ชิ้น)" };
                        ApplyTableHeader(ws, headerRow, headers);

                        string sql = isAllPeriod
                            ? @"SELECT si.StockIn_Date as Dt, ('IN-' || si.StockIn_ID) as SiNo, COALESCE(si.Note, p.Partner_Name, '-') as Sup, COALESCE(SUM(sd.StockIn_Qty), 0) as Qty
                                FROM tblStockIn_H si
                                LEFT JOIN tblPO_H po ON si.PO_ID = po.PO_ID
                                LEFT JOIN tblPartner p ON po.Partner_ID = p.Partner_ID
                                LEFT JOIN tblStockInDetail sd ON si.StockIn_ID = sd.StockIn_ID
                                GROUP BY si.StockIn_ID
                                ORDER BY si.StockIn_Date DESC"
                            : @"SELECT si.StockIn_Date as Dt, ('IN-' || si.StockIn_ID) as SiNo, COALESCE(si.Note, p.Partner_Name, '-') as Sup, COALESCE(SUM(sd.StockIn_Qty), 0) as Qty
                                FROM tblStockIn_H si
                                LEFT JOIN tblPO_H po ON si.PO_ID = po.PO_ID
                                LEFT JOIN tblPartner p ON po.Partner_ID = p.Partner_ID
                                LEFT JOIN tblStockInDetail sd ON si.StockIn_ID = sd.StockIn_ID
                                WHERE (si.StockIn_Date >= @S AND si.StockIn_Date <= @E) OR (si.StockIn_Date >= @S_BE AND si.StockIn_Date <= @E_BE)
                                GROUP BY si.StockIn_ID
                                ORDER BY si.StockIn_Date DESC";

                        var data = (await conn.QueryAsync(sql, dateParams)).ToList();
                        int rowIdx = headerRow + 1;
                        int seq = 1;
                        long totalQty = 0;

                        foreach (var item in data)
                        {
                            ws.Row(rowIdx).Height = 20;
                            string dt = item.Dt ?? "-";
                            string siNo = item.SiNo ?? "-";
                            string sup = item.Sup ?? "-";
                            long qty = Convert.ToInt64(item.Qty ?? 0);

                            ws.Cell(rowIdx, 1).Value = seq;
                            ws.Cell(rowIdx, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 2).Value = dt;
                            ws.Cell(rowIdx, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 3).Value = siNo;
                            ws.Cell(rowIdx, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 4).Value = sup;

                            ws.Cell(rowIdx, 5).Value = qty;
                            ws.Cell(rowIdx, 5).Style.NumberFormat.Format = "#,##0";
                            ws.Cell(rowIdx, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            ApplyDataRowStyle(ws, rowIdx, 5, seq % 2 == 0);
                            totalQty += qty;
                            rowIdx++;
                            seq++;
                        }

                        ApplyTableSummaryRow(ws, rowIdx, headers.Length, "รวมทั้งสิ้น", new Dictionary<int, (object val, string fmt)>
                        {
                            { 5, (totalQty, "#,##0") }
                        });
                        break;
                    }

                    case "รายงานข้อมูลการจัดส่งสินค้า":
                    {
                        var headers = new[] { "ลำดับ", "วันที่สั่ง", "เลขที่บิล", "Tracking No", "ชื่อผู้รับ", "เบอร์โทร", "ที่อยู่จัดส่ง", "สถานะจัดส่ง" };
                        ApplyTableHeader(ws, headerRow, headers);

                        string sql = @"SELECT s.Sales_Date as Dt, s.RefNo as RefNo, COALESCE(d.Tracking_No, '-') as Tracking, 
                                              COALESCE(d.Recipient_Name, '-') as Name, COALESCE(d.Recipient_Tel, '-') as Tel, 
                                              COALESCE(d.Recipient_Address, '-') as Addr, COALESCE(d.Delivery_Status, 'รอจัดส่ง') as Status
                                       FROM tblDelivery d
                                       LEFT JOIN tblSales_H s ON d.Sales_ID = s.Sales_ID
                                       ORDER BY d.Delivery_ID DESC";

                        var data = (await conn.QueryAsync(sql)).ToList();
                        int rowIdx = headerRow + 1;
                        int seq = 1;

                        foreach (var item in data)
                        {
                            ws.Row(rowIdx).Height = 20;
                            ws.Cell(rowIdx, 1).Value = seq;
                            ws.Cell(rowIdx, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 2).Value = (string)(item.Dt ?? "-");
                            ws.Cell(rowIdx, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 3).Value = (string)(item.RefNo ?? "-");
                            ws.Cell(rowIdx, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 4).Value = (string)(item.Tracking ?? "-");
                            ws.Cell(rowIdx, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 5).Value = (string)(item.Name ?? "-");
                            ws.Cell(rowIdx, 6).Value = (string)(item.Tel ?? "-");
                            ws.Cell(rowIdx, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 7).Value = (string)(item.Addr ?? "-");

                            ws.Cell(rowIdx, 8).Value = (string)(item.Status ?? "-");
                            ws.Cell(rowIdx, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ApplyDataRowStyle(ws, rowIdx, 8, seq % 2 == 0);
                            rowIdx++;
                            seq++;
                        }

                        ApplyTableSummaryRow(ws, rowIdx, headers.Length, $"รวมรายการจัดส่งทั้งหมด: {data.Count} รายการ", null);
                        break;
                    }

                    case "รายงานข้อมูลการเคลม":
                    {
                        var headers = new[] { "ลำดับ", "วันที่เคลม", "เลขที่เคลม", "สินค้า / รายละเอียด", "สาเหตุการเคลม", "รูปแบบการเคลม", "สถานะเคลม" };
                        ApplyTableHeader(ws, headerRow, headers);

                        string sql = isAllPeriod
                            ? @"SELECT c.Claim_Date as Dt, ('CLM-' || c.Claim_ID) as ClmNo, 
                                       COALESCE(p.Pro_Name, 'สินค้าทั่วไป') as Prod, 
                                       COALESCE(c.Claim_Reason, 'ไม่ระบุ') as Reason, 
                                       COALESCE(c.Claim_Action, 'เปลี่ยนสินค้าใหม่') as Action, 
                                       COALESCE(c.Claim_Status, 'รอดำเนินการ') as Status
                                FROM tblClaim c
                                LEFT JOIN tblProduct p ON c.Pro_ID = p.Pro_ID
                                ORDER BY c.Claim_ID DESC"
                            : @"SELECT c.Claim_Date as Dt, ('CLM-' || c.Claim_ID) as ClmNo, 
                                       COALESCE(p.Pro_Name, 'สินค้าทั่วไป') as Prod, 
                                       COALESCE(c.Claim_Reason, 'ไม่ระบุ') as Reason, 
                                       COALESCE(c.Claim_Action, 'เปลี่ยนสินค้าใหม่') as Action, 
                                       COALESCE(c.Claim_Status, 'รอดำเนินการ') as Status
                                FROM tblClaim c
                                LEFT JOIN tblProduct p ON c.Pro_ID = p.Pro_ID
                                WHERE (c.Claim_Date >= @S AND c.Claim_Date <= @E) OR (c.Claim_Date >= @S_BE AND c.Claim_Date <= @E_BE)
                                ORDER BY c.Claim_ID DESC";

                        var data = (await conn.QueryAsync(sql, dateParams)).ToList();
                        int rowIdx = headerRow + 1;
                        int seq = 1;

                        foreach (var item in data)
                        {
                            ws.Row(rowIdx).Height = 20;
                            ws.Cell(rowIdx, 1).Value = seq;
                            ws.Cell(rowIdx, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 2).Value = (string)(item.Dt ?? "-");
                            ws.Cell(rowIdx, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 3).Value = (string)(item.ClmNo ?? "-");
                            ws.Cell(rowIdx, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 4).Value = (string)(item.Prod ?? "-");
                            ws.Cell(rowIdx, 5).Value = (string)(item.Reason ?? "-");
                            ws.Cell(rowIdx, 6).Value = (string)(item.Action ?? "-");

                            ws.Cell(rowIdx, 7).Value = (string)(item.Status ?? "-");
                            ws.Cell(rowIdx, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ApplyDataRowStyle(ws, rowIdx, 7, seq % 2 == 0);
                            rowIdx++;
                            seq++;
                        }

                        ApplyTableSummaryRow(ws, rowIdx, headers.Length, $"รวมรายการเคลมสินค้า: {data.Count} รายการ", null);
                        break;
                    }

                    case "รายงานข้อมูลพนักงาน":
                    {
                        var headers = new[] { "ลำดับ", "รหัสพนักงาน", "ชื่อพนักงาน", "ชื่อผู้ใช้งาน", "บทบาท / ตำแหน่ง", "เบอร์โทรศัพท์", "ยอดขายที่ทำได้ (บาท)" };
                        ApplyTableHeader(ws, headerRow, headers);

                        string sql = isAllPeriod
                            ? @"SELECT e.Emp_ID as Id, e.Emp_Name as Name, e.Emp_Username as Username, e.Emp_Role as Role, 
                                       COALESCE(e.Emp_Tel, '-') as Tel, COALESCE(SUM(s.Sales_Total), 0) as Total
                                FROM tblEmployee e
                                LEFT JOIN tblSales_H s ON e.Emp_ID = s.Emp_ID
                                GROUP BY e.Emp_ID
                                ORDER BY Total DESC"
                            : @"SELECT e.Emp_ID as Id, e.Emp_Name as Name, e.Emp_Username as Username, e.Emp_Role as Role, 
                                       COALESCE(e.Emp_Tel, '-') as Tel, COALESCE(SUM(s.Sales_Total), 0) as Total
                                FROM tblEmployee e
                                LEFT JOIN tblSales_H s ON e.Emp_ID = s.Emp_ID AND ((s.Sales_Date >= @S AND s.Sales_Date <= @E) OR (s.Sales_Date >= @S_BE AND s.Sales_Date <= @E_BE))
                                GROUP BY e.Emp_ID
                                ORDER BY Total DESC";

                        var data = (await conn.QueryAsync(sql, dateParams)).ToList();
                        int rowIdx = headerRow + 1;
                        int seq = 1;
                        decimal totalEmpSales = 0;

                        foreach (var item in data)
                        {
                            ws.Row(rowIdx).Height = 20;
                            int id = Convert.ToInt32(item.Id ?? 0);
                            string name = item.Name ?? "-";
                            string user = item.Username ?? "-";
                            string role = item.Role ?? "-";
                            string tel = item.Tel ?? "-";
                            decimal total = Convert.ToDecimal(item.Total ?? 0);

                            ws.Cell(rowIdx, 1).Value = seq;
                            ws.Cell(rowIdx, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 2).Value = id;
                            ws.Cell(rowIdx, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 3).Value = name;
                            ws.Cell(rowIdx, 4).Value = user;
                            ws.Cell(rowIdx, 5).Value = role;

                            ws.Cell(rowIdx, 6).Value = tel;
                            ws.Cell(rowIdx, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 7).Value = total;
                            ws.Cell(rowIdx, 7).Style.NumberFormat.Format = "#,##0.00";
                            ws.Cell(rowIdx, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            ApplyDataRowStyle(ws, rowIdx, 7, seq % 2 == 0);
                            totalEmpSales += total;
                            rowIdx++;
                            seq++;
                        }

                        ApplyTableSummaryRow(ws, rowIdx, headers.Length, "รวมทั้งสิ้น", new Dictionary<int, (object val, string fmt)>
                        {
                            { 7, (totalEmpSales, "#,##0.00") }
                        });
                        break;
                    }

                    case "รายงานข้อมูลบริษัทคู่ค้า":
                    {
                        var headers = new[] { "ลำดับ", "ชื่อบริษัทคู่ค้า", "ผู้ติดต่อ", "เบอร์โทรศัพท์", "ที่อยู่", "มูลค่าการสั่งซื้อสะสม (บาท)" };
                        ApplyTableHeader(ws, headerRow, headers);

                        string sql = @"SELECT p.Partner_Name as Name, COALESCE(p.Partner_Contact, '-') as Contact, 
                                              COALESCE(p.Partner_Tel, '-') as Tel, COALESCE(p.Partner_Address, '-') as Addr, 
                                              COALESCE(SUM(po.PO_Total), 0) as Total
                                       FROM tblPartner p
                                       LEFT JOIN tblPO_H po ON p.Partner_ID = po.Partner_ID
                                       GROUP BY p.Partner_ID
                                       ORDER BY Total DESC";

                        var data = (await conn.QueryAsync(sql)).ToList();
                        int rowIdx = headerRow + 1;
                        int seq = 1;
                        decimal totalPartnerPo = 0;

                        foreach (var item in data)
                        {
                            ws.Row(rowIdx).Height = 20;
                            string name = item.Name ?? "-";
                            string contact = item.Contact ?? "-";
                            string tel = item.Tel ?? "-";
                            string addr = item.Addr ?? "-";
                            decimal total = Convert.ToDecimal(item.Total ?? 0);

                            ws.Cell(rowIdx, 1).Value = seq;
                            ws.Cell(rowIdx, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 2).Value = name;
                            ws.Cell(rowIdx, 3).Value = contact;

                            ws.Cell(rowIdx, 4).Value = tel;
                            ws.Cell(rowIdx, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 5).Value = addr;

                            ws.Cell(rowIdx, 6).Value = total;
                            ws.Cell(rowIdx, 6).Style.NumberFormat.Format = "#,##0.00";
                            ws.Cell(rowIdx, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            ApplyDataRowStyle(ws, rowIdx, 6, seq % 2 == 0);
                            totalPartnerPo += total;
                            rowIdx++;
                            seq++;
                        }

                        ApplyTableSummaryRow(ws, rowIdx, headers.Length, "รวมทั้งสิ้น", new Dictionary<int, (object val, string fmt)>
                        {
                            { 6, (totalPartnerPo, "#,##0.00") }
                        });
                        break;
                    }

                    case "รายงานข้อมูลประเภทสินค้า":
                    {
                        var headers = new[] { "ลำดับ", "รหัสประเภท", "ชื่อประเภทสินค้า", "คำอธิบาย", "จำนวนสินค้าในหมวดหมู่" };
                        ApplyTableHeader(ws, headerRow, headers);

                        string sql = @"SELECT c.CategoryCode as Code, c.CategoryName as Name, COALESCE(c.Description, '-') as Descr, COUNT(p.Pro_ID) as ProdCount
                                       FROM tblProductCategory c
                                       LEFT JOIN tblProduct p ON (p.CategoryId = c.CategoryId OR TRIM(p.Pro_Category) = TRIM(c.CategoryName))
                                       GROUP BY c.CategoryId
                                       ORDER BY c.CategoryCode ASC";

                        var data = (await conn.QueryAsync(sql)).ToList();
                        int rowIdx = headerRow + 1;
                        int seq = 1;
                        long totalItems = 0;

                        foreach (var item in data)
                        {
                            ws.Row(rowIdx).Height = 20;
                            string code = item.Code ?? "-";
                            string name = item.Name ?? "-";
                            string desc = item.Descr ?? "-";
                            long count = Convert.ToInt64(item.ProdCount ?? 0);

                            ws.Cell(rowIdx, 1).Value = seq;
                            ws.Cell(rowIdx, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 2).Value = code;
                            ws.Cell(rowIdx, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            ws.Cell(rowIdx, 3).Value = name;
                            ws.Cell(rowIdx, 4).Value = desc;

                            ws.Cell(rowIdx, 5).Value = count;
                            ws.Cell(rowIdx, 5).Style.NumberFormat.Format = "#,##0";
                            ws.Cell(rowIdx, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            ApplyDataRowStyle(ws, rowIdx, 5, seq % 2 == 0);
                            totalItems += count;
                            rowIdx++;
                            seq++;
                        }

                        ApplyTableSummaryRow(ws, rowIdx, headers.Length, "รวมทั้งสิ้น", new Dictionary<int, (object val, string fmt)>
                        {
                            { 5, (totalItems, "#,##0") }
                        });
                        break;
                    }

                    default:
                    {
                        var validHeaders = new List<string> { "ลำดับ" };
                        var activeColIndices = new List<int>();
                        for (int i = 0; i < _colHeaders.Length; i++)
                        {
                            if (_colHeaders[i] != "-" && !string.IsNullOrWhiteSpace(_colHeaders[i]))
                            {
                                validHeaders.Add(_colHeaders[i]);
                                activeColIndices.Add(i + 1);
                            }
                        }
                        ApplyTableHeader(ws, headerRow, validHeaders);

                        int rowIdx = headerRow + 1;
                        int seq = 1;
                        foreach (var row in ReportRows)
                        {
                            ws.Row(rowIdx).Height = 20;
                            ws.Cell(rowIdx, 1).Value = seq;
                            ws.Cell(rowIdx, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            for (int c = 0; c < activeColIndices.Count; c++)
                            {
                                int colNum = activeColIndices[c];
                                string val = colNum switch
                                {
                                    1 => row.Col1,
                                    2 => row.Col2,
                                    3 => row.Col3,
                                    4 => row.Col4,
                                    5 => row.Col5,
                                    _ => ""
                                };

                                int cellCol = c + 2;
                                if (decimal.TryParse(val, out decimal numVal) && !val.StartsWith("0") && val.Length < 11)
                                {
                                    ws.Cell(rowIdx, cellCol).Value = numVal;
                                    ws.Cell(rowIdx, cellCol).Style.NumberFormat.Format = (numVal % 1 == 0) ? "#,##0" : "#,##0.00";
                                    ws.Cell(rowIdx, cellCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                }
                                else
                                {
                                    ws.Cell(rowIdx, cellCol).Value = val;
                                }
                            }

                            ApplyDataRowStyle(ws, rowIdx, validHeaders.Count, seq % 2 == 0);
                            rowIdx++;
                            seq++;
                        }

                        ApplyTableSummaryRow(ws, rowIdx, validHeaders.Count, $"รวมข้อมูลทั้งหมด: {ReportRows.Count} รายการ", null);
                        break;
                    }
                }
            }
        }

        private static void ApplyTableHeader(IXLWorksheet ws, int row, IList<string> headers)
        {
            for (int i = 0; i < headers.Count; i++)
            {
                var cell = ws.Cell(row, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontSize = 11;
                cell.Style.Font.FontColor = XLColor.FromHtml("#1E293B");
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E2E8F0");
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#CBD5E1");
            }
        }

        private static void ApplyDataRowStyle(IXLWorksheet ws, int row, int colCount, bool isEven)
        {
            var fillCol = isEven ? XLColor.FromHtml("#F8FAFC") : XLColor.White;
            var range = ws.Range(row, 1, row, colCount);
            range.Style.Fill.BackgroundColor = fillCol;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorderColor = XLColor.FromHtml("#E2E8F0");
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.OutsideBorderColor = XLColor.FromHtml("#CBD5E1");
            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        private static void ApplyTableSummaryRow(IXLWorksheet ws, int row, int colCount, string label, Dictionary<int, (object val, string fmt)>? values)
        {
            ws.Row(row).Height = 24;
            var range = ws.Range(row, 1, row, colCount);
            range.Style.Font.Bold = true;
            range.Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F5F9");
            range.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            range.Style.Border.TopBorderColor = XLColor.FromHtml("#94A3B8");
            range.Style.Border.BottomBorder = XLBorderStyleValues.Double;
            range.Style.Border.BottomBorderColor = XLColor.FromHtml("#475569");
            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            if (values != null && values.Any())
            {
                ws.Cell(row, 1).Value = label;
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                foreach (var kvp in values)
                {
                    var cell = ws.Cell(row, kvp.Key);
                    if (kvp.Value.val is decimal decVal)
                        cell.Value = decVal;
                    else if (kvp.Value.val is long longVal)
                        cell.Value = longVal;
                    else if (kvp.Value.val is int intVal)
                        cell.Value = intVal;
                    else
                        cell.Value = kvp.Value.val?.ToString() ?? "";

                    cell.Style.NumberFormat.Format = kvp.Value.fmt;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }
            }
            else
            {
                ws.Range(row, 1, row, colCount).Merge();
                ws.Cell(row, 1).Value = label;
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }
        }

        #endregion

        // --- User Management Logic ---
        private async Task PerformLogin()
        {
            try
            {
                using (var conn = _databaseService.GetConnection())
                {
                    string sql = "SELECT Emp_ID AS Emp_ID, Emp_ID AS Id, Emp_Name AS Emp_Name, Emp_Name AS Name, Emp_Username AS Username, Emp_Password AS Password, Emp_Role AS Role, Emp_Tel AS Tel, Emp_Tel AS Phone FROM tblEmployee WHERE Emp_Username = @Username LIMIT 1";
                    var user = await conn.QueryFirstOrDefaultAsync<User>(sql, new { Username = LoginUsername });

                    bool isPasswordValid = false;
                    if (user != null && !string.IsNullOrEmpty(user.Password))
                    {
                        if (user.Password == LoginPassword)
                        {
                            isPasswordValid = true;
                        }
                        else if (DatabaseService.IsBCryptHash(user.Password))
                        {
                            try
                            {
                                if (BCrypt.Net.BCrypt.Verify(LoginPassword, user.Password))
                                {
                                    isPasswordValid = true;
                                    await conn.ExecuteAsync("UPDATE tblEmployee SET Emp_Password = @Password WHERE Emp_ID = @Id", new { Password = LoginPassword, Id = user.Id });
                                    user.Password = LoginPassword;
                                }
                            }
                            catch
                            {
                                isPasswordValid = false;
                            }
                        }
                    }

                    if (user != null && isPasswordValid)
                    {
                        CurrentUser = user;
                        IsLoginDialogVisible = false;
                        IsHomeMode = true; // Show home screen after successful login
                        LoginErrorMessage = string.Empty;
                        LoginPassword = string.Empty; // Clear for security
                        _ = LoadDashboardData();
                    }
                    else
                    {
                        LoginErrorMessage = "Invalid Username หรือ Password";
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert($"เข้าสู่ระบบไม่สำเร็จ: {ex.Message}", "ข้อผิดพลาด", "❌");
            }
        }

        private void Logout()
        {
            CurrentUser = null;
            IsLoginDialogVisible = false; // Stay on the screen but logged out
            IsHomeMode = true; // Reset view behind it
            IsManagementMode = false;
            IsStockInMode = false;
            IsReportsMode = false;
            IsHistoryMode = false;
            IsStaffMode = false;
            LoginUsername = string.Empty;
            LoginPassword = string.Empty;
            LoginErrorMessage = string.Empty;
        }

        private async Task OpenStaffMode()
        {
            if (IsAdmin)
            {
                IsHomeMode = false;
                IsManagementMode = false;
                IsStockInMode = false;
                IsReportsMode = false;
                IsHistoryMode = false;
                IsStaffMode = true;
                SelectedUser = new User();
                await LoadUsers();
            }
            else
            {
                ShowAlert("เข้าถึงไม่ได้ เฉพาะผู้ดูแลระบบเท่านั้น", "แจ้งเตือน", "⚠️");
            }
        }

        private async Task LoadUsers()
        {
            try
            {
                using (var conn = _databaseService.GetConnection())
                {
                    var users = await conn.QueryAsync<User>("SELECT Emp_ID AS Emp_ID, Emp_ID AS Id, Emp_Name AS Emp_Name, Emp_Name AS Name, Emp_Username AS Username, Emp_Password AS Password, Emp_Role AS Role, Emp_Tel AS Tel, Emp_Tel AS Phone FROM tblEmployee");
                    UsersList.Clear();
                    foreach (var u in users) UsersList.Add(u);

                    FilteredStaffs?.Refresh();

                    OnPropertyChanged(nameof(TotalStaffCount));
                    OnPropertyChanged(nameof(ActiveStaffCount));
                    OnPropertyChanged(nameof(AdminStaffCount));
                    OnPropertyChanged(nameof(FilteredStaffList));
                }
            }
            catch (Exception ex)
            {
                ShowAlert($"เกิดข้อผิดพลาดในการโหลดข้อมูลพนักงาน: {ex.Message}", "ข้อผิดพลาด", "❌");
            }
        }

        private async Task AddUser()
        {
            try
            {
                if (SelectedUser == null || string.IsNullOrWhiteSpace(SelectedUser.Username)) return;

                // Check if username already exists locally for quick validation
                if (UsersList.Any(u => string.Equals(u.Username, SelectedUser.Username, StringComparison.OrdinalIgnoreCase)))
                {
                    ShowAlert("Username นี้มีอยู่แล้วในระบบ", "แจ้งเตือน", "⚠️");
                    return;
                }

                string rawPassword = SelectedUser.Password ?? string.Empty;

                using (var conn = _databaseService.GetConnection())
                {
                    string sql = "INSERT INTO tblEmployee (Emp_Name, Emp_Tel, Emp_Username, Emp_Password, Emp_Role) VALUES (@Name, @Phone, @Username, @Password, @Role)";
                    await conn.ExecuteAsync(sql, new
                    {
                        Name = SelectedUser.Name,
                        Phone = SelectedUser.Phone,
                        Username = SelectedUser.Username,
                        Password = rawPassword,
                        Role = SelectedUser.Role
                    });
                    IsStaffModalOpen = false;
                    await LoadUsers();
                    SelectedUser = new User();
                    ShowAlert("เพิ่มพนักงานสำเร็จ", "สำเร็จ", "🎉");
                }
            }
            catch (Exception ex)
            {
                ShowAlert($"เกิดข้อผิดพลาดในการเพิ่มพนักงาน: {ex.Message}", "ข้อผิดพลาด", "❌");
            }
        }

        private async Task UpdateUser()
        {
            try
            {
                if (SelectedUser == null || SelectedUser.Id == 0) return;

                string rawPassword = SelectedUser.Password ?? string.Empty;

                using (var conn = _databaseService.GetConnection())
                {
                    string sql = "UPDATE tblEmployee SET Emp_Name = @Name, Emp_Tel = @Phone, Emp_Username = @Username, Emp_Password = @Password, Emp_Role = @Role WHERE Emp_ID = @Id";
                    await conn.ExecuteAsync(sql, new
                    {
                        Id = SelectedUser.Id,
                        Name = SelectedUser.Name,
                        Phone = SelectedUser.Phone,
                        Username = SelectedUser.Username,
                        Password = rawPassword,
                        Role = SelectedUser.Role
                    });
                    IsStaffModalOpen = false;
                    await LoadUsers();
                    ShowAlert("อัปเดตข้อมูลพนักงานสำเร็จ", "สำเร็จ", "🎉");
                }
            }
            catch (Exception ex)
            {
                ShowAlert($"เกิดข้อผิดพลาดในการอัปเดตข้อมูลพนักงาน: {ex.Message}", "ข้อผิดพลาด", "❌");
            }
        }

        private async Task DeleteUser()
        {
            await Task.CompletedTask;
            if (SelectedUser == null || SelectedUser.Id == 0) return;

            ShowConfirm($"คุณแน่ใจหรือไม่ที่จะลบบัญชีพนักงาน {SelectedUser.Name}?", async () =>
            {
                try
                {
                    using (var conn = _databaseService.GetConnection())
                    {
                        string sql = "DELETE FROM tblEmployee WHERE Emp_ID = @Id";
                        await conn.ExecuteAsync(sql, new { Id = SelectedUser.Id });
                        await LoadUsers();
                        SelectedUser = new User();
                    }
                }
                catch (Exception ex)
                {
                    ShowAlert($"เกิดข้อผิดพลาดในการลบพนักงาน: {ex.Message}", "ข้อผิดพลาด", "❌");
                }
            }, "ยืนยันการลบ");
        }

        private bool _isPOModalOpen;
        public bool IsPOModalOpen
        {
            get => _isPOModalOpen;
            set
            {
                if (SetProperty(ref _isPOModalOpen, value))
                {
                    OnPropertyChanged(nameof(IsPOModalOpen));
                    IsModalOpen = value;
                }
            }
        }

        private bool _isNewPOMode = true;
        public bool IsNewPOMode
        {
            get => _isNewPOMode;
            set
            {
                if (SetProperty(ref _isNewPOMode, value))
                {
                    OnPropertyChanged(nameof(CanEditPOStatus));
                }
            }
        }

        private string _poModalTitle = "สร้างใบสั่งซื้อสินค้าใหม่";
        public string POModalTitle
        {
            get => _poModalTitle;
            set => SetProperty(ref _poModalTitle, value);
        }

        private string _poStatusFilter = "ทั้งหมด";
        public string POStatusFilter
        {
            get => _poStatusFilter;
            set { if (SetProperty(ref _poStatusFilter, value)) _ = LoadPurchaseOrders(); }
        }

        public ObservableCollection<string> POStatusFilterOptions { get; } = new ObservableCollection<string> { "ทั้งหมด", "รอดำเนินการ", "ได้รับสินค้าแล้ว", "ยกเลิก" };

        public int TotalPOCount => PurchaseOrders?.Count ?? 0;
        public int PendingPOCount => PurchaseOrders?.Count(p => p.Status == "รอดำเนินการ" || p.Status == "รอรับของ" || p.Status == "Pending") ?? 0;
        public int CompletedPOCount => PurchaseOrders?.Count(p => p.Status == "ได้รับสินค้าแล้ว" || p.Status == "รับของแล้ว" || p.Status == "Completed") ?? 0;
        public decimal TotalPOAmount => PurchaseOrders?.Sum(p => p.TotalAmount) ?? 0;

        public ICommand OpenAddPOModalCommand { get; private set; }
        public ICommand OpenEditPOModalCommand { get; private set; }
        public ICommand ClosePOModalCommand { get; private set; }
        public ICommand ClearPOFilterCommand { get; private set; }
        public ICommand CreatePOFromSelectedLowStockCommand { get; private set; }
        public ICommand SelectPOSuggestionItemCommand { get; private set; }

        private async Task OpenPurchaseOrderMode()
        {
            IsPurchaseOrderMode = true;
            IsHomeMode = false;
            IsManagementMode = false;
            IsStockInMode = false;
            IsReportsMode = false;
            IsHistoryMode = false;
            IsDeliveryMode = false;
            IsCustomerMode = false;
            IsPartnerMode = false;
            IsClaimMode = false;
            
            await LoadPurchaseOrders();
            await LoadLowStockProducts();
            await LoadSuppliersAsync();
        }

        private async Task LoadPurchaseOrders()
        {
            PurchaseOrders.Clear();
            var pos = await Task.Run(() => _databaseService.GetPurchaseOrders(POSearchKeyword));
            if (!string.IsNullOrWhiteSpace(POStatusFilter) && POStatusFilter != "ทั้งหมด")
            {
                pos = pos.Where(p => p.Status != null && p.Status.Equals(POStatusFilter, StringComparison.OrdinalIgnoreCase));
            }
            foreach(var p in pos) PurchaseOrders.Add(p);

            OnPropertyChanged(nameof(TotalPOCount));
            OnPropertyChanged(nameof(PendingPOCount));
            OnPropertyChanged(nameof(CompletedPOCount));
            OnPropertyChanged(nameof(TotalPOAmount));
            OnPropertyChanged(nameof(FilteredOrdersCountText));
        }

        public int SelectedLowStockCount => LowStockProducts?.Count(p => p.IsSelected) ?? 0;

        private async Task LoadLowStockProducts()
        {
            LowStockProducts.Clear();
            using (var conn = _databaseService.GetConnection())
            {
                var query = await conn.QueryAsync<Product>("SELECT Pro_ID AS Pro_ID, Pro_ID AS Id, Pro_Barcode AS Code, Pro_Name AS Name, Pro_Price AS Price, Pro_Cost AS Cost, Pro_Qty AS Stock, Pro_MinQty AS ReorderPoint FROM tblProduct WHERE Pro_Qty <= Pro_MinQty ORDER BY Pro_Qty ASC");
                foreach (var p in query)
                {
                    p.IsSelected = false;
                    p.SelectionChangedAction = () => OnPropertyChanged(nameof(SelectedLowStockCount));
                    LowStockProducts.Add(p);
                }
            }
            OnPropertyChanged(nameof(SelectedLowStockCount));
        }

        public async Task CreatePOFromSelectedLowStock()
        {
            var selected = LowStockProducts.Where(p => p.IsSelected).ToList();
            if (selected.Count == 0)
            {
                selected = LowStockProducts.ToList();
            }

            if (selected.Count == 0)
            {
                ShowAlert("ไม่มีรายการสินค้าใกล้หมดให้สร้างใบสั่งซื้อ", "แจ้งเตือน", "ℹ️");
                return;
            }

            ClearPurchaseOrderForm();
            IsNewPOMode = true;
            IsPOReadOnly = false;
            POModalTitle = "สร้างใบสั่งซื้อสินค้าใหม่";
            CurrentPOItems.Clear();

            foreach (var p in selected)
            {
                int recQty = System.Math.Max(1, (p.ReorderPoint * 2) - p.Stock);
                CurrentPOItems.Add(new PurchaseOrderItem
                {
                    Pro_ID = p.Id,
                    ProductCode = p.Code,
                    ProductName = p.Name,
                    Quantity = recQty,
                    CostPrice = p.Cost,
                    ItemChanged = CalculatePOTotal
                });
            }

            CalculatePOTotal();
            IsPOModalOpen = true;
            await LoadSuppliersAsync();
            await LoadProducts();
        }

        private async Task AutoFillReorderItems()
        {
            try
            {
                using (var conn = _databaseService.GetConnection())
                {
                    var lowStock = await conn.QueryAsync<Product>("SELECT Pro_ID AS Pro_ID, Pro_ID AS Id, Pro_Barcode AS Code, Pro_Name AS Name, Pro_Price AS Price, Pro_Cost AS Cost, Pro_Qty AS Stock, Pro_MinQty AS ReorderPoint FROM tblProduct WHERE Pro_Qty <= Pro_MinQty ORDER BY Pro_Qty ASC");

                    int addedCount = 0;
                    foreach (var p in lowStock)
                    {
                        int recQty = System.Math.Max(1, (p.ReorderPoint * 2) - p.Stock);
                        var existing = CurrentPOItems.FirstOrDefault(i => i.Pro_ID == p.Id || (i.ProductCode == p.Code && i.ProductName == p.Name));
                        if (existing != null)
                        {
                            existing.Quantity = recQty;
                            if (p.Cost > 0) existing.CostPrice = p.Cost;
                        }
                        else
                        {
                            CurrentPOItems.Add(new PurchaseOrderItem
                            {
                                Pro_ID = p.Id,
                                ProductCode = p.Code,
                                ProductName = p.Name,
                                Quantity = recQty,
                                CostPrice = p.Cost,
                                ItemChanged = CalculatePOTotal
                            });
                        }
                        addedCount++;
                    }
                    CalculatePOTotal();
                    if (addedCount > 0)
                    {
                        ShowAlert($"ดึงรายการสินค้าถึงจุดสั่งซื้อเรียบร้อย {addedCount} รายการ", "สำเร็จ", "⚡");
                    }
                    else
                    {
                        ShowAlert("ไม่มีสินค้าที่ถึงจุดสั่งซื้อในขณะนี้", "แจ้งเตือน", "ℹ️");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert($"เกิดข้อผิดพลาดในการดึงรายการสินค้า: {ex.Message}", "ข้อผิดพลาด", "❌");
            }
        }

        private async Task LoadPendingPurchaseOrders()
        {
            var rawList = (await Task.Run(() => _databaseService.GetPendingPurchaseOrders(PendingPOSearchKeyword)))?.ToList() ?? new List<PurchaseOrder>();

            _allPendingPOCount = rawList.Count(p => !p.IsReceived);
            _allReceivedPOCount = rawList.Count(p => p.IsReceived);

            IEnumerable<PurchaseOrder> filtered = rawList;
            if (SelectedStockInStatus == "รอดำเนินการ")
            {
                filtered = filtered.Where(p => !p.IsReceived);
            }
            else if (SelectedStockInStatus == "ตรวจรับแล้ว")
            {
                filtered = filtered.Where(p => p.IsReceived);
            }

            PendingPurchaseOrders.Clear();
            foreach (var p in filtered)
            {
                PendingPurchaseOrders.Add(p);
            }
            OnPropertyChanged(nameof(PendingPOCount));
            OnPropertyChanged(nameof(StockInSummaryText));
            OnPropertyChanged(nameof(DisplaySummaryCountText));
            _ = LoadStockTransactions();
        }

        private void OpenGoodsReceiptModal(PurchaseOrder? po)
        {
            if (po == null && SelectedReceiptPO != null && SelectedReceiptPO.Id > 0)
            {
                po = SelectedReceiptPO;
            }
            if (po == null && SelectedPurchaseOrder != null && SelectedPurchaseOrder.Id > 0)
            {
                po = SelectedPurchaseOrder;
            }
            if (po == null)
            {
                ShowAlert("กรุณาเลือกรายการใบสั่งซื้อที่ต้องการตรวจรับ", "แจ้งเตือน", "⚠️");
                return;
            }

            SelectedReceiptPO = po;
            IsReceiptViewOnly = po.IsReceived;
            ReceiptDeliveryNoteNo = string.Empty;
            ReceiptItems.Clear();

            if (po.IsReceived)
            {
                try
                {
                    using (var conn = _databaseService.GetConnection())
                    {
                        var note = conn.ExecuteScalar<string>("SELECT Note FROM tblStockIn_H WHERE PO_ID = @PO_ID ORDER BY StockIn_ID DESC LIMIT 1;", new { PO_ID = po.Id });
                        if (!string.IsNullOrWhiteSpace(note))
                        {
                            ReceiptDeliveryNoteNo = note;
                        }
                    }
                }
                catch { }
            }

            var poItems = _databaseService.GetPurchaseOrderItems(po.Id);
            foreach (var item in poItems)
            {
                ReceiptItems.Add(new GoodsReceiptItem
                {
                    Detail_ID = item.Detail_ID,
                    PO_ID = item.PO_ID,
                    Pro_ID = item.Pro_ID,
                    ProductCode = item.ProductCode,
                    ProductName = item.ProductName,
                    OrderedQty = item.Quantity,
                    CostPrice = item.CostPrice,
                    ReceivedQty = item.Quantity,
                    ItemChanged = CalculateReceiptTotal
                });
            }

            CalculateReceiptTotal();
            OnPropertyChanged(nameof(IsReceiptDeliveryNoteValid));
            OnPropertyChanged(nameof(IsReceiptDeliveryNoteInvalid));
            OnPropertyChanged(nameof(CanConfirmGoodsReceipt));
            CommandManager.InvalidateRequerySuggested();
            IsGoodsReceiptModalOpen = true;
        }

        private void CalculateReceiptTotal()
        {
            ReceiptTotalAmount = ReceiptItems.Sum(i => i.ReceivedQty * i.CostPrice);
            OnPropertyChanged(nameof(IsReceiptDeliveryNoteValid));
            OnPropertyChanged(nameof(IsReceiptDeliveryNoteInvalid));
            OnPropertyChanged(nameof(CanConfirmGoodsReceipt));
            CommandManager.InvalidateRequerySuggested();
        }

        private void ConfirmGoodsReceipt()
        {
            if (RolePermissions.IsReadOnly(CurrentUser?.Role, "stock_in"))
            {
                ShowAlert("ไม่มีสิทธิ์ในการดำเนินการนี้ (สิทธิ์ดูอย่างเดียว)", "ไม่มีสิทธิ์", "⚠️");
                return;
            }

            if (SelectedReceiptPO == null || SelectedReceiptPO.Id == 0)
            {
                ShowAlert("ไม่พบข้อมูลใบสั่งซื้อ", "ข้อผิดพลาด", "❌");
                return;
            }

            if (SelectedReceiptPO.IsReceived)
            {
                ShowAlert("ใบสั่งซื้อนี้ทำการตรวจรับสินค้าเข้าคลังเรียบร้อยแล้ว ไม่สามารถตรวจรับซ้ำได้", "แจ้งเตือน", "ℹ️");
                return;
            }

            // Guard clause: Validate Invoice / Delivery Note Number (Must not be empty)
            if (string.IsNullOrWhiteSpace(ReceiptDeliveryNoteNo))
            {
                ShowAlert("กรุณากรอกเลขที่ใบส่งของ / ใบกำกับภาษี (ห้ามเว้นว่าง)", "แจ้งเตือน", "⚠️");
                return;
            }

            if (ReceiptItems.Count == 0 || !ReceiptItems.Any(i => i.ReceivedQty > 0))
            {
                ShowAlert("กรุณาระบุจำนวนรับจริงอย่างน้อย 1 รายการ", "แจ้งเตือน", "⚠️");
                return;
            }

            try
            {
                string formattedInvoiceNo = ReceiptDeliveryNoteNo.Trim().ToUpper();
                _databaseService.ProcessGoodsReceipt(
                    SelectedReceiptPO.Id,
                    formattedInvoiceNo,
                    ReceiptItems.ToList(),
                    CurrentUser?.Id ?? 1
                );

                ShowAlert($"ตรวจรับสินค้าสำหรับใบสั่งซื้อ {SelectedReceiptPO.PONumber} และอัปเดตสต็อกเรียบร้อยแล้ว", "สำเร็จ", "🎉");
                IsGoodsReceiptModalOpen = false;

                _ = LoadPendingPurchaseOrders();
                _ = LoadPurchaseOrders();
                _ = LoadStockTransactions();
                _ = LoadProducts();
            }
            catch (Exception ex)
            {
                ShowAlert($"เกิดข้อผิดพลาดในการบันทึกรับสินค้า: {ex.Message}", "ข้อผิดพลาด", "❌");
            }
        }

        private void SavePurchaseOrder()
        {
            var po = new PurchaseOrder
            {
                Id = SelectedPurchaseOrder?.Id ?? 0,
                PONumber = string.IsNullOrWhiteSpace(PONumber) ? "PO" + DateTime.Now.ToString("yyyyMMddHHmmss") : PONumber,
                SupplierName = POSupplierName,
                OrderDate = SelectedPurchaseOrder?.OrderDate ?? DateTime.Now,
                ExpectedDate = POExpectedDate,
                TotalAmount = POTotalAmount,
                Status = (SelectedPurchaseOrder?.Id ?? 0) == 0 ? "รอดำเนินการ" : (string.IsNullOrWhiteSpace(POStatus) ? "รอดำเนินการ" : POStatus)
            };

            var items = CurrentPOItems.ToList();
            
            if (po.Id == 0)
                _databaseService.SavePurchaseOrder(po, items);
            else
                _databaseService.UpdatePurchaseOrder(po, items);

            ShowAlert("บันทึกข้อมูลการสั่งซื้อสำเร็จ", "สำเร็จ", "🎉");
            _ = LoadPurchaseOrders();
            ClearPurchaseOrderForm();
            IsPOModalOpen = false;
        }

        private void DeletePurchaseOrder()
        {
            if (SelectedPurchaseOrder == null || SelectedPurchaseOrder.Id == 0) return;

            ShowConfirm($"ยืนยันการลบใบสั่งซื้อ {SelectedPurchaseOrder.PONumber}?", () =>
            {
                _databaseService.DeletePurchaseOrder(SelectedPurchaseOrder.Id);
                _ = LoadPurchaseOrders();
                ClearPurchaseOrderForm();
            }, "ยืนยันการลบ");
        }

        private void ClearPurchaseOrderForm()
        {
            SelectedPurchaseOrder = new PurchaseOrder();
            PONumber = string.Empty;
            SelectedSupplier = null;
            POSupplierId = null;
            POSupplierName = string.Empty;
            POExpectedDate = DateTime.Now.AddDays(3);
            POTotalAmount = 0;
            POStatus = "รอดำเนินการ";
            IsNewPOMode = true;
            IsPOReadOnly = false;
            CurrentPOItems.Clear();
            ClearPOItemForm();
        }

        private void AddPOItem()
        {
            if (string.IsNullOrWhiteSpace(POItemProductCode) && !string.IsNullOrWhiteSpace(POItemProductName))
            {
                var matched = _allProducts?.FirstOrDefault(p => p.Name != null && p.Name.Equals(POItemProductName, StringComparison.OrdinalIgnoreCase));
                if (matched != null)
                {
                    POItemProductCode = matched.Code;
                }
                else
                {
                    POItemProductCode = "-";
                }
            }

            var existingItem = CurrentPOItems.FirstOrDefault(i => i.ProductName == POItemProductName && (i.ProductCode == POItemProductCode || string.IsNullOrEmpty(POItemProductCode)));
            if (existingItem != null)
            {
                existingItem.Quantity += POItemQuantity;
                if (POItemCost > 0) existingItem.CostPrice = POItemCost;
                CalculatePOTotal();
            }
            else
            {
                CurrentPOItems.Add(new PurchaseOrderItem
                {
                    ProductCode = POItemProductCode,
                    ProductName = POItemProductName,
                    Quantity = POItemQuantity,
                    CostPrice = POItemCost,
                    ItemChanged = CalculatePOTotal
                });
            }
            CalculatePOTotal();
            ClearPOItemForm();
            IsItemModalOpen = false;
            OnPropertyChanged("POItemAdded");
        }

        private void RemovePOItem(PurchaseOrderItem item)
        {
            if (item != null)
            {
                ShowConfirm($"ยืนยันการลบรายการ {item.ProductName}?", () =>
                {
                    CurrentPOItems.Remove(item);
                    CalculatePOTotal();
                }, "ยืนยันการลบ");
            }
        }
        
        private void ClearPOItemForm()
        {
            _isSelectingPOSuggestion = true;
            try
            {
                POItemProductCode = string.Empty;
                POItemProductName = string.Empty;
                SelectedPOProduct = null;
                POItemQuantity = 1;
                POItemCost = 0;
            }
            finally
            {
                _isSelectingPOSuggestion = false;
            }
            _poSuggestedProducts.Clear();
            IsPOSuggestionsOpen = false;
        }

        private void UpdatePOSuggestions(string searchTerm, bool isBarcode)
        {
            if (_isSelectingPOSuggestion) return;

            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Trim().Length < 1)
            {
                _poSuggestedProducts.Clear();
                IsPOSuggestionsOpen = false;
                return;
            }

            if (_allProducts == null || !_allProducts.Any())
            {
                _ = LoadProducts();
            }

            string term = searchTerm.Trim();
            var sourceList = (_allProducts != null && _allProducts.Any()) ? _allProducts : (Products?.ToList() ?? new List<Product>());

            if (!sourceList.Any())
            {
                IsPOSuggestionsOpen = false;
                return;
            }

            IEnumerable<Product> matches;
            if (isBarcode)
            {
                matches = sourceList.Where(p =>
                    (!string.IsNullOrEmpty(p.Code) && p.Code.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.Name) && p.Name.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.Pro_Barcode) && p.Pro_Barcode.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Pro_ID > 0 && p.Pro_ID.ToString().Contains(term))
                ).Take(10);
            }
            else
            {
                matches = sourceList.Where(p =>
                    (!string.IsNullOrEmpty(p.Name) && p.Name.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.Code) && p.Code.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.Pro_Barcode) && p.Pro_Barcode.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Pro_ID > 0 && p.Pro_ID.ToString().Contains(term))
                ).Take(10);
            }

            _poSuggestedProducts.Clear();
            foreach (var p in matches)
            {
                _poSuggestedProducts.Add(p);
            }

            IsPOSuggestionsOpen = _poSuggestedProducts.Count > 0;
        }

        private void SelectPOSuggestion(Product product)
        {
            if (product == null) return;

            _isSelectingPOSuggestion = true;
            try
            {
                POItemProductCode = product.Code;
                POItemProductName = product.Name;
                POItemCost = product.Cost > 0 ? product.Cost : product.Price;
                if (POItemQuantity <= 0) POItemQuantity = 1;
            }
            finally
            {
                _isSelectingPOSuggestion = false;
            }

            _poSuggestedProducts.Clear();
            IsPOSuggestionsOpen = false;
        }

        private void CalculatePOTotal()
        {
            POTotalAmount = CurrentPOItems.Sum(i => i.Quantity * i.CostPrice);
        }

        private void AddLowStockToPO(Product product)
        {
            if (product != null)
            {
                POItemProductCode = product.Code;
                POItemProductName = product.Name;
                POItemCost = product.Cost;
                POItemQuantity = product.ReorderPoint > product.Stock ? (product.ReorderPoint - product.Stock) + 5 : 5; // Suggested quantity
                IsItemModalOpen = true;
            }
        }

        private void PrintPurchaseOrder()
        {
            if (SelectedPurchaseOrder != null && SelectedPurchaseOrder.Id > 0)
            {
                var receiptWindow = new POReceiptWindow(SelectedPurchaseOrder);
                receiptWindow.ShowDialog();
            }
            else
            {
                ShowAlert("กรุณาเลือกหรือบันทึกใบสั่งซื้อก่อนพิมพ์", "แจ้งเตือน", "⚠️");
            }
        }

        // Expense Management Logic handled in ExpenseViewModel.cs

        private void BackupDatabase()
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Database files (*.db)|*.db",
                FileName = $"matcha_backup_{System.DateTime.Now:yyyyMMdd_HHmmss}.db",
                Title = "สำรองข้อมูลระบบ (Backup Database)"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string sourceDbPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "matcha.db");
                    if (System.IO.File.Exists(sourceDbPath))
                    {
                        System.IO.File.Copy(sourceDbPath, saveFileDialog.FileName, true);
                        ShowAlert("สำรองข้อมูลสำเร็จ!", "สำเร็จ", "🎉");
                    }
                }
                catch (System.Exception ex)
                {
                    ShowAlert($"เกิดข้อผิดพลาดในการสำรองข้อมูล: {ex.Message}", "ข้อผิดพลาด", "❌");
                }
            }
        }

        private void RestoreDatabase()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Database files (*.db)|*.db",
                Title = "เลือกไฟล์สำรองข้อมูล (Restore Database)"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ShowConfirm(
                    "คำเตือน: การกู้คืนข้อมูลจะลบข้อมูลปัจจุบันทั้งหมดและแทนที่ด้วยข้อมูลจากไฟล์สำรอง\nคุณแน่ใจหรือไม่ว่าต้องการดำเนินการต่อ?",
                    () =>
                    {
                        try
                        {
                            string targetDbPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "matcha.db");
                            
                            // Close any existing connection pool so the DB file is free to be overwritten
                            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                            
                            System.GC.Collect();
                            System.GC.WaitForPendingFinalizers();

                            System.IO.File.Copy(openFileDialog.FileName, targetDbPath, true);
                            ShowAlert("กู้คืนข้อมูลสำเร็จ! กรุณาเปิดโปรแกรมใหม่อีกครั้งเพื่อความสมบูรณ์", "สำเร็จ", "🎉");
                            
                            // Force a shutdown to ensure no db queries fire after restoring half-way
                            Application.Current.Shutdown();
                        }
                        catch (System.Exception ex)
                        {
                            ShowAlert($"เกิดข้อผิดพลาดในการกู้คืนข้อมูล: {ex.Message}", "ข้อผิดพลาด", "❌");
                        }
                    },
                    "ยืนยันการกู้คืนข้อมูล");
            }
        }
    }
}
