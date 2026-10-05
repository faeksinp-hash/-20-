using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Porjai20.Common;
using Porjai20.Models;

namespace Porjai20.Services
{
    /// <summary>
    /// ข้อมูลรายละเอียดโปรโมชั่นแต่ละรายการที่ถูกนำไปใช้ในบิล (สำหรับแสดงรายการแยกแถวในหน้าชำระเงินและบันทึก tblSalesDetail)
    /// </summary>
    public class AppliedPromotionResult
    {
        public string PromoID { get; set; } = string.Empty;
        public string PromoName { get; set; } = string.Empty;
        public string PromoType { get; set; } = "Discount"; // Discount, Gift, Freebie, Redeem
        public decimal DiscountAmount { get; set; }
        public string DisplayText { get; set; } = string.Empty;
        public string DisplayDiscount => $"-{DiscountAmount:N2}";
        public string TextColor { get; set; } = "#E11D48";

        [System.Text.Json.Serialization.JsonIgnore]
        public System.Windows.Media.Brush TextBrush
        {
            get
            {
                try
                {
                    return (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(TextColor);
                }
                catch
                {
                    return System.Windows.Media.Brushes.Crimson;
                }
            }
        }

        public int? TargetProductId { get; set; }
        public string? TargetProductName { get; set; }
        public int? GiftProductId { get; set; }
        public string? GiftProductName { get; set; }
        public int GiftQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsAutoIncluded { get; set; }
        public decimal AutoIncludedAmount { get; set; }
        public PromotionModel? Promotion { get; set; }
    }

    /// <summary>
    /// ผลลัพธ์จากการประเมินโปรโมชั่นอัตโนมัติที่ดีที่สุด (Auto-Apply Promotion Engine)
    /// </summary>
    public class AutoPromotionResult
    {
        public string PromoID { get; set; } = string.Empty;
        public string PromoName { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public decimal AutoIncludedAmount => AppliedItems?.Where(x => x.IsAutoIncluded).Sum(x => x.AutoIncludedAmount) ?? 0m;
        public PromotionModel? Promotion { get; set; }
        public List<PromotionModel> AppliedPromotions { get; set; } = new();
        public List<AppliedPromotionResult> AppliedItems { get; set; } = new();
    }

    /// <summary>
    /// ผลลัพธ์จากการประเมินโปรโมชั่นที่เคาน์เตอร์ POS
    /// </summary>
    public class PromotionEvaluationResult
    {
        public List<PromotionModel> AppliedPromotions { get; set; } = new();
        public decimal TotalDiscount { get; set; }
        public List<CartFreebieItem> FreebiesToAdd { get; set; } = new();
        public string SummaryText { get; set; } = string.Empty;
        public decimal TotalBenefit => TotalDiscount + FreebiesToAdd.Sum(f => f.Quantity * f.OriginalPrice);
    }

    /// <summary>
    /// รายการของแถม / ของแจก ที่ได้จากโปรโมชั่น
    /// </summary>
    public class CartFreebieItem
    {
        public string PromoID { get; set; } = string.Empty;
        public string PromoName { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal Cost { get; set; }
        public string FreebieType { get; set; } = "Freebie"; // Freebie, Gift
    }

    /// <summary>
    /// Service กลางสำหรับจัดการโปรโมชั่น (PromotionService)
    /// รองรับทั้ง CRUD แบบสมบูรณ์, Duplicate Logic, Auto-Expire, และ POS Evaluation Engine
    /// </summary>
    public class PromotionService
    {
        private static PromotionService? _instance;
        public static PromotionService Instance => _instance ??= new PromotionService();

        private readonly DatabaseService _db;

        public PromotionService() : this(new DatabaseService())
        {
        }

        public PromotionService(DatabaseService db)
        {
            _db = db;
        }

        #region CRUD Operations

        /// <summary>
        /// ดึงรายการโปรโมชั่นทั้งหมด พร้อมสินค้าที่ผูก และชื่อสินค้าของแถม
        /// </summary>
        public async Task<List<PromotionModel>> GetAllPromotionsAsync()
        {
            using (var conn = _db.GetConnection())
            {
                string sqlPromo = @"
                    SELECT 
                        PromoID, PromoCode, PromoName, PromoType, TargetScope,
                        DiscountType, DiscountAmount, MaxDiscountAmount, MinSpend,
                        PointsRequired, BuyQuantity, FreeQuantity, FreeProductID,
                        CanRepeat, IsMemberOnly, CanCombine, MaxUsagePerCustomer,
                        TotalQuota, CurrentUsage, StartDate, EndDate, IsActive,
                        IsDraft, CreatedAt
                    FROM tblPromotion
                    ORDER BY IsDraft ASC, IsActive DESC, PromoID ASC;";

                var promos = new List<PromotionModel>();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sqlPromo;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var p = new PromotionModel
                            {
                                PromoID = reader["PromoID"] != DBNull.Value ? reader["PromoID"].ToString()! : "",
                                PromoCode = reader["PromoCode"] != DBNull.Value ? reader["PromoCode"].ToString() : null,
                                PromoName = reader["PromoName"] != DBNull.Value ? reader["PromoName"].ToString()! : "",
                                PromoType = reader["PromoType"] != DBNull.Value ? reader["PromoType"].ToString()! : "Discount",
                                TargetScope = reader["TargetScope"] != DBNull.Value ? reader["TargetScope"].ToString()! : "AllStore",
                                DiscountType = reader["DiscountType"] != DBNull.Value ? reader["DiscountType"].ToString()! : "Cash",
                                DiscountAmount = reader["DiscountAmount"] != DBNull.Value ? Convert.ToDecimal(reader["DiscountAmount"]) : 0m,
                                MaxDiscountAmount = reader["MaxDiscountAmount"] != DBNull.Value ? Convert.ToDecimal(reader["MaxDiscountAmount"]) : null,
                                MinSpend = reader["MinSpend"] != DBNull.Value ? Convert.ToDecimal(reader["MinSpend"]) : 0m,
                                PointsRequired = reader["PointsRequired"] != DBNull.Value ? Convert.ToInt32(reader["PointsRequired"]) : 0,
                                BuyQuantity = reader["BuyQuantity"] != DBNull.Value ? Convert.ToInt32(reader["BuyQuantity"]) : 0,
                                FreeQuantity = reader["FreeQuantity"] != DBNull.Value ? Convert.ToInt32(reader["FreeQuantity"]) : 0,
                                FreeProductID = reader["FreeProductID"] != DBNull.Value ? reader["FreeProductID"].ToString() : null,
                                CanRepeat = reader["CanRepeat"] != DBNull.Value && Convert.ToInt32(reader["CanRepeat"]) == 1,
                                IsMemberOnly = reader["IsMemberOnly"] != DBNull.Value && Convert.ToInt32(reader["IsMemberOnly"]) == 1,
                                CanCombine = reader["CanCombine"] != DBNull.Value && Convert.ToInt32(reader["CanCombine"]) == 1,
                                MaxUsagePerCustomer = reader["MaxUsagePerCustomer"] != DBNull.Value ? Convert.ToInt32(reader["MaxUsagePerCustomer"]) : 0,
                                TotalQuota = reader["TotalQuota"] != DBNull.Value ? Convert.ToInt32(reader["TotalQuota"]) : 0,
                                CurrentUsage = reader["CurrentUsage"] != DBNull.Value ? Convert.ToInt32(reader["CurrentUsage"]) : 0,
                                StartDate = reader["StartDate"] != DBNull.Value ? reader["StartDate"].ToString()! : "",
                                EndDate = reader["EndDate"] != DBNull.Value ? reader["EndDate"].ToString()! : "",
                                IsActive = reader["IsActive"] != DBNull.Value && Convert.ToInt32(reader["IsActive"]) == 1,
                                IsDraft = reader["IsDraft"] != DBNull.Value && Convert.ToInt32(reader["IsDraft"]) == 1,
                                CreatedAt = reader["CreatedAt"] != DBNull.Value ? reader["CreatedAt"].ToString()! : ""
                            };
                            promos.Add(p);
                        }
                    }
                }

                // ดึงสินค้าที่ผูกทั้งหมด
                var promoProducts = (await conn.QueryAsync<(string PromoID, string ProductID)>(
                    "SELECT PromoID, ProductID FROM tblPromotionProducts;")).ToList();

