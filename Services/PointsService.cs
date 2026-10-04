using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Porjai20.Models;

namespace Porjai20.Services
{
    public class PointsConfig
    {
        public decimal EarnRateAmount { get; set; } = 100m; // ซื้อครบทุก X บาท
        public int EarnRatePoints { get; set; } = 1;         // ได้ Y แต้ม
        public int ExpiryMonths { get; set; } = 0;           // 0 = ไม่มีวันหมดอายุ
    }

    public class PointsService
    {
        private static PointsService? _instance;
        public static PointsService Instance => _instance ??= new PointsService();

        private PointsConfig _config = new PointsConfig();
        public PointsConfig Config
        {
            get => _config;
            set => _config = value ?? new PointsConfig();
        }

        public PointsService()
        {
            LoadConfigFromDb();
        }

        public void LoadConfigFromDb()
        {
            try
            {
                using var conn = DatabaseService.GetSharedConnection();
                conn.Execute(@"
                    CREATE TABLE IF NOT EXISTS tblPointsConfig (
                        Key TEXT PRIMARY KEY,
                        Value TEXT NOT NULL
                    );");

                var rows = conn.Query<(string Key, string Value)>("SELECT Key, Value FROM tblPointsConfig");
                foreach (var row in rows)
                {
                    if (row.Key == "EarnRateAmount" && decimal.TryParse(row.Value, out var amt))
                        _config.EarnRateAmount = amt;
                    else if (row.Key == "EarnRatePoints" && int.TryParse(row.Value, out var pts))
                        _config.EarnRatePoints = pts;
                    else if (row.Key == "ExpiryMonths" && int.TryParse(row.Value, out var exp))
                        _config.ExpiryMonths = exp;
                }
            }
            catch
            {
                // Fallback to default
            }
        }

        public void SaveConfigToDb(PointsConfig config)
        {
            _config = config;
            try
            {
                using var conn = DatabaseService.GetSharedConnection();
                conn.Execute(@"
                    CREATE TABLE IF NOT EXISTS tblPointsConfig (
                        Key TEXT PRIMARY KEY,
                        Value TEXT NOT NULL
                    );
                    INSERT OR REPLACE INTO tblPointsConfig (Key, Value) VALUES ('EarnRateAmount', @EarnRateAmount);
                    INSERT OR REPLACE INTO tblPointsConfig (Key, Value) VALUES ('EarnRatePoints', @EarnRatePoints);
                    INSERT OR REPLACE INTO tblPointsConfig (Key, Value) VALUES ('ExpiryMonths', @ExpiryMonths);",
                    new
                    {
                        EarnRateAmount = config.EarnRateAmount.ToString(),
                        EarnRatePoints = config.EarnRatePoints.ToString(),
                        ExpiryMonths = config.ExpiryMonths.ToString()
                    });
            }
            catch
            {
            }
        }

        /// <summary>
        /// Calculates earned points: Math.Floor(netAmount / EarnRateAmount) * EarnRatePoints (ปัดเศษลงจากยอดสุทธิหลังหักส่วนลด)
        /// </summary>
        public int CalculateEarnedPoints(decimal netAmount)
        {
            if (Config.EarnRateAmount <= 0) return 0;
            if (netAmount <= 0) return 0;
            return (int)Math.Floor(netAmount / Config.EarnRateAmount) * Config.EarnRatePoints;
        }

        /// <summary>
        /// Gets current balance of points from tblPointsLedger (minimum 0).
        /// Matches by CustomerId or CustomerCode using TRIM.
        /// </summary>
        public int GetCustomerPoints(string customerId, IDbConnection conn, IDbTransaction? trans = null, string? customerCode = null)
        {
            if (string.IsNullOrWhiteSpace(customerId) && string.IsNullOrWhiteSpace(customerCode)) return 0;
            string key1 = customerId?.Trim() ?? string.Empty;
            string key2 = customerCode?.Trim() ?? string.Empty;

            const string sql = @"
                SELECT COALESCE(SUM(Points), 0) 
                FROM tblPointsLedger 
                WHERE TRIM(CustomerId) = @Key1 
                   OR (length(@Key2) > 0 AND TRIM(CustomerId) = @Key2);";

            int pts = conn.ExecuteScalar<int>(sql, new { Key1 = key1, Key2 = key2 }, trans);
            if (pts <= 0)
            {
                // Fallback check from tblCustomer.Cus_Points
                int dbCusPoints = conn.ExecuteScalar<int>(@"
                    SELECT COALESCE(Cus_Points, 0) FROM tblCustomer 
                    WHERE TRIM(CAST(Cus_ID AS TEXT)) = @Key1 
                       OR TRIM(Cus_Code) = @Key1 
                       OR (length(@Key2) > 0 AND (TRIM(Cus_Code) = @Key2 OR TRIM(CAST(Cus_ID AS TEXT)) = @Key2));",
                    new { Key1 = key1, Key2 = key2 }, trans);
                if (dbCusPoints > 0) return dbCusPoints;
            }
            return Math.Max(0, pts);
        }

        /// <summary>
        /// Gets points summary: (TotalEarned (+), TotalRedeemed (-), CurrentPoints, Balance).
        /// Matches by CustomerId or CustomerCode using TRIM.
        /// </summary>
        public (int TotalEarned, int TotalRedeemed, int CurrentPoints, int Balance) GetPointsSummary(string customerId, IDbConnection conn, IDbTransaction? trans = null, string? customerCode = null)
        {
            if (string.IsNullOrWhiteSpace(customerId) && string.IsNullOrWhiteSpace(customerCode)) return (0, 0, 0, 0);
            string key1 = customerId?.Trim() ?? string.Empty;
            string key2 = customerCode?.Trim() ?? string.Empty;

            const string sql = @"
                SELECT 
                    COALESCE(SUM(CASE WHEN Points > 0 THEN Points ELSE 0 END), 0) AS Earned,
                    COALESCE(ABS(SUM(CASE WHEN Points < 0 THEN Points ELSE 0 END)), 0) AS Redeemed,
                    COALESCE(SUM(Points), 0) AS Balance
                FROM tblPointsLedger 
                WHERE TRIM(CustomerId) = @Key1 
                   OR (length(@Key2) > 0 AND TRIM(CustomerId) = @Key2);";

            var result = conn.QueryFirstOrDefault<(int Earned, int Redeemed, int Balance)>(sql, new { Key1 = key1, Key2 = key2 }, trans);
            int current = Math.Max(0, result.Balance);
            int earned = result.Earned;

            if (current == 0 && earned == 0)
            {
                // Fallback check from tblCustomer.Cus_Points
                int dbCusPoints = conn.ExecuteScalar<int>(@"
                    SELECT COALESCE(Cus_Points, 0) FROM tblCustomer 
                    WHERE TRIM(CAST(Cus_ID AS TEXT)) = @Key1 
                       OR TRIM(Cus_Code) = @Key1 
                       OR (length(@Key2) > 0 AND (TRIM(Cus_Code) = @Key2 OR TRIM(CAST(Cus_ID AS TEXT)) = @Key2));",
                    new { Key1 = key1, Key2 = key2 }, trans);
                if (dbCusPoints > 0)
                {
                    return (dbCusPoints, 0, dbCusPoints, dbCusPoints);
                }
            }

            return (earned, result.Redeemed, current, current);
        }

        /// <summary>
        /// Records an 'earn' entry into tblPointsLedger in the given transaction.
        /// </summary>
        public void RecordEarn(string customerId, string billId, int points, string? note, IDbConnection conn, IDbTransaction? trans = null)
        {
            if (string.IsNullOrWhiteSpace(customerId) || points <= 0) return;

            string sql = @"
                INSERT INTO tblPointsLedger (CustomerId, BillId, Type, Points, Note, CreatedAt)
                VALUES (@CustomerId, @BillId, 'earn', @Points, @Note, @CreatedAt);";

            conn.Execute(sql, new
            {
                CustomerId = customerId,
                BillId = billId,
                Points = points,
                Note = string.IsNullOrWhiteSpace(note) ? $"ได้รับแต้มจากบิล {billId}" : note,
                CreatedAt = DateTime.UtcNow.ToString("o")
            }, trans);

            SyncCustomerPoints(customerId, conn, trans);
        }

        /// <summary>
        /// Records a 'redeem' entry into tblPointsLedger in the given transaction (Points stored as negative).
        /// </summary>
        public void RecordRedeem(string customerId, string billId, int points, string? note, IDbConnection conn, IDbTransaction? trans = null)
        {
            if (string.IsNullOrWhiteSpace(customerId) || points <= 0) return;

            string sql = @"
                INSERT INTO tblPointsLedger (CustomerId, BillId, Type, Points, Note, CreatedAt)
                VALUES (@CustomerId, @BillId, 'redeem', @Points, @Note, @CreatedAt);";

            conn.Execute(sql, new
            {
                CustomerId = customerId,
                BillId = billId,
                Points = -Math.Abs(points),
                Note = string.IsNullOrWhiteSpace(note) ? "ใช้แต้มแลกส่วนลด" : note,
                CreatedAt = DateTime.UtcNow.ToString("o")
            }, trans);

            SyncCustomerPoints(customerId, conn, trans);
        }

        /// <summary>
        /// Records an 'adjust' entry into tblPointsLedger.
        /// </summary>
        public void RecordAdjust(string customerId, string? billId, int points, string? note, IDbConnection conn, IDbTransaction? trans = null)
        {
            if (string.IsNullOrWhiteSpace(customerId) || points == 0) return;

            string sql = @"
                INSERT INTO tblPointsLedger (CustomerId, BillId, Type, Points, Note, CreatedAt)
                VALUES (@CustomerId, @BillId, 'adjust', @Points, @Note, @CreatedAt);";

            conn.Execute(sql, new
            {
                CustomerId = customerId,
                BillId = billId,
                Points = points,
                Note = string.IsNullOrWhiteSpace(note) ? "ปรับปรุงแต้ม" : note,
                CreatedAt = DateTime.UtcNow.ToString("o")
            }, trans);

            SyncCustomerPoints(customerId, conn, trans);
        }

        /// <summary>
        /// Reverses points for a voided/cancelled bill.
        /// </summary>
        public void ReverseBillPoints(string billId, string customerId, string reason, IDbConnection conn, IDbTransaction? trans = null)
        {
            if (string.IsNullOrWhiteSpace(billId) || string.IsNullOrWhiteSpace(customerId)) return;

            // Find all ledger entries for this bill
            var entries = conn.Query<PointsLedger>(
                "SELECT * FROM tblPointsLedger WHERE BillId = @BillId AND CustomerId = @CustomerId",
                new { BillId = billId, CustomerId = customerId }, trans);

            foreach (var item in entries)
            {
                // Opposite points
                int reversePoints = -item.Points;
                if (reversePoints != 0)
                {
                    string note = $"ย้อนแต้มบิล {billId} ({item.TypeDisplay}) เหตุผล: {reason}";
                    RecordAdjust(customerId, billId, reversePoints, note, conn, trans);
                }
            }

            SyncCustomerPoints(customerId, conn, trans);
        }

        /// <summary>
        /// Synchronizes tblCustomer.Cus_Points with current sum of tblPointsLedger.
        /// </summary>
        public void SyncCustomerPoints(string customerId, IDbConnection conn, IDbTransaction? trans = null, string? customerCode = null)
        {
            try
            {
                string key1 = customerId?.Trim() ?? string.Empty;
                string key2 = customerCode?.Trim() ?? string.Empty;

                string syncSql = @"
                    UPDATE tblCustomer 
                    SET Cus_Points = MAX(0, (
                        SELECT COALESCE(SUM(Points), 0) 
                        FROM tblPointsLedger 
                        WHERE TRIM(CustomerId) = @Key1 
                           OR (length(@Key2) > 0 AND TRIM(CustomerId) = @Key2)
                    ))
                    WHERE TRIM(CAST(Cus_ID AS TEXT)) = @Key1 
                       OR TRIM(Cus_Code) = @Key1
                       OR (length(@Key2) > 0 AND (TRIM(Cus_Code) = @Key2 OR TRIM(CAST(Cus_ID AS TEXT)) = @Key2));";

                conn.Execute(syncSql, new { Key1 = key1, Key2 = key2 }, trans);
            }
            catch
            {
            }
        }
    }
}
