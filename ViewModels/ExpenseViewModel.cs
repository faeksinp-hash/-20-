using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Porjai20.Models;
using Porjai20.Services;

namespace Porjai20.ViewModels
{
    /// <summary>
    /// Partial ViewModel class for Expense Management in ProductViewModel.
    /// Handles DataGrid selection, dynamic modal title, edit/add state, single in-page modal CRUD operations, auto-generated expense code, and ICollectionView real-time filtering.
    /// </summary>
    public partial class ProductViewModel
    {
        private ObservableCollection<Expense>? _expenses;
        public ObservableCollection<Expense> Expenses
        {
            get => _expenses ??= new ObservableCollection<Expense>();
            set => SetProperty(ref _expenses, value);
        }

        private ICollectionView? _filteredExpenses;
        public ICollectionView FilteredExpenses
        {
            get
            {
                if (_filteredExpenses == null && Expenses != null)
                {
                    _filteredExpenses = CollectionViewSource.GetDefaultView(Expenses);
                    _filteredExpenses.Filter = item =>
                    {
                        string query = (SearchText ?? ExpenseSearchKeyword ?? string.Empty).Trim().ToLower();
                        if (string.IsNullOrWhiteSpace(query)) return true;
                        if (item is Expense model)
                        {
                            bool matchCode = !string.IsNullOrEmpty(model.Code) && model.Code.ToLower().Contains(query);
                            bool matchDesc = !string.IsNullOrEmpty(model.Description) && model.Description.ToLower().Contains(query);
                            bool matchCat = !string.IsNullOrEmpty(model.Category) && model.Category.ToLower().Contains(query);
                            return matchCode || matchDesc || matchCat;
                        }
                        return false;
                    };
                }
                return _filteredExpenses;
            }
        }

        private Expense? _selectedExpense;
        public Expense? SelectedExpense
        {
            get => _selectedExpense;
            set
            {
                if (SetProperty(ref _selectedExpense, value) && value != null)
                {
                    SelectedExpenseCategory = string.IsNullOrWhiteSpace(value.Category) ? "ค่าน้ำ/ค่าไฟ" : value.Category;
                    ExpenseDescription = SelectedExpenseCategory == "ค่าใช้จ่ายอื่นๆ" ? value.Description : SelectedExpenseCategory;
                    ExpenseAmount = value.Amount;
                    ExpenseDate = value.ExpenseDate;
                    ExpenseNote = value.Note;
                }
            }
        }

        private string _expenseModalTitle = "⚙️ แก้ไขข้อมูลรายจ่าย";
        public string ExpenseModalTitle
        {
            get => _expenseModalTitle;
            set => SetProperty(ref _expenseModalTitle, value);
        }

        private bool _isExpenseModalOpen;
        public bool IsExpenseModalOpen
        {
            get => _isExpenseModalOpen;
            set => SetProperty(ref _isExpenseModalOpen, value);
        }

        private string _expenseValidationMessage = string.Empty;
        public string ExpenseValidationMessage
        {
            get => _expenseValidationMessage;
            set
            {
                if (SetProperty(ref _expenseValidationMessage, value))
                {
                    OnPropertyChanged(nameof(HasExpenseValidationMessage));
                }
            }
        }

        public bool HasExpenseValidationMessage => !string.IsNullOrWhiteSpace(ExpenseValidationMessage);

        public bool IsDescriptionEnabled => SelectedExpenseCategory == "ค่าใช้จ่ายอื่นๆ";

        private ObservableCollection<string> _expenseCategories = new ObservableCollection<string>
        {
            "ค่าน้ำ/ค่าไฟ",
            "ค่าเช่าสถานที่",
            "ค่าอุปกรณ์สำนักงาน",
            "ค่าขนส่ง/เดินทาง",
            "ค่าใช้จ่ายอื่นๆ"
        };
        public ObservableCollection<string> ExpenseCategories
        {
            get => _expenseCategories;
            set => SetProperty(ref _expenseCategories, value);
        }

        private string _selectedExpenseCategory = "ค่าน้ำ/ค่าไฟ";
        public string SelectedExpenseCategory
        {
            get => _selectedExpenseCategory;
            set
            {
                if (SetProperty(ref _selectedExpenseCategory, value))
                {
                    OnPropertyChanged(nameof(IsDescriptionEnabled));
                    if (value != "ค่าใช้จ่ายอื่นๆ")
                    {
                        ExpenseDescription = value;
                    }
                    else if (ExpenseDescription == "ค่าน้ำ/ค่าไฟ" || ExpenseDescription == "ค่าเช่าสถานที่" || ExpenseDescription == "ค่าอุปกรณ์สำนักงาน" || ExpenseDescription == "ค่าขนส่ง/เดินทาง")
                    {
                        ExpenseDescription = string.Empty;
                    }
                }
            }
        }