                // ดึงข้อมูลสินค้าทั้งหมดจาก tblProduct เพื่อนำชื่อ, ราคา และต้นทุนมาแสดงผล
                var allProds = (await conn.QueryAsync<(int Pro_ID, string Pro_Barcode, string Pro_Name, decimal Pro_Price, decimal Pro_Cost)>(
                    "SELECT Pro_ID, COALESCE(Pro_Barcode, '') AS Pro_Barcode, Pro_Name, Pro_Price, Pro_Cost FROM tblProduct;")).ToList();

                var prodDict = allProds.ToDictionary(p => p.Pro_ID.ToString(), p => p);

                foreach (var p in promos)
                {
                    // Map linked products
                    var linkedIds = promoProducts.Where(x => x.PromoID == p.PromoID).Select(x => x.ProductID).ToList();
                    p.ProductIds = linkedIds;

                    var names = new List<string>();
                    foreach (var id in linkedIds)
                    {
                        if (prodDict.TryGetValue(id, out var prodInfo))
                        {
                            names.Add(prodInfo.Pro_Name);
                        }
                        else
                        {
                            names.Add($"สินค้า #{id}");
                        }
                    }
                    p.ProductNames = names;

                    // Map Free Product Name, Price & Cost
                    if (!string.IsNullOrWhiteSpace(p.FreeProductID))
                    {
                        var freeProd = allProds.FirstOrDefault(x =>
                            x.Pro_ID.ToString() == p.FreeProductID ||
                            (!string.IsNullOrEmpty(x.Pro_Barcode) && x.Pro_Barcode.Equals(p.FreeProductID, StringComparison.OrdinalIgnoreCase)) ||
                            $"P-{x.Pro_ID:D4}".Equals(p.FreeProductID, StringComparison.OrdinalIgnoreCase));

                        if (freeProd.Pro_ID > 0)
                        {
                            p.FreeProductName = freeProd.Pro_Name;
                            p.FreeProductPrice = freeProd.Pro_Price;
                            p.ItemCost = freeProd.Pro_Cost;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(p.FreeProductName))
                    {
                        var freeProd = allProds.FirstOrDefault(x =>
                            string.Equals(x.Pro_Name.Trim(), p.FreeProductName.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (freeProd.Pro_ID > 0)
                        {
                            p.FreeProductID = freeProd.Pro_ID.ToString();
                            p.FreeProductPrice = freeProd.Pro_Price;
                            p.ItemCost = freeProd.Pro_Cost;
                        }
                    }
                    else if (p.ProductIds.Count > 0 && prodDict.TryGetValue(p.ProductIds[0], out var mainProd))
                    {
                        p.ItemCost = mainProd.Pro_Cost;
                    }

                    // Database Cleanup: ตรวจสอบและแก้ไขวันที่ปีเพี้ยน (> 2600 หรือ +543 ทบยอด) ให้กลับสู่มาตรฐาน ค.ศ.
                    var cleanStart = ThaiDateHelper.FormatToDbStorage(ThaiDateHelper.ParseToUniversalDate(p.StartDate));
                    var cleanEnd = ThaiDateHelper.FormatToDbStorage(ThaiDateHelper.ParseToUniversalDate(p.EndDate));
                    if (cleanStart != p.StartDate || cleanEnd != p.EndDate)
                    {
                        await conn.ExecuteAsync("UPDATE tblPromotion SET StartDate = @StartDate, EndDate = @EndDate WHERE PromoID = @PromoID;",
                            new { StartDate = cleanStart, EndDate = cleanEnd, PromoID = p.PromoID });
                        p.StartDate = cleanStart;
                        p.EndDate = cleanEnd;
                    }

                    // Auto-Expire Rule: ตรวจสอบเฉพาะโปรโมชั่นที่ระบุวันสิ้นสุดแล้ว และหมดอายุตามวันจริงเท่านั้น
                    if (p.IsActive && SafeParseDate(p.EndDate, out DateTime endDt) && endDt.Date < DateTime.Today)
                    {
                        // อัปเดตใน DB ให้สอดคล้องกันแบบเงียบๆ
                        await conn.ExecuteAsync("UPDATE tblPromotion SET IsActive = 0 WHERE PromoID = @PromoID;", new { p.PromoID });
                        p.IsActive = false;
                    }
                }

                return promos;
            }
        }

        /// <summary>
        /// ดึงโปรโมชั่นตาม PromoID
        /// </summary>
        public async Task<PromotionModel?> GetPromotionByIdAsync(string promoId)
        {
            var all = await GetAllPromotionsAsync();
            return all.FirstOrDefault(p => string.Equals(p.PromoID, promoId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// บันทึกหรือสร้างโปรโมชั่น พร้อมผูกสินค้าใน tblPromotionProducts
        /// </summary>
        public async Task<bool> SavePromotionAsync(PromotionModel promo, IEnumerable<string>? productIds = null)
        {
            promo.StartDate = ThaiDateHelper.FormatToDbStorage(ThaiDateHelper.ParseToUniversalDate(promo.StartDate));
            promo.EndDate = ThaiDateHelper.FormatToDbStorage(ThaiDateHelper.ParseToUniversalDate(promo.EndDate));

            using (var conn = _db.GetConnection())
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    string sqlUpsert = @"
                        INSERT INTO tblPromotion (
                            PromoID, PromoCode, PromoName, PromoType, TargetScope,
                            DiscountType, DiscountAmount, MaxDiscountAmount, MinSpend,
                            PointsRequired, BuyQuantity, FreeQuantity, FreeProductID,
                            CanRepeat, IsMemberOnly, CanCombine, MaxUsagePerCustomer,
                            TotalQuota, CurrentUsage, StartDate, EndDate, IsActive,
                            IsDraft, CreatedAt
                        ) VALUES (
                            @PromoID, @PromoCode, @PromoName, @PromoType, @TargetScope,
                            @DiscountType, @DiscountAmount, @MaxDiscountAmount, @MinSpend,
                            @PointsRequired, @BuyQuantity, @FreeQuantity, @FreeProductID,
                            @CanRepeat, @IsMemberOnly, @CanCombine, @MaxUsagePerCustomer,
                            @TotalQuota, @CurrentUsage, @StartDate, @EndDate, @IsActive,
                            @IsDraft, @CreatedAt
                        )
                        ON CONFLICT(PromoID) DO UPDATE SET
                            PromoCode = excluded.PromoCode,
                            PromoName = excluded.PromoName,
                            PromoType = excluded.PromoType,
                            TargetScope = excluded.TargetScope,
                            DiscountType = excluded.DiscountType,
                            DiscountAmount = excluded.DiscountAmount,
                            MaxDiscountAmount = excluded.MaxDiscountAmount,
                            MinSpend = excluded.MinSpend,
                            PointsRequired = excluded.PointsRequired,
                            BuyQuantity = excluded.BuyQuantity,
                            FreeQuantity = excluded.FreeQuantity,
                            FreeProductID = excluded.FreeProductID,
                            CanRepeat = excluded.CanRepeat,
                            IsMemberOnly = excluded.IsMemberOnly,
                            CanCombine = excluded.CanCombine,
                            MaxUsagePerCustomer = excluded.MaxUsagePerCustomer,
                            TotalQuota = excluded.TotalQuota,
                            CurrentUsage = excluded.CurrentUsage,
                            StartDate = excluded.StartDate,
                            EndDate = excluded.EndDate,
                            IsActive = excluded.IsActive,
                            IsDraft = excluded.IsDraft;";

                    var pParams = new
                    {
                        promo.PromoID,
                        promo.PromoCode,
                        promo.PromoName,
                        promo.PromoType,
                        promo.TargetScope,
                        promo.DiscountType,
                        promo.DiscountAmount,
                        promo.MaxDiscountAmount,
                        promo.MinSpend,
                        promo.PointsRequired,
                        promo.BuyQuantity,
                        promo.FreeQuantity,
                        promo.FreeProductID,
                        CanRepeat = promo.CanRepeat ? 1 : 0,
                        IsMemberOnly = promo.IsMemberOnly ? 1 : 0,
                        CanCombine = promo.CanCombine ? 1 : 0,
                        promo.MaxUsagePerCustomer,
                        promo.TotalQuota,
                        promo.CurrentUsage,
                        promo.StartDate,
                        promo.EndDate,
                        IsActive = promo.IsActive ? 1 : 0,
                        IsDraft = promo.IsDraft ? 1 : 0,
                        CreatedAt = string.IsNullOrWhiteSpace(promo.CreatedAt) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : promo.CreatedAt
                    };

                    await conn.ExecuteAsync(sqlUpsert, pParams, trans);

                    // Sync tblPromotionProducts
                    var targetProducts = (productIds ?? promo.ProductIds ?? new List<string>()).Distinct().ToList();

                    await conn.ExecuteAsync(
                        "DELETE FROM tblPromotionProducts WHERE PromoID = @PromoID;",
                        new { promo.PromoID }, trans);

                    if (targetProducts.Count > 0)
                    {
                        string sqlInsertProd = "INSERT INTO tblPromotionProducts (PromoID, ProductID) VALUES (@PromoID, @ProductID);";
                        var batch = targetProducts.Select(pid => new { PromoID = promo.PromoID, ProductID = pid });
                        await conn.ExecuteAsync(sqlInsertProd, batch, trans);
                    }

                    trans.Commit();
                    return true;
                }
                catch
                {
                    trans.Rollback();
                    return false;
                }
            }
        }

        /// <summary>
        /// อัปเดตสถานะเปิด-ปิดโปรโมชั่น
        /// </summary>
        public async Task<bool> UpdatePromotionStatusAsync(string promoId, bool isActive)
        {
            using (var conn = _db.GetConnection())
            {
                int rows = await conn.ExecuteAsync(@"
                    UPDATE tblPromotion
                    SET IsActive = @IsActive, IsDraft = 0
                    WHERE PromoID = @PromoID;",
                    new { PromoID = promoId, IsActive = isActive ? 1 : 0 });
                return rows > 0;
            }
        }

        /// <summary>
        /// ลบโปรโมชั่นและสินค้าที่ผูก
        /// </summary>
        public async Task<bool> DeletePromotionAsync(string promoId)
        {
            if (string.IsNullOrWhiteSpace(promoId)) return false;

            using (var conn = _db.GetConnection())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Open();
                }

                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. ลบรายการสินค้าที่ผูกกับโปรโมชั่นใน tblPromotionProducts
                        await conn.ExecuteAsync("DELETE FROM tblPromotionProducts WHERE PromoID = @PromoID;", new { PromoID = promoId }, trans);

                        // 2. ลบหัวโปรโมชั่นใน tblPromotion
                        int rows = await conn.ExecuteAsync("DELETE FROM tblPromotion WHERE PromoID = @PromoID;", new { PromoID = promoId }, trans);

                        trans.Commit();
                        return rows > 0;
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// กฎการทำสำเนา (Duplicate Logic):
        /// - ชื่อเดิม + " (สำเนา)"
        /// - สถานะเริ่มต้นเป็น IsDraft = 1 และ IsActive = 0
        /// - รหัสใหม่ไม่ซ้ำ
        /// </summary>
        public async Task<PromotionModel?> DuplicatePromotionAsync(PromotionModel source)
        {
            string baseCode = source.PromoID;
            string newId = $"{baseCode}-COPY";
            int copyIdx = 1;

            using (var conn = _db.GetConnection())
            {
                while (await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(1) FROM tblPromotion WHERE PromoID = @ID;", new { ID = newId }) > 0)
                {
                    copyIdx++;
                    newId = $"{baseCode}-CP{copyIdx}";
                }
            }

            var duplicate = source.Clone();
            duplicate.PromoID = newId;
            duplicate.PromoCode = string.IsNullOrWhiteSpace(source.PromoCode) ? null : $"{source.PromoCode}-COPY";
            duplicate.PromoName = $"{source.PromoName} (สำเนา)";
            duplicate.IsDraft = true;
            duplicate.IsActive = false; // ปิดสวิตช์อยู่เสมอ
            duplicate.CurrentUsage = 0;
            duplicate.CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            bool saved = await SavePromotionAsync(duplicate, duplicate.ProductIds);
            return saved ? duplicate : null;
        }

        /// <summary>
        /// ดึงรายการสินค้าทั้งหมดในร้าน เพื่อนำไปแสดงในตารางเลือกสินค้า (ขั้นตอนที่ 3)
        /// </summary>
        public async Task<List<Product>> GetAllActiveProductsAsync()
        {
            using (var conn = _db.GetConnection())
            {
                var rows = await conn.QueryAsync<Product>(@"
                    SELECT 
                        Pro_ID, Pro_Barcode, Pro_Name, Pro_Price, Pro_Cost, 
                        Pro_Qty, Pro_MinQty, Pro_Category, Pro_Image, Pro_Unit, CategoryId
                    FROM tblProduct
                    WHERE Pro_Qty > 0
                    ORDER BY Pro_Name ASC;");
                return rows.ToList();
            }
        }

        #endregion

        #region POS Evaluation Engine (ประเมินโปรโมชั่นที่จุดขาย)

        /// <summary>
        /// ประเมินโปรโมชั่นสำหรับตะกร้าสินค้า POS
        /// กฎ:
        /// 1. เช็คช่วงเวลา StartDate <= Today <= EndDate และ IsActive = 1 และ IsDraft = 0
        /// 2. หากมี PromoCode ต้องตรงกับที่ลูกค้ากรอก (ถ้าโปรโมชั่นไม่มีโค้ด ให้ทำงานอัตโนมัติ)
        /// 3. ตรวจสอบ IsMemberOnly กับข้อมูลลูกค้า
        /// 4. ตรวจสอบเงื่อนไขตามประเภท (ลด, แลก, แจก, แถม)
        /// 5. กฎการซ้อนทับ (CanCombine): หากไม่ได้ติ๊ก ให้เลือกเฉพาะโปรโมชั่นที่คุ้มค่าที่สุดเพียง 1 เดียว
        /// </summary>
        public async Task<PromotionEvaluationResult> EvaluateCartPromotionsAsync(
            IEnumerable<CartItem> cartItems,
            Customer? customer,
            string? enteredPromoCode = null,
            List<PromotionModel>? preloadedPromos = null)
        {
            var result = new PromotionEvaluationResult();
            var itemsList = cartItems?.ToList() ?? new List<CartItem>();
            if (itemsList.Count == 0) return result;

            var activePromos = preloadedPromos ?? await GetAllPromotionsAsync();
            var today = DateTime.Today;

            // กรองเฉพาะโปรโมชั่นที่เปิดใช้งาน ไม่ใช่ดราฟท์ และยังไม่หมดอายุ (รองรับ พ.ศ. 2569)
            var candidates = activePromos.Where(p =>
                p.IsActive &&
                !p.IsDraft &&
                !p.IsExpired &&
                IsPromotionDateValid(p.StartDate, p.EndDate, today)
            ).ToList();

            decimal cartTotal = itemsList.Sum(i => i.Total);

            // เก็บรายการโปรโมชั่นที่ผ่านเงื่อนไข พร้อมคำนวณมูลค่าความคุ้มค่า
            var qualified = new List<(PromotionModel promo, decimal discount, List<CartFreebieItem> freebies, decimal totalBenefit)>();

            foreach (var promo in candidates)
            {
                // ตรวจสอบรหัสโปรโมชั่น: ถ้ามีรหัส ต้องตรงกับที่กรอกเข้ามา
                if (!string.IsNullOrWhiteSpace(promo.PromoCode))
                {
                    if (string.IsNullOrWhiteSpace(enteredPromoCode) ||
                        !string.Equals(promo.PromoCode.Trim(), enteredPromoCode.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                // ตรวจสอบสมาชิก
                if (promo.IsMemberOnly && customer == null)
                {
                    continue;
                }

                // ตรวจสอบโควตาสิทธิ์รวม
                if (promo.TotalQuota > 0 && promo.CurrentUsage >= promo.TotalQuota)
                {
                    continue;
                }

                // ตรวจสอบยอดขั้นต่ำ
                if (promo.MinSpend > 0 && cartTotal < promo.MinSpend)
                {
                    continue;
                }

                // ประเมินตามประเภท
                decimal promoDiscount = 0;
                var promoFreebies = new List<CartFreebieItem>();

                switch (promo.PromoType)
                {
                    case "Freebie": // แถม (ซื้อ N แถม M)
                        {
                            // หาสินค้าในตะกร้าที่ตรงกับสินค้าที่กำหนด
                            var matchingItems = itemsList.Where(item =>
                                promo.ProductIds.Count == 0 ||
                                promo.ProductIds.Contains(item.Product.Id.ToString())
                            ).ToList();

                            int totalMatchingQty = matchingItems.Sum(x => x.Quantity);
                            if (promo.BuyQuantity > 0 && totalMatchingQty >= promo.BuyQuantity)
                            {
                                int times = promo.CanRepeat
                                    ? (totalMatchingQty / promo.BuyQuantity)
                                    : 1;

                                int freeQty = times * promo.FreeQuantity;
                                if (freeQty > 0)
                                {
                                    // หาข้อมูลสินค้าที่จะแถม
                                    int freeProdId = matchingItems[0].Product.Id;
                                    string freeProdName = matchingItems[0].Product.Name;
                                    decimal freeProdPrice = matchingItems[0].Product.Price;
                                    decimal freeProdCost = matchingItems[0].Product.Cost;

                                    if (!string.IsNullOrWhiteSpace(promo.FreeProductID) &&
                                        int.TryParse(promo.FreeProductID, out int targetFreeId))
                                    {
                                        freeProdId = targetFreeId;
                                        freeProdName = promo.FreeProductName ?? $"สินค้า #{targetFreeId}";
                                        freeProdPrice = 20.00m; // Default MatchaShop unit price
                                        freeProdCost = promo.ItemCost;
                                    }

                                    promoFreebies.Add(new CartFreebieItem
                                    {
                                        PromoID = promo.PromoID,
                                        PromoName = promo.PromoName,
                                        ProductId = freeProdId,
                                        ProductName = freeProdName,
                                        Quantity = freeQty,
                                        OriginalPrice = freeProdPrice,
                                        Cost = freeProdCost,
                                        FreebieType = "Freebie"
                                    });
                                }
                            }
                        }
                        break;

                    case "Gift": // แจก (ซื้อครบ แจกของฟรี)
                        {
                            if (cartTotal >= promo.MinSpend && promo.FreeQuantity > 0)
                            {
                                int giftProdId = 0;
                                string giftProdName = promo.FreeProductName ?? "ของสมนาคุณ";
                                decimal giftProdPrice = 0m;
                                decimal giftProdCost = promo.ItemCost;

                                if (!string.IsNullOrWhiteSpace(promo.FreeProductID) &&
                                    int.TryParse(promo.FreeProductID, out int gId))
                                {
                                    giftProdId = gId;
                                    giftProdPrice = 20.00m;
                                }

                                promoFreebies.Add(new CartFreebieItem
                                {
                                    PromoID = promo.PromoID,
                                    PromoName = promo.PromoName,
                                    ProductId = giftProdId,
                                    ProductName = giftProdName,
                                    Quantity = promo.FreeQuantity,
                                    OriginalPrice = giftProdPrice,
                                    Cost = giftProdCost,
                                    FreebieType = "Gift"
                                });
                            }
                        }
                        break;

                    case "Redeem": // แลก (ใช้แต้มแลกส่วนลด)
                        {
                            if (customer != null && promo.PointsRequired > 0 && customer.Points >= promo.PointsRequired)
                            {
                                promoDiscount = promo.DiscountAmount;
                            }
                        }
                        break;

                    case "Discount": // ลด (เงินสด หรือ %)
                    default:
                        {
                            decimal eligibleAmount = cartTotal;
                            if (promo.TargetScope == "SpecificProducts" && promo.ProductIds.Count > 0)
                            {
                                eligibleAmount = itemsList
                                    .Where(i => promo.ProductIds.Contains(i.Product.Id.ToString()))
                                    .Sum(i => i.Total);
                            }

                            if (eligibleAmount > 0)
                            {
                                if (promo.DiscountType == "Percentage")
                                {
                                    decimal calc = eligibleAmount * (promo.DiscountAmount / 100m);
                                    if (promo.MaxDiscountAmount.HasValue && promo.MaxDiscountAmount.Value > 0)
                                    {
                                        calc = Math.Min(calc, promo.MaxDiscountAmount.Value);
                                    }
                                    promoDiscount = Math.Round(calc, 2);
                                }
                                else
                                {
                                    promoDiscount = Math.Min(promo.DiscountAmount, eligibleAmount);
                                }
                            }
                        }
                        break;
                }

                decimal benefit = promoDiscount + promoFreebies.Sum(f => f.Quantity * f.OriginalPrice);
                if (benefit > 0 || promoFreebies.Count > 0)
                {
                    qualified.Add((promo, promoDiscount, promoFreebies, benefit));
                }
            }

            if (qualified.Count == 0) return result;

            // กฎการซ้อนทับ (Stackable Rules):
            // แยกกลุ่มโปรโมชั่นที่ CanCombine และโปรโมชั่นที่ไม่ Combine
            var combinable = qualified.Where(q => q.promo.CanCombine).ToList();
            var nonCombinable = qualified.Where(q => !q.promo.CanCombine).ToList();

            if (nonCombinable.Count > 0)
            {
                // เลือกโปรโมชั่นเดี่ยวที่ให้ประโยชน์สูงสุดเพียง 1 รายการ
                var bestSingle = nonCombinable.OrderByDescending(q => q.totalBenefit).First();

                decimal totalCombinableBenefit = combinable.Sum(c => c.totalBenefit);
                if (bestSingle.totalBenefit >= totalCombinableBenefit)
                {
                    // ใช้โปรโมชั่นเดี่ยวที่ดีที่สุด
                    result.AppliedPromotions.Add(bestSingle.promo);
                    result.TotalDiscount = bestSingle.discount;
                    result.FreebiesToAdd.AddRange(bestSingle.freebies);
                    result.SummaryText = $"ได้รับโปรโมชั่น: {bestSingle.promo.PromoName}";
                }
                else
                {
                    // ใช้กลุ่มที่ซ้อนทับกันได้
                    foreach (var c in combinable)
                    {
                        result.AppliedPromotions.Add(c.promo);
                        result.TotalDiscount += c.discount;
                        result.FreebiesToAdd.AddRange(c.freebies);
                    }
                    result.SummaryText = $"ได้รับ {combinable.Count} โปรโมชั่น";
                }
            }
            else
            {
                // รวมโปรโมชั่นทั้งหมดที่ซ้อนกันได้
                foreach (var c in combinable)
                {
                    result.AppliedPromotions.Add(c.promo);
                    result.TotalDiscount += c.discount;
                    result.FreebiesToAdd.AddRange(c.freebies);
                }
                result.SummaryText = $"ได้รับ {combinable.Count} โปรโมชั่น";
            }

            return result;
        }

        /// <summary>
        /// บันทึกตัดสต็อกและรายการของแถม/ของแจกลงใน tblSalesDetail ของบิลขายจริง
        /// UnitPrice = 0, Discount = 0, PromoID = ...
        /// และอัปเดต CurrentUsage ใน tblPromotion
        /// </summary>
        public async Task ApplyPromotionToSaleAsync(
            IDbConnection conn,
            IDbTransaction trans,
            int salesId,
            PromotionEvaluationResult evalResult)
        {
            if (evalResult == null) return;

            // 1. บันทึกของแถมลง tblSalesDetail และตัดสต็อกสินค้าใน tblProduct
            foreach (var freebie in evalResult.FreebiesToAdd)
            {
                if (freebie.ProductId > 0 && freebie.Quantity > 0)
                {
                    // ตัดสต็อกสินค้าที่แถมจริงทันที
                    string sqlDeduct = "UPDATE tblProduct SET Pro_Qty = MAX(0, Pro_Qty - @Qty) WHERE Pro_ID = @Id;";
                    await conn.ExecuteAsync(sqlDeduct, new { Qty = freebie.Quantity, Id = freebie.ProductId }, trans);

                    // บันทึกลง tblSalesDetail: UnitPrice = 0, Discount = 0, พร้อม PromoID
                    string sqlDetail = @"
                        INSERT INTO tblSalesDetail (Sales_ID, Pro_ID, Pro_Price, Sales_Qty, Sales_Subtotal, PromoID, Discount)
                        VALUES (@Sales_ID, @Pro_ID, 0, @Sales_Qty, 0, @PromoID, 0);";

                    await conn.ExecuteAsync(sqlDetail, new
                    {
                        Sales_ID = salesId,
                        Pro_ID = freebie.ProductId,
                        Sales_Qty = freebie.Quantity,
                        PromoID = freebie.PromoID
                    }, trans);
                }
            }

            // 2. อัปเดต CurrentUsage และ UsageCount ใน tblPromotion
            foreach (var promo in evalResult.AppliedPromotions)
            {
                string sqlUsage = @"
                    UPDATE tblPromotion 
                    SET CurrentUsage = CurrentUsage + 1,
                        UsageCount = COALESCE(UsageCount, 0) + 1 
                    WHERE PromoID = @PromoID;";
                try
                {
                    await conn.ExecuteAsync(sqlUsage, new { promo.PromoID }, trans);
                }
                catch
                {
                    await conn.ExecuteAsync("UPDATE tblPromotion SET CurrentUsage = CurrentUsage + 1 WHERE PromoID = @PromoID;", new { promo.PromoID }, trans);
                }
            }
        }

        #endregion

        #region Safe Date Normalization & Auto-Promotion Engine

        /// <summary>
        /// แปลงสตริงวันที่เป็น DateTime อย่างปลอดภัย รองรับทั้ง ค.ศ. (2026) และ พ.ศ. (2569)
        /// หากปี > 3000 ให้ทำการ Normalize กลับเป็นปีปัจจุบัน ห้ามโยน Exception ทิ้ง
        /// </summary>
        public static bool SafeParseDate(string? dateStr, out DateTime date)
        {
            return PromotionModel.SafeParseDate(dateStr, out date);
        }

        /// <summary>
        /// แปลงสตริงวันที่เป็น DateTime อย่างปลอดภัย (Nullable)
        /// </summary>
        public static DateTime? ParseDateSafe(string? dateStr)
        {
            return PromotionModel.ParseDateSafe(dateStr);
        }

        /// <summary>
        /// ดึงช่วงเวลาเริ่มต้นและสิ้นสุดอย่างปลอดภัย รองรับทั้งแบบแยกฟิลด์และแบบช่วงในสตริงเดียว เช่น "05/10/2569 - 05/12/2569"
        /// </summary>
        public static (DateTime? start, DateTime? end) GetDateRangeSafe(string? startDateStr, string? endDateStr)
        {
            DateTime? start = null;
            DateTime? end = null;

            if (!string.IsNullOrWhiteSpace(startDateStr) && startDateStr.Contains(" - "))
            {
                var parts = startDateStr.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    start = ParseDateSafe(parts[0]);
                    end = ParseDateSafe(parts[1]);
                }
                else if (parts.Length == 1)
                {
                    start = ParseDateSafe(parts[0]);
                }
            }
            else
            {
                start = ParseDateSafe(startDateStr);
            }

            if (!end.HasValue && !string.IsNullOrWhiteSpace(endDateStr))
            {
                if (endDateStr.Contains(" - "))
                {
                    var parts = endDateStr.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                    end = ParseDateSafe(parts.Length >= 2 ? parts[1] : parts[0]);
                }
                else
                {
                    end = ParseDateSafe(endDateStr);
                }
            }

            return (start, end);
        }

        /// <summary>
        /// ตรวจสอบว่าวันที่โปรโมชั่นเปิดใช้งานอยู่หรือไม่ (ปลอดภัยต่อทั้งปี ค.ศ. และ พ.ศ.)
        /// </summary>
        public static bool IsDateActive(string? startDateStr, string? endDateStr)
        {
            DateTime today = DateTime.Today;
            if (!SafeParseDate(startDateStr, out DateTime start)) return false;
            if (!SafeParseDate(endDateStr, out DateTime end)) return false;
            return today >= start.Date && today <= end.Date;
        }

        /// <summary>
        /// ตรวจสอบว่าเงื่อนไข StartDate <= checkDate && EndDate >= checkDate ถูกต้องหรือไม่ (เทียบกับ DateTime.Today ค.ศ. 2026)
        /// </summary>
        public static bool IsPromotionDateValid(string? startDateStr, string? endDateStr, DateTime? checkDate = null)
        {
            DateTime check = (checkDate ?? DateTime.Today).Date;
            if (!SafeParseDate(startDateStr, out DateTime start)) return false;
            if (!SafeParseDate(endDateStr, out DateTime end)) return false;
            return check >= start.Date && check <= end.Date;
        }

        /// <summary>
        /// ดึงโปรโมชั่นทั้งหมดแบบ Synchronous
        /// </summary>
        public List<PromotionModel> GetAllPromotionsSync()
        {
            using (var conn = _db.GetConnection())
            {
                string sqlPromo = @"
                    SELECT 
                        PromoID, PromoCode, PromoName, PromoType, TargetScope,
                        DiscountType, DiscountAmount, MaxDiscountAmount, MinSpend,
                        PointsRequired, BuyQuantity, FreeQuantity, FreeProductID,
                        CanRepeat, IsMemberOnly, CanCombine, MaxUsagePerCustomer,
                        TotalQuota, CurrentUsage, StartDate, EndDate, IsActive,
                        IsDraft, CreatedAt
                    FROM tblPromotion
                    ORDER BY IsDraft ASC, IsActive DESC, PromoID ASC;";

                var promos = new List<PromotionModel>();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sqlPromo;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var p = new PromotionModel
                            {
                                PromoID = reader["PromoID"] != DBNull.Value ? reader["PromoID"].ToString()! : "",
                                PromoCode = reader["PromoCode"] != DBNull.Value ? reader["PromoCode"].ToString() : null,
                                PromoName = reader["PromoName"] != DBNull.Value ? reader["PromoName"].ToString()! : "",
                                PromoType = reader["PromoType"] != DBNull.Value ? reader["PromoType"].ToString()! : "Discount",
                                TargetScope = reader["TargetScope"] != DBNull.Value ? reader["TargetScope"].ToString()! : "AllStore",
                                DiscountType = reader["DiscountType"] != DBNull.Value ? reader["DiscountType"].ToString()! : "Cash",
                                DiscountAmount = reader["DiscountAmount"] != DBNull.Value ? Convert.ToDecimal(reader["DiscountAmount"]) : 0m,
                                MaxDiscountAmount = reader["MaxDiscountAmount"] != DBNull.Value ? Convert.ToDecimal(reader["MaxDiscountAmount"]) : null,
                                MinSpend = reader["MinSpend"] != DBNull.Value ? Convert.ToDecimal(reader["MinSpend"]) : 0m,
                                PointsRequired = reader["PointsRequired"] != DBNull.Value ? Convert.ToInt32(reader["PointsRequired"]) : 0,
                                BuyQuantity = reader["BuyQuantity"] != DBNull.Value ? Convert.ToInt32(reader["BuyQuantity"]) : 0,
                                FreeQuantity = reader["FreeQuantity"] != DBNull.Value ? Convert.ToInt32(reader["FreeQuantity"]) : 0,
                                FreeProductID = reader["FreeProductID"] != DBNull.Value ? reader["FreeProductID"].ToString() : null,
                                CanRepeat = reader["CanRepeat"] != DBNull.Value && Convert.ToInt32(reader["CanRepeat"]) == 1,
                                IsMemberOnly = reader["IsMemberOnly"] != DBNull.Value && Convert.ToInt32(reader["IsMemberOnly"]) == 1,
                                CanCombine = reader["CanCombine"] != DBNull.Value && Convert.ToInt32(reader["CanCombine"]) == 1,
                                MaxUsagePerCustomer = reader["MaxUsagePerCustomer"] != DBNull.Value ? Convert.ToInt32(reader["MaxUsagePerCustomer"]) : 0,
                                TotalQuota = reader["TotalQuota"] != DBNull.Value ? Convert.ToInt32(reader["TotalQuota"]) : 0,
                                CurrentUsage = reader["CurrentUsage"] != DBNull.Value ? Convert.ToInt32(reader["CurrentUsage"]) : 0,
                                StartDate = reader["StartDate"] != DBNull.Value ? reader["StartDate"].ToString()! : "",
                                EndDate = reader["EndDate"] != DBNull.Value ? reader["EndDate"].ToString()! : "",
                                IsActive = reader["IsActive"] != DBNull.Value && Convert.ToInt32(reader["IsActive"]) == 1,
                                IsDraft = reader["IsDraft"] != DBNull.Value && Convert.ToInt32(reader["IsDraft"]) == 1,
                                CreatedAt = reader["CreatedAt"] != DBNull.Value ? reader["CreatedAt"].ToString()! : ""
                            };
                            promos.Add(p);
                        }
                    }
                }

                var promoProducts = conn.Query<(string PromoID, string ProductID)>(
                    "SELECT PromoID, ProductID FROM tblPromotionProducts;").ToList();

                var allProds = conn.Query<(int Pro_ID, string Pro_Barcode, string Pro_Name, decimal Pro_Price, decimal Pro_Cost)>(
                    "SELECT Pro_ID, COALESCE(Pro_Barcode, '') AS Pro_Barcode, Pro_Name, Pro_Price, Pro_Cost FROM tblProduct;").ToList();

                var prodDict = allProds.ToDictionary(p => p.Pro_ID.ToString(), p => p);

                foreach (var p in promos)
                {
                    var linkedIds = promoProducts.Where(x => x.PromoID == p.PromoID).Select(x => x.ProductID).ToList();
                    p.ProductIds = linkedIds;

                    var names = new List<string>();
                    foreach (var id in linkedIds)
                    {
                        if (prodDict.TryGetValue(id, out var prodInfo))
                            names.Add(prodInfo.Pro_Name);
                        else
                            names.Add($"สินค้า #{id}");
                    }
                    p.ProductNames = names;

                    if (!string.IsNullOrWhiteSpace(p.FreeProductID))
                    {
                        var freeProd = allProds.FirstOrDefault(x =>
                            x.Pro_ID.ToString() == p.FreeProductID ||
                            (!string.IsNullOrEmpty(x.Pro_Barcode) && x.Pro_Barcode.Equals(p.FreeProductID, StringComparison.OrdinalIgnoreCase)) ||
                            $"P-{x.Pro_ID:D4}".Equals(p.FreeProductID, StringComparison.OrdinalIgnoreCase));

                        if (freeProd.Pro_ID > 0)
                        {
                            p.FreeProductName = freeProd.Pro_Name;
                            p.FreeProductPrice = freeProd.Pro_Price;
                            p.ItemCost = freeProd.Pro_Cost;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(p.FreeProductName))
                    {
                        var freeProd = allProds.FirstOrDefault(x =>
                            string.Equals(x.Pro_Name.Trim(), p.FreeProductName.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (freeProd.Pro_ID > 0)
                        {
                            p.FreeProductID = freeProd.Pro_ID.ToString();
                            p.FreeProductPrice = freeProd.Pro_Price;
                            p.ItemCost = freeProd.Pro_Cost;
                        }
                    }
                    else if (p.ProductIds.Count > 0 && prodDict.TryGetValue(p.ProductIds[0], out var mainProd))
                    {
                        p.ItemCost = mainProd.Pro_Cost;
                    }

                    // Database Cleanup: ตรวจสอบและแก้ไขวันที่ปีเพี้ยน (> 2600 หรือ +543 ทบยอด) ให้กลับสู่มาตรฐาน ค.ศ.
                    var cleanStartSync = ThaiDateHelper.FormatToDbStorage(ThaiDateHelper.ParseToUniversalDate(p.StartDate));
                    var cleanEndSync = ThaiDateHelper.FormatToDbStorage(ThaiDateHelper.ParseToUniversalDate(p.EndDate));
                    if (cleanStartSync != p.StartDate || cleanEndSync != p.EndDate)
                    {
                        conn.Execute("UPDATE tblPromotion SET StartDate = @StartDate, EndDate = @EndDate WHERE PromoID = @PromoID;",
                            new { StartDate = cleanStartSync, EndDate = cleanEndSync, PromoID = p.PromoID });
                        p.StartDate = cleanStartSync;
                        p.EndDate = cleanEndSync;
                    }

                    // Auto-Expire Rule: ตรวจสอบเฉพาะโปรโมชั่นที่ระบุวันสิ้นสุดแล้ว และหมดอายุตามวันจริงเท่านั้น
                    if (p.IsActive && SafeParseDate(p.EndDate, out DateTime endDt) && endDt.Date < DateTime.Today)
                    {
                        conn.Execute("UPDATE tblPromotion SET IsActive = 0 WHERE PromoID = @PromoID;", new { p.PromoID });
                        p.IsActive = false;
                    }
                }

                return promos;
            }
        }

        /// <summary>
        /// Engine ประเมินโปรโมชั่นอัตโนมัติ (Auto-Apply Promotion Engine):
        /// ขั้นที่ 1: คัดกรองโปรโมชั่นที่ผ่านเกณฑ์จริง (Eligible Promotions)
        /// - IsActive == 1, IsDraft == 0
        /// - ไม่มี PromoCode (เป็นโปรโมชั่นคำนวณอัตโนมัติ)
        /// - Safe Date Evaluation: ตรวจสอบทั้ง ค.ศ. (2026) และ พ.ศ. (2569) ป้องกันบั๊กปี > 3000
        /// - ตรวจสอบยอดซื้อขั้นต่ำ (MinSpend), สินค้าที่ร่วมรายการ (tblPromotionProducts), และสถานะสมาชิก (IsMemberOnly)
        /// 
        /// ขั้นที่ 2: ตรรกะ CanCombine ที่ถูกต้อง (ห้ามทิ้งโปรเดี่ยว)
        /// - แยกโปรโมชั่นที่ผ่านเกณฑ์เป็น:
        ///   * stackableList: โปรโมชั่นที่ CanCombine == true
        ///   * singleList: โปรโมชั่นที่ CanCombine == false
        /// - กฎเหล็ก: แม้ใน stackableList จะมีโปรโมชั่นผ่านเกณฑ์เพียงรายการเดียว (Count == 1) ก็ต้องนำมาคำนวณส่วนลดให้ลูกค้าตามปกติ
        /// - คำนวณเปรียบเทียบผลประโยชน์:
        ///   * ยอดลดรวมของ stackableList ทั้งหมด (ผลรวมส่วนลดของทุกโปรที่รวมกันได้)
        ///   * ยอดลดของโปรโมชั่นเดี่ยวที่ดีที่สุดใน singleList (Best Single Discount)
        ///   * เลือกระบบที่ลูกค้าได้ส่วนลดมากที่สุด (Best Benefit for Customer)
        /// </summary>
        public AutoPromotionResult? EvaluateAutoPromotions(
            IEnumerable<CartItem>? cartItems,
            bool isMember,
            decimal subtotal,
            List<PromotionModel>? preloadedPromos = null)
        {
            var itemsList = cartItems?.ToList() ?? new List<CartItem>();
            if (itemsList.Count == 0 && subtotal <= 0) return null;

            var allPromos = preloadedPromos ?? GetAllPromotionsSync();
            var today = DateTime.Today;

            // ขั้นที่ 1: คัดกรองโปรโมชั่นที่ผ่านเกณฑ์จริง
            var eligiblePromotions = allPromos.Where(p =>
                p.IsActive &&
                !p.IsDraft &&
                !p.IsExpired &&
                string.IsNullOrWhiteSpace(p.PromoCode) &&
                IsDateActive(p.StartDate, p.EndDate) &&
                (p.TotalQuota <= 0 || p.CurrentUsage < p.TotalQuota)
            ).ToList();

            decimal billTotal = subtotal > 0 ? subtotal : itemsList.Sum(i => i.Total);
            var evaluatedList = new List<(PromotionModel promo, decimal discount, AppliedPromotionResult item)>();

            foreach (var promo in eligiblePromotions)
            {
                // ตรวจสอบสมาชิก
                if (promo.IsMemberOnly && !isMember)
                {
                    continue;
                }

                // ตรวจสอบยอดซื้อขั้นต่ำและคำนวณส่วนลดตามประเภทโปรโมชั่น
                decimal discount = 0m;
                AppliedPromotionResult? appliedItem = null;

                switch (promo.PromoType)
                {
                    case "Gift": // แจก (ซื้อครบ แจกของฟรี)
                        {
                            decimal qualifyingSpend = billTotal;
                            if (promo.TargetScope == "SpecificProducts" && promo.ProductIds != null && promo.ProductIds.Count > 0)
                            {
                                var matching = itemsList.Where(i =>
                                    promo.ProductIds.Contains(i.Product.Id.ToString()) ||
                                    (!string.IsNullOrEmpty(i.Product.Code) && promo.ProductIds.Contains(i.Product.Code)) ||
                                    promo.ProductIds.Contains($"P-{i.Product.Id:D4}")
                                ).ToList();
                                qualifyingSpend = matching.Sum(i => i.Total);
                            }

                            if ((promo.MinSpend <= 0 || qualifyingSpend >= promo.MinSpend) && promo.FreeQuantity > 0)
                            {
                                // ค้นหาสินค้าของแจกในตะกร้า
                                CartItem? giftCartItem = null;
                                if (!string.IsNullOrWhiteSpace(promo.FreeProductID))
                                {
                                    giftCartItem = itemsList.FirstOrDefault(i =>
                                        i.Product.Id.ToString() == promo.FreeProductID ||
                                        (!string.IsNullOrEmpty(i.Product.Code) && i.Product.Code.Equals(promo.FreeProductID, StringComparison.OrdinalIgnoreCase)) ||
                                        $"P-{i.Product.Id:D4}".Equals(promo.FreeProductID, StringComparison.OrdinalIgnoreCase));
                                }
                                if (giftCartItem == null && !string.IsNullOrWhiteSpace(promo.FreeProductName))
                                {
                                    giftCartItem = itemsList.FirstOrDefault(i =>
                                        string.Equals(i.Product.Name.Trim(), promo.FreeProductName.Trim(), StringComparison.OrdinalIgnoreCase));
                                }

                                int freeQty = promo.FreeQuantity > 0 ? promo.FreeQuantity : 1;
                                string giftLabel = !string.IsNullOrWhiteSpace(promo.PromoName) ? promo.PromoName : "แจก";

                                if (giftCartItem != null)
                                {
                                    // กรณีที่ 1: ในตะกร้ามีสินค้าของแจกอยู่แล้ว
                                    int appliedQty = Math.Min(freeQty, giftCartItem.Quantity);
                                    decimal unitPrice = giftCartItem.Product.Price;
                                    discount = appliedQty * unitPrice;
                                    string giftName = giftCartItem.Product.Name;

                                    if (discount > 0)
                                    {
                                        appliedItem = new AppliedPromotionResult
                                        {
                                            PromoID = promo.PromoID,
                                            PromoName = promo.PromoName,
                                            PromoType = "Gift",
                                            DiscountAmount = discount,
                                            GiftProductId = giftCartItem.Product.Id,
                                            GiftProductName = giftName,
                                            GiftQuantity = appliedQty,
                                            UnitPrice = unitPrice,
                                            IsAutoIncluded = false,
                                            AutoIncludedAmount = 0m,
                                            DisplayText = $"ของแจกฟรี ({giftLabel} - {giftName} {appliedQty} ชิ้น):",
                                            TextColor = "#16A34A",
                                            Promotion = promo
                                        };
                                    }
                                }
                                else
                                {
                                    // กรณีที่ 2: ในตะกร้ายังไม่มีสินค้าของแจก (Auto-Inclusion of Free Item)
                                    string giftName = !string.IsNullOrWhiteSpace(promo.FreeProductName) ? promo.FreeProductName : "ของแจกฟรี";
                                    decimal unitPrice = promo.FreeProductPrice > 0 ? promo.FreeProductPrice : 10.00m;
                                    int giftProdId = 0;
                                    if (!string.IsNullOrWhiteSpace(promo.FreeProductID))
                                    {
                                        if (int.TryParse(promo.FreeProductID, out int pId))
                                            giftProdId = pId;
                                        else if (promo.FreeProductID.StartsWith("P-", StringComparison.OrdinalIgnoreCase) &&
                                                 int.TryParse(promo.FreeProductID.Substring(2), out int pId2))
                                            giftProdId = pId2;
                                    }

                                    discount = freeQty * unitPrice;
                                    if (discount > 0)
                                    {
                                        appliedItem = new AppliedPromotionResult
                                        {
                                            PromoID = promo.PromoID,
                                            PromoName = promo.PromoName,
                                            PromoType = "Gift",
                                            DiscountAmount = discount,
                                            GiftProductId = giftProdId,
                                            GiftProductName = giftName,
                                            GiftQuantity = freeQty,
                                            UnitPrice = unitPrice,
                                            IsAutoIncluded = true,
                                            AutoIncludedAmount = discount,
                                            DisplayText = $"ของแจกฟรี ({giftLabel} - {giftName} {freeQty} ชิ้น):",
                                            TextColor = "#16A34A",
                                            Promotion = promo
                                        };
                                    }
                                }
                            }
                        }
                        break;

                    case "Freebie": // แถม (ซื้อ N แถม M)
                        {
                            var matchingFreebieItems = itemsList.Where(item =>
                                promo.ProductIds == null || promo.ProductIds.Count == 0 || promo.ProductIds.Contains(item.Product.Id.ToString()) ||
                                (!string.IsNullOrEmpty(item.Product.Code) && promo.ProductIds.Contains(item.Product.Code))
                            ).ToList();

                            int totalMatchingQty = matchingFreebieItems.Sum(x => x.Quantity);
                            int buyQ = promo.BuyQuantity > 0 ? promo.BuyQuantity : 1;
                            int freeQ = promo.FreeQuantity > 0 ? promo.FreeQuantity : 1;

                            if (totalMatchingQty >= buyQ)
                            {
                                int times = promo.CanRepeat ? (totalMatchingQty / (buyQ + freeQ > 0 ? (buyQ + freeQ) : buyQ)) : 1;
                                if (times <= 0) times = 1;
                                int actualFreeQty = times * freeQ;

                                CartItem? freebieCartItem = null;
                                if (!string.IsNullOrWhiteSpace(promo.FreeProductID))
                                {
                                    freebieCartItem = itemsList.FirstOrDefault(i =>
                                        i.Product.Id.ToString() == promo.FreeProductID ||
                                        (!string.IsNullOrEmpty(i.Product.Code) && i.Product.Code.Equals(promo.FreeProductID, StringComparison.OrdinalIgnoreCase)) ||
                                        $"P-{i.Product.Id:D4}".Equals(promo.FreeProductID, StringComparison.OrdinalIgnoreCase));
                                }
                                else if (matchingFreebieItems.Count > 0)
                                {
                                    freebieCartItem = matchingFreebieItems[0];
                                }

                                string freeLabel = !string.IsNullOrWhiteSpace(promo.PromoName) ? promo.PromoName : "แถม";

                                if (freebieCartItem != null)
                                {
                                    actualFreeQty = Math.Min(actualFreeQty, freebieCartItem.Quantity);
                                    decimal unitPrice = freebieCartItem.Product.Price;
                                    discount = actualFreeQty * unitPrice;
                                    if (discount > 0)
                                    {
                                        string freeName = freebieCartItem.Product.Name;
                                        appliedItem = new AppliedPromotionResult
                                        {
                                            PromoID = promo.PromoID,
                                            PromoName = promo.PromoName,
                                            PromoType = "Freebie",
                                            DiscountAmount = discount,
                                            GiftProductId = freebieCartItem.Product.Id,
                                            GiftProductName = freeName,
                                            GiftQuantity = actualFreeQty,
                                            UnitPrice = unitPrice,
                                            IsAutoIncluded = false,
                                            AutoIncludedAmount = 0m,
                                            DisplayText = $"สินค้าแถมฟรี ({freeLabel} - {freeName} {actualFreeQty} ชิ้น):",
                                            TextColor = "#9333EA",
                                            Promotion = promo
                                        };
                                    }
                                }
                                else
                                {
                                    string freeName = !string.IsNullOrWhiteSpace(promo.FreeProductName) ? promo.FreeProductName : "สินค้าแถม";
                                    decimal unitPrice = promo.FreeProductPrice > 0 ? promo.FreeProductPrice : 10.00m;
                                    int freeProdId = 0;
                                    if (!string.IsNullOrWhiteSpace(promo.FreeProductID))
                                    {
                                        if (int.TryParse(promo.FreeProductID, out int pId))
                                            freeProdId = pId;
                                    }
                                    discount = actualFreeQty * unitPrice;
                                    if (discount > 0)
                                    {
                                        appliedItem = new AppliedPromotionResult
                                        {
                                            PromoID = promo.PromoID,
                                            PromoName = promo.PromoName,
                                            PromoType = "Freebie",
                                            DiscountAmount = discount,
                                            GiftProductId = freeProdId,
                                            GiftProductName = freeName,
                                            GiftQuantity = actualFreeQty,
                                            UnitPrice = unitPrice,
                                            IsAutoIncluded = true,
                                            AutoIncludedAmount = discount,
                                            DisplayText = $"สินค้าแถมฟรี ({freeLabel} - {freeName} {actualFreeQty} ชิ้น):",
                                            TextColor = "#9333EA",
                                            Promotion = promo
                                        };
                                    }
                                }
                            }
                        }
                        break;

                    case "Redeem": // แลกแต้ม
                        {
                            if (promo.PointsRequired == 0 && promo.DiscountAmount > 0)
                            {
                                decimal eligibleAmount = billTotal;
                                if (promo.MinSpend <= 0 || eligibleAmount >= promo.MinSpend)
                                {
                                    discount = Math.Min(promo.DiscountAmount, eligibleAmount);
                                    if (discount > 0)
                                    {
                                        appliedItem = new AppliedPromotionResult
                                        {
                                            PromoID = promo.PromoID,
                                            PromoName = promo.PromoName,
                                            PromoType = "Redeem",
                                            DiscountAmount = discount,
                                            DisplayText = "สิทธิ์แลกแต้ม (แลก):",
                                            TextColor = "#D97706",
                                            Promotion = promo
                                        };
                                    }
                                }
                            }
                        }
                        break;

                    case "Discount": // ลด (เงินสด หรือ %)
                    default:
                        {
                            decimal eligibleAmount = 0m;
                            if (promo.TargetScope == "SpecificProducts")
                            {
                                if (promo.ProductIds != null && promo.ProductIds.Count > 0 && itemsList.Count > 0)
                                {
                                    var matchingItems = itemsList.Where(item =>
                                        promo.ProductIds.Contains(item.Product.Id.ToString())
                                    ).ToList();

                                    if (matchingItems.Count > 0)
                                    {
                                        eligibleAmount = matchingItems.Sum(item => item.Total);
                                    }
                                }
                            }
                            else // AllStore
                            {
                                eligibleAmount = billTotal;
                            }

                            if (eligibleAmount > 0 && (promo.MinSpend <= 0 || eligibleAmount >= promo.MinSpend))
                            {
                                if (promo.DiscountType == "Percentage")
                                {
                                    decimal calc = eligibleAmount * (promo.DiscountAmount / 100m);
                                    if (promo.MaxDiscountAmount.HasValue && promo.MaxDiscountAmount.Value > 0)
                                    {
                                        calc = Math.Min(calc, promo.MaxDiscountAmount.Value);
                                    }
                                    discount = Math.Round(calc, 2);
                                }
                                else // Cash
                                {
                                    discount = Math.Min(promo.DiscountAmount, eligibleAmount);
                                }

                                if (discount > 0)
                                {
                                    string discountLabel = !string.IsNullOrWhiteSpace(promo.PromoName) ? promo.PromoName : "ลด";
                                    int? targetId = null;
                                    string? targetName = null;
                                    if (promo.ProductIds != null && promo.ProductIds.Count == 1 && int.TryParse(promo.ProductIds[0], out int parsedTId))
                                    {
                                        targetId = parsedTId;
                                        targetName = promo.ProductNames.FirstOrDefault();
                                    }

                                    appliedItem = new AppliedPromotionResult
                                    {
                                        PromoID = promo.PromoID,
                                        PromoName = promo.PromoName,
                                        PromoType = "Discount",
                                        DiscountAmount = discount,
                                        TargetProductId = targetId,
                                        TargetProductName = targetName,
                                        DisplayText = $"ส่วนลดโปรโมชั่น ({discountLabel}):",
                                        TextColor = "#E11D48",
                                        Promotion = promo
                                    };
                                }
                            }
                        }
                        break;
                }

                if (discount > 0 && appliedItem != null)
                {
                    evaluatedList.Add((promo, discount, appliedItem));
                }
            }

            if (evaluatedList.Count == 0) return null;

            // ขั้นที่ 2: ตรรกะการตรวจสอบสิทธิ์ใช้ร่วมกัน (CanCombine Logic)
            // กลุ่ม B (โปรโมชั่นใช้ร่วมกันได้ - CanCombine == true): สามารถนำมารวมกันได้ทั้งหมด
            var stackableList = evaluatedList.Where(x => x.promo.CanCombine).ToList();

            // กลุ่ม A (โปรโมชั่นเดี่ยว - CanCombine == false): หากเลือกใช้ จะใช้ได้เพียงโปรโมชั่นเดียว (เลือกตัวที่คุ้มค่าที่สุด)
            var singleList = evaluatedList.Where(x => !x.promo.CanCombine).ToList();

            // ยอดลดรวมของกลุ่ม B (โปรที่ CanCombine ทั้งหมดรวมกัน)
            decimal stackableTotalDiscount = stackableList.Sum(x => x.discount);
            decimal autoIncludedInStack = stackableList.Sum(x => x.item.AutoIncludedAmount);
            decimal effectiveBillTotal = billTotal + autoIncludedInStack;
            if (effectiveBillTotal > 0)
            {
                stackableTotalDiscount = Math.Min(stackableTotalDiscount, effectiveBillTotal);
            }

            // ส่วนลดของโปรโมชั่นเดี่ยวที่ดีที่สุดในกลุ่ม A
            var bestSingle = singleList
                .OrderByDescending(x => x.discount)
                .ThenBy(x => x.promo.PromoID)
                .FirstOrDefault();

            decimal bestSingleDiscount = bestSingle.promo != null ? bestSingle.discount : 0m;
            decimal autoIncludedInSingle = bestSingle.promo != null ? bestSingle.item.AutoIncludedAmount : 0m;
            decimal singleEffectiveTotal = billTotal + autoIncludedInSingle;
            if (singleEffectiveTotal > 0)
            {
                bestSingleDiscount = Math.Min(bestSingleDiscount, singleEffectiveTotal);
            }

            // เลือกระบบที่ลูกค้าได้ประโยชน์สูงสุด (Best Benefit for Customer):
            // หากกลุ่ม B ได้ลดรวมมากกว่าหรือเท่ากับกลุ่ม A (และกลุ่ม B มีโปรอย่างน้อย 1 ตัว) ให้ใช้ชุดกลุ่ม B ทั้งหมด
            if (stackableList.Count > 0 && stackableTotalDiscount >= bestSingleDiscount)
            {
                var appliedPromos = stackableList.Select(x => x.promo).ToList();
                var appliedItems = stackableList.Select(x => x.item).ToList();
                string promoId = string.Join(", ", appliedPromos.Select(p => p.PromoID));
                string promoName = appliedPromos.Count == 1
                    ? (!string.IsNullOrWhiteSpace(appliedPromos[0].PromoName) ? appliedPromos[0].PromoName : appliedPromos[0].ConditionDescription)
                    : string.Join(" + ", appliedPromos.Select(p => !string.IsNullOrWhiteSpace(p.PromoName) ? p.PromoName : p.ConditionDescription));

                return new AutoPromotionResult
                {
                    PromoID = promoId,
                    PromoName = promoName,
                    DiscountAmount = stackableTotalDiscount,
                    Promotion = appliedPromos[0],
                    AppliedPromotions = appliedPromos,
                    AppliedItems = appliedItems
                };
            }
            else if (bestSingle.promo != null && bestSingleDiscount > 0)
            {
                return new AutoPromotionResult
                {
                    PromoID = bestSingle.promo.PromoID,
                    PromoName = !string.IsNullOrWhiteSpace(bestSingle.promo.PromoName) ? bestSingle.promo.PromoName : bestSingle.promo.ConditionDescription,
                    DiscountAmount = bestSingle.discount,
                    Promotion = bestSingle.promo,
                    AppliedPromotions = new List<PromotionModel> { bestSingle.promo },
                    AppliedItems = new List<AppliedPromotionResult> { bestSingle.item }
                };
            }

            return null;
        }

        /// <summary>
        /// Alias รองรับการเรียกชื่อเดิม EvaluateBestAutoPromotion
        /// </summary>
        public AutoPromotionResult? EvaluateBestAutoPromotion(
            IEnumerable<CartItem>? cartItems,
            bool isMember,
            decimal subtotal,
            List<PromotionModel>? preloadedPromos = null)
        {
            return EvaluateAutoPromotions(cartItems, isMember, subtotal, preloadedPromos);
        }

        /// <summary>
        /// ประเมินโปรโมชั่นอัตโนมัติแบบ Asynchronous
        /// </summary>
        public async Task<AutoPromotionResult?> EvaluateAutoPromotionsAsync(
            IEnumerable<CartItem>? cartItems,
            bool isMember,
            decimal subtotal)
        {
            var allPromos = await GetAllPromotionsAsync();
            return EvaluateAutoPromotions(cartItems, isMember, subtotal, allPromos);
        }

        /// <summary>
        /// Alias รองรับการเรียกชื่อเดิม EvaluateBestAutoPromotionAsync
        /// </summary>
        public async Task<AutoPromotionResult?> EvaluateBestAutoPromotionAsync(
            IEnumerable<CartItem>? cartItems,
            bool isMember,
            decimal subtotal)
        {
            return await EvaluateAutoPromotionsAsync(cartItems, isMember, subtotal);
        }

        #endregion
    }
}
