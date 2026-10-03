using Porjai20.Models;
using Porjai20.ViewModels;
using System.Collections.Generic;
using System.Windows.Input;

namespace Porjai20.Services
{
    /// <summary>
    /// Single source of truth สำหรับรายการเมนูทั้งหมดบนหน้า Home Dashboard
    /// แต่ละรายการระบุ Category, สี (semantic), Icon, ลำดับการจัดเรียง และ Role ที่มีสิทธิ์
    /// </summary>
    public static class MenuConfigService
    {
        /// <summary>
        /// สร้างรายการเมนูพร้อม Command จาก ProductViewModel
        /// </summary>
        public static List<MenuItemModel> BuildMenuItems(ProductViewModel vm)
        {
            return new List<MenuItemModel>
            {
                // ─── กลุ่ม: งานขายและลูกค้า (โทนสีน้ำเงิน) ───────────────────────────────────
                new MenuItemModel
                {
                    Id = "delivery",
                    Label = "จัดส่งสินค้า",
                    IconPathKey = "IconTruck",
                    Category = MenuCategory.Sales,
                    IconBgColorKey = "MenuSalesBg",
                    IconFgColorKey = "MenuSalesFg",
                    RequiredRole = null,
                    NavigateCommand = vm.OpenDeliveryCommand,
                    SortOrder = 1
                },
                new MenuItemModel
                {
                    Id = "claim",
                    Label = "เคลมสินค้า",
                    IconGlyph = "\uE8B9",
                    Category = MenuCategory.Sales,
                    IconBgColorKey = "MenuSalesBg",
                    IconFgColorKey = "MenuSalesFg",
                    RequiredRole = null,
                    NavigateCommand = vm.OpenClaimCommand,
                    SortOrder = 2
                },
                new MenuItemModel
                {
                    Id = "promotion",
                    Label = "โปรโมชั่น",
                    IconGlyph = "\uE8EC",
                    Category = MenuCategory.Sales,
                    IconBgColorKey = "MenuSalesBg",
                    IconFgColorKey = "MenuSalesFg",
                    RequiredRole = null,
                    NavigateCommand = vm.OpenPromotionCommand,
                    SortOrder = 3
                },
                new MenuItemModel
                {
                    Id = "sales_history",
                    Label = "ประวัติการขาย",
                    IconGlyph = "\uE81C",
                    Category = MenuCategory.Sales,
                    IconBgColorKey = "MenuSalesBg",
                    IconFgColorKey = "MenuSalesFg",
                    RequiredRole = null,
                    NavigateCommand = vm.OpenSalesHistoryCommand,
                    SortOrder = 4
                },

                // ─── กลุ่ม: คลังและซัพพลาย (โทนสีเขียว) ──────────────────────────────────────
                new MenuItemModel
                {
                    Id = "stock_manage",
                    Label = "สต็อกสินค้า",
                    IconGlyph = "\uE8B7",
                    Category = MenuCategory.Inventory,
                    IconBgColorKey = "MenuInventoryBg",
                    IconFgColorKey = "MenuInventoryFg",
                    RequiredRole = null,
                    NavigateCommand = vm.SwitchToAdminCommand,
                    SortOrder = 1
                },
                new MenuItemModel
                {
                    Id = "purchase_order",
                    Label = "สั่งซื้อสินค้า",
                    IconGlyph = "\uE8C0",
                    Category = MenuCategory.Inventory,
                    IconBgColorKey = "MenuInventoryBg",
                    IconFgColorKey = "MenuInventoryFg",
                    RequiredRole = RolePermissions.RoleAdmin,
                    NavigateCommand = vm.OpenPurchaseOrderCommand,
                    SortOrder = 2
                },
                new MenuItemModel
                {
                    Id = "stock_in",
                    Label = "รับเข้าสินค้า",
                    IconGlyph = "\uE896",
                    Category = MenuCategory.Inventory,
                    IconBgColorKey = "MenuInventoryBg",
                    IconFgColorKey = "MenuInventoryFg",
                    RequiredRole = null,
                    NavigateCommand = vm.SwitchToStockInCommand,
                    SortOrder = 3
                },

                // ─── กลุ่ม: ธุรกิจและบุคคล (โทนสีส้ม) ───────────────────────────────────────
                new MenuItemModel
                {
                    Id = "customer",
                    Label = "ลูกค้า",
                    IconGlyph = "\uE77B",
                    Category = MenuCategory.Business,
                    IconBgColorKey = "MenuBusinessBg",
                    IconFgColorKey = "MenuBusinessFg",
                    RequiredRole = null,
                    NavigateCommand = vm.OpenCustomerCommand,
                    SortOrder = 1
                },
                new MenuItemModel
                {
                    Id = "staff",
                    Label = "พนักงาน",
                    IconGlyph = "\uE716",
                    Category = MenuCategory.Business,
                    IconBgColorKey = "MenuBusinessBg",
                    IconFgColorKey = "MenuBusinessFg",
                    RequiredRole = RolePermissions.RoleAdmin,
                    NavigateCommand = vm.SwitchToStaffCommand,
                    SortOrder = 2
                },
                new MenuItemModel
                {
                    Id = "partner",
                    Label = "บริษัทคู่ค้า",
                    IconGlyph = "\uE82D",
                    Category = MenuCategory.Business,
                    IconBgColorKey = "MenuBusinessBg",
                    IconFgColorKey = "MenuBusinessFg",
                    RequiredRole = RolePermissions.RoleAdmin,
                    NavigateCommand = vm.OpenPartnerCommand,
                    SortOrder = 3
                },
                new MenuItemModel
                {
                    Id = "expense",
                    Label = "รายจ่าย",
                    IconPathKey = "IconMoney",
                    Category = MenuCategory.Business,
                    IconBgColorKey = "MenuBusinessBg",
                    IconFgColorKey = "MenuBusinessFg",
                    RequiredRole = RolePermissions.RoleAdmin,
                    NavigateCommand = vm.OpenExpenseCommand,
                    SortOrder = 4
                },
                new MenuItemModel
                {
                    Id = "reports",
                    Label = "รายงานสรุปผล",
                    IconPathKey = "IconChart",
                    Category = MenuCategory.Business,
                    IconBgColorKey = "MenuBusinessBg",
                    IconFgColorKey = "MenuBusinessFg",
                    RequiredRole = RolePermissions.RoleAdmin,
                    NavigateCommand = vm.SwitchToReportsCommand,
                    SortOrder = 5
                },
            };
        }
    }
}
