using System;
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

        public ICommand OpenAddClaimModalCommand { get; private set; }
        public ICommand OpenEditModalCommand { get; private set; }
        public ICommand CloseModalCommand { get; private set; }
        public ICommand LookupReceiptCommand { get; private set; }
        public ICommand CloseClaimDetailModalCommand { get; private set; }
        public ICommand ApproveClaimActionCommand { get; private set; }
        public ICommand RejectClaimActionCommand { get; private set; }
        public ICommand ReplaceProductClaimActionCommand { get; private set; }

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

        public int PendingClaimsCount => Claims.Count(c => c.Status == "รอดำเนินการ" || c.Status == "Pending");
        public int ApprovedClaimsCount => Claims.Count(c => c.Status == "เคลมสำเร็จ" || c.Status == "ได้รับของใหม่" || c.Status == "อนุมัติเคลม" || c.Status == "คืนเงิน" || c.Status == "Approved");
        public int CompletedClaimsCount => Claims.Count(c => c.Status == "ยกเลิก" || c.Status == "จัดส่งสำเร็จ" || c.Status == "เสร็จสิ้น" || c.Status == "Completed");

        private async Task LoadClaims()
        {
            Claims.Clear();
            var claims = await Task.Run(() => _databaseService.GetClaims(ClaimSearchKeyword, ClaimStatusFilter));

            if (ClaimStartDate != default && ClaimEndDate != default)
            {
                var start = ClaimStartDate.Date;
                var end = ClaimEndDate.Date.AddDays(1).AddTicks(-1);
                claims = claims.Where(c => c.CreatedDate >= start && c.CreatedDate <= end);
            }

            foreach (var c in claims)
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
            ApproveClaimActionCommand = new RelayCommand(_ => ExecuteChangeClaimStatus("ได้รับของใหม่"), _ => IsClaimSelected);
            RejectClaimActionCommand = new RelayCommand(_ => ExecuteChangeClaimStatus("ยกเลิก"), _ => IsClaimSelected);
            ReplaceProductClaimActionCommand = new RelayCommand(_ => ExecuteChangeClaimStatus("คืนเงิน"), _ => IsClaimSelected);

            // Listen to SelectedClaim changes to notify UI of IsClaimSelected
            this.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(SelectedClaim))
                {
                    OnPropertyChanged(nameof(IsClaimSelected));
                }
            };
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
