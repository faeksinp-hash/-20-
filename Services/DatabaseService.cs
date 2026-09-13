using System.Data;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Porjai20.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            string appDataPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "MatchaShop");
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            string dbPath = Path.Combine(appDataPath, "matcha.db");

            // Migration / fallback: If matcha.db exists in BaseDirectory (from previous run) but not in AppData, copy it over once
            string legacyDbPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "matcha.db");
            if (File.Exists(legacyDbPath) && !File.Exists(dbPath))
            {
                try
                {
                    File.Copy(legacyDbPath, dbPath, overwrite: false);
                }
                catch { }
            }

            _connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        public IDbConnection GetConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA foreign_keys = ON;";
                cmd.ExecuteNonQuery();
            }
            return connection;
        }

        private void InitializeDatabase()
        {
            using (var connection = GetConnection())
            {
                // 1) tblEmployee
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS tblEmployee (
                        Emp_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Emp_Name TEXT,
                        Emp_Address TEXT,
                        Emp_Tel TEXT,
                        Emp_Username TEXT,
                        Emp_Password TEXT,
                        Emp_Role TEXT
                    );");

                // 2) tblCustomer
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS tblCustomer (
                        Cus_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Cus_Code TEXT,
                        Cus_Name TEXT,
                        Cus_Address TEXT,
                        Cus_Tel TEXT,
                        Cus_Points INTEGER DEFAULT 0,
                        Cus_RegDate TEXT
                    );");

                // Schema Migration for existing databases: ensure Cus_Code and Cus_RegDate exist
                try { connection.Execute("ALTER TABLE tblCustomer ADD COLUMN Cus_Code TEXT;"); } catch { }
                try { connection.Execute("ALTER TABLE tblCustomer ADD COLUMN Cus_RegDate TEXT;"); } catch { }

                // 3) tblPartner
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS tblPartner (
                        Partner_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Partner_Name TEXT,
                        Partner_Address TEXT,
                        Partner_Tel TEXT,
                        Partner_Contact TEXT
                    );");

                // 4) tblProduct
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS tblProduct (
                        Pro_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Pro_Barcode TEXT,
                        Pro_Name TEXT,
                        Pro_Price REAL,
                        Pro_Cost REAL,
                        Pro_Qty INTEGER,
                        Pro_MinQty INTEGER,
                        Pro_Category TEXT,
                        Pro_Image TEXT
                    );");

                // 5) tblPO_H
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS tblPO_H (
                        PO_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        PO_Date TEXT,
                        Partner_ID INTEGER,
                        Emp_ID INTEGER,
                        PO_Total REAL,
                        PO_Status TEXT,
                        FOREIGN KEY(Partner_ID) REFERENCES tblPartner(Partner_ID),
                        FOREIGN KEY(Emp_ID) REFERENCES tblEmployee(Emp_ID)
                    );");

                // 6) tblPODetail
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS tblPODetail (
                        Detail_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        PO_ID INTEGER,
                        Pro_ID INTEGER,
                        PO_Cost REAL,
                        PO_Qty INTEGER,
                        PO_Subtotal REAL,
                        FOREIGN KEY(PO_ID) REFERENCES tblPO_H(PO_ID),
                        FOREIGN KEY(Pro_ID) REFERENCES tblProduct(Pro_ID)
                    );");

                // 7) tblStockIn_H
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS tblStockIn_H (
                        StockIn_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        StockIn_Date TEXT,
                        PO_ID INTEGER,
                        Emp_ID INTEGER,
                        Note TEXT,
                        FOREIGN KEY(PO_ID) REFERENCES tblPO_H(PO_ID),
                        FOREIGN KEY(Emp_ID) REFERENCES tblEmployee(Emp_ID)
                    );");

                // 8) tblStockInDetail
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS tblStockInDetail (
                        Detail_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        StockIn_ID INTEGER,
                        Pro_ID INTEGER,
                        StockIn_Qty INTEGER,
                        FOREIGN KEY(StockIn_ID) REFERENCES tblStockIn_H(StockIn_ID),
                        FOREIGN KEY(Pro_ID) REFERENCES tblProduct(Pro_ID)
                    );");

                // 9) tblSales_H
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS tblSales_H (
                        Sales_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        RefNo TEXT,
                        Sales_Date TEXT,
                        Cus_ID INTEGER,
                        Emp_ID INTEGER,
                        Sales_Total REAL,
                        Sales_Cash REAL,
                        Sales_Change REAL,
                        Sales_PaymentType TEXT,
                        Sales_Status TEXT,
                        FOREIGN KEY(Cus_ID) REFERENCES tblCustomer(Cus_ID),
                        FOREIGN KEY(Emp_ID) REFERENCES tblEmployee(Emp_ID)
                    );");

                // 10) tblSalesDetail
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS tblSalesDetail (
                        Detail_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Sales_ID INTEGER,
                        Pro_ID INTEGER,
                        Pro_Price REAL,
                        Sales_Qty INTEGER,
                        Sales_Subtotal REAL,
                        FOREIGN KEY(Sales_ID) REFERENCES tblSales_H(Sales_ID),
                        FOREIGN KEY(Pro_ID) REFERENCES tblProduct(Pro_ID)
                    );");

                // 11) tblDelivery
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS tblDelivery (
                        Delivery_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Sales_ID INTEGER,
                        Recipient_Name TEXT,
                        Recipient_Tel TEXT,
                        Recipient_Address TEXT,
                        Tracking_No TEXT,
                        Delivery_Status TEXT,
                        FOREIGN KEY(Sales_ID) REFERENCES tblSales_H(Sales_ID)
                    );");

                // 12) tblClaim
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS tblClaim (
                        Claim_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Sales_ID INTEGER,
                        Pro_ID INTEGER,
                        Claim_Date TEXT,
                        Claim_Reason TEXT,
                        Claim_Status TEXT,
                        Claim_Action TEXT,
                        FOREIGN KEY(Sales_ID) REFERENCES tblSales_H(Sales_ID),
                        FOREIGN KEY(Pro_ID) REFERENCES tblProduct(Pro_ID)
                    );");
                try { connection.Execute("ALTER TABLE tblClaim ADD COLUMN Claim_Action TEXT;"); } catch { }

                // 13) tblExpense
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS tblExpense (
                        Expense_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Expense_Date TEXT,
                        Expense_Category TEXT,
                        Expense_Amount REAL,
                        Expense_Note TEXT,
                        Emp_ID INTEGER,
                        FOREIGN KEY(Emp_ID) REFERENCES tblEmployee(Emp_ID)
                    );");

                // 14) tblProductCategory
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS tblProductCategory (
                        CategoryId INTEGER PRIMARY KEY AUTOINCREMENT,
                        CategoryCode TEXT,
                        CategoryName TEXT NOT NULL,
                        Description TEXT
                    );");

                // Seed Default Categories if empty
                int catCount = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM tblProductCategory;");
                if (catCount == 0)
                {
                    connection.Execute(@"
                        INSERT INTO tblProductCategory (CategoryCode, CategoryName, Description) VALUES
                        ('CAT-001', 'เครื่องเขียน', 'อุปกรณ์เครื่องเขียนและเครื่องใช้สำนักงาน'),
                        ('CAT-002', 'ของใช้ในครัวเรือน', 'สินค้าและอุปกรณ์ของใช้ในบ้านเรือน'),
                        ('CAT-003', 'ของเล่น', 'ของเล่นเสริมทักษะและของเล่นเด็ก'),
                        ('CAT-004', 'เบ็ดเตล็ด', 'สินค้าเบ็ดเตล็ดทั่วไป');");
                }

                // Seed Default Admin User ONLY if employee table is empty
                int employeeCount = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM tblEmployee;");
                if (employeeCount == 0)
                {
                    string adminHash = BCrypt.Net.BCrypt.HashPassword("password");
                    connection.Execute(@"
                        INSERT INTO tblEmployee (Emp_Name, Emp_Username, Emp_Password, Emp_Role) 
                        VALUES ('Administrator', 'admin', @Password, @Role);",
                        new { Password = adminHash, Role = RolePermissions.RoleOwner });
                }

                // Migrate existing plain-text passwords in tblEmployee to BCrypt hashes
                MigrateEmployeePasswords(connection);

                // Migrate existing employee roles from legacy values ('Admin', 'User', null/empty) to standard roles
                MigrateEmployeeRoles(connection);

                // Migrate sales dates and payment methods to standard formats
                MigrateSalesRecords(connection);
            }
        }

        /// <summary>
        /// ตรวจสอบว่าข้อความเป็น BCrypt Hash หรือไม่ (ความยาว 60 ตัวอักษร และขึ้นต้นด้วย $2a$, $2b$, $2y$, $2x$)
        /// </summary>
        public static bool IsBCryptHash(string? password)
        {
            if (string.IsNullOrEmpty(password) || password.Length != 60)
                return false;

            return password.StartsWith("$2a$") || password.StartsWith("$2b$") || password.StartsWith("$2y$") || password.StartsWith("$2x$");
        }

        /// <summary>
        /// Migration สำหรับแปลงรหัสผ่านเดิมที่เป็น plain text ในตาราง tblEmployee ให้เป็น BCrypt hash อัตโนมัติ
        /// </summary>
        private void MigrateEmployeePasswords(IDbConnection connection)
        {
            try
            {
                var employees = connection.Query<(int Emp_ID, string? Emp_Password)>(
                    "SELECT Emp_ID, Emp_Password FROM tblEmployee WHERE Emp_Password IS NOT NULL AND Emp_Password != '';"
                );

                foreach (var emp in employees)
                {
                    if (!string.IsNullOrEmpty(emp.Emp_Password) && !IsBCryptHash(emp.Emp_Password))
                    {
                        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(emp.Emp_Password);
                        connection.Execute(
                            "UPDATE tblEmployee SET Emp_Password = @HashedPassword WHERE Emp_ID = @Emp_ID;",
                            new { HashedPassword = hashedPassword, Emp_ID = emp.Emp_ID }
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Migration] Error migrating employee passwords: {ex.Message}");
            }
        }

        /// <summary>
        /// Migration สำหรับแปลง Role พนักงานเดิมที่เป็นค่าเก่า ('Admin', 'User', หรือค่าว่าง) ให้เป็น 3 สิทธิ์มาตรฐาน
        /// </summary>
        private void MigrateEmployeeRoles(IDbConnection connection)
        {
            try
            {
                // แปลงสิทธิ์เก่า 'Admin' -> 'เจ้าของร้าน'
                connection.Execute(
                    "UPDATE tblEmployee SET Emp_Role = @OwnerRole WHERE Emp_Role = 'Admin';",
                    new { OwnerRole = RolePermissions.RoleOwner }
                );

                // แปลงสิทธิ์เก่า 'User' หรือค่าว่าง/NULL -> 'พนักงานทั่วไป'
                connection.Execute(
                    "UPDATE tblEmployee SET Emp_Role = @EmployeeRole WHERE Emp_Role = 'User' OR Emp_Role IS NULL OR TRIM(Emp_Role) = '';",
                    new { EmployeeRole = RolePermissions.RoleEmployee }
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Migration] Error migrating employee roles: {ex.Message}");
            }
        }

        /// <summary>
        /// Migration สำหรับแปลงฟิลด์วันที่ พ.ศ. (เช่น 2569) ให้เป็น ค.ศ. สากล (2026) และเติมค่าชำระเงินเริ่มต้น
        /// </summary>
        private void MigrateSalesRecords(IDbConnection connection)
        {
            try
            {
                connection.Execute(@"
                    UPDATE tblSales_H
                    SET Sales_Date = CAST(CAST(SUBSTR(Sales_Date, 1, 4) AS INTEGER) - 543 AS TEXT) || SUBSTR(Sales_Date, 5)
                    WHERE LENGTH(Sales_Date) >= 10 AND CAST(SUBSTR(Sales_Date, 1, 4) AS INTEGER) > 2400;
                ");

                connection.Execute(@"
                    UPDATE tblSales_H
                    SET Sales_PaymentType = 'เงินสด'
                    WHERE Sales_PaymentType IS NULL OR TRIM(Sales_PaymentType) = '';
                ");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Migration] Error migrating sales records: {ex.Message}");
            }
        }

        public int SaveSalesOrder(Models.SalesOrder order)
        {
            using (var connection = GetConnection())
            {
                string sql = @"
                    INSERT INTO tblSales_H (RefNo, Sales_Date, Cus_ID, Emp_ID, Sales_Total, Sales_Cash, Sales_Change, Sales_PaymentType, Sales_Status) 
                    VALUES (@RefNo, @Sales_Date, @Cus_ID, @Emp_ID, @Sales_Total, @Sales_Cash, @Sales_Change, @Sales_PaymentType, @Sales_Status);
                    SELECT last_insert_rowid();";
                var param = new
                {
                    order.RefNo,
                    Sales_Date = string.IsNullOrWhiteSpace(order.Sales_Date) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) : order.Sales_Date,
                    Cus_ID = order.Cus_ID > 0 ? (int?)order.Cus_ID : null,
                    Emp_ID = order.Emp_ID > 0 ? (int?)order.Emp_ID : null,
                    order.Sales_Total,
                    order.Sales_Cash,
                    order.Sales_Change,
                    Sales_PaymentType = !string.IsNullOrWhiteSpace(order.Sales_PaymentType) ? order.Sales_PaymentType : "เงินสด",
                    Sales_Status = !string.IsNullOrWhiteSpace(order.Sales_Status) ? order.Sales_Status : "ชำระเงินแล้ว"
                };
                return connection.ExecuteScalar<int>(sql, param);
            }
        }

        public void SaveSalesOrderItem(Models.SalesOrderItem item)
        {
            using (var connection = GetConnection())
            {
                string sql = "INSERT INTO tblSalesDetail (Sales_ID, Pro_ID, Pro_Price, Sales_Qty, Sales_Subtotal) VALUES (@Sales_ID, @Pro_ID, @Pro_Price, @Sales_Qty, @Sales_Subtotal)";
                connection.Execute(sql, item);
            }
        }

        public System.Collections.Generic.IEnumerable<Models.SalesOrder> GetDeliveries(string searchTerm = "")
        {
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        s.Sales_ID AS Sales_ID, s.Sales_ID AS Id, s.RefNo, s.Sales_Date, s.Sales_Total,
                        s.Sales_Cash, s.Sales_Change, s.Sales_PaymentType, s.Sales_Status, s.Cus_ID,
                        COALESCE(NULLIF(d.Recipient_Name, ''), NULLIF(c.Cus_Name, ''), '-') AS CustomerName,
                        COALESCE(NULLIF(d.Recipient_Tel, ''), NULLIF(c.Cus_Tel, ''), '-') AS CustomerPhone,
                        COALESCE(NULLIF(d.Recipient_Address, ''), NULLIF(c.Cus_Address, ''), 'ไม่ระบุที่อยู่') AS CustomerAddress,
                        COALESCE(NULLIF(d.Delivery_Status, ''), 'รอจัดส่ง') AS DeliveryStatus
                    FROM tblSales_H s 
                    JOIN tblDelivery d ON s.Sales_ID = d.Sales_ID
                    LEFT JOIN tblCustomer c ON s.Cus_ID = c.Cus_ID
                    WHERE d.Recipient_Name LIKE @Search OR d.Recipient_Tel LIKE @Search OR c.Cus_Name LIKE @Search OR c.Cus_Tel LIKE @Search OR s.RefNo LIKE @Search
                    ORDER BY s.Sales_Date DESC";
                return connection.Query<Models.SalesOrder>(sql, new { Search = "%" + searchTerm + "%" });
            }
        }

        public void UpdateDeliveryStatus(int orderId, string status)
        {
            using (var connection = GetConnection())
            {
                string sql = "UPDATE tblDelivery SET Delivery_Status = @Status WHERE Sales_ID = @Id";
                connection.Execute(sql, new { Status = status, Id = orderId });
            }
        }

        // --- Customer Management Methods ---
        public System.Collections.Generic.IEnumerable<Models.Customer> GetCustomers(string searchKeyword = "")
        {
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        c.Cus_ID AS Cus_ID, c.Cus_ID AS Id,
                        COALESCE(c.Cus_Code, 'CUS-' || printf('%05d', c.Cus_ID)) AS Code,
                        c.Cus_Name AS Cus_Name, c.Cus_Name AS Name,
                        c.Cus_Address AS Cus_Address, c.Cus_Address AS Address,
                        c.Cus_Tel AS Cus_Tel, c.Cus_Tel AS Phone,
                        c.Cus_Points AS Cus_Points, c.Cus_Points AS Points,
                        c.Cus_RegDate AS RegistrationDate,
                        COALESCE(COUNT(s.Sales_ID), 0) AS TotalPurchases,
                        COALESCE(CAST(SUM(s.Sales_Total) AS REAL), 0.0) AS TotalSpent
                    FROM tblCustomer c
                    LEFT JOIN tblSales_H s ON s.Cus_ID = c.Cus_ID";

                if (!string.IsNullOrWhiteSpace(searchKeyword))
                {
                    sql += " WHERE c.Cus_Name LIKE @Search OR c.Cus_Code LIKE @Search OR c.Cus_Tel LIKE @Search";
                }

                sql += @"
                    GROUP BY c.Cus_ID, c.Cus_Code, c.Cus_Name, c.Cus_Address, c.Cus_Tel, c.Cus_Points, c.Cus_RegDate
                    ORDER BY c.Cus_ID DESC";

                return connection.Query<Models.Customer>(sql, new { Search = "%" + searchKeyword + "%" });
            }
        }

        public void SaveCustomer(Models.Customer customer)
        {
            using (var connection = GetConnection())
            {
                string sql = @"
                    INSERT INTO tblCustomer (Cus_Code, Cus_Name, Cus_Address, Cus_Tel, Cus_Points, Cus_RegDate) 
                    VALUES (@Code, @Cus_Name, @Cus_Address, @Cus_Tel, @Cus_Points, @RegistrationDate)";
                connection.Execute(sql, customer);
            }
        }

        public void UpdateCustomer(Models.Customer customer)
        {
            using (var connection = GetConnection())
            {
                string sql = @"
                    UPDATE tblCustomer 
                    SET Cus_Code = @Code, Cus_Name = @Cus_Name, Cus_Address = @Cus_Address, Cus_Tel = @Cus_Tel, Cus_Points = @Cus_Points 
                    WHERE Cus_ID = @Cus_ID";
                connection.Execute(sql, customer);
            }
        }

        public void DeleteCustomer(int customerId)
        {
            using (var connection = GetConnection())
            {
                string sql = "DELETE FROM tblCustomer WHERE Cus_ID = @Id";
                connection.Execute(sql, new { Id = customerId });
            }
        }

        // --- Partner Management Methods ---
        public System.Collections.Generic.IEnumerable<Models.Partner> GetPartners(string searchKeyword = "")
        {
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        Partner_ID AS Partner_ID, Partner_ID AS Id,
                        Partner_Name AS Partner_Name, Partner_Name AS Name,
                        Partner_Address AS Partner_Address, Partner_Address AS Address,
                        Partner_Tel AS Partner_Tel, Partner_Tel AS Phone,
                        Partner_Contact AS Partner_Contact, Partner_Contact AS ContactPerson
                    FROM tblPartner";
                if (!string.IsNullOrWhiteSpace(searchKeyword))
                {
                    sql += " WHERE Partner_Name LIKE @Search OR Partner_Tel LIKE @Search OR Partner_Contact LIKE @Search";
                }
                sql += " ORDER BY Partner_ID DESC";
                return connection.Query<Models.Partner>(sql, new { Search = "%" + searchKeyword + "%" });
            }
        }

        public void SavePartner(Models.Partner partner)
        {
            using (var connection = GetConnection())
            {
                string sql = @"
                    INSERT INTO tblPartner (Partner_Name, Partner_Address, Partner_Tel, Partner_Contact) 
                    VALUES (@Partner_Name, @Partner_Address, @Partner_Tel, @Partner_Contact)";
                connection.Execute(sql, partner);
            }
        }

        public void UpdatePartner(Models.Partner partner)
        {
            using (var connection = GetConnection())
            {
                string sql = @"
                    UPDATE tblPartner 
                    SET Partner_Name = @Partner_Name, Partner_Address = @Partner_Address, 
                        Partner_Tel = @Partner_Tel, Partner_Contact = @Partner_Contact
                    WHERE Partner_ID = @Partner_ID";
                connection.Execute(sql, partner);
            }
        }

        public void DeletePartner(int partnerId)
        {
            using (var connection = GetConnection())
            {
                string sql = "DELETE FROM tblPartner WHERE Partner_ID = @Id";
                connection.Execute(sql, new { Id = partnerId });
            }
        }

        // --- Claims Management Methods ---
        public System.Collections.Generic.IEnumerable<Models.Claim> GetClaims(string searchKeyword = "", string statusFilter = "")
        {
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        c.Claim_ID AS Claim_ID, c.Claim_ID AS Id,
                        c.Sales_ID AS Sales_ID, c.Pro_ID AS Pro_ID,
                        c.Claim_Date AS Claim_Date, c.Claim_Reason AS Claim_Reason,
                        c.Claim_Status AS Claim_Status,
                        COALESCE(c.Claim_Action, 'เปลี่ยนสินค้าใหม่') AS Claim_Action,
                        COALESCE(c.Claim_Action, 'เปลี่ยนสินค้าใหม่') AS ClaimAction,
                        p.Pro_Name AS ProductName, p.Pro_Barcode AS ProductCode
                    FROM tblClaim c
                    LEFT JOIN tblProduct p ON c.Pro_ID = p.Pro_ID
                    ORDER BY c.Claim_ID DESC";
                return connection.Query<Models.Claim>(sql);
            }
        }

        public Models.SalesOrder? LookupSalesOrder(string refNo)
        {
            using (var connection = GetConnection())
            {
                return connection.QueryFirstOrDefault<Models.SalesOrder>(
                    "SELECT Sales_ID AS Sales_ID, Sales_ID AS Id, * FROM tblSales_H WHERE RefNo = @RefNo", new { RefNo = refNo });
            }
        }

        public Models.ClaimedReceiptDetails? LookupReceiptDetails(string refNo)
        {
            if (string.IsNullOrWhiteSpace(refNo)) return null;
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        so.Sales_ID AS Sales_ID,
                        soi.Pro_ID AS Pro_ID,
                        so.Sales_Date AS SaleDate,
                        p.Pro_Barcode AS ProductCode,
                        soi.Sales_Qty AS Quantity,
                        p.Pro_Name AS ProductName,
                        c.Cus_Name AS CustomerName,
                        c.Cus_Tel AS CustomerPhone
                    FROM tblSales_H so
                    JOIN tblSalesDetail soi ON so.Sales_ID = soi.Sales_ID
                    JOIN tblProduct p ON soi.Pro_ID = p.Pro_ID
                    LEFT JOIN tblCustomer c ON so.Cus_ID = c.Cus_ID
                    WHERE so.RefNo = @RefNo OR CAST(so.Sales_ID AS TEXT) = @RefNo
                    LIMIT 1";
                return connection.QueryFirstOrDefault<Models.ClaimedReceiptDetails>(sql, new { RefNo = refNo.Trim() });
            }
        }

        public Models.StockTransaction? LookupStockTransaction(string refNo)
        {
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT d.Detail_ID AS Id, h.StockIn_Date AS Timestamp, p.Pro_Name AS ProductName, d.StockIn_Qty AS Quantity
                    FROM tblStockInDetail d
                    JOIN tblStockIn_H h ON d.StockIn_ID = h.StockIn_ID
                    JOIN tblProduct p ON d.Pro_ID = p.Pro_ID
                    LIMIT 1";
                return connection.QueryFirstOrDefault<Models.StockTransaction>(sql);
            }
        }

        public System.Collections.Generic.IEnumerable<Models.StockTransaction> GetStockTransactions(string keyword = "")
        {
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        d.Detail_ID AS Detail_ID, d.Detail_ID AS Id,
                        d.StockIn_ID AS StockIn_ID, d.Pro_ID AS Pro_ID,
                        d.StockIn_Qty AS Quantity, h.StockIn_Date AS Timestamp,
                        p.Pro_Name AS ProductName, p.Pro_Barcode AS ProductCode
                    FROM tblStockInDetail d
                    JOIN tblStockIn_H h ON d.StockIn_ID = h.StockIn_ID
                    JOIN tblProduct p ON d.Pro_ID = p.Pro_ID
                    ORDER BY d.Detail_ID DESC";
                return connection.Query<Models.StockTransaction>(sql);
            }
        }

        public Models.ClaimedStockInDetails? LookupStockInDetails(string refNo)
        {
            if (string.IsNullOrWhiteSpace(refNo)) return null;
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        d.Pro_ID AS Pro_ID,
                        h.StockIn_Date AS StockInDate,
                        p.Pro_Barcode AS ProductCode,
                        d.StockIn_Qty AS Quantity,
                        p.Pro_Name AS ProductName
                    FROM tblStockInDetail d
                    JOIN tblStockIn_H h ON d.StockIn_ID = h.StockIn_ID
                    JOIN tblProduct p ON d.Pro_ID = p.Pro_ID
                    WHERE h.StockIn_ID = @RefNo OR CAST(h.StockIn_ID AS TEXT) = @RefNo OR h.Note LIKE '%' || @RefNo || '%'
                    LIMIT 1";
                return connection.QueryFirstOrDefault<Models.ClaimedStockInDetails>(sql, new { RefNo = refNo.Trim() });
            }
        }

        public int GetSalesIdByRefNo(string refNo)
        {
            if (string.IsNullOrWhiteSpace(refNo)) return 0;
            using (var connection = GetConnection())
            {
                string sql = "SELECT Sales_ID FROM tblSales_H WHERE RefNo = @RefNo OR CAST(Sales_ID AS TEXT) = @RefNo LIMIT 1";
                return connection.ExecuteScalar<int?>(sql, new { RefNo = refNo.Trim() }) ?? 0;
            }
        }

        public int GetProductIdByBarcodeOrName(string code, string name = "")
        {
            using (var connection = GetConnection())
            {
                if (!string.IsNullOrWhiteSpace(code))
                {
                    string sqlCode = "SELECT Pro_ID FROM tblProduct WHERE Pro_Barcode = @Code OR CAST(Pro_ID AS TEXT) = @Code LIMIT 1";
                    var id = connection.ExecuteScalar<int?>(sqlCode, new { Code = code.Trim() });
                    if (id.HasValue && id.Value > 0) return id.Value;
                }

                if (!string.IsNullOrWhiteSpace(name))
                {
                    string sqlName = "SELECT Pro_ID FROM tblProduct WHERE Pro_Name = @Name LIMIT 1";
                    var id = connection.ExecuteScalar<int?>(sqlName, new { Name = name.Trim() });
                    if (id.HasValue && id.Value > 0) return id.Value;
                }

                return 0;
            }
        }

        public bool SaveClaim(Models.Claim claim, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (claim == null)
            {
                errorMessage = "กรุณาเลือกใบเสร็จและสินค้าที่ต้องการเคลมให้ถูกต้อง";
                return false;
            }

            // 1. Map Foreign Keys before INSERT if not set
            if (claim.Sales_ID <= 0 && !string.IsNullOrWhiteSpace(claim.SalesOrderRefNo))
            {
                claim.Sales_ID = GetSalesIdByRefNo(claim.SalesOrderRefNo);
            }

            if (claim.Pro_ID <= 0)
            {
                claim.Pro_ID = GetProductIdByBarcodeOrName(claim.ProductCode, claim.ProductName);
            }

            // 2. Validate Foreign Keys before INSERT to prevent SQLite FK error 19
            if (claim.Sales_ID <= 0 || claim.Pro_ID <= 0)
            {
                errorMessage = "กรุณาเลือกใบเสร็จและสินค้าที่ต้องการเคลมให้ถูกต้อง";
                return false;
            }

            // 3. Try-Catch SQLite Execution to prevent application crash
            try
            {
                using (var connection = GetConnection())
                {
                    string sql = @"
                        INSERT INTO tblClaim (Sales_ID, Pro_ID, Claim_Date, Claim_Reason, Claim_Status, Claim_Action) 
                        VALUES (@Sales_ID, @Pro_ID, @Claim_Date, @Claim_Reason, @Claim_Status, @ClaimAction)";
                    connection.Execute(sql, claim);
                }
                return true;
            }
            catch (SqliteException ex)
            {
                errorMessage = $"เกิดข้อผิดพลาดจากฐานข้อมูล SQLite (Error {ex.SqliteErrorCode}): {ex.Message}";
                return false;
            }
            catch (System.Exception ex)
            {
                errorMessage = $"เกิดข้อผิดพลาดในการบันทึกข้อมูลเคลม: {ex.Message}";
                return false;
            }
        }

        public void SaveClaim(Models.Claim claim)
        {
            if (!SaveClaim(claim, out string errorMessage))
            {
                throw new System.InvalidOperationException(errorMessage);
            }
        }

        public void UpdateClaim(Models.Claim claim)
        {
            if (claim == null) return;

            if (claim.Sales_ID <= 0 && !string.IsNullOrWhiteSpace(claim.SalesOrderRefNo))
            {
                claim.Sales_ID = GetSalesIdByRefNo(claim.SalesOrderRefNo);
            }

            if (claim.Pro_ID <= 0)
            {
                claim.Pro_ID = GetProductIdByBarcodeOrName(claim.ProductCode, claim.ProductName);
            }

            try
            {
                using (var connection = GetConnection())
                {
                    string sql = @"
                        UPDATE tblClaim 
                        SET Sales_ID = @Sales_ID, Pro_ID = @Pro_ID, Claim_Date = @Claim_Date, 
                            Claim_Reason = @Claim_Reason, Claim_Status = @Claim_Status, Claim_Action = @ClaimAction
                        WHERE Claim_ID = @Claim_ID";
                    connection.Execute(sql, claim);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateClaim failed: {ex.Message}");
            }
        }

        public void UpdateClaimStatus(int claimId, string status)
        {
            using (var connection = GetConnection())
            {
                string sql = "UPDATE tblClaim SET Claim_Status = @Status WHERE Claim_ID = @Id";
                connection.Execute(sql, new { Status = status, Id = claimId });
            }
        }

        public void DeleteClaim(int claimId)
        {
            using (var connection = GetConnection())
            {
                string sql = "DELETE FROM tblClaim WHERE Claim_ID = @Id";
                connection.Execute(sql, new { Id = claimId });
            }
        }

        // --- Purchase Order Methods ---
        public System.Collections.Generic.IEnumerable<Models.PurchaseOrder> GetPurchaseOrders(string searchKeyword = "")
        {
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        po.PO_ID AS PO_ID, po.PO_ID AS Id,
                        po.PO_Date AS PO_Date, po.PO_Total AS PO_Total, po.PO_Status AS PO_Status,
                        p.Partner_Name AS SupplierName
                    FROM tblPO_H po
                    LEFT JOIN tblPartner p ON po.Partner_ID = p.Partner_ID
                    ORDER BY po.PO_ID DESC";
                return connection.Query<Models.PurchaseOrder>(sql);
            }
        }

        public System.Collections.Generic.IEnumerable<Models.PurchaseOrderItem> GetPurchaseOrderItems(int poId)
        {
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        d.Detail_ID AS Detail_ID, d.Detail_ID AS Id,
                        d.PO_ID AS PO_ID, d.Pro_ID AS Pro_ID,
                        d.PO_Cost AS PO_Cost, d.PO_Qty AS PO_Qty, d.PO_Subtotal AS PO_Subtotal,
                        p.Pro_Name AS ProductName, p.Pro_Barcode AS ProductCode
                    FROM tblPODetail d
                    JOIN tblProduct p ON d.Pro_ID = p.Pro_ID
                    WHERE d.PO_ID = @POId";
                return connection.Query<Models.PurchaseOrderItem>(sql, new { POId = poId });
            }
        }

        private int? ResolvePartnerId(IDbConnection connection, string supplierName, int partnerId = 0)
        {
            if (partnerId > 0)
            {
                var existing = connection.QueryFirstOrDefault<int?>("SELECT Partner_ID FROM tblPartner WHERE Partner_ID = @Id LIMIT 1", new { Id = partnerId });
                if (existing.HasValue && existing.Value > 0) return existing.Value;
            }

            if (!string.IsNullOrWhiteSpace(supplierName))
            {
                var byName = connection.QueryFirstOrDefault<int?>("SELECT Partner_ID FROM tblPartner WHERE Partner_Name = @Name LIMIT 1", new { Name = supplierName.Trim() });
                if (byName.HasValue && byName.Value > 0) return byName.Value;

                string sqlInsert = @"
                    INSERT INTO tblPartner (Partner_Name, Partner_Address, Partner_Tel, Partner_Contact) 
                    VALUES (@Name, '', '', '');
                    SELECT last_insert_rowid();";
                return connection.ExecuteScalar<int>(sqlInsert, new { Name = supplierName.Trim() });
            }

            return connection.QueryFirstOrDefault<int?>("SELECT Partner_ID FROM tblPartner LIMIT 1");
        }

        private int? ResolveEmpId(IDbConnection connection, int empId = 0)
        {
            if (empId > 0)
            {
                var existing = connection.QueryFirstOrDefault<int?>("SELECT Emp_ID FROM tblEmployee WHERE Emp_ID = @Id LIMIT 1", new { Id = empId });
                if (existing.HasValue && existing.Value > 0) return existing.Value;
            }

            return connection.QueryFirstOrDefault<int?>("SELECT Emp_ID FROM tblEmployee LIMIT 1");
        }

        private int? ResolveProductId(IDbConnection connection, int proId, string code, string name)
        {
            if (proId > 0)
            {
                var existing = connection.QueryFirstOrDefault<int?>("SELECT Pro_ID FROM tblProduct WHERE Pro_ID = @Id LIMIT 1", new { Id = proId });
                if (existing.HasValue && existing.Value > 0) return existing.Value;
            }

            if (!string.IsNullOrWhiteSpace(code))
            {
                var byCode = connection.QueryFirstOrDefault<int?>("SELECT Pro_ID FROM tblProduct WHERE Pro_Barcode = @Code LIMIT 1", new { Code = code.Trim() });
                if (byCode.HasValue && byCode.Value > 0) return byCode.Value;
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                var byName = connection.QueryFirstOrDefault<int?>("SELECT Pro_ID FROM tblProduct WHERE Pro_Name = @Name LIMIT 1", new { Name = name.Trim() });
                if (byName.HasValue && byName.Value > 0) return byName.Value;
            }

            return null;
        }

        public void SavePurchaseOrder(Models.PurchaseOrder po, System.Collections.Generic.List<Models.PurchaseOrderItem> items)
        {
            using (var connection = GetConnection())
            {
                int? partnerId = ResolvePartnerId(connection, po.SupplierName, po.Partner_ID);
                int? empId = ResolveEmpId(connection, po.Emp_ID);

                string sqlPO = @"
                    INSERT INTO tblPO_H (PO_Date, Partner_ID, Emp_ID, PO_Total, PO_Status) 
                    VALUES (@PO_Date, @Partner_ID, @Emp_ID, @PO_Total, @PO_Status);
                    SELECT last_insert_rowid();";
                int newPoId = connection.ExecuteScalar<int>(sqlPO, new
                {
                    PO_Date = po.PO_Date,
                    Partner_ID = partnerId,
                    Emp_ID = empId,
                    PO_Total = po.PO_Total,
                    PO_Status = po.PO_Status
                });

                string sqlItem = @"
                    INSERT INTO tblPODetail (PO_ID, Pro_ID, PO_Cost, PO_Qty, PO_Subtotal) 
                    VALUES (@PO_ID, @Pro_ID, @PO_Cost, @PO_Qty, @PO_Subtotal)";
                
                foreach (var item in items)
                {
                    int? proId = ResolveProductId(connection, item.Pro_ID, item.ProductCode, item.ProductName);
                    connection.Execute(sqlItem, new
                    {
                        PO_ID = newPoId,
                        Pro_ID = proId,
                        PO_Cost = item.CostPrice,
                        PO_Qty = item.Quantity,
                        PO_Subtotal = item.PO_Subtotal
                    });
                }
            }
        }

        public void UpdatePurchaseOrder(Models.PurchaseOrder po, System.Collections.Generic.List<Models.PurchaseOrderItem> items)
        {
            using (var connection = GetConnection())
            {
                int? partnerId = ResolvePartnerId(connection, po.SupplierName, po.Partner_ID);
                int? empId = ResolveEmpId(connection, po.Emp_ID);

                string sqlPO = @"
                    UPDATE tblPO_H SET
                        PO_Date = @PO_Date, Partner_ID = @Partner_ID, Emp_ID = @Emp_ID, 
                        PO_Total = @PO_Total, PO_Status = @PO_Status
                    WHERE PO_ID = @PO_ID";
                connection.Execute(sqlPO, new
                {
                    PO_Date = po.PO_Date,
                    Partner_ID = partnerId,
                    Emp_ID = empId,
                    PO_Total = po.PO_Total,
                    PO_Status = po.PO_Status,
                    PO_ID = po.PO_ID
                });

                connection.Execute("DELETE FROM tblPODetail WHERE PO_ID = @Id", new { Id = po.PO_ID });

                string sqlItem = @"
                    INSERT INTO tblPODetail (PO_ID, Pro_ID, PO_Cost, PO_Qty, PO_Subtotal) 
                    VALUES (@PO_ID, @Pro_ID, @PO_Cost, @PO_Qty, @PO_Subtotal)";
                
                foreach (var item in items)
                {
                    int? proId = ResolveProductId(connection, item.Pro_ID, item.ProductCode, item.ProductName);
                    connection.Execute(sqlItem, new
                    {
                        PO_ID = po.PO_ID,
                        Pro_ID = proId,
                        PO_Cost = item.CostPrice,
                        PO_Qty = item.Quantity,
                        PO_Subtotal = item.PO_Subtotal
                    });
                }
            }
        }

        public void UpdatePurchaseOrderStatus(int poId, string status)
        {
            using (var connection = GetConnection())
            {
                string sql = "UPDATE tblPO_H SET PO_Status = @Status WHERE PO_ID = @Id";
                connection.Execute(sql, new { Status = status, Id = poId });
            }
        }

        public System.Collections.Generic.IEnumerable<Models.PurchaseOrder> GetPendingPurchaseOrders(string searchKeyword = "")
        {
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        po.PO_ID AS PO_ID, po.PO_ID AS Id,
                        po.PO_Date AS PO_Date, po.PO_Total AS PO_Total, po.PO_Status AS PO_Status,
                        p.Partner_Name AS SupplierName
                    FROM tblPO_H po
                    LEFT JOIN tblPartner p ON po.Partner_ID = p.Partner_ID
                    WHERE (po.PO_Status = 'รอดำเนินการ' OR po.PO_Status = 'Pending' OR po.PO_Status = 'รอรับของ')";
                if (!string.IsNullOrWhiteSpace(searchKeyword))
                {
                    sql += " AND (po.PO_ID LIKE @Search OR p.Partner_Name LIKE @Search)";
                }
                sql += " ORDER BY po.PO_ID DESC";
                return connection.Query<Models.PurchaseOrder>(sql, new { Search = "%" + searchKeyword + "%" });
            }
        }

        public void ProcessGoodsReceipt(int poId, string deliveryNoteNo, System.Collections.Generic.List<Models.GoodsReceiptItem> items, int empId = 1)
        {
            using (var connection = GetConnection())
            {
                using (var trans = connection.BeginTransaction())
                {
                    try
                    {
                        // 1) Record Stock In Header (GR-XXXXXX)
                        string sqlHeader = @"
                            INSERT INTO tblStockIn_H (StockIn_Date, PO_ID, Emp_ID, Note) 
                            VALUES (@StockIn_Date, @PO_ID, @Emp_ID, @Note);
                            SELECT last_insert_rowid();";

                        int stockInId = connection.ExecuteScalar<int>(sqlHeader, new
                        {
                            StockIn_Date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            PO_ID = poId,
                            Emp_ID = empId > 0 ? empId : 1,
                            Note = string.IsNullOrWhiteSpace(deliveryNoteNo) ? "รับสินค้าเข้าคลัง" : deliveryNoteNo
                        }, transaction: trans);

                        // 2) Loop Items: Record Detail, Update Stock, Calculate Moving Average Cost
                        string sqlDetail = @"
                            INSERT INTO tblStockInDetail (StockIn_ID, Pro_ID, StockIn_Qty) 
                            VALUES (@StockIn_ID, @Pro_ID, @StockIn_Qty);";

                        string sqlProdQuery = "SELECT Pro_Qty, Pro_Cost FROM tblProduct WHERE Pro_ID = @Pro_ID;";

                        string sqlProdUpdate = @"
                            UPDATE tblProduct 
                            SET Pro_Qty = @NewQty, Pro_Cost = @NewCost 
                            WHERE Pro_ID = @Pro_ID;";

                        foreach (var item in items)
                        {
                            if (item.ReceivedQty <= 0) continue;

                            // Insert detail log
                            connection.Execute(sqlDetail, new
                            {
                                StockIn_ID = stockInId,
                                Pro_ID = item.Pro_ID,
                                StockIn_Qty = item.ReceivedQty
                            }, transaction: trans);

                            // Get current product stock & cost
                            var prod = connection.QueryFirstOrDefault<Models.Product>(sqlProdQuery, new { Pro_ID = item.Pro_ID }, transaction: trans);

                            int oldQty = prod != null ? System.Math.Max(0, prod.Pro_Qty) : 0;
                            decimal oldCost = prod != null ? prod.Pro_Cost : 0;
                            int receivedQty = item.ReceivedQty;
                            decimal newUnitCost = item.CostPrice;

                            int newQty = oldQty + receivedQty;
                            decimal newCost = newQty > 0
                                ? ((oldQty * oldCost) + (receivedQty * newUnitCost)) / newQty
                                : newUnitCost;

                            // Round cost to 2 decimal places
                            newCost = System.Math.Round(newCost, 2);

                            // Update stock & cost
                            connection.Execute(sqlProdUpdate, new
                            {
                                NewQty = newQty,
                                NewCost = newCost,
                                Pro_ID = item.Pro_ID
                            }, transaction: trans);
                        }

                        // 3) Update PO status to "ได้รับสินค้าแล้ว" (Completed)
                        if (poId > 0)
                        {
                            string sqlPOStatus = "UPDATE tblPO_H SET PO_Status = 'ได้รับสินค้าแล้ว' WHERE PO_ID = @PO_ID;";
                            connection.Execute(sqlPOStatus, new { PO_ID = poId }, transaction: trans);
                        }

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public void DeletePurchaseOrder(int poId)
        {
            using (var connection = GetConnection())
            {
                connection.Execute("DELETE FROM tblPODetail WHERE PO_ID = @Id", new { Id = poId });
                connection.Execute("DELETE FROM tblPO_H WHERE PO_ID = @Id", new { Id = poId });
            }
        }


        // --- Expense Methods ---
        public System.Collections.Generic.IEnumerable<Models.Expense> GetExpenses(string searchKeyword = "")
        {
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        Expense_ID AS Expense_ID, Expense_ID AS Id,
                        Expense_Date AS Expense_Date, Expense_Category AS Expense_Category,
                        Expense_Amount AS Expense_Amount, Expense_Note AS Expense_Note,
                        Emp_ID
                    FROM tblExpense";
                if (!string.IsNullOrWhiteSpace(searchKeyword))
                {
                    sql += " WHERE Expense_Category LIKE @Search OR Expense_Note LIKE @Search";
                }
                sql += " ORDER BY Expense_ID DESC";
                return connection.Query<Models.Expense>(sql, new { Search = $"%{searchKeyword}%" });
            }
        }

        public void SaveExpense(Models.Expense expense)
        {
            using (var connection = GetConnection())
            {
                if (expense.Emp_ID <= 0)
                {
                    // ดึง ID พนักงานคนแรกในระบบป้องกัน FK Error
                    expense.Emp_ID = connection.ExecuteScalar<int>("SELECT Emp_ID FROM tblEmployee LIMIT 1;");
                }
                else
                {
                    int empExists = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM tblEmployee WHERE Emp_ID = @Emp_ID;", new { expense.Emp_ID });
                    if (empExists == 0)
                    {
                        expense.Emp_ID = connection.ExecuteScalar<int>("SELECT Emp_ID FROM tblEmployee LIMIT 1;");
                    }
                }

                string sql = @"
                    INSERT INTO tblExpense (Expense_Date, Expense_Category, Expense_Amount, Expense_Note, Emp_ID) 
                    VALUES (@Expense_Date, @Expense_Category, @Expense_Amount, @Expense_Note, @Emp_ID)";
                connection.Execute(sql, expense);
            }
        }

        public void UpdateExpense(Models.Expense expense)
        {
            using (var connection = GetConnection())
            {
                if (expense.Emp_ID <= 0)
                {
                    // ดึง ID พนักงานคนแรกในระบบป้องกัน FK Error
                    expense.Emp_ID = connection.ExecuteScalar<int>("SELECT Emp_ID FROM tblEmployee LIMIT 1;");
                }
                else
                {
                    int empExists = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM tblEmployee WHERE Emp_ID = @Emp_ID;", new { expense.Emp_ID });
                    if (empExists == 0)
                    {
                        expense.Emp_ID = connection.ExecuteScalar<int>("SELECT Emp_ID FROM tblEmployee LIMIT 1;");
                    }
                }

                string sql = @"
                    UPDATE tblExpense 
                    SET Expense_Date = @Expense_Date, Expense_Category = @Expense_Category, 
                        Expense_Amount = @Expense_Amount, Expense_Note = @Expense_Note, Emp_ID = @Emp_ID
                    WHERE Expense_ID = @Expense_ID";
                connection.Execute(sql, expense);
            }
        }

        public void DeleteExpense(int id)
        {
            using (var connection = GetConnection())
            {
                string sql = "DELETE FROM tblExpense WHERE Expense_ID = @Id";
                connection.Execute(sql, new { Id = id });
            }
        }

        // --- Product Category CRUD Operations ---
        public System.Collections.Generic.IEnumerable<Models.ProductCategory> GetProductCategories()
        {
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        c.CategoryId AS CategoryId,
                        c.CategoryId AS Id,
                        c.CategoryCode AS CategoryCode,
                        c.CategoryCode AS Code,
                        c.CategoryName AS CategoryName,
                        c.CategoryName AS Name,
                        c.Description AS Description,
                        (SELECT COUNT(1) FROM tblProduct p WHERE p.Pro_Category = c.CategoryName) AS ProductCount
                    FROM tblProductCategory c
                    ORDER BY c.CategoryId ASC;";
                return connection.Query<Models.ProductCategory>(sql);
            }
        }

        public void SaveProductCategory(Models.ProductCategory category)
        {
            using (var connection = GetConnection())
            {
                string sql = @"
                    INSERT INTO tblProductCategory (CategoryCode, CategoryName, Description) 
                    VALUES (@CategoryCode, @CategoryName, @Description)";
                connection.Execute(sql, category);
            }
        }

        public void UpdateProductCategory(Models.ProductCategory category)
        {
            using (var connection = GetConnection())
            {
                string sql = @"
                    UPDATE tblProductCategory 
                    SET CategoryCode = @CategoryCode, CategoryName = @CategoryName, Description = @Description 
                    WHERE CategoryId = @CategoryId";
                connection.Execute(sql, category);
            }
        }

        public void DeleteProductCategory(int id)
        {
            using (var connection = GetConnection())
            {
                string sql = "DELETE FROM tblProductCategory WHERE CategoryId = @Id";
                connection.Execute(sql, new { Id = id });
            }
        }
    }
}
