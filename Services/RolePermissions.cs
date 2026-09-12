namespace Porjai20.Services
{
    /// <summary>
    /// Single Source of Truth สำหรับการกำหนดสิทธิ์และการตรวจสอบ Role ในระบบ
    /// มี 3 สิทธิ์หลัก: "เจ้าของร้าน", "ผู้ดูแลระบบ", "พนักงานทั่วไป"
    /// </summary>
    public static class RolePermissions
    {
        public const string RoleOwner = "เจ้าของร้าน";
        public const string RoleAdmin = "ผู้ดูแลระบบ";
        public const string RoleEmployee = "พนักงานทั่วไป";

        public enum AccessLevel
        {
            None,       // ไม่มีสิทธิ์เข้าถึง (ซ่อนเมนู / ปิดกั้น)
            ReadOnly,   // ดูได้อย่างเดียว (แสดงเมนู / ห้ามแก้ไข)
            Full        // มีสิทธิ์แก้ไขเต็มที่ (แสดงเมนู / แก้ไขได้)
        }

        /// <summary>
        /// ตรวจสอบว่าผู้ใช้มีสิทธิ์ระดับบริหาร (เจ้าของร้าน หรือ ผู้ดูแลระบบ) หรือไม่
        /// </summary>
        public static bool IsAdminOrOwner(string? role)
        {
            if (string.IsNullOrWhiteSpace(role)) return false;
            return role == RoleOwner || 
                   role == RoleAdmin || 
                   role.Equals("Admin", System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// ดึงระดับสิทธิ์การเข้าถึง (None, ReadOnly, Full) ของ Role ในแต่ละโมดูล/เมนู
        /// </summary>
        public static AccessLevel GetAccessLevel(string? role, string? menuId)
        {
            if (string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(menuId))
                return AccessLevel.None;

            // เจ้าของร้าน และ ผู้ดูแลระบบ มีสิทธิ์แก้ไข (Full) ทุกโมดูล/เมนู
            if (IsAdminOrOwner(role))
            {
                return AccessLevel.Full;
            }

            // พนักงานทั่วไป (Employee)
            return menuId.ToLowerInvariant() switch
            {
                // ข้อมูลพื้นฐาน (สินค้า/หมวดหมู่/ลูกค้า) -> ดูอย่างเดียว
                "stock_manage" => AccessLevel.ReadOnly,
                "customer"     => AccessLevel.ReadOnly,

                // สั่งซื้อสินค้า -> ไม่มีสิทธิ์เข้า
                "purchase_order" => AccessLevel.None,

                // รับเข้าสินค้า -> ดูอย่างเดียว
                "stock_in"     => AccessLevel.ReadOnly,

                // ขายหน้าร้าน (POS) -> แก้ไข
                "pos"          => AccessLevel.Full,

                // จัดส่งสินค้า -> แก้ไข
                "delivery"     => AccessLevel.Full,

                // ประวัติการขาย -> ดูอย่างเดียว
                "sales_history" => AccessLevel.ReadOnly,

                // รายจ่าย -> ไม่มีสิทธิ์เข้า
                "expense"      => AccessLevel.None,

                // เคลมสินค้า -> ดูอย่างเดียว
                "claim"        => AccessLevel.ReadOnly,

                // รายงาน -> ไม่มีสิทธิ์เข้า
                "reports"      => AccessLevel.None,

                // จัดการพนักงาน/ผู้ใช้ระบบ -> ไม่มีสิทธิ์เข้า
                "staff"        => AccessLevel.None,

                // ข้อมูลคู่ค้า -> ไม่มีสิทธิ์เข้า
                "partner"      => AccessLevel.None,

                // หน้าแรก (Home)
                "home"         => AccessLevel.Full,

                _              => AccessLevel.None
            };
        }

        /// <summary>
        /// ตรวจสอบว่า Role นี้สามารถมองเห็นหรือเข้าใช้งานเมนูนี้ได้หรือไม่ (Full หรือ ReadOnly)
        /// </summary>
        public static bool CanAccessMenu(string? role, string? menuId)
        {
            return GetAccessLevel(role, menuId) != AccessLevel.None;
        }

        /// <summary>
        /// ตรวจสอบว่า Role นี้มีสิทธิ์แบบดูอย่างเดียว (ReadOnly) ในเมนูนี้หรือไม่
        /// </summary>
        public static bool IsReadOnly(string? role, string? menuId)
        {
            return GetAccessLevel(role, menuId) == AccessLevel.ReadOnly;
        }
    }
}