        private string _expenseDescription = "ค่าน้ำ/ค่าไฟ";
        public string ExpenseDescription
        {
            get => _expenseDescription;
            set => SetProperty(ref _expenseDescription, value);
        }

        private decimal _expenseAmount;
        public decimal ExpenseAmount
        {
            get => _expenseAmount;
            set => SetProperty(ref _expenseAmount, value);
        }

        private DateTime _expenseDate = DateTime.Now;
        public DateTime ExpenseDate
        {
            get => _expenseDate;
            set => SetProperty(ref _expenseDate, value);
        }

        private string _expenseNote = string.Empty;
        public string ExpenseNote
        {
            get => _expenseNote;
            set => SetProperty(ref _expenseNote, value);
        }

        private string _expenseSearchKeyword = string.Empty;
        public string ExpenseSearchKeyword
        {
            get => _expenseSearchKeyword;
            set
            {
                if (SetProperty(ref _expenseSearchKeyword, value))
                {
                    FilteredExpenses?.Refresh();
                }
            }
        }

        public decimal MonthlyTotalExpense => Expenses?.Where(e => e.ExpenseDate.Month == DateTime.Today.Month && e.ExpenseDate.Year == DateTime.Today.Year).Sum(e => e.Amount) ?? 0;
        public int MonthlyExpenseCount => Expenses?.Where(e => e.ExpenseDate.Month == DateTime.Today.Month && e.ExpenseDate.Year == DateTime.Today.Year).Count() ?? 0;
        public string TopExpenseCategoryName
        {
            get
            {
                if (Expenses == null || !Expenses.Any()) return "ไม่มีข้อมูล";
                var topCat = Expenses.GroupBy(e => e.Category)
                                     .OrderByDescending(g => g.Sum(x => x.Amount))
                                     .FirstOrDefault();
                return topCat?.Key ?? "ไม่มีข้อมูล";
            }
        }

        public ICommand OpenExpenseCommand { get; set; }
        public ICommand OpenManageExpenseModalCommand { get; set; }
        public ICommand OpenAddExpenseModalCommand { get; set; }
        public ICommand SaveExpenseCommand { get; set; }
        public ICommand AddExpenseCommand { get; set; }
        public ICommand UpdateExpenseCommand { get; set; }
        public ICommand DeleteExpenseCommand { get; set; }
        public ICommand CloseExpenseModalCommand { get; set; }
        public ICommand ClearExpenseCommand { get; set; }
        public ICommand SearchExpenseCommand { get; set; }
        public ICommand ClearExpenseFilterCommand { get; set; }

        public void InitializeExpenseViewModelCommands()
        {
            OpenExpenseCommand = new RelayCommand(_ => { _ = OpenExpenseMode(); });
            OpenManageExpenseModalCommand = new RelayCommand(param =>
            {
                if (param is Expense item)
                {
                    SelectedExpense = item;
                }
                ExecuteOpenManageExpenseModal();
            });
            OpenAddExpenseModalCommand = new RelayCommand(_ => ExecuteOpenAddExpenseModal());
            
            SaveExpenseCommand = new RelayCommand(_ => ExecuteSaveExpense());
            AddExpenseCommand = new RelayCommand(_ => ExecuteSaveExpense());
            UpdateExpenseCommand = new RelayCommand(_ => ExecuteSaveExpense());
            DeleteExpenseCommand = new RelayCommand(_ => ExecuteDeleteExpense());
            
            CloseExpenseModalCommand = new RelayCommand(_ => ExecuteCloseExpenseModal());
            ClearExpenseCommand = new RelayCommand(_ => ClearExpenseForm());
            
            SearchExpenseCommand = new RelayCommand(_ => FilteredExpenses?.Refresh());
            ClearExpenseFilterCommand = new RelayCommand(_ =>
            {
                ExpenseSearchKeyword = string.Empty;
                SearchText = string.Empty;
                FilteredExpenses?.Refresh();
            });
        }

