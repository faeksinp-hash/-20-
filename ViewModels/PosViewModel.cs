using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dapper;
using Porjai20.Models;
using Porjai20.Services;
using Porjai20.Views;

namespace Porjai20.ViewModels
{
    public class PosViewModel : ViewModelBase
    {
        private readonly DatabaseService _databaseService;
        private List<Product> _allProducts = new List<Product>();

        // Catalog Properties
        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();
        public ObservableCollection<Product> FilteredProducts => Products;

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterProducts();
                }
            }
        }

        public string SearchQuery
        {
            get => SearchText;
            set => SearchText = value;
        }

        private string _selectedCategory = "ทั้งหมด";
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    FilterProducts();
                }
            }
        }

        // Cart Properties
        public ObservableCollection<CartItem> CartItems { get; } = new ObservableCollection<CartItem>();

        public int CartTotalItems => CartItems.Sum(item => item.Quantity);
        public string CartTotalString => TotalAmount.ToString("N2");

        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                if (SetProperty(ref _totalAmount, value))
                {
                    OnPropertyChanged(nameof(CartTotalString));
                    OnPropertyChanged(nameof(CartTotal));
                    OnPropertyChanged(nameof(CheckoutGrandTotal));
                    OnPropertyChanged(nameof(ChangeAmount));
                }
            }
        }

        // Error Modal Overlay Properties
        private bool _isErrorModalOpen;
        public bool IsErrorModalOpen
        {
            get => _isErrorModalOpen;
            set => SetProperty(ref _isErrorModalOpen, value);
        }

        private string _errorModalMessage = string.Empty;
        public string ErrorModalMessage
        {
            get => _errorModalMessage;
            set => SetProperty(ref _errorModalMessage, value);
        }

        public ICommand CloseErrorModalCommand { get; }

        // Checkout State Machine Setup
        private bool _isCheckoutModalVisible;
        public bool IsCheckoutModalVisible
        {
            get => _isCheckoutModalVisible;
            set 
            { 
                if (SetProperty(ref _isCheckoutModalVisible, value))
                {
                    OnPropertyChanged(nameof(IsCheckoutWizardOpen));
                }
            }
        }
        public bool IsCheckoutWizardOpen
        {
            get => IsCheckoutModalVisible;
            set => IsCheckoutModalVisible = value;
        }

        private int _currentStep = 1;
        public int CurrentStep
        {
            get => _currentStep;
            set => SetProperty(ref _currentStep, value);
        }

        private string _selectedShippingMethod = "Pickup"; // "Pickup" or "Delivery"
        public string SelectedShippingMethod
        {
            get => _selectedShippingMethod;
            set
            {
                if (SetProperty(ref _selectedShippingMethod, value))
                {
                    OnPropertyChanged(nameof(ShippingFee));
                    OnPropertyChanged(nameof(CartTotal));
                    OnPropertyChanged(nameof(CheckoutGrandTotal));
                    OnPropertyChanged(nameof(ChangeAmount));
                    OnPropertyChanged(nameof(IsPickupSelected));
                    OnPropertyChanged(nameof(IsDeliverySelected));
                }
            }
        }

        public bool IsPickupSelected
        {
            get => SelectedShippingMethod == "Pickup";
            set { if (value) SelectedShippingMethod = "Pickup"; }
        }

        public bool IsDeliverySelected
        {
            get => SelectedShippingMethod == "Delivery";
            set 
            { 
                if (value) 
                {
                    if (SelectedShippingMethod != "Delivery")
                    {
                        SelectedShippingMethod = "Delivery";
                        if (SelectedCustomer != null)
                        {
                            if (string.IsNullOrWhiteSpace(CustomerName)) CustomerName = SelectedCustomer.Name ?? "";
                            if (string.IsNullOrWhiteSpace(CustomerPhone)) CustomerPhone = SelectedCustomer.Phone ?? "";
                            if (string.IsNullOrWhiteSpace(CustomerAddress)) CustomerAddress = SelectedCustomer.Address ?? "";
                        }
                        OpenDeliveryModal();
                    }
                }
                else 
                {
                    if (SelectedShippingMethod != "Pickup")
                    {
                        SelectedShippingMethod = "Pickup";
                    }
                    IsDeliveryModalOpen = false;
                    // Reset delivery-linked customer state
                    _selectedDeliveryCustomer = null;
                    CustomerName = "";
                    CustomerPhone = "";
                    CustomerAddress = "";
                    SelectedCustomer = null;
                    IsMemberSelected = false;
                    IsManualMemberSearchMode = false;
                    CustomerSearchText = "";
                    MemberSearchResultText = "";
                    OnPropertyChanged(nameof(SelectedDeliveryCustomer));
                    OnPropertyChanged(nameof(MemberSubtitleText));
                }
            }
        }

        // ══════════════════════════════════════════════════════════
        // Delivery Modal & Customer Address Management Properties
        // ══════════════════════════════════════════════════════════
        private bool _isDeliveryModalOpen = false;
        public bool IsDeliveryModalOpen
        {
            get => _isDeliveryModalOpen;
            set => SetProperty(ref _isDeliveryModalOpen, value);
        }

        private bool _isAddNewAddressMode;
        public bool IsAddNewAddressMode
        {
            get => _isAddNewAddressMode;
            set => SetProperty(ref _isAddNewAddressMode, value);
        }

        private Customer? _selectedDeliveryCustomer;
        public Customer? SelectedDeliveryCustomer
        {
            get => _selectedDeliveryCustomer;
            set
            {
                if (SetProperty(ref _selectedDeliveryCustomer, value))
                {
                    if (value != null)
                    {
                        CustomerName = value.Name ?? "";
                        CustomerPhone = value.Phone ?? "";
                        CustomerAddress = value.Address ?? "";
                        SelectedCustomer = value;
                        CustomerSearchText = value.Phone ?? "";
                        MemberPhone = value.Phone ?? "";
                        IsMemberSelected = true;
                        IsManualMemberSearchMode = false;
                        OnPropertyChanged(nameof(MemberSubtitleText));
                    }
                    OnPropertyChanged(nameof(DeliveryCustomerName));
                    OnPropertyChanged(nameof(DeliveryCustomerPhone));
                    OnPropertyChanged(nameof(DeliveryCustomerAddress));
                }
            }
        }

        private string _newDeliveryName = "";
        public string NewDeliveryName
        {
            get => _newDeliveryName;
            set => SetProperty(ref _newDeliveryName, value);
        }

        private string _newDeliveryPhone = "";
        public string NewDeliveryPhone
        {
            get => _newDeliveryPhone;
            set => SetProperty(ref _newDeliveryPhone, value);
        }

        private string _newDeliveryAddress = "";
        public string NewDeliveryAddress
        {
            get => _newDeliveryAddress;
            set => SetProperty(ref _newDeliveryAddress, value);
        }

        private decimal _customShippingFee = 50;
        public decimal CustomShippingFee
        {
            get => _customShippingFee;
            set
            {
                if (SetProperty(ref _customShippingFee, value))
                {
                    OnPropertyChanged(nameof(ShippingFee));
                    OnPropertyChanged(nameof(CartTotal));
                    OnPropertyChanged(nameof(CheckoutGrandTotal));
                    OnPropertyChanged(nameof(ChangeAmount));
                }
            }
        }

        public ICommand OpenDeliveryModalCommand { get; set; }
        public ICommand CloseDeliveryModalCommand { get; set; }
        public ICommand CancelDeliveryModalCommand { get; set; }
        public ICommand SaveDeliveryDetailsCommand { get; set; }
        public ICommand ShowAddNewAddressFormCommand { get; set; }
        public ICommand SaveNewCustomerAddressCommand { get; set; }
        public ICommand CancelAddNewAddressCommand { get; set; }

        private string? _selectedPaymentMethod = null; // null = Default, "เงินสด", "สแกน QR / โอนเงิน", "บัตรเครดิต / เดบิต"
        public string? SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                if (SetProperty(ref _selectedPaymentMethod, value))
                {
                    OnPropertyChanged(nameof(SelectedPaymentMethodDisplay));
                    OnPropertyChanged(nameof(IsCashSelected));
                    OnPropertyChanged(nameof(IsCashPayment));
                    OnPropertyChanged(nameof(IsTransferSelected));
                    OnPropertyChanged(nameof(IsTransferPayment));
                    OnPropertyChanged(nameof(IsCardPayment));
                    OnPropertyChanged(nameof(SubmitButtonText));
                }
            }
        }

        private string _selectedBank = "KBANK";
        public string SelectedBank
        {
            get => _selectedBank;
            set
            {
                if (SetProperty(ref _selectedBank, value))
                {
                    OnPropertyChanged(nameof(SelectedBankName));
                }
            }
        }

        public string SelectedBankName => SelectedBank switch
        {
            "KBANK" => "กสิกรไทย (KBANK)",
            "GSB" => "ออมสิน (GSB)",
            "SCB" => "ไทยพาณิชย์ (SCB)",
            "BBL" => "กรุงเทพ (BBL)",
            "KTB" => "กรุงไทย (KTB)",
            _ => SelectedBank
        };

        public ICommand SelectBankCommand { get; set; }

        public string SelectedPaymentMethodDisplay
        {
            get => SelectedPaymentMethod ?? "เงินสด";
        }

        public bool IsCashSelected
        {
            get => SelectedPaymentMethod == "Cash" || SelectedPaymentMethod == "เงินสด";
            set
            {
                if (value)
                {
                    if (SelectedPaymentMethod == "เงินสด") SelectedPaymentMethod = null;
                    else SelectedPaymentMethod = "เงินสด";
                }
                else if (SelectedPaymentMethod == "เงินสด")
                {
                    SelectedPaymentMethod = null;
                }
            }
        }

        public bool IsCashPayment
        {
            get => IsCashSelected || string.IsNullOrEmpty(SelectedPaymentMethod);
            set => IsCashSelected = value;
        }

        public bool IsTransferSelected
        {
            get => SelectedPaymentMethod == "Transfer" || SelectedPaymentMethod == "โอนเงิน" || SelectedPaymentMethod == "สแกน QR / โอนเงิน";
            set
            {
                if (value)
                {
                    if (SelectedPaymentMethod == "สแกน QR / โอนเงิน") SelectedPaymentMethod = null;
                    else SelectedPaymentMethod = "สแกน QR / โอนเงิน";
                }
                else if (SelectedPaymentMethod == "สแกน QR / โอนเงิน")
                {
                    SelectedPaymentMethod = null;
                }
            }
        }

        public bool IsTransferPayment
        {
            get => IsTransferSelected;
            set => IsTransferSelected = value;
        }

        public bool IsCardPayment
        {
            get => SelectedPaymentMethod == "Card" || SelectedPaymentMethod == "บัตรเครดิต" || SelectedPaymentMethod == "บัตรเครดิต / เดบิต";
            set
            {
                if (value)
                {
                    if (SelectedPaymentMethod == "บัตรเครดิต / เดบิต") SelectedPaymentMethod = null;
                    else SelectedPaymentMethod = "บัตรเครดิต / เดบิต";
                }
                else if (SelectedPaymentMethod == "บัตรเครดิต / เดบิต")
                {
                    SelectedPaymentMethod = null;
                }
            }
        }

        public string SubmitButtonText
        {
            get
            {
                if (string.IsNullOrEmpty(SelectedPaymentMethod))
                {
                    return "✓ ยืนยันการชำระเงิน";
                }
                return $"✓ ยืนยันการชำระเงิน ({SelectedPaymentMethod})";
            }
        }

        private List<Customer> _allCustomers = new List<Customer>();
        // Member Lookup & AutoComplete Properties
        public ObservableCollection<Customer> CustomerList { get; } = new ObservableCollection<Customer>();
        public ObservableCollection<Customer> FilteredCustomerList => CustomerList;
        public ObservableCollection<Customer> FilteredCustomers => CustomerList;
        public ObservableCollection<Customer> CustomerSearchResults => CustomerList;

        private bool _isMemberSelected;
        public bool IsMemberSelected
        {
            get => _isMemberSelected;
            set
            {
                if (SetProperty(ref _isMemberSelected, value))
                {
                    OnPropertyChanged(nameof(IsMemberInputVisible));
                    OnPropertyChanged(nameof(IsLinkedMemberCardVisible));
                    OnPropertyChanged(nameof(IsMemberSearchBoxVisible));
                    OnPropertyChanged(nameof(MemberSubtitleText));
                    OnPropertyChanged(nameof(HasMember));
                    if (!value)
                    {
                        ClearSelectedCustomer();
                        IsManualMemberSearchMode = false;
                    }
                    else
                    {
                        // If delivery customer is already selected, link it automatically
                        if (SelectedCustomer == null && IsDeliverySelected && SelectedDeliveryCustomer != null)
                        {
                            SelectedCustomer = SelectedDeliveryCustomer;
                            IsManualMemberSearchMode = false;
                        }
                    }
                }
            }
        }

        public bool IsMemberInputVisible => IsMemberSelected;

        private bool _isManualMemberSearchMode = false;
        public bool IsManualMemberSearchMode
        {
            get => _isManualMemberSearchMode;
            set
            {
                if (SetProperty(ref _isManualMemberSearchMode, value))
                {
                    OnPropertyChanged(nameof(IsLinkedMemberCardVisible));
                    OnPropertyChanged(nameof(IsMemberSearchBoxVisible));
                }
            }
        }

        public bool IsLinkedMemberCardVisible => IsMemberSelected && HasSelectedCustomer && !IsManualMemberSearchMode;
        public bool IsMemberSearchBoxVisible => IsMemberSelected && (!HasSelectedCustomer || IsManualMemberSearchMode);

        public bool IsMemberConfirmed => HasSelectedCustomer && !IsManualMemberSearchMode;
        public bool IsSearchingMember => IsManualMemberSearchMode || !HasSelectedCustomer;
        public bool IsMemberEditing => IsManualMemberSearchMode;

        public string LinkedMemberDisplayText => SelectedCustomer != null
            ? $"{SelectedCustomer.Name}"
            : "";

        public bool HasMember => HasSelectedCustomer;

        public string SearchPhoneNumber
        {
            get => CustomerSearchText;
            set => CustomerSearchText = value;
        }

        public string MemberSearchText
        {
            get => CustomerSearchText;
            set => CustomerSearchText = value;
        }

        public string MemberSubtitleText
        {
            get
            {
                if (SelectedCustomer != null)
                {
                    string phoneStr = !string.IsNullOrWhiteSpace(SelectedCustomer.Phone) ? SelectedCustomer.Phone : CustomerPhone;
                    string nameStr = !string.IsNullOrWhiteSpace(SelectedCustomer.Name) ? SelectedCustomer.Name : CustomerName;
                    return $"{phoneStr} ({nameStr}) • แต้มสะสม: {SelectedCustomer.Points:N0} แต้ม";
                }
                if (!string.IsNullOrWhiteSpace(CustomerPhone))
                {
                    string nameStr = !string.IsNullOrWhiteSpace(CustomerName) ? CustomerName : "ลูกค้า";
                    return $"{CustomerPhone} ({nameStr})";
                }
                return "ค้นหาด้วยเบอร์โทรศัพท์เพื่อสะสมแต้ม";
            }
        }

        private bool _isCustomerSearchOpen;
        public bool IsCustomerSearchOpen
        {
            get => _isCustomerSearchOpen;
            set => SetProperty(ref _isCustomerSearchOpen, value);
        }

        private bool _isCustomerDropDownOpen;
        public bool IsCustomerDropDownOpen
        {
            get => _isCustomerDropDownOpen;
            set => SetProperty(ref _isCustomerDropDownOpen, value);
        }

        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    OnPropertyChanged(nameof(SelectedCustomerItem));
                    OnPropertyChanged(nameof(EarnedPoints));
                    OnPropertyChanged(nameof(HasSelectedCustomer));
                    OnPropertyChanged(nameof(HasMember));
                    OnPropertyChanged(nameof(MemberSubtitleText));
                    OnPropertyChanged(nameof(LinkedMemberDisplayText));
                    OnPropertyChanged(nameof(IsLinkedMemberCardVisible));
                    OnPropertyChanged(nameof(IsMemberSearchBoxVisible));
                    if (value != null)
                    {
                        CustomerName = value.Name;
                        CustomerPhone = value.Phone;
                        CustomerAddress = value.Address;
                        _customerSearchText = value.Phone;
                        OnPropertyChanged(nameof(CustomerSearchText));
                        OnPropertyChanged(nameof(MemberPhone));
                        OnPropertyChanged(nameof(SearchPhoneNumber));
                        OnPropertyChanged(nameof(MemberSearchText));
                        MemberSearchResultText = $"👤 คุณ {value.Name} (สะสมเดิม: {value.Points} แต้ม)";
                        IsCustomerDropDownOpen = false;
                        _isManualMemberSearchMode = false;
                        OnPropertyChanged(nameof(IsManualMemberSearchMode));
                        OnPropertyChanged(nameof(IsLinkedMemberCardVisible));
                        OnPropertyChanged(nameof(IsMemberSearchBoxVisible));
                    }
                }
            }
        }

        public bool HasSelectedCustomer => SelectedCustomer != null;

        public int EarnedPoints
        {
            get
            {
                if (SelectedCustomer == null) return 0;
                // Every 100 Baht = 1 Point (Floor)
                return (int)Math.Floor(CheckoutGrandTotal / 100m);
            }
        }

        public Customer? SelectedCustomerItem
        {
            get => SelectedCustomer;
            set => SelectedCustomer = value;
        }

        private string _customerSearchText = "";
        public string CustomerSearchText
        {
            get => _customerSearchText;
            set
            {
                if (SetProperty(ref _customerSearchText, value))
                {
                    if (!string.IsNullOrWhiteSpace(value) && !IsMemberSelected)
                    {
                        IsMemberSelected = true;
                    }
                    OnPropertyChanged(nameof(MemberPhone));
                    OnPropertyChanged(nameof(SearchPhoneNumber));
                    OnPropertyChanged(nameof(MemberSearchText));
                    FilterCustomers(value);
                }
            }
        }

        public string MemberPhone
        {
            get => CustomerSearchText;
            set => CustomerSearchText = value;
        }

        private string _memberSearchResultText = "";
        public string MemberSearchResultText
        {
            get => _memberSearchResultText;
            set => SetProperty(ref _memberSearchResultText, value);
        }

        private decimal _cashAmountReceived;
        public decimal CashAmountReceived
        {
            get => _cashAmountReceived;
            set
            {
                if (SetProperty(ref _cashAmountReceived, value))
                {
                    OnPropertyChanged(nameof(ChangeAmount));
                    OnPropertyChanged(nameof(CashReceived));
                    OnPropertyChanged(nameof(Change));
                }
            }
        }

        public decimal CashReceived => CashAmountReceived;
        public decimal Change => ChangeAmount;

        private string _numpadInput = "0";
        private bool _isNewInput = true;
        public string NumpadInputString
        {
            get => decimal.TryParse(_numpadInput, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal val) ? val.ToString("N0") : "0";
        }

        public decimal ShippingFee => SelectedShippingMethod == "Delivery" ? CustomShippingFee : 0;
        public decimal CartTotal => TotalAmount + ShippingFee;
        public decimal CheckoutGrandTotal => CartTotal;

        public decimal ChangeAmount => Math.Max(0, CashAmountReceived - CartTotal);

        private string _customerName = "";
        public string CustomerName
        {
            get => _customerName;
            set
            {
                if (SetProperty(ref _customerName, value))
                {
                    OnPropertyChanged(nameof(DeliveryCustomerName));
                }
            }
        }
        public string DeliveryCustomerName 
        { 
            get => CustomerName; 
            set => CustomerName = value; 
        }

        private string _customerPhone = "";
        public string CustomerPhone
        {
            get => _customerPhone;
            set
            {
                if (SetProperty(ref _customerPhone, value))
                {
                    OnPropertyChanged(nameof(DeliveryCustomerPhone));
                }
            }
        }
        public string DeliveryCustomerPhone 
        { 
            get => CustomerPhone; 
            set => CustomerPhone = value; 
        }

        private string _customerAddress = "";
        public string CustomerAddress
        {
            get => _customerAddress;
            set
            {
                if (SetProperty(ref _customerAddress, value))
                {
                    OnPropertyChanged(nameof(DeliveryCustomerAddress));
                }
            }
        }
        public string DeliveryCustomerAddress 
        { 
            get => CustomerAddress; 
            set => CustomerAddress = value; 
        }

        private string _customerNote = "";
        public string CustomerNote
        {
            get => _customerNote;
            set
            {
                if (SetProperty(ref _customerNote, value))
                {
                    OnPropertyChanged(nameof(DeliveryNote));
                }
            }
        }
        public string DeliveryNote { get => CustomerNote; set => CustomerNote = value; }

        private string _lastRefNo = "";
        public string LastRefNo
        {
            get => _lastRefNo;
            set => SetProperty(ref _lastRefNo, value);
        }

        private decimal _finalCashReceived;
        public decimal FinalCashReceived
        {
            get => _finalCashReceived;
            set => SetProperty(ref _finalCashReceived, value);
        }

        private decimal _finalChange;
        public decimal FinalChange
        {
            get => _finalChange;
            set => SetProperty(ref _finalChange, value);
        }

        private decimal _finalGrandTotal;
        public decimal FinalGrandTotal
        {
            get => _finalGrandTotal;
            set => SetProperty(ref _finalGrandTotal, value);
        }

        private string _finalPaymentMethod = "เงินสด";
        public string FinalPaymentMethod
        {
            get => _finalPaymentMethod;
            set => SetProperty(ref _finalPaymentMethod, value);
        }

        private string _finalBank = "";
        public string FinalBank
        {
            get => _finalBank;
            set => SetProperty(ref _finalBank, value);
        }

        public bool HasFinalBank => !string.IsNullOrEmpty(FinalBank);

        // Commands
        public ICommand FilterCategoryCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand LoadProductsCommand { get; }

        // Member Commands
        public ICommand SearchCustomerCommand { get; }
        public ICommand SearchMemberCommand { get; }
        public ICommand ToggleMemberCommand { get; }
        public ICommand SelectCustomerCommand { get; }
        public ICommand ClearSelectedCustomerCommand { get; }
        public ICommand ToggleCustomerDropDownCommand { get; }
        public ICommand ChangeMemberCommand { get; }
        public ICommand CancelChangeMemberCommand { get; }

        // Checkout Wizard Commands
        public ICommand CheckoutCommand { get; } // Triggers the modal
        public ICommand NumpadCommand { get; }
        public ICommand NextStepCommand { get; }
        public ICommand PrevStepCommand { get; }
        public ICommand ConfirmOrderCommand { get; }
        public ICommand CloseCheckoutCommand { get; }
        public ICommand PrintReceiptCommand { get; }

        public PosViewModel()
        {
            _databaseService = new DatabaseService();

            // Catalog Commands
            FilterCategoryCommand = new RelayCommand(param => { SelectedCategory = param?.ToString() ?? "ทั้งหมด"; });
            AddToCartCommand = new RelayCommand(param => { if (param is Product p) AddToCart(p); });
            DecreaseQuantityCommand = new RelayCommand(param => { if (param is CartItem item) DecreaseQuantity(item); });
            IncreaseQuantityCommand = new RelayCommand(param => { if (param is CartItem item) IncreaseQuantity(item); });
            ClearCartCommand = new RelayCommand(_ => ClearCart());
            LoadProductsCommand = new RelayCommand(async _ => await LoadProducts());

            // Member Commands
            ToggleMemberCommand = new RelayCommand(_ => { IsMemberSelected = !IsMemberSelected; });
            SearchCustomerCommand = new RelayCommand(async _ => await ExecuteSearchCustomer());
            SearchMemberCommand = SearchCustomerCommand;
            SelectCustomerCommand = new RelayCommand(param => { 
                if (param is Customer c) 
                {
                    SelectedCustomer = c;
                    IsManualMemberSearchMode = false;
                }
            });
            ClearSelectedCustomerCommand = new RelayCommand(_ => {
                ClearSelectedCustomer();
                IsMemberSelected = false;
                IsManualMemberSearchMode = false;
                CustomerSearchText = "";
                MemberPhone = "";
                MemberSearchResultText = "";
                OnPropertyChanged(nameof(MemberSubtitleText));
                OnPropertyChanged(nameof(IsLinkedMemberCardVisible));
                OnPropertyChanged(nameof(IsMemberSearchBoxVisible));
                OnPropertyChanged(nameof(IsMemberConfirmed));
                OnPropertyChanged(nameof(IsSearchingMember));
                OnPropertyChanged(nameof(IsMemberEditing));
            });
            ChangeMemberCommand = new RelayCommand(_ => {
                IsManualMemberSearchMode = true;
                IsCustomerDropDownOpen = true;
                OnPropertyChanged(nameof(IsLinkedMemberCardVisible));
                OnPropertyChanged(nameof(IsMemberSearchBoxVisible));
                OnPropertyChanged(nameof(IsMemberConfirmed));
                OnPropertyChanged(nameof(IsSearchingMember));
                OnPropertyChanged(nameof(IsMemberEditing));
            });
            CancelChangeMemberCommand = new RelayCommand(_ => {
                IsManualMemberSearchMode = false;
                IsCustomerDropDownOpen = false;
                OnPropertyChanged(nameof(IsLinkedMemberCardVisible));
                OnPropertyChanged(nameof(IsMemberSearchBoxVisible));
                OnPropertyChanged(nameof(IsMemberConfirmed));
                OnPropertyChanged(nameof(IsSearchingMember));
                OnPropertyChanged(nameof(IsMemberEditing));
            });
            ToggleCustomerDropDownCommand = new RelayCommand(_ => { IsCustomerDropDownOpen = !IsCustomerDropDownOpen; });

            _ = LoadCustomersAsync();

            // Checkout Commands
            CheckoutCommand = new RelayCommand(async _ => { 
                if (CartItems.Count > 0)
                {
                    IsCheckoutModalVisible = true; 
                    CurrentStep = 2; // Direct Navigation: Go straight to Payment Workstation (Step 2)
                    _numpadInput = "0";
                    _isNewInput = true;
                    CashAmountReceived = 0;
                    SelectedPaymentMethod = null;
                    IsCustomerSearchOpen = false;
                    IsManualMemberSearchMode = false;

                    if (IsDeliverySelected)
                    {
                        await InitializeMemberAsync();
                    }
                    else
                    {
                        // Regular POS checkout without delivery: reset member state if not already selected
                        if (SelectedCustomer == null)
                        {
                            IsMemberSelected = false;
                            CustomerSearchText = "";
                            MemberPhone = "";
                            MemberSearchResultText = "";
                            OnPropertyChanged(nameof(MemberSubtitleText));
                        }
                    }
                }
            });
            NumpadCommand = new RelayCommand(param => NumpadInput(param?.ToString()), _ => true);
            NextStepCommand = new RelayCommand(async _ => await NextStep());
            PrevStepCommand = new RelayCommand(_ => PrevStep());
            ConfirmOrderCommand = new RelayCommand(async _ => await ConfirmOrder());
            // Delivery Modal Commands
            OpenDeliveryModalCommand = new RelayCommand(_ => OpenDeliveryModal());
            CloseDeliveryModalCommand = new RelayCommand(_ => { 
                IsDeliveryModalOpen = false; 
            });
            CancelDeliveryModalCommand = new RelayCommand(_ => {
                IsDeliveryModalOpen = false;
                if (string.IsNullOrWhiteSpace(CustomerName) && SelectedDeliveryCustomer == null && SelectedCustomer == null)
                {
                    IsDeliverySelected = false; // Uncheck delivery checkbox only if cancelled without any info
                }
            });
            SaveDeliveryDetailsCommand = new RelayCommand(async _ => {
                if (string.IsNullOrWhiteSpace(CustomerName) || string.IsNullOrWhiteSpace(CustomerPhone) || string.IsNullOrWhiteSpace(CustomerAddress))
                {
                    ErrorModalMessage = "กรุณากรอกชื่อลูกค้า เบอร์โทร และที่อยู่จัดส่งให้ครบถ้วนก่อนยืนยัน";
                    IsErrorModalOpen = true;
                    return;
                }

                await InitializeMemberAsync();
                IsDeliveryModalOpen = false;
                OnPropertyChanged(nameof(ShippingFee));
                OnPropertyChanged(nameof(CartTotal));
                OnPropertyChanged(nameof(CheckoutGrandTotal));
                OnPropertyChanged(nameof(ChangeAmount));
            });
            ShowAddNewAddressFormCommand = new RelayCommand(_ => {
                NewDeliveryName = CustomerName;
                NewDeliveryPhone = CustomerPhone;
                NewDeliveryAddress = "";
                IsAddNewAddressMode = true;
            });
            CancelAddNewAddressCommand = new RelayCommand(_ => {
                IsAddNewAddressMode = false;
            });
            SaveNewCustomerAddressCommand = new RelayCommand(async _ => await SaveNewCustomerAddressAsync());
            CloseErrorModalCommand = new RelayCommand(_ => { IsErrorModalOpen = false; });
            PrintReceiptCommand = new RelayCommand(_ => PrintCurrentReceipt());
            CloseCheckoutCommand = new RelayCommand(_ => CloseCheckout());
            SelectBankCommand = new RelayCommand(param => { if (param != null) SelectedBank = param.ToString()!; });

            // Load Initial Data
            _ = LoadProducts();
            _ = LoadCustomersAsync();
        }

        private void OpenDeliveryModal()
        {
            IsAddNewAddressMode = false;
            IsDeliveryModalOpen = true;
            _ = LoadCustomersAsync();
            if (SelectedCustomer != null)
            {
                SelectedDeliveryCustomer = SelectedCustomer;
            }
            else if (SelectedDeliveryCustomer != null)
            {
                SelectedCustomer = SelectedDeliveryCustomer;
            }
        }

        private async Task SaveNewCustomerAddressAsync()
        {
            if (string.IsNullOrWhiteSpace(NewDeliveryName) || string.IsNullOrWhiteSpace(NewDeliveryPhone) || string.IsNullOrWhiteSpace(NewDeliveryAddress))
            {
                ErrorModalMessage = "กรุณากรอกชื่อ เบอร์โทร และที่อยู่จัดส่งใหม่ให้ครบถ้วน";
                IsErrorModalOpen = true;
                return;
            }

            try
            {
                var newCustomer = new Customer
                {
                    Code = "CUS-" + DateTime.Now.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture),
                    Name = NewDeliveryName.Trim(),
                    Phone = NewDeliveryPhone.Trim(),
                    Address = NewDeliveryAddress.Trim(),
                    RegistrationDate = DateTime.Now
                };

                _databaseService.SaveCustomer(newCustomer);
                await LoadCustomersAsync();

                // Select newly created customer
                var created = _allCustomers.FirstOrDefault(c => c.Phone == newCustomer.Phone || c.Name == newCustomer.Name) ?? newCustomer;
                SelectedDeliveryCustomer = created;
                CustomerName = created.Name;
                CustomerPhone = created.Phone;
                CustomerAddress = created.Address;

                IsAddNewAddressMode = false;
            }
            catch (Exception ex)
            {
                ErrorModalMessage = "เกิดข้อผิดพลาดในการบันทึกที่อยู่ใหม่: " + ex.Message;
                IsErrorModalOpen = true;
            }
        }

        public async Task LoadCustomersAsync()
        {
            try
            {
                var results = await _databaseService.GetCustomersForPosAsync();
                _allCustomers = results.ToList();

                // Preserve current selection ID to prevent WPF ComboBox from wiping it out on Clear()
                int? selectedCusId = SelectedCustomer?.Id ?? SelectedDeliveryCustomer?.Id;
                string? prevPhone = !string.IsNullOrWhiteSpace(CustomerPhone) ? CustomerPhone : SelectedCustomer?.Phone;

                CustomerList.Clear();
                foreach (var c in _allCustomers)
                {
                    CustomerList.Add(c);
                }

                if (selectedCusId.HasValue && selectedCusId.Value > 0)
                {
                    var reselected = _allCustomers.FirstOrDefault(c => c.Id == selectedCusId.Value);
                    if (reselected != null)
                    {
                        _selectedDeliveryCustomer = reselected;
                        _selectedCustomer = reselected;
                        CustomerName = reselected.Name;
                        CustomerPhone = reselected.Phone;
                        OnPropertyChanged(nameof(SelectedDeliveryCustomer));
                        OnPropertyChanged(nameof(SelectedCustomer));
                        OnPropertyChanged(nameof(HasSelectedCustomer));
                        OnPropertyChanged(nameof(HasMember));
                        OnPropertyChanged(nameof(MemberSubtitleText));
                        OnPropertyChanged(nameof(LinkedMemberDisplayText));
                    }
                }
                else if (!string.IsNullOrWhiteSpace(prevPhone))
                {
                    string cleanPrev = prevPhone.Replace("-", "").Replace(" ", "").Trim();
                    var reselected = _allCustomers.FirstOrDefault(c => 
                        !string.IsNullOrEmpty(c.Phone) && 
                        c.Phone.Replace("-", "").Replace(" ", "").Trim() == cleanPrev);
                    if (reselected != null)
                    {
                        _selectedDeliveryCustomer = reselected;
                        _selectedCustomer = reselected;
                        CustomerName = reselected.Name;
                        CustomerPhone = reselected.Phone;
                        OnPropertyChanged(nameof(SelectedDeliveryCustomer));
                        OnPropertyChanged(nameof(SelectedCustomer));
                        OnPropertyChanged(nameof(HasSelectedCustomer));
                        OnPropertyChanged(nameof(HasMember));
                        OnPropertyChanged(nameof(MemberSubtitleText));
                        OnPropertyChanged(nameof(LinkedMemberDisplayText));
                    }
                }

                OnPropertyChanged(nameof(CustomerList));
                OnPropertyChanged(nameof(FilteredCustomerList));
                OnPropertyChanged(nameof(CustomerSearchResults));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading customers: {ex.Message}");
            }
        }

        public void FilterCustomers(string keyword)
        {
            if (_allCustomers.Count == 0)
            {
                _ = LoadCustomersAsync();
            }

            if (string.IsNullOrWhiteSpace(keyword))
            {
                CustomerList.Clear();
                foreach (var c in _allCustomers)
                {
                    CustomerList.Add(c);
                }
                IsCustomerDropDownOpen = false;
                OnPropertyChanged(nameof(CustomerList));
                OnPropertyChanged(nameof(FilteredCustomerList));
                return;
            }

            string cleanSearch = keyword.Replace("-", "").Replace(" ", "").Trim().ToLower();
            string rawSearch = keyword.Trim().ToLower();

            var matches = _allCustomers.Where(c => 
                (!string.IsNullOrEmpty(c.Phone) && c.Phone.Replace("-", "").Replace(" ", "").ToLower().Contains(cleanSearch)) ||
                (!string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(rawSearch)) ||
                (!string.IsNullOrEmpty(c.Code) && c.Code.ToLower().Contains(rawSearch))
            ).ToList();

            CustomerList.Clear();
            foreach (var c in matches)
            {
                CustomerList.Add(c);
            }

            // Open dropdown suggestions if typing
            IsCustomerDropDownOpen = true;
            OnPropertyChanged(nameof(CustomerList));
            OnPropertyChanged(nameof(FilteredCustomerList));
        }

        private void ClearSelectedCustomer()
        {
            SelectedCustomer = null;
            CustomerSearchText = "";
            MemberSearchResultText = "";
        }

        private async Task ExecuteSearchCustomer()
        {
            string searchText = (CustomerSearchText ?? "").Trim();
            if (_allCustomers.Count == 0)
            {
                await LoadCustomersAsync();
            }

            if (string.IsNullOrWhiteSpace(searchText))
            {
                IsCustomerDropDownOpen = true;
                return;
            }

            string cleanSearch = searchText.Replace("-", "").Replace(" ", "").ToLower();
            string rawSearch = searchText.ToLower();

            var matches = _allCustomers.Where(c => 
                (!string.IsNullOrEmpty(c.Phone) && c.Phone.Replace("-", "").Replace(" ", "").ToLower().Contains(cleanSearch)) ||
                (!string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(rawSearch)) ||
                (!string.IsNullOrEmpty(c.Code) && c.Code.ToLower().Contains(rawSearch))
            ).ToList();

            if (matches.Count > 0)
            {
                CustomerList.Clear();
                foreach (var c in matches)
                {
                    CustomerList.Add(c);
                }
                SelectedCustomer = matches[0];
                IsMemberSelected = true;
                IsCustomerDropDownOpen = true;
                OnPropertyChanged(nameof(CustomerList));
                OnPropertyChanged(nameof(FilteredCustomerList));
            }
            else
            {
                SelectedCustomer = null;
                IsCustomerDropDownOpen = false;
                MessageBox.Show("ไม่พบข้อมูลสมาชิกเบอร์นี้", "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async Task SearchMember()
        {
            await ExecuteSearchCustomer();
        }

        public void InitializeMember(string phone, string? name = null)
        {
            if (string.IsNullOrWhiteSpace(phone) && string.IsNullOrWhiteSpace(name)) return;
            CustomerSearchText = phone ?? "";
            MemberPhone = phone ?? "";
            IsMemberSelected = true;
            _ = InitializeMemberAsync(phone, name);
        }

        public async Task InitializeMemberAsync(string? phone = null, string? name = null)
        {
            try
            {
                string targetPhone = !string.IsNullOrWhiteSpace(phone) 
                    ? phone 
                    : (!string.IsNullOrWhiteSpace(CustomerPhone) 
                        ? CustomerPhone 
                        : (SelectedDeliveryCustomer?.Phone ?? SelectedCustomer?.Phone ?? ""));

                string targetName = !string.IsNullOrWhiteSpace(name) 
                    ? name 
                    : (!string.IsNullOrWhiteSpace(CustomerName) 
                        ? CustomerName 
                        : (SelectedDeliveryCustomer?.Name ?? SelectedCustomer?.Name ?? ""));

                if (_allCustomers.Count == 0)
                {
                    await LoadCustomersAsync();
                }

                Customer? found = SelectedDeliveryCustomer ?? SelectedCustomer;

                string cleanPhone = (targetPhone ?? "").Replace("-", "").Replace(" ", "").Trim();
                string rawName = (targetName ?? "").Trim();

                if (found == null && !string.IsNullOrEmpty(cleanPhone))
                {
                    found = _allCustomers.FirstOrDefault(c => 
                        !string.IsNullOrEmpty(c.Phone) && 
                        c.Phone.Replace("-", "").Replace(" ", "").Trim() == cleanPhone);
                }

                if (found == null && !string.IsNullOrEmpty(rawName))
                {
                    found = _allCustomers.FirstOrDefault(c => 
                        !string.IsNullOrEmpty(c.Name) && 
                        c.Name.Trim().Equals(rawName, StringComparison.OrdinalIgnoreCase));
                }

                // Fallback direct DB query if not in cache
                if (found == null && (!string.IsNullOrEmpty(cleanPhone) || !string.IsNullOrEmpty(rawName)))
                {
                    try
                    {
                        using (var conn = _databaseService.GetConnection())
                        {
                            string sql = @"SELECT Cus_ID as Id, Cus_Code as Code, Cus_Name as Name, 
                                                  Cus_Address as Address, Cus_Tel as Phone, Cus_Points as Points 
                                           FROM tblCustomer 
                                           WHERE (REPLACE(REPLACE(Cus_Tel, '-', ''), ' ', '') = @cleanPhone AND @cleanPhone <> '')
                                              OR (Cus_Name = @rawName AND @rawName <> '')
                                           LIMIT 1";
                            found = await conn.QueryFirstOrDefaultAsync<Customer>(sql, new { cleanPhone, rawName });
                            if (found != null && !_allCustomers.Any(c => c.Id == found.Id))
                            {
                                _allCustomers.Add(found);
                                CustomerList.Add(found);
                            }
                        }
                    }
                    catch (Exception dbEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"DB lookup failed: {dbEx.Message}");
                    }
                }

                if (found != null)
                {
                    SelectedCustomer = found;
                    SelectedDeliveryCustomer = found;
                    CustomerName = found.Name;
                    CustomerPhone = found.Phone;
                    if (!string.IsNullOrWhiteSpace(found.Address) && string.IsNullOrWhiteSpace(CustomerAddress))
                    {
                        CustomerAddress = found.Address;
                    }
                    _customerSearchText = found.Phone ?? "";
                    OnPropertyChanged(nameof(CustomerSearchText));
                    OnPropertyChanged(nameof(MemberPhone));
                    OnPropertyChanged(nameof(SearchPhoneNumber));
                    OnPropertyChanged(nameof(MemberSearchText));
                    IsMemberSelected = true;
                    IsManualMemberSearchMode = false;
                    MemberSearchResultText = $"👤 คุณ {found.Name} (สะสมเดิม: {found.Points} แต้ม)";
                }
                else
                {
                    // Even if customer is not found in database, set phone and keep member checked
                    if (!string.IsNullOrWhiteSpace(targetPhone))
                    {
                        _customerSearchText = targetPhone;
                        OnPropertyChanged(nameof(CustomerSearchText));
                        OnPropertyChanged(nameof(MemberPhone));
                        OnPropertyChanged(nameof(SearchPhoneNumber));
                        OnPropertyChanged(nameof(MemberSearchText));
                        IsMemberSelected = true;
                    }
                }

                OnPropertyChanged(nameof(SelectedCustomer));
                OnPropertyChanged(nameof(SelectedDeliveryCustomer));
                OnPropertyChanged(nameof(HasSelectedCustomer));
                OnPropertyChanged(nameof(HasMember));
                OnPropertyChanged(nameof(IsMemberSelected));
                OnPropertyChanged(nameof(IsManualMemberSearchMode));
                OnPropertyChanged(nameof(IsLinkedMemberCardVisible));
                OnPropertyChanged(nameof(IsMemberSearchBoxVisible));
                OnPropertyChanged(nameof(IsMemberConfirmed));
                OnPropertyChanged(nameof(IsSearchingMember));
                OnPropertyChanged(nameof(IsMemberEditing));
                OnPropertyChanged(nameof(MemberSubtitleText));
                OnPropertyChanged(nameof(LinkedMemberDisplayText));
                OnPropertyChanged(nameof(EarnedPoints));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in InitializeMemberAsync: {ex.Message}");
            }
        }

        public async Task<Customer?> LookupCustomer(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;
            string clean = phone.Replace("-", "").Replace(" ", "").Trim();
            var matched = _allCustomers.FirstOrDefault(c => 
                !string.IsNullOrEmpty(c.Phone) && 
                c.Phone.Replace("-", "").Replace(" ", "").Trim() == clean);
            if (matched == null)
            {
                try
                {
                    using (var conn = _databaseService.GetConnection())
                    {
                        string sql = @"SELECT Cus_ID as Id, Cus_Code as Code, Cus_Name as Name, 
                                              Cus_Address as Address, Cus_Tel as Phone, Cus_Points as Points 
                                       FROM tblCustomer 
                                       WHERE REPLACE(REPLACE(Cus_Tel, '-', ''), ' ', '') = @clean 
                                       LIMIT 1";
                        matched = await conn.QueryFirstOrDefaultAsync<Customer>(sql, new { clean });
                    }
                }
                catch { }
            }
            return matched;
        }

        public async Task<Customer?> FindMemberByPhone(string phone)
        {
            return await LookupCustomer(phone);
        }

        private async Task SearchCustomersAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                FilteredCustomers.Clear();
                IsCustomerSearchOpen = false;
                OnPropertyChanged(nameof(CustomerSearchResults));
                return;
            }

            try
            {
                var results = await _databaseService.GetCustomersForPosAsync();
                _allCustomers = results.ToList();

                var kw = keyword.Trim().ToLower();
                var matches = _allCustomers.Where(c => 
                    (!string.IsNullOrEmpty(c.Phone) && c.Phone.ToLower().Contains(kw)) ||
                    (!string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(kw)) ||
                    (!string.IsNullOrEmpty(c.Code) && c.Code.ToLower().Contains(kw))
                ).ToList();

                FilteredCustomers.Clear();
                foreach (var c in matches)
                {
                    FilteredCustomers.Add(c);
                }

                IsCustomerSearchOpen = FilteredCustomers.Any() && !string.IsNullOrWhiteSpace(keyword);
                OnPropertyChanged(nameof(CustomerSearchResults));
            }
            catch (Exception ex)
            {
                IsCustomerSearchOpen = false;
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void NumpadInput(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            // 1. ปุ่มลัด Quick Cash: 20, 50, 100, 500, 1000, Exact (แทนที่ค่าโดยตรง ไม่บวกเพิ่มหรือต่อท้าย)
            if (key == "20" || key == "50" || key == "100" || key == "500" || key == "1000")
            {
                _numpadInput = key;
                _isNewInput = true;
            }
            else if (key == "Exact")
            {
                _numpadInput = CheckoutGrandTotal.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
                if (string.IsNullOrEmpty(_numpadInput) || _numpadInput == "0")
                {
                    _numpadInput = "0";
                }
                _isNewInput = true;
            }
            // 2. ปุ่มล้าง (CLEAR / C) -> รีเซ็ตค่ายอดเงินที่รับมากลับเป็น "0" ทันที และพร้อมรับตัวเลขใหม่
            else if (key == "CLEAR" || key == "C")
            {
                _numpadInput = "0";
                _isNewInput = true;
            }
            // 3. ปุ่มลบ (BACK / <- / ⌫) -> ลบตัวอักษรขวาสุดออกทีละ 1 ตัวอักษร หากเหลือตัวเดียวหรือว่างให้กลับเป็น "0"
            else if (key == "BACK" || key == "<-" || key == "⌫")
            {
                if (_isNewInput || _numpadInput.Length <= 1)
                {
                    _numpadInput = "0";
                    _isNewInput = true;
                }
                else
                {
                    _numpadInput = _numpadInput.Substring(0, _numpadInput.Length - 1);
                    if (string.IsNullOrEmpty(_numpadInput) || _numpadInput == "-" || _numpadInput == "0")
                    {
                        _numpadInput = "0";
                        _isNewInput = true;
                    }
                }
            }
            // 4. ปุ่มจุดทศนิยม (.) -> ตรวจสอบก่อนเสมอ หากมีจุดแล้วให้ return ข้าม (Ignore/No-op)
            else if (key == ".")
            {
                if (_isNewInput)
                {
                    _numpadInput = "0.";
                    _isNewInput = false;
                }
                else
                {
                    if (_numpadInput.Contains("."))
                    {
                        return; // มีจุดทศนิยมอยู่แล้ว ให้ข้ามทันที ไม่ล้างค่าและไม่เกิด Error
                    }

                    if (string.IsNullOrEmpty(_numpadInput) || _numpadInput == "0")
                    {
                        _numpadInput = "0.";
                    }
                    else
                    {
                        _numpadInput += ".";
                    }
                    _isNewInput = false;
                }
            }
            // 5. ปุ่ม "00"
            else if (key == "00")
            {
                if (_isNewInput)
                {
                    _numpadInput = "0";
                    _isNewInput = true;
                }
                else if (_numpadInput == "0")
                {
                    _isNewInput = true;
                }
                else if (_numpadInput.Contains("."))
                {
                    int dotIndex = _numpadInput.IndexOf('.');
                    int decimals = _numpadInput.Length - 1 - dotIndex;
                    if (decimals == 0)
                    {
                        _numpadInput += "00";
                    }
                    else if (decimals == 1)
                    {
                        _numpadInput += "0";
                    }
                }
                else
                {
                    if (_numpadInput.Length < 9)
                    {
                        _numpadInput += "00";
                    }
                }
            }
            // 6. ปุ่มตัวเลข 0 - 9
            else
            {
                if (!char.IsDigit(key[0])) return;

                if (_isNewInput)
                {
                    if (key == "0")
                    {
                        _numpadInput = "0";
                        _isNewInput = true;
                    }
                    else
                    {
                        _numpadInput = key;
                        _isNewInput = false;
                    }
                }
                else
                {
                    if (_numpadInput == "0")
                    {
                        if (key == "0")
                        {
                            _numpadInput = "0";
                            _isNewInput = true;
                        }
                        else
                        {
                            _numpadInput = key;
                            _isNewInput = false;
                        }
                    }
                    else
                    {
                        if (_numpadInput.Contains("."))
                        {
                            int dotIndex = _numpadInput.IndexOf('.');
                            if (_numpadInput.Length - 1 - dotIndex < 2)
                            {
                                _numpadInput += key;
                            }
                        }
                        else
                        {
                            if (_numpadInput.Length < 9)
                            {
                                _numpadInput += key;
                            }
                        }
                    }
                }
            }

            // คำนวณและอัปเดตยอดเงินแบบ Real-time ด้วย decimal.TryParse เสมอ
            decimal parsedVal = 0;
            if (!string.IsNullOrEmpty(_numpadInput))
            {
                if (!decimal.TryParse(_numpadInput, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsedVal))
                {
                    decimal.TryParse(_numpadInput, out parsedVal);
                }
            }

            CashAmountReceived = parsedVal;
            OnPropertyChanged(nameof(NumpadInputString));
            OnPropertyChanged(nameof(CashReceived));
            OnPropertyChanged(nameof(ChangeAmount));
            OnPropertyChanged(nameof(Change));
        }

        private async Task NextStep()
        {
            if (CurrentStep == 1)
            {
                if (IsDeliverySelected)
                {
                    if (string.IsNullOrWhiteSpace(CustomerName) || 
                        string.IsNullOrWhiteSpace(CustomerPhone) || 
                        string.IsNullOrWhiteSpace(CustomerAddress))
                    {
                        ErrorModalMessage = "กรุณากรอกชื่อลูกค้า / ผู้สั่ง เบอร์โทรศัพท์ และที่อยู่จัดส่งให้ครบถ้วนก่อนไปหน้าชำระเงิน";
                        IsErrorModalOpen = true;
                        return;
                    }

                    await InitializeMemberAsync();
                }
                CurrentStep = 2;
            }
            else if (CurrentStep == 2)
            {
                if (IsCashPayment && CashAmountReceived < CartTotal)
                {
                    ErrorModalMessage = "จำนวนเงินที่รับมาไม่เพียงพอ";
                    IsErrorModalOpen = true;
                    return;
                }
                await ConfirmOrder();
            }
        }

        private void PrevStep()
        {
            if (CurrentStep == 2)
            {
                // Close the modal and return directly to the main POS catalog screen
                IsCheckoutModalVisible = false;
                CurrentStep = 1;
            }
            else if (CurrentStep == 1)
            {
                IsCheckoutModalVisible = false;
            }
        }

        private async Task ConfirmOrder()
        {
            if (CartItems.Count == 0) return;

            // 1. Stock Check
            foreach (var item in CartItems)
            {
                if (item.Product.Stock < item.Quantity)
                {
                    ErrorModalMessage = $"สินค้า '{item.Product.Name}' มีคงเหลือไม่เพียงพอ (คงเหลือ {item.Product.Stock})";
                    IsErrorModalOpen = true;
                    return;
                }
            }

            // 2. Transaction DB Save
            using (var conn = _databaseService.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        var orderRef = "SALE-" + System.DateTime.Now.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                        LastRefNo = orderRef;
                        var effectivePayment = !string.IsNullOrWhiteSpace(SelectedPaymentMethod) ? SelectedPaymentMethod : "เงินสด";

                        int? customerId = null;
                        if (SelectedCustomer != null && SelectedCustomer.Id > 0)
                        {
                            customerId = SelectedCustomer.Id;
                        }
                        else if (SelectedDeliveryCustomer != null && SelectedDeliveryCustomer.Id > 0)
                        {
                            customerId = SelectedDeliveryCustomer.Id;
                        }
                        else if (!string.IsNullOrWhiteSpace(CustomerPhone) || !string.IsNullOrWhiteSpace(CustomerName))
                        {
                            string cleanP = (CustomerPhone ?? "").Replace("-", "").Replace(" ", "").Trim();
                            string rawN = (CustomerName ?? "").Trim();
                            var matched = _allCustomers.FirstOrDefault(c => 
                                (!string.IsNullOrEmpty(c.Phone) && !string.IsNullOrEmpty(cleanP) && c.Phone.Replace("-", "").Replace(" ", "").Trim() == cleanP)
                                || (!string.IsNullOrEmpty(c.Name) && !string.IsNullOrEmpty(rawN) && c.Name.Trim().Equals(rawN, StringComparison.OrdinalIgnoreCase)));
                            
                            if (matched == null && (!string.IsNullOrEmpty(cleanP) || !string.IsNullOrEmpty(rawN)))
                            {
                                try
                                {
                                    string sqlFind = @"SELECT Cus_ID as Id, Cus_Code as Code, Cus_Name as Name, Cus_Address as Address, Cus_Tel as Phone, Cus_Points as Points 
                                                       FROM tblCustomer 
                                                       WHERE (REPLACE(REPLACE(Cus_Tel, '-', ''), ' ', '') = @cleanP AND @cleanP <> '') 
                                                          OR (Cus_Name = @rawN AND @rawN <> '') 
                                                       LIMIT 1";
                                    matched = await conn.QueryFirstOrDefaultAsync<Customer>(sqlFind, new { cleanP, rawN }, trans);
                                }
                                catch { }
                            }

                            if (matched != null && matched.Id > 0)
                            {
                                customerId = matched.Id;
                                if (SelectedCustomer == null) SelectedCustomer = matched;
                            }
                        }

                        var salesOrder = new SalesOrder
                        {
                            RefNo = orderRef,
                            TotalAmount = CartTotal,
                            CashReceived = IsCashPayment ? CashAmountReceived : CartTotal,
                            Change = IsCashPayment ? ChangeAmount : 0,
                            PaymentMethod = effectivePayment,
                            Timestamp = System.DateTime.Now,
                            IsDelivery = SelectedShippingMethod == "Delivery",
                            CustomerName = SelectedShippingMethod == "Delivery" ? CustomerName : (SelectedCustomer?.Name ?? null),
                            CustomerPhone = SelectedShippingMethod == "Delivery" ? CustomerPhone : (SelectedCustomer?.Phone ?? null),
                            CustomerAddress = SelectedShippingMethod == "Delivery" ? CustomerAddress : (SelectedCustomer?.Address ?? null),
                            DeliveryStatus = SelectedShippingMethod == "Delivery" ? "รอจัดส่ง" : null,
                            Cus_ID = customerId ?? 0
                        };

                        var salesOrderParam = new
                        {
                            RefNo = orderRef,
                            Sales_Date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                            Cus_ID = customerId,
                            Emp_ID = (int?)null,
                            Sales_Total = CartTotal,
                            Sales_Cash = IsCashPayment ? CashAmountReceived : CartTotal,
                            Sales_Change = IsCashPayment ? ChangeAmount : 0,
                            Sales_PaymentType = effectivePayment,
                            Sales_Status = "ชำระเงินแล้ว"
                        };

                        int orderId;
                        try
                        {
                            string sqlOrderH = @"INSERT INTO tblSales_H (RefNo, Sales_Date, Cus_ID, Emp_ID, Sales_Total, Sales_Cash, Sales_Change, Sales_PaymentType, Sales_Status)
                                                VALUES (@RefNo, @Sales_Date, @Cus_ID, @Emp_ID, @Sales_Total, @Sales_Cash, @Sales_Change, @Sales_PaymentType, @Sales_Status);
                                                SELECT last_insert_rowid();";
                            orderId = await conn.ExecuteScalarAsync<int>(sqlOrderH, salesOrderParam, trans);
                        }
                        catch
                        {
                            string sqlOrder = @"INSERT INTO SalesOrders (RefNo, TotalAmount, CashReceived, Change, PaymentMethod, Timestamp, Status, IsDelivery, CustomerName, CustomerPhone, CustomerAddress, DeliveryStatus)
                                                VALUES (@RefNo, @TotalAmount, @CashReceived, @Change, @PaymentMethod, @Timestamp, 'ชำระเงินแล้ว', @IsDelivery, @CustomerName, @CustomerPhone, @CustomerAddress, @DeliveryStatus);
                                                SELECT last_insert_rowid();";
                            orderId = await conn.ExecuteScalarAsync<int>(sqlOrder, salesOrder, trans);
                        }

                        if (SelectedShippingMethod == "Delivery")
                        {
                            string recipientName = !string.IsNullOrWhiteSpace(CustomerName) 
                                ? CustomerName.Trim() 
                                : (!string.IsNullOrWhiteSpace(DeliveryCustomerName) ? DeliveryCustomerName.Trim() : (SelectedCustomer?.Name?.Trim() ?? "ลูกค้าทั่วไป"));

                            string recipientTel = !string.IsNullOrWhiteSpace(CustomerPhone) 
                                ? CustomerPhone.Trim() 
                                : (!string.IsNullOrWhiteSpace(DeliveryCustomerPhone) ? DeliveryCustomerPhone.Trim() : (SelectedCustomer?.Phone?.Trim() ?? "-"));

                            string recipientAddress = !string.IsNullOrWhiteSpace(CustomerAddress) 
                                ? CustomerAddress.Trim() 
                                : (!string.IsNullOrWhiteSpace(DeliveryCustomerAddress) ? DeliveryCustomerAddress.Trim() : (SelectedCustomer?.Address?.Trim() ?? "ไม่ระบุที่อยู่"));

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
                            // Deduct Stock from tblProduct
                            string sqlUpdate = "UPDATE tblProduct SET Pro_Qty = Pro_Qty - @Qty WHERE Pro_ID = @Id";
                            await conn.ExecuteAsync(sqlUpdate, new { Qty = item.Quantity, Id = item.Product.Id }, trans);

                            // Record Sales Detail
                            string sqlDetail = @"INSERT INTO tblSalesDetail (Sales_ID, Pro_ID, Pro_Price, Sales_Qty, Sales_Subtotal)
                                                 VALUES (@Sales_ID, @Pro_ID, @Pro_Price, @Sales_Qty, @Sales_Subtotal)";
                            await conn.ExecuteAsync(sqlDetail, new 
                            { 
                                Sales_ID = orderId, 
                                Pro_ID = item.Product.Id, 
                                Pro_Price = item.Product.Price, 
                                Sales_Qty = item.Quantity, 
                                Sales_Subtotal = item.Total 
                            }, trans);

                            // Update Local Product Instance
                            item.Product.Stock -= item.Quantity;
                            var existing = _allProducts.FirstOrDefault(p => p.Id == item.Product.Id);
                            if (existing != null)
                            {
                                existing.Stock = item.Product.Stock;
                            }
                        }

                        // Accumulate Member Points and Update Spending if Customer is Selected
                        if (customerId.HasValue && customerId.Value > 0)
                        {
                            if (EarnedPoints > 0)
                            {
                                try
                                {
                                    string sqlPointsTbl = "UPDATE tblCustomer SET Cus_Points = COALESCE(Cus_Points, 0) + @Points WHERE Cus_ID = @Id";
                                    await conn.ExecuteAsync(sqlPointsTbl, new { Points = EarnedPoints, Id = customerId.Value }, trans);
                                }
                                catch
                                {
                                    try
                                    {
                                        string sqlPoints = "UPDATE Customers SET Points = COALESCE(Points, 0) + @Points WHERE Id = @Id";
                                        await conn.ExecuteAsync(sqlPoints, new { Points = EarnedPoints, Id = customerId.Value }, trans);
                                    }
                                    catch { }
                                }
                            }

                            try
                            {
                                string sqlSpend = "UPDATE tblCustomer SET Cus_TotalSpent = COALESCE(Cus_TotalSpent, 0) + @Spent, Cus_TotalPurchases = COALESCE(Cus_TotalPurchases, 0) + 1 WHERE Cus_ID = @Id";
                                await conn.ExecuteAsync(sqlSpend, new { Spent = CartTotal, Id = customerId.Value }, trans);
                            }
                            catch { }

                            if (SelectedCustomer != null)
                            {
                                SelectedCustomer.Points += EarnedPoints;
                                SelectedCustomer.TotalSpent += (double)CartTotal;
                                SelectedCustomer.TotalPurchases += 1;
                            }

                            var inMem = _allCustomers.FirstOrDefault(c => c.Id == customerId.Value);
                            if (inMem != null)
                            {
                                inMem.Points += EarnedPoints;
                                inMem.TotalSpent += (double)CartTotal;
                                inMem.TotalPurchases += 1;
                            }
                        }

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        ErrorModalMessage = $"เกิดข้อผิดพลาดในการทำรายการ: {ex.Message}";
                        IsErrorModalOpen = true;
                        return;
                    }
                }
            }

            // Capture snapshot for Step 4 Receipt Card
            FinalCashReceived = IsCashPayment ? CashAmountReceived : CartTotal;
            FinalChange = IsCashPayment ? ChangeAmount : 0;
            FinalGrandTotal = CartTotal;
            if (IsTransferPayment)
            {
                FinalPaymentMethod = "สแกน QR / โอนเงิน";
                FinalBank = SelectedBank switch
                {
                    "KBANK" => "กสิกรไทย (KBANK)",
                    "GSB" => "ออมสิน (GSB)",
                    "SCB" => "ไทยพาณิชย์ (SCB)",
                    "BBL" => "กรุงเทพ (BBL)",
                    "KTB" => "กรุงไทย (KTB)",
                    _ => SelectedBank
                };
            }
            else
            {
                FinalPaymentMethod = !string.IsNullOrEmpty(SelectedPaymentMethod) ? SelectedPaymentMethod : "เงินสด";
                FinalBank = "";
            }
            OnPropertyChanged(nameof(HasFinalBank));

            // Move to success page
            CurrentStep = 4;
            FilterProducts();
        }

        private void PrintCurrentReceipt()
        {
            try
            {
                var order = new SalesOrder
                {
                    RefNo = string.IsNullOrWhiteSpace(LastRefNo) ? "SALE-" + System.DateTime.Now.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture) : LastRefNo,
                    Sales_Date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                    CustomerName = !string.IsNullOrWhiteSpace(CustomerName) ? CustomerName : (SelectedCustomer?.Name ?? "ลูกค้าทั่วไป"),
                    TotalAmount = FinalGrandTotal,
                    CashReceived = FinalCashReceived,
                    Change = FinalChange,
                    PaymentMethod = !string.IsNullOrWhiteSpace(SelectedPaymentMethod) ? SelectedPaymentMethod : "เงินสด",
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
                receiptWindow.Owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                                     ?? Application.Current?.MainWindow;
                receiptWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                ErrorModalMessage = "เกิดข้อผิดพลาดในการพิมพ์ใบเสร็จ: " + ex.Message;
                IsErrorModalOpen = true;
            }
        }

        private void CloseCheckout()
        {
            ClearCart();
            IsCheckoutModalVisible = false;
            CurrentStep = 1;
            CustomerName = "";
            CustomerPhone = "";
            CustomerAddress = "";
            SelectedCustomer = null;
            SelectedDeliveryCustomer = null;
            IsDeliverySelected = false;
            IsMemberSelected = false;
            IsManualMemberSearchMode = false;
            CashAmountReceived = 0;
            _numpadInput = "0";
            _isNewInput = true;
            SelectedPaymentMethod = null;
        }

        private async Task LoadProducts()
        {
            try
            {
                using (var conn = _databaseService.GetConnection())
                {
                    string sql = @"SELECT 
                                    Pro_ID AS Pro_ID, 
                                    Pro_ID AS Id, 
                                    Pro_Barcode AS Pro_Barcode, 
                                    Pro_Barcode AS Barcode, 
                                    Pro_Barcode AS Code, 
                                    Pro_Name AS Pro_Name, 
                                    Pro_Name AS Name, 
                                    Pro_Category AS Pro_Category, 
                                    Pro_Category AS Category, 
                                    Pro_Price AS Pro_Price, 
                                    Pro_Price AS Price, 
                                    Pro_Cost AS Pro_Cost, 
                                    Pro_Cost AS Cost, 
                                    Pro_Qty AS Pro_Qty, 
                                    Pro_Qty AS Stock, 
                                    Pro_MinQty AS Pro_MinQty, 
                                    Pro_MinQty AS ReorderPoint, 
                                    Pro_Image AS Pro_Image, 
                                    Pro_Image AS ImagePath 
                                   FROM tblProduct";
                    var products = await conn.QueryAsync<Product>(sql);
                    _allProducts = products.ToList();
                    FilterProducts();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading products: {ex.Message}");
            }
        }

        private void FilterProducts()
        {
            Products.Clear();
            var query = _allProducts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SelectedCategory) && SelectedCategory != "ทั้งหมด")
            {
                query = query.Where(p => p.Category != null && p.Category.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string s = SearchText.ToLower();
                query = query.Where(p => 
                    (p.Code?.ToLower().Contains(s) ?? false) ||
                    (p.Name?.ToLower().Contains(s) ?? false)
                );
            }

            foreach (var p in query)
            {
                Products.Add(p);
            }
        }

        private void AddToCart(Product product)
        {
            if (product == null || product.QTY <= 0)
            {
                return; // ป้องกันการเพิ่มสินค้าหมดสต็อกลงตะกร้าทุกกรณี
            }
            var existingItem = CartItems.FirstOrDefault(c => c.Product.Id == product.Id);
            if (existingItem != null)
            {
                if (existingItem.Quantity >= product.QTY) return;
                existingItem.Quantity++;
            }
            else
            {
                CartItems.Add(new CartItem { Product = product, Quantity = 1 });
            }
            CalculateTotal();
        }

        private void DecreaseQuantity(CartItem item)
        {
            if (item == null) return;
            if (item.Quantity > 1)
            {
                item.Quantity--;
            }
            else
            {
                CartItems.Remove(item);
            }
            CalculateTotal();
        }

        private void IncreaseQuantity(CartItem item)
        {
            if (item == null) return;
            item.Quantity++;
            CalculateTotal();
        }

        private void ClearCart()
        {
            CartItems.Clear();
            CalculateTotal();
        }

        private void CalculateTotal()
        {
            TotalAmount = CartItems.Sum(i => i.Total);
            OnPropertyChanged(nameof(CartTotalItems));
            OnPropertyChanged(nameof(CartTotalString));
        }
    }
}
