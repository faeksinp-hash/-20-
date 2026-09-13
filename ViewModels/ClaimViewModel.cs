using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;
using Porjai20.Models;
using Porjai20.Services;

namespace Porjai20.ViewModels
{
    public partial class ProductViewModel
    {
        private bool _isModalOpen;
        public bool IsModalOpen
        {
            get => _isModalOpen;
            set => SetProperty(ref _isModalOpen, value);
        }

        public bool IsClaimSelected => SelectedClaim != null && SelectedClaim.Id > 0;

        private bool _isClaimDetailModalOpen;
        public bool IsClaimDetailModalOpen
        {
            get => _isClaimDetailModalOpen;
            set
            {
                if (SetProperty(ref _isClaimDetailModalOpen, value) && !value)
                {
                    SelectedClaim = null;
                    OnPropertyChanged(nameof(IsClaimSelected));
                }
            }
        }

        // --- Dynamic Receipt Item Selector State ---
        private bool _isReceiptProductSelectorOpen;
        public bool IsReceiptProductSelectorOpen
        {
            get => _isReceiptProductSelectorOpen;
            set => SetProperty(ref _isReceiptProductSelectorOpen, value);
        }

        public ObservableCollection<ClaimReceiptItemSelection> AvailableReceiptItems { get; } = new ObservableCollection<ClaimReceiptItemSelection>();
        public ObservableCollection<ClaimItemLine> CurrentClaimItems { get; } = new ObservableCollection<ClaimItemLine>();

        public ICommand OpenAddClaimModalCommand { get; private set; }
        public ICommand OpenEditModalCommand { get; private set; }
        public ICommand CloseModalCommand { get; private set; }
        public ICommand LookupReceiptCommand { get; private set; }
        public ICommand CloseClaimDetailModalCommand { get; private set; }
        public ICommand ApproveClaimActionCommand { get; private set; }
        public ICommand RejectClaimActionCommand { get; private set; }
        public ICommand ReplaceProductClaimActionCommand { get; private set; }
        public ICommand ToggleReceiptProductSelectorCommand { get; private set; }
        public ICommand AddSelectedReceiptItemsCommand { get; private set; }
        public ICommand RemoveClaimItemLineCommand { get; private set; }

        private string _claimStatusFilter = "ทั้งหมด";
        public string ClaimStatusFilter
        {
            get => _claimStatusFilter;
            set
            {
                string normValue = value;
                if (string.IsNullOrWhiteSpace(value) || 
                    value.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    normValue = "ทั้งหมด";
                }
                if (SetProperty(ref _claimStatusFilter, normValue))
                {
                    _ = LoadClaims();
                }
            }
        }

        private DateTime _claimStartDate = DateTime.Today.AddMonths(-1);
        public DateTime ClaimStartDate
        {
            get => _claimStartDate;
            set { if (SetProperty(ref _claimStartDate, value)) _ = LoadClaims(); }
        }

        private DateTime _claimEndDate = DateTime.Today;
        public DateTime ClaimEndDate
        {
            get => _claimEndDate;
            set { if (SetProperty(ref _claimEndDate, value)) _ = LoadClaims(); }
        }

        private int _pendingClaimsCount;
        public int PendingClaimsCount
        {
            get => _pendingClaimsCount;
            private set => SetProperty(ref _pendingClaimsCount, value);
        }

        private int _approvedClaimsCount;
        public int ApprovedClaimsCount
        {
            get => _approvedClaimsCount;
            private set => SetProperty(ref _approvedClaimsCount, value);
        }

        private int _completedClaimsCount;
        public int CompletedClaimsCount
        {
            get => _completedClaimsCount;
            private set => SetProperty(ref _completedClaimsCount, value);
        }