        private string GenerateNextExpenseCode()
        {
            int maxId = 0;
            if (Expenses != null && Expenses.Count > 0)
            {
                maxId = Expenses.Max(e => e.Id);
            }
            return $"EXP-{(maxId + 1):D3}";
        }

        public void ExecuteOpenManageExpenseModal()
        {
            if (SelectedExpense == null || SelectedExpense.Id <= 0)
            {
                ShowAlert("กรุณาคลิกเลือกรายการรายจ่ายในตารางก่อนดำเนินการ", "แจ้งเตือน", "⚠️");
                return;
            }

            IsEditMode = true;
            ExpenseModalTitle = "⚙️ แก้ไขข้อมูลรายจ่าย";
            ModalTitle = ExpenseModalTitle;
            ExpenseValidationMessage = string.Empty;
            
            SelectedExpenseCategory = string.IsNullOrWhiteSpace(SelectedExpense.Category) ? "ค่าน้ำ/ค่าไฟ" : SelectedExpense.Category;
            ExpenseDescription = SelectedExpenseCategory == "ค่าใช้จ่ายอื่นๆ" ? SelectedExpense.Description : SelectedExpenseCategory;
            ExpenseAmount = SelectedExpense.Amount;
            ExpenseDate = SelectedExpense.ExpenseDate;
            ExpenseNote = SelectedExpense.Note;

            IsModalOpen = true;
            IsExpenseModalOpen = true;
        }

        public void ExecuteOpenAddExpenseModal()
        {
            ClearExpenseForm();
            IsEditMode = false;
            ExpenseModalTitle = "➕ เพิ่มข้อมูลรายจ่าย";
            ModalTitle = ExpenseModalTitle;
            ExpenseValidationMessage = string.Empty;
            ExpenseDate = DateTime.Now;
            IsModalOpen = true;
            IsExpenseModalOpen = true;
        }

        public void ExecuteSaveExpense()
        {
            if (string.IsNullOrWhiteSpace(ExpenseDescription))
            {
                ExpenseValidationMessage = "กรุณากรอกรายละเอียด/ชื่อรายการรายจ่าย";
                return;
            }

            if (ExpenseAmount <= 0)
            {
                ExpenseValidationMessage = "กรุณากรอกจำนวนเงินรายจ่ายที่ถูกต้อง";
                return;
            }

            if (IsEditMode && SelectedExpense != null && SelectedExpense.Id > 0)
            {
                SelectedExpense.Category = SelectedExpenseCategory;
                SelectedExpense.Description = SelectedExpenseCategory == "ค่าใช้จ่ายอื่นๆ" ? ExpenseDescription : SelectedExpenseCategory;
                SelectedExpense.Amount = ExpenseAmount;
                SelectedExpense.ExpenseDate = ExpenseDate;
                SelectedExpense.Note = ExpenseNote;
                if (SelectedExpense.Emp_ID <= 0)
                {
                    SelectedExpense.Emp_ID = CurrentUser != null && CurrentUser.Emp_ID > 0 
                        ? CurrentUser.Emp_ID 
                        : 1;
                }

                _databaseService.UpdateExpense(SelectedExpense);
                ClearExpenseForm();
                SetExpenseModalsClosed();
                _ = LoadExpenses();
                ShowAlert("อัปเดตข้อมูลรายจ่ายเรียบร้อยแล้ว", "สำเร็จ", "🎉");
            }
            else
            {
                string autoCode = GenerateNextExpenseCode();
                var newExpense = new Expense
                {
                    Code = autoCode,
                    Category = string.IsNullOrWhiteSpace(SelectedExpenseCategory) ? "ค่าใช้จ่ายอื่นๆ" : SelectedExpenseCategory,
                    Description = SelectedExpenseCategory == "ค่าใช้จ่ายอื่นๆ" ? ExpenseDescription : SelectedExpenseCategory,
                    Amount = ExpenseAmount,
                    ExpenseDate = ExpenseDate,
                    Note = ExpenseNote,
                    Emp_ID = CurrentUser != null && CurrentUser.Emp_ID > 0 
                        ? CurrentUser.Emp_ID 
                        : 1 // Fallback ใช้ Admin ID = 1
                };

                _databaseService.SaveExpense(newExpense);
                ClearExpenseForm();
                SetExpenseModalsClosed();
                _ = LoadExpenses();
                ShowAlert("บันทึกข้อมูลรายจ่ายเรียบร้อยแล้ว", "สำเร็จ", "🎉");
            }
        }

