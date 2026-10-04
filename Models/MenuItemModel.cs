using System.Windows.Input;

namespace Porjai20.Models
{
    /// <summary>
    /// หมวดหมู่ของเมนูบนหน้า Home
    /// </summary>
    public enum MenuCategory
    {
        Sales,     // งานขายและลูกค้า
        Inventory, // คลังและซัพพลาย
        Business   // ธุรกิจและบุคคล (Admin เท่านั้น)
    }

    /// <summary>
    /// โมเดลสำหรับเมนูแต่ละรายการบนหน้า Home Dashboard
    /// </summary>
    public class MenuItemModel
    {
        /// <summary>ชื่อ unique ของเมนู</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>ชื่อที่แสดงบนการ์ด</summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// ตัวอักษร Segoe MDL2 Assets เช่น "\uE81C"
        /// ใช้แทน Path icon สำหรับไอคอนส่วนใหญ่
        /// </summary>
        public string? IconGlyph { get; set; }

        /// <summary>
        /// StaticResource key ของ Geometry Path สำหรับไอคอนที่ใช้ Path
        /// เช่น "IconChart", "IconMoney", "IconTruck"
        /// </summary>
        public string? IconPathKey { get; set; }

        /// <summary>หมวดหมู่ของเมนู ใช้สำหรับ Grouping</summary>
        public MenuCategory Category { get; set; }

        /// <summary>Key ของสีพื้นหลังวงกลมไอคอน (SolidColorBrush ใน App.xaml)</summary>
        public string IconBgColorKey { get; set; } = string.Empty;

        /// <summary>Key ของสีไอคอน (SolidColorBrush ใน App.xaml)</summary>
        public string IconFgColorKey { get; set; } = string.Empty;

        /// <summary>
        /// Role ที่ต้องการ: null = ทุก role, "Admin" = เฉพาะ Admin
        /// </summary>
        public string? RequiredRole { get; set; }

        /// <summary>Command ที่จะรันเมื่อกดการ์ดนี้</summary>
        public ICommand? NavigateCommand { get; set; }

        /// <summary>ลำดับการจัดเรียงในกลุ่ม</summary>
        public int SortOrder { get; set; }

        /// <summary>ชื่อหมวดหมู่ที่แสดงเป็นภาษาไทย</summary>
        public string CategoryDisplayName => Category switch
        {
            MenuCategory.Sales     => "การขาย & บริการ",
            MenuCategory.Inventory => "จัดการคลังสินค้า",
            MenuCategory.Business  => "บริหารจัดการร้าน",
            _                      => string.Empty
        };
    }
}