        private async Task LoadClaims()
        {
            var rawClaims = await Task.Run(() => _databaseService.GetClaims());
            var claimsList = (rawClaims ?? Enumerable.Empty<Claim>()).ToList();

            if (ClaimStartDate != default && ClaimEndDate != default)
            {
                var start = ClaimStartDate.Date;
                var end = ClaimEndDate.Date.AddDays(1).AddTicks(-1);
                claimsList = claimsList.Where(c => c.CreatedDate >= start && c.CreatedDate <= end).ToList();
            }

            // Summary KPI Cards (calculated from all active records in the date range)
            PendingClaimsCount = claimsList.Count(c => 
                string.Equals(c.Status, "รอดำเนินการ", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(c.Status, "Pending", StringComparison.OrdinalIgnoreCase));

            ApprovedClaimsCount = claimsList.Count(c => 
                string.Equals(c.Status, "อนุมัติแล้ว", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(c.Status, "อนุมัติเคลม", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(c.Status, "เปลี่ยนสินค้าใหม่", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(c.Status, "ได้รับของใหม่", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(c.Status, "คืนเงิน", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(c.Status, "Approved", StringComparison.OrdinalIgnoreCase));

            CompletedClaimsCount = claimsList.Count(c => 
                string.Equals(c.Status, "เคลมสำเร็จ", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(c.Status, "อนุมัติแล้ว", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(c.Status, "เสร็จสิ้น", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(c.Status, "จัดส่งสำเร็จ", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(c.Status, "Completed", StringComparison.OrdinalIgnoreCase));

            // Filtering for DataGrid display
            IEnumerable<Claim> filtered = claimsList;

            // Search Keyword Filter
            if (!string.IsNullOrWhiteSpace(ClaimSearchKeyword))
            {
                var kw = ClaimSearchKeyword.Trim();
                filtered = filtered.Where(c =>
                    (!string.IsNullOrEmpty(c.SalesOrderRefNo) && c.SalesOrderRefNo.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(c.CustomerName) && c.CustomerName.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(c.CustomerPhone) && c.CustomerPhone.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(c.ProductName) && c.ProductName.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(c.ProductCode) && c.ProductCode.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(c.ClaimNo) && c.ClaimNo.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(c.StockInRefNo) && c.StockInRefNo.Contains(kw, StringComparison.OrdinalIgnoreCase))
                );
            }

            // Claim Status Filter Mapping
            if (!string.IsNullOrWhiteSpace(ClaimStatusFilter) && ClaimStatusFilter != "ทั้งหมด")
            {
                if (ClaimStatusFilter == "เคลมสำเร็จ")
                {
                    filtered = filtered.Where(c =>
                        string.Equals(c.Status, "เคลมสำเร็จ", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.Status, "อนุมัติแล้ว", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.Status, "เสร็จสิ้น", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.Status, "จัดส่งสำเร็จ", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.Status, "Completed", StringComparison.OrdinalIgnoreCase));
                }
                else if (ClaimStatusFilter == "ปฏิเสธ")
                {
                    filtered = filtered.Where(c =>
                        string.Equals(c.Status, "ปฏิเสธการเคลม", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.Status, "ปฏิเสธ", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.Status, "ยกเลิก", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.Status, "Rejected", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.Status, "Voided", StringComparison.OrdinalIgnoreCase));
                }
                else if (ClaimStatusFilter == "รอดำเนินการ")
                {
                    filtered = filtered.Where(c =>
                        string.Equals(c.Status, "รอดำเนินการ", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.Status, "Pending", StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    filtered = filtered.Where(c => string.Equals(c.Status, ClaimStatusFilter, StringComparison.OrdinalIgnoreCase));
                }
            }

            Claims.Clear();
            foreach (var c in filtered)
                Claims.Add(c);

            OnPropertyChanged(nameof(PendingClaimsCount));
            OnPropertyChanged(nameof(ApprovedClaimsCount));
            OnPropertyChanged(nameof(CompletedClaimsCount));
            OnPropertyChanged(nameof(IsClaimSelected));
        }

        private void InitializeClaimModal()
        {
            OpenAddClaimModalCommand = new RelayCommand(_ => OpenAddModal());
            OpenEditModalCommand = new RelayCommand(_ => OpenEditModal(), _ => IsClaimSelected);
            CloseModalCommand = new RelayCommand(_ => CloseModal());
            LookupReceiptCommand = new RelayCommand(_ => LookupReceipt());
            CloseClaimDetailModalCommand = new RelayCommand(_ => CloseClaimDetail());
            ApproveClaimActionCommand = new RelayCommand(_ => ExecuteApproveClaim(), _ => IsClaimSelected);
            ReplaceProductClaimActionCommand = new RelayCommand(_ => ExecuteReplaceProductClaim(), _ => IsClaimSelected);
            RejectClaimActionCommand = new RelayCommand(_ => ExecuteRejectClaim(), _ => IsClaimSelected);

            ToggleReceiptProductSelectorCommand = new RelayCommand(_ =>
            {
                IsReceiptProductSelectorOpen = !IsReceiptProductSelectorOpen;
            });

            AddSelectedReceiptItemsCommand = new RelayCommand(_ =>
            {
                var selected = AvailableReceiptItems.Where(x => x.IsSelected).ToList();
                if (selected.Count == 0)
                {
                    ShowAlert("กรุณาทำเครื่องหมายเลือกสินค้าในรายการใบเสร็จอย่างน้อย 1 รายการ", "แจ้งเตือน", "⚠️");
                    return;
                }

                foreach (var item in selected)
                {
                    var existing = CurrentClaimItems.FirstOrDefault(c => c.Pro_ID == item.Pro_ID);
                    if (existing != null)
                    {
                        existing.ClaimQty = item.ClaimQty;
                    }
                    else
                    {
                        CurrentClaimItems.Add(new ClaimItemLine
                        {
                            Pro_ID = item.Pro_ID,
                            ProductCode = item.ProductCode,
                            ProductName = item.ProductName,
                            ReceiptQty = item.ReceiptQty,
                            ClaimQty = item.ClaimQty
                        });
                    }
                }

                // Sync main claim item fields with first item for backwards compatibility
                if (CurrentClaimItems.Count > 0)
                {
                    var first = CurrentClaimItems[0];
                    ClaimProId = first.Pro_ID;
                    ClaimProductCode = first.ProductCode;
                    ClaimProductName = first.ProductName;
                    ClaimQuantity = first.ClaimQty;
                }

                // Collapse selector immediately as requested
                IsReceiptProductSelectorOpen = false;
            });

            RemoveClaimItemLineCommand = new RelayCommand(param =>
            {
                if (param is ClaimItemLine line)
                {
                    CurrentClaimItems.Remove(line);
                    if (CurrentClaimItems.Count > 0)
                    {
                        var first = CurrentClaimItems[0];
                        ClaimProId = first.Pro_ID;
                        ClaimProductCode = first.ProductCode;
                        ClaimProductName = first.ProductName;
                        ClaimQuantity = first.ClaimQty;
                    }
                    else
                    {
                        ClaimProId = 0;
                        ClaimProductCode = string.Empty;
                        ClaimProductName = string.Empty;
                        ClaimQuantity = 1;
                    }
                }
            });

            // Listen to SelectedClaim changes to notify UI of IsClaimSelected
            this.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(SelectedClaim))
                {
                    OnPropertyChanged(nameof(IsClaimSelected));
                }
            };
        }

        private void ExecuteApproveClaim()
        {
            if (RolePermissions.IsReadOnly(CurrentUser?.Role, "claim"))
            {
                ShowAlert("ไม่มีสิทธิ์ในการดำเนินการนี้ (สิทธิ์ดูอย่างเดียว)", "ไม่มีสิทธิ์", "⚠️");
                return;
            }

            if (SelectedClaim == null || SelectedClaim.Id == 0) return;

            SelectedClaim.Status = "อนุมัติแล้ว";
            ClaimStatus = "อนุมัติแล้ว";
            _databaseService.UpdateClaim(SelectedClaim);

            // Close modal immediately and show modern success alert
            IsClaimDetailModalOpen = false;
            SelectedClaim = null;
            ShowAlert("บันทึกการอนุมัติเคลมเรียบร้อยแล้ว", "สำเร็จ", "✅");
            _ = LoadClaims();
        }

        private void ExecuteReplaceProductClaim()
        {
            if (RolePermissions.IsReadOnly(CurrentUser?.Role, "claim"))
            {
                ShowAlert("ไม่มีสิทธิ์ในการดำเนินการนี้ (สิทธิ์ดูอย่างเดียว)", "ไม่มีสิทธิ์", "⚠️");
                return;
            }

            if (SelectedClaim == null || SelectedClaim.Id == 0) return;

            SelectedClaim.Status = "เปลี่ยนสินค้าใหม่";
            SelectedClaim.ClaimAction = "เปลี่ยนสินค้าใหม่";
            ClaimStatus = "เปลี่ยนสินค้าใหม่";
            ClaimAction = "เปลี่ยนสินค้าใหม่";
            _databaseService.UpdateClaim(SelectedClaim);

            // Deduct stock for replacement product if valid product id
            if (SelectedClaim.Pro_ID > 0)
            {
                _databaseService.DeductProductStockForClaim(SelectedClaim.Pro_ID, SelectedClaim.Quantity > 0 ? SelectedClaim.Quantity : 1);
            }

            // Close modal immediately and show modern success alert
            IsClaimDetailModalOpen = false;
            SelectedClaim = null;
            ShowAlert("บันทึกการเปลี่ยนสินค้าเรียบร้อยแล้ว", "สำเร็จ", "✅");
            _ = LoadClaims();
        }

        private void ExecuteRejectClaim()
        {
            if (RolePermissions.IsReadOnly(CurrentUser?.Role, "claim"))
            {
                ShowAlert("ไม่มีสิทธิ์ในการดำเนินการนี้ (สิทธิ์ดูอย่างเดียว)", "ไม่มีสิทธิ์", "⚠️");
                return;
            }

            if (SelectedClaim == null || SelectedClaim.Id == 0) return;

            SelectedClaim.Status = "ปฏิเสธการเคลม";
            ClaimStatus = "ปฏิเสธการเคลม";
            _databaseService.UpdateClaim(SelectedClaim);

            // Close modal immediately and show modern success alert
            IsClaimDetailModalOpen = false;
            SelectedClaim = null;
            ShowAlert("ปฏิเสธรายการเคลมเรียบร้อยแล้ว", "สำเร็จ", "✅");
            _ = LoadClaims();
        }

        private void ExecuteChangeClaimStatus(string status, string action = null)
        {
            if (RolePermissions.IsReadOnly(CurrentUser?.Role, "claim"))
            {
                ShowAlert("ไม่มีสิทธิ์ในการดำเนินการนี้ (สิทธิ์ดูอย่างเดียว)", "ไม่มีสิทธิ์", "⚠️");
                return;
            }

            if (SelectedClaim == null || SelectedClaim.Id == 0) return;
            SelectedClaim.Status = status;
            ClaimStatus = status;
            if (!string.IsNullOrWhiteSpace(action))
            {
                SelectedClaim.ClaimAction = action;
                ClaimAction = action;
            }
            _databaseService.UpdateClaim(SelectedClaim);
            _ = LoadClaims();
        }

        private void CloseClaimDetail()
        {
            IsClaimDetailModalOpen = false;
            SelectedClaim = null;
        }

        private void LookupReceipt()
        {
            if (ClaimType == "ลูกค้า")
            {
                LookupSalesOrderForClaim();
            }
            else
            {
                LookupStockInForClaim();
            }
        }

        private void OpenAddModal()
        {
            ClearClaimForm();
            IsModalOpen = true;
        }

        private void OpenEditModal()
        {
            if (SelectedClaim != null && SelectedClaim.Id > 0)
            {
                // Fields are already mapped by SelectedClaim's setter in ProductViewModel.cs
                IsModalOpen = true;
            }
        }

        private void CloseModal()
        {
            ClearClaimForm();
            IsModalOpen = false;
        }
    }
}