        public void ExecuteDeleteExpense()
        {
            if (SelectedExpense == null || SelectedExpense.Id <= 0)
            {
                ExpenseValidationMessage = "กรุณาคลิกเลือกรายการรายจ่ายในตารางก่อนดำเนินการ";
                return;
            }

            ShowConfirm(
                $"คุณต้องการลบข้อมูลรายจ่าย '{SelectedExpense.Description}' ใช่หรือไม่?",
                () =>
                {
                    _databaseService.DeleteExpense(SelectedExpense.Id);
                    ClearExpenseForm();
                    SetExpenseModalsClosed();
                    _ = LoadExpenses();
                    ShowAlert("ลบข้อมูลรายจ่ายเรียบร้อยแล้ว", "สำเร็จ", "🗑️");
                },
                "ยืนยันการลบข้อมูลรายจ่าย");
        }

        public void ExecuteCloseExpenseModal()
        {
            ClearExpenseForm();
            SetExpenseModalsClosed();
        }

        private void SetExpenseModalsClosed()
        {
            IsModalOpen = false;
            IsExpenseModalOpen = false;
            ExpenseValidationMessage = string.Empty;
        }

        public void ClearExpenseForm()
        {
            SelectedExpense = new Expense();
            SelectedExpenseCategory = "ค่าน้ำ/ค่าไฟ";
            ExpenseDescription = "ค่าน้ำ/ค่าไฟ";
            ExpenseAmount = 0;
            ExpenseDate = DateTime.Now;
            ExpenseNote = string.Empty;
            ExpenseValidationMessage = string.Empty;
        }

        public async Task OpenExpenseMode()
        {
            IsExpenseMode = true;
            IsHomeMode = false;
            IsDeliveryMode = false;
            IsStaffMode = false;
            IsCustomerMode = false;
            IsPartnerMode = false;
            IsClaimMode = false;
            IsPurchaseOrderMode = false;
            await LoadExpenses();
        }

        public async Task LoadExpenses()
        {
            Expenses.Clear();
            var items = await Task.Run(() => _databaseService.GetExpenses());
            foreach (var item in items)
            {
                Expenses.Add(item);
            }
            FilteredExpenses?.Refresh();

            OnPropertyChanged(nameof(MonthlyTotalExpense));
            OnPropertyChanged(nameof(MonthlyExpenseCount));
            OnPropertyChanged(nameof(TopExpenseCategoryName));
        }
    }

    /// <summary>
    /// Standalone ViewModel class for Expense Management, inheriting from ViewModelBase.
    /// Provides full compatibility if ExpenseView is bound directly to ExpenseViewModel.
    /// </summary>
    public class ExpenseViewModel : ViewModelBase
    {
        private readonly DatabaseService _databaseService;

        public ObservableCollection<Expense> Expenses { get; } = new ObservableCollection<Expense>();

        public ICollectionView FilteredExpenses { get; private set; }
        public ICollectionView ExpensesView => FilteredExpenses;

