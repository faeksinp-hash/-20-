using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Porjai20.Models;

namespace Porjai20.Services
{
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

                var promos = (await conn.QueryAsync<PromotionModel>(sqlPromo)).ToList();

                // ดึงสินค้าที่ผูกทั้งหมด
                var promoProducts = (await conn.QueryAsync<(string PromoID, string ProductID)>(
                    "SELECT PromoID, ProductID FROM tblPromotionProducts;")).ToList();

                // ดึงข้อมูลสินค้าทั้งหมดจาก tblProduct เพื่อนำชื่อและต้นทุนมาแสดงผล
                var allProds = (await conn.QueryAsync<(int Pro_ID, string Pro_Name, decimal Pro_Price, decimal Pro_Cost)>(
                    "SELECT Pro_ID, Pro_Name, Pro_Price, Pro_Cost FROM tblProduct;")).ToList();

                var prodDict = allProds.ToDictionary(p => p.Pro_ID.ToString(), p => p);

                string todayStr = DateTime.Today.ToString("yyyy-MM-dd");

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

                    // Map Free Product Name & Cost
                    if (!string.IsNullOrWhiteSpace(p.FreeProductID) && prodDict.TryGetValue(p.FreeProductID, out var freeProd))
                    {
                        p.FreeProductName = freeProd.Pro_Name;
                        p.ItemCost = freeProd.Pro_Cost;
                    }
                    else if (p.ProductIds.Count > 0 && prodDict.TryGetValue(p.ProductIds[0], out var mainProd))
                    {
                        p.ItemCost = mainProd.Pro_Cost;
                    }

                    // Auto-Expire Rule: หาก EndDate < Today ให้ล็อกและถือว่าหมดอายุ
                    if (p.IsExpired && p.IsActive)
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
            using (var conn = _db.GetConnection())
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    await conn.ExecuteAsync("DELETE FROM tblPromotionProducts WHERE PromoID = @PromoID;", new { PromoID = promoId }, trans);
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

            // กรองเฉพาะโปรโมชั่นที่เปิดใช้งาน ไม่ใช่ดราฟท์ และยังไม่หมดอายุ
            var candidates = activePromos.Where(p =>
                p.IsActive &&
                !p.IsDraft &&
                !p.IsExpired &&
                (DateTime.TryParse(p.StartDate, out var start) ? start.Date <= today : true)
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

            // 2. อัปเดต CurrentUsage ใน tblPromotion
            foreach (var promo in evalResult.AppliedPromotions)
            {
                string sqlUsage = "UPDATE tblPromotion SET CurrentUsage = CurrentUsage + 1 WHERE PromoID = @PromoID;";
                await conn.ExecuteAsync(sqlUsage, new { promo.PromoID }, trans);
            }
        }

        #endregion
    }
}
