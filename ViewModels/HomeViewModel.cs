using Porjai20.Models;
using Porjai20.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace Porjai20.ViewModels
{
    /// <summary>
    /// ViewModel สำหรับหน้า Home Dashboard
    /// รับผิดชอบ: filter เมนูตาม role, จัด group ตาม Category
    /// </summary>
    public class HomeViewModel : ViewModelBase
    {
        // อ้างอิง ProductViewModel หลักของแอป (เพื่อ pass Commands และ CurrentUser)
        private readonly ProductViewModel _root;

        private ObservableCollection<MenuItemModel> _filteredMenuItems;

        /// <summary>รายการเมนูที่ filter ตาม role แล้ว (ไม่รวม POS ที่เป็น Hero)</summary>
        public ObservableCollection<MenuItemModel> FilteredMenuItems
        {
            get => _filteredMenuItems;
            private set => SetProperty(ref _filteredMenuItems, value);
        }

        private ICollectionView? _groupedMenuItems;

        /// <summary>
        /// CollectionView ที่ group ตาม CategoryDisplayName
        /// ใช้ bind กับ ItemsControl + GroupStyle ใน XAML
        /// </summary>
        public ICollectionView? GroupedMenuItems
        {
            get => _groupedMenuItems;
            private set => SetProperty(ref _groupedMenuItems, value);
        }

        // เปิดเผย ProductViewModel ออกมาเพื่อให้ XAML bind Login/Logout/CurrentUser ได้โดยตรง
        public ProductViewModel Root => _root;

        public HomeViewModel(ProductViewModel root)
        {
            _root = root;
            _filteredMenuItems = new ObservableCollection<MenuItemModel>();

            // ติดตามการเปลี่ยนแปลง CurrentUser เพื่อ refresh เมนู
            _root.PropertyChanged += Root_PropertyChanged;

            RefreshMenuItems();
        }

        private void Root_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProductViewModel.CurrentUser) ||
                e.PropertyName == nameof(ProductViewModel.IsAdmin))
            {
                RefreshMenuItems();
            }
        }

        /// <summary>
        /// สร้างรายการเมนูใหม่ตาม role ของผู้ใช้ปัจจุบัน
        /// </summary>
        public void RefreshMenuItems()
        {
            var allItems = MenuConfigService.BuildMenuItems(_root);
            var userRole = _root.CurrentUser?.Role; // null = ยังไม่ล็อกอิน

            // filter: ใช้ RolePermissions.CanAccessMenu เป็น Single Source of Truth
            var filtered = allItems
                .Where(m => RolePermissions.CanAccessMenu(userRole, m.Id))
                .ToList();

            FilteredMenuItems = new ObservableCollection<MenuItemModel>(filtered);

            // สร้าง CollectionView พร้อม GroupDescription ตาม Category
            var view = CollectionViewSource.GetDefaultView(FilteredMenuItems);
            view.GroupDescriptions.Clear();
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(MenuItemModel.CategoryDisplayName)));

            // จัด sort ตามลำดับ Category (Sales -> Inventory -> Business)
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription(nameof(MenuItemModel.Category), ListSortDirection.Ascending));

            GroupedMenuItems = view;
        }
    }
}