        private Expense? _selectedExpense;
        public Expense? SelectedExpense
        {
            get => _selectedExpense;
            set
            {
                if (SetProperty(ref _selectedExpense, value) && value != null)
                {
                    SelectedExpenseCategory = value.Category;
                    ExpenseDescription = value.Description;
                    ExpenseAmount = value.Amount;
                    ExpenseDate = value.ExpenseDate;
                    ExpenseNote = value.Note;
                }
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        private string _modalTitle = "⚙️ แก้ไขข้อมูลรายจ่าย";
        public string ModalTitle
        {
            get => _modalTitle;
            set => SetProperty(ref _modalTitle, value);
        }

        public string ExpenseModalTitle
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

        public bool IsExpenseModalOpen
        {
            get => IsModalOpen;
            set => IsModalOpen = value;
        }

        private string _expenseValidationMessage = string.Empty;
        public string ExpenseValidationMessage
        {
            get => _expenseValidationMessage;
            set
            {
                if (SetProperty(ref _expenseValidationMessage, value))
                {
                    OnPropertyChanged(nameof(HasExpenseValidationMessage));
                }
            }
        }

        public bool HasExpenseValidationMessage => !string.IsNullOrWhiteSpace(ExpenseValidationMessage);

        public bool IsDescriptionEnabled => SelectedExpenseCategory == "ค่าใช้จ่ายอื่นๆ";

        public ObservableCollection<string> ExpenseCategories { get; } = new ObservableCollection<string>
        {
            "ค่าน้ำ/ค่าไฟ",
            "ค่าเช่าสถานที่",
            "ค่าอุปกรณ์สำนักงาน",
            "ค่าขนส่ง/เดินทาง",
            "ค่าใช้จ่ายอื่นๆ"
        };

        private string _selectedExpenseCategory = "ค่าน้ำ/ค่าไฟ";
        public string SelectedExpenseCategory
        {
            get => _selectedExpenseCategory;
            set
            {
                if (SetProperty(ref _selectedExpenseCategory, value))
                {
                    OnPropertyChanged(nameof(IsDescriptionEnabled));
                    if (value != "ค่าใช้จ่ายอื่นๆ")
                    {
                        ExpenseDescription = value;
                    }
                    else if (ExpenseDescription == "ค่าน้ำ/ค่าไฟ" || ExpenseDescription == "ค่าเช่าสถานที่" || ExpenseDescription == "ค่าอุปกรณ์สำนักงาน" || ExpenseDescription == "ค่าขนส่ง/เดินทาง")
                    {
                        ExpenseDescription = string.Empty;
                    }
                }
            }
        }

        private string _expenseDescription = "ค่าน้ำ/ค่าไฟ";
        public string ExpenseDescription
        {
            get => _expenseDescription;
            set => SetProperty(ref _expenseDescription, value);
        }

        private decimal _expenseAmount;
        public decimal ExpenseAmount
        {
            get => _expenseAmount;
            set => SetProperty(ref _expenseAmount, value);
        }

        private DateTime _expenseDate = DateTime.Now;
        public DateTime ExpenseDate
        {
            get => _expenseDate;
            set => SetProperty(ref _expenseDate, value);
        }

        private string _expenseNote = string.Empty;
        public string ExpenseNote
        {
            get => _expenseNote;
            set => SetProperty(ref _expenseNote, value);
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilteredExpenses?.Refresh();
                }
            }
        }

        public string ExpenseSearchKeyword
        {
            get => SearchText;
            set => SearchText = value;
        }

        public decimal MonthlyTotalExpense => Expenses?.Where(e => e.ExpenseDate.Month == DateTime.Today.Month && e.ExpenseDate.Year == DateTime.Today.Year).Sum(e => e.Amount) ?? 0;
        public int MonthlyExpenseCount => Expenses?.Where(e => e.ExpenseDate.Month == DateTime.Today.Month && e.ExpenseDate.Year == DateTime.Today.Year).Count() ?? 0;

        public ICommand OpenManageModalCommand { get; }
        public ICommand OpenAddExpenseModalCommand { get; }
        public ICommand SaveExpenseCommand { get; }
        public ICommand DeleteExpenseCommand { get; }
        public ICommand CloseModalCommand { get; }
        public ICommand ClearExpenseCommand { get; }
        public ICommand SearchExpenseCommand { get; }
        public ICommand ClearExpenseFilterCommand { get; }

        public ExpenseViewModel()
        {
            _databaseService = new DatabaseService();

            FilteredExpenses = CollectionViewSource.GetDefaultView(Expenses);
            FilteredExpenses.Filter = item =>
            {
                if (string.IsNullOrWhiteSpace(SearchText)) return true;
                if (item is Expense model)
                {
                    string query = SearchText.Trim().ToLower();
                    bool matchCode = !string.IsNullOrEmpty(model.Code) && model.Code.ToLower().Contains(query);
                    bool matchDesc = !string.IsNullOrEmpty(model.Description) && model.Description.ToLower().Contains(query);
                    bool matchCat = !string.IsNullOrEmpty(model.Category) && model.Category.ToLower().Contains(query);
                    return matchCode || matchDesc || matchCat;
                }
                return false;
            };

            OpenManageModalCommand = new RelayCommand(param =>
            {
                if (param is Expense item) SelectedExpense = item;
                ExecuteOpenManageModal();
            });
            OpenAddExpenseModalCommand = new RelayCommand(_ => ExecuteOpenAddModal());
            SaveExpenseCommand = new RelayCommand(_ => ExecuteSaveExpense());
            DeleteExpenseCommand = new RelayCommand(_ => ExecuteDeleteExpense());
            CloseModalCommand = new RelayCommand(_ => ExecuteCloseModal());
            ClearExpenseCommand = new RelayCommand(_ => ClearExpenseForm());
            SearchExpenseCommand = new RelayCommand(_ => FilteredExpenses?.Refresh());
            ClearExpenseFilterCommand = new RelayCommand(_ =>
            {
                SearchText = string.Empty;
                FilteredExpenses?.Refresh();
            });

            LoadExpenses();
        }

        private string GenerateNextExpenseCode()
        {
            int maxId = (Expenses != null && Expenses.Count > 0) ? Expenses.Max(e => e.Id) : 0;
            return $"EXP-{(maxId + 1):D3}";
        }

        public void LoadExpenses()
        {
            Expenses.Clear();
            var items = _databaseService.GetExpenses();
            foreach (var item in items)
            {
                Expenses.Add(item);
            }
            FilteredExpenses?.Refresh();

            OnPropertyChanged(nameof(MonthlyTotalExpense));
            OnPropertyChanged(nameof(MonthlyExpenseCount));
        }

        public void ExecuteOpenManageModal()
        {
            if (SelectedExpense == null || SelectedExpense.Id <= 0) return;

            IsEditMode = true;
            ModalTitle = "⚙️ แก้ไขข้อมูลรายจ่าย";
            ExpenseValidationMessage = string.Empty;

            SelectedExpenseCategory = SelectedExpense.Category;
            ExpenseDescription = SelectedExpense.Description;
            ExpenseAmount = SelectedExpense.Amount;
            ExpenseDate = SelectedExpense.ExpenseDate;
            ExpenseNote = SelectedExpense.Note;

            IsModalOpen = true;
        }

        public void ExecuteOpenAddModal()
        {
            ClearExpenseForm();
            IsEditMode = false;
            ModalTitle = "➕ เพิ่มข้อมูลรายจ่าย";
            ExpenseValidationMessage = string.Empty;
            ExpenseDate = DateTime.Now;
            IsModalOpen = true;
        }

        public void ExecuteSaveExpense()
        {
            if (string.IsNullOrWhiteSpace(ExpenseDescription))
            {
                ExpenseValidationMessage = "กรุณากรอกรายละเอียด/ชื่อรายการรายจ่าย";
                return;
            }

            if (ExpenseAmount <= 0)
            {
                ExpenseValidationMessage = "กรุณากรอกจำนวนเงินรายจ่ายที่ถูกต้อง";
                return;
            }

            if (IsEditMode && SelectedExpense != null && SelectedExpense.Id > 0)
            {
                SelectedExpense.Category = SelectedExpenseCategory;
                SelectedExpense.Description = ExpenseDescription;
                SelectedExpense.Amount = ExpenseAmount;
                SelectedExpense.ExpenseDate = ExpenseDate;
                SelectedExpense.Note = ExpenseNote;
                if (SelectedExpense.Emp_ID <= 0)
                {
                    SelectedExpense.Emp_ID = 1;
                }

                _databaseService.UpdateExpense(SelectedExpense);
                ClearExpenseForm();
                IsModalOpen = false;
                LoadExpenses();
            }
            else
            {
                string autoCode = GenerateNextExpenseCode();
                var expense = new Expense
                {
                    Code = autoCode,
                    Category = string.IsNullOrWhiteSpace(SelectedExpenseCategory) ? "ค่าใช้จ่ายอื่นๆ" : SelectedExpenseCategory,
                    Description = ExpenseDescription,
                    Amount = ExpenseAmount,
                    ExpenseDate = ExpenseDate,
                    Note = ExpenseNote,
                    Emp_ID = 1 // Fallback ใช้ Admin ID = 1
                };

                _databaseService.SaveExpense(expense);
                ClearExpenseForm();
                IsModalOpen = false;
                LoadExpenses();
            }
        }

        public void ExecuteDeleteExpense()
        {
            if (SelectedExpense == null || SelectedExpense.Id <= 0) return;

            _databaseService.DeleteExpense(SelectedExpense.Id);
            ClearExpenseForm();
            IsModalOpen = false;
            LoadExpenses();
        }

        public void ExecuteCloseModal()
        {
            ClearExpenseForm();
            IsModalOpen = false;
        }

        public void ClearExpenseForm()
        {
            SelectedExpense = null;
            SelectedExpenseCategory = "ค่าน้ำ/ค่าไฟ";
            ExpenseDescription = "ค่าน้ำ/ค่าไฟ";
            ExpenseAmount = 0;
            ExpenseDate = DateTime.Now;
            ExpenseNote = string.Empty;
            ExpenseValidationMessage = string.Empty;
        }
    }
}
