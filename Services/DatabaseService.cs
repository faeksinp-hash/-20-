using System.Data;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;
using Porjai20.Common;

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

                // 3) tblPartner & tblSupplier
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS tblPartner (
                        Partner_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Partner_Name TEXT,
                        Partner_Address TEXT,
                        Partner_Tel TEXT,
                        Partner_Contact TEXT
                    );");

                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS tblSupplier (
                        SupplierID INTEGER PRIMARY KEY AUTOINCREMENT,
                        SupplierName TEXT NOT NULL,
                        ContactPerson TEXT,
                        PhoneNumber TEXT
                    );");

                // Sync/migrate from tblPartner to tblSupplier if tblSupplier is empty
                int supplierCount = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM tblSupplier;");
                if (supplierCount == 0)
                {
                    int partnerCount = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM tblPartner;");
                    if (partnerCount > 0)
                    {
                        connection.Execute(@"
                            INSERT INTO tblSupplier (SupplierID, SupplierName, ContactPerson, PhoneNumber)
                            SELECT Partner_ID, Partner_Name, Partner_Contact, Partner_Tel 
                            FROM tblPartner
                            WHERE Partner_Name IS NOT NULL AND TRIM(Partner_Name) != '';");
                    }
                    else
                    {
                        connection.Execute(@"
                            INSERT INTO tblSupplier (SupplierName, ContactPerson, PhoneNumber) VALUES
                            ('บริษัท พอใจ ซัพพลาย จำกัด', 'ฝ่ายจัดซื้อ', '02-111-2222'),
                            ('บริษัท ค้าส่งอุปกรณ์เบ็ดเตล็ด จำกัด', 'คุณสมชาย', '089-999-8888');");
                    }
                }

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
                        Pro_Image TEXT,
                        Pro_Unit TEXT,
                        CategoryId INTEGER
                    );");

                try { connection.Execute("ALTER TABLE tblProduct ADD COLUMN Pro_Unit TEXT;"); } catch { }
                try { connection.Execute("ALTER TABLE tblProduct ADD COLUMN CategoryId INTEGER;"); } catch { }

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
                    string adminPassword = "1234";
                    connection.Execute(@"
                        INSERT INTO tblEmployee (Emp_Name, Emp_Username, Emp_Password, Emp_Role) 
                        VALUES ('Administrator', 'admin', @Password, @Role);",
                        new { Password = adminPassword, Role = RolePermissions.RoleOwner });
                }

                // Data Patch: Revert any BCrypt hashes in tblEmployee back to plaintext default passwords
                MigrateEmployeePasswords(connection);

                // Migrate existing employee roles from legacy values ('Admin', 'User', null/empty) to standard roles
                MigrateEmployeeRoles(connection);

                // Migrate sales dates and payment methods to standard formats
                MigrateSalesRecords(connection);

                // Migrate and self-heal delivery recipient info if blank
                MigrateDeliveryRecords(connection);

                // Migrate and self-heal product categories if CategoryId is missing or 0
                MigrateProductCategories(connection);
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
        /// Data Patch: ปรับค่ารหัสผ่านเดิมที่เป็น BCrypt Hash ให้กลับมาเป็นรหัสผ่านเริ่มต้น (Plaintext "1234")
        /// </summary>
        private void MigrateEmployeePasswords(IDbConnection connection)
        {
            try
            {
                var employees = connection.Query<(int Emp_ID, string? Emp_Username, string? Emp_Password)>(
                    "SELECT Emp_ID, Emp_Username, Emp_Password FROM tblEmployee WHERE Emp_Password IS NOT NULL AND Emp_Password != '';"
                );

                foreach (var emp in employees)
                {
                    if (!string.IsNullOrEmpty(emp.Emp_Password) && IsBCryptHash(emp.Emp_Password))
                    {
                        string defaultPassword = "1234";
                        connection.Execute(
                            "UPDATE tblEmployee SET Emp_Password = @Password WHERE Emp_ID = @Emp_ID;",
                            new { Password = defaultPassword, Emp_ID = emp.Emp_ID }
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DataPatch] Error patching employee passwords: {ex.Message}");
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

                // Ensure existing tblSales_H records have a real 14-digit timestamp RefNo (SALE-YYYYMMDDHHMMSS)
                var existingSales = connection.Query<(int Sales_ID, string? RefNo, string? Sales_Date)>(
                    "SELECT Sales_ID, RefNo, Sales_Date FROM tblSales_H;"
                ).AsList();

                foreach (var s in existingSales)
                {
                    if (string.IsNullOrWhiteSpace(s.RefNo) || !ValidationHelper.IsSaleReference(s.RefNo))
                    {
                        string ts;
                        if (DateTime.TryParse(s.Sales_Date, out DateTime parsedDate))
                        {
                            ts = parsedDate.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            ts = DateTime.Now.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                        }
                        string newRefNo = $"SALE-{ts}";
                        connection.Execute("UPDATE tblSales_H SET RefNo = @NewRefNo WHERE Sales_ID = @Id;", new { NewRefNo = newRefNo, Id = s.Sales_ID });
                    }
                }

                // If tblSales_H is completely empty, seed a sample sale with real 14-digit RefNo
                if (existingSales.Count == 0)
                {
                    int proId = connection.ExecuteScalar<int>("SELECT Pro_ID FROM tblProduct LIMIT 1;");
                    int cusId = connection.ExecuteScalar<int>("SELECT Cus_ID FROM tblCustomer LIMIT 1;");
                    int empId = connection.ExecuteScalar<int>("SELECT Emp_ID FROM tblEmployee LIMIT 1;");
                    string nowTs = DateTime.Now.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                    string sampleRef = $"SALE-{nowTs}";
                    string sampleDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                    connection.Execute(@"
                        INSERT INTO tblSales_H (RefNo, Sales_Date, Cus_ID, Emp_ID, Sales_Total, Sales_Cash, Sales_Change, Sales_PaymentType, Sales_Status)
                        VALUES (@RefNo, @Sales_Date, @Cus_ID, @Emp_ID, 100.0, 100.0, 0.0, 'เงินสด', 'ชำระเงินแล้ว');",
                        new { RefNo = sampleRef, Sales_Date = sampleDate, Cus_ID = cusId > 0 ? (int?)cusId : null, Emp_ID = empId > 0 ? (int?)empId : null });

                    int salesId = connection.ExecuteScalar<int>("SELECT last_insert_rowid();");
                    if (proId > 0 && salesId > 0)
                    {
                        connection.Execute(@"
                            INSERT INTO tblSalesDetail (Sales_ID, Pro_ID, Pro_Price, Sales_Qty, Sales_Subtotal)
                            VALUES (@Sales_ID, @Pro_ID, 20.0, 5, 100.0);",
                            new { Sales_ID = salesId, Pro_ID = proId });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Migration] Error migrating sales records: {ex.Message}");
            }
        }

        /// <summary>
        /// Migration สำหรับซ่อมแซมข้อมูลการจัดส่งสินค้า (tblDelivery) ที่ชื่อ เบอร์โทร หรือที่อยู่ว่างเปล่า โดยดึงข้อมูลจาก tblCustomer ผ่าน tblSales_H
        /// </summary>
        private void MigrateDeliveryRecords(IDbConnection connection)
        {
            try
            {
                connection.Execute(@"
                    UPDATE tblDelivery
                    SET 
                        Recipient_Name = COALESCE(NULLIF(TRIM(Recipient_Name), ''), (SELECT c.Cus_Name FROM tblSales_H s JOIN tblCustomer c ON s.Cus_ID = c.Cus_ID WHERE s.Sales_ID = tblDelivery.Sales_ID), 'ลูกค้าทั่วไป'),
                        Recipient_Tel = COALESCE(NULLIF(TRIM(Recipient_Tel), ''), (SELECT c.Cus_Tel FROM tblSales_H s JOIN tblCustomer c ON s.Cus_ID = c.Cus_ID WHERE s.Sales_ID = tblDelivery.Sales_ID), '-'),
                        Recipient_Address = COALESCE(NULLIF(TRIM(Recipient_Address), ''), (SELECT c.Cus_Address FROM tblSales_H s JOIN tblCustomer c ON s.Cus_ID = c.Cus_ID WHERE s.Sales_ID = tblDelivery.Sales_ID), 'ไม่ระบุที่อยู่'),
                        Delivery_Status = COALESCE(NULLIF(TRIM(Delivery_Status), ''), 'รอจัดส่ง')
                    WHERE Recipient_Name IS NULL OR TRIM(Recipient_Name) = ''
                       OR Recipient_Tel IS NULL OR TRIM(Recipient_Tel) = ''
                       OR Recipient_Address IS NULL OR TRIM(Recipient_Address) = '';
                ");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Migration] Error migrating delivery records: {ex.Message}");
            }
        }

        /// <summary>
        /// Migration สำหรับจับคู่ CategoryId ใน tblProduct จาก tblProductCategory ตามชื่อหมวดหมู่ หาก CategoryId เป็น NULL หรือ 0
        /// </summary>
        private void MigrateProductCategories(IDbConnection connection)
        {
            try
            {
                connection.Execute(@"
                    UPDATE tblProduct
                    SET CategoryId = (
                        SELECT c.CategoryId 
                        FROM tblProductCategory c 
                        WHERE TRIM(c.CategoryName) = TRIM(tblProduct.Pro_Category)
                        LIMIT 1
                    )
                    WHERE (CategoryId IS NULL OR CategoryId = 0)
                      AND Pro_Category IS NOT NULL 
                      AND TRIM(Pro_Category) != ''
                      AND EXISTS (
                          SELECT 1 FROM tblProductCategory c 
                          WHERE TRIM(c.CategoryName) = TRIM(tblProduct.Pro_Category)
                      );
                ");

                // ถ้า CategoryId ยังคงเป็น NULL หรือ 0 ให้กำหนดเป็นหมวดหมู่แรกสุดใน tblProductCategory
                int defaultCatId = connection.ExecuteScalar<int>(
                    "SELECT CategoryId FROM tblProductCategory ORDER BY CategoryId ASC LIMIT 1;"
                );
                if (defaultCatId > 0)
                {
                    connection.Execute(@"
                        UPDATE tblProduct
                        SET CategoryId = @DefaultCatId
                        WHERE CategoryId IS NULL OR CategoryId = 0;
                    ", new { DefaultCatId = defaultCatId });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Migration] Error migrating product categories: {ex.Message}");
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

        public void SaveDelivery(int salesId, string recipientName, string recipientTel, string recipientAddress, string? trackingNo = null, string deliveryStatus = "รอจัดส่ง", IDbConnection? conn = null, IDbTransaction? trans = null)
        {
            string sql = @"
                INSERT INTO tblDelivery (Sales_ID, Recipient_Name, Recipient_Tel, Recipient_Address, Tracking_No, Delivery_Status)
                VALUES (@Sales_ID, @Recipient_Name, @Recipient_Tel, @Recipient_Address, @Tracking_No, @Delivery_Status);";

            var param = new
            {
                Sales_ID = salesId,
                Recipient_Name = !string.IsNullOrWhiteSpace(recipientName) ? recipientName.Trim() : "ลูกค้าทั่วไป",
                Recipient_Tel = !string.IsNullOrWhiteSpace(recipientTel) ? recipientTel.Trim() : "-",
                Recipient_Address = !string.IsNullOrWhiteSpace(recipientAddress) ? recipientAddress.Trim() : "ไม่ระบุที่อยู่",
                Tracking_No = trackingNo,
                Delivery_Status = !string.IsNullOrWhiteSpace(deliveryStatus) ? deliveryStatus.Trim() : "รอจัดส่ง"
            };

            if (conn != null)
            {
                conn.Execute(sql, param, trans);
            }
            else
            {
                using (var connection = GetConnection())
                {
                    connection.Execute(sql, param);
                }
            }
        }

        // --- Employee / Staff Management Methods ---
        public System.Collections.Generic.IEnumerable<Models.User> GetEmployees()
        {
            using (var connection = GetConnection())
            {
                string sql = "SELECT Emp_ID AS Emp_ID, Emp_ID AS Id, Emp_Name AS Emp_Name, Emp_Name AS Name, Emp_Username AS Username, Emp_Password AS Password, Emp_Role AS Role, Emp_Tel AS Tel, Emp_Tel AS Phone FROM tblEmployee ORDER BY Emp_ID ASC";
                return connection.Query<Models.User>(sql);
            }
        }

        public async Task<System.Collections.Generic.IEnumerable<Models.User>> GetEmployeesAsync()
        {
            using (var connection = GetConnection())
            {
                string sql = "SELECT Emp_ID AS Emp_ID, Emp_ID AS Id, Emp_Name AS Emp_Name, Emp_Name AS Name, Emp_Username AS Username, Emp_Password AS Password, Emp_Role AS Role, Emp_Tel AS Tel, Emp_Tel AS Phone FROM tblEmployee ORDER BY Emp_ID ASC";
                return await connection.QueryAsync<Models.User>(sql);
            }
        }

        public void SaveEmployee(Models.User user)
        {
            using (var connection = GetConnection())
            {
                string sql = "INSERT INTO tblEmployee (Emp_Name, Emp_Tel, Emp_Username, Emp_Password, Emp_Role) VALUES (@Name, @Phone, @Username, @Password, @Role)";
                connection.Execute(sql, user);
            }
        }

        public void UpdateEmployee(Models.User user)
        {
            using (var connection = GetConnection())
            {
                string sql = "UPDATE tblEmployee SET Emp_Name = @Name, Emp_Tel = @Phone, Emp_Username = @Username, Emp_Password = @Password, Emp_Role = @Role WHERE Emp_ID = @Id";
                connection.Execute(sql, user);
            }
        }

        public void DeleteEmployee(int empId)
        {
            using (var connection = GetConnection())
            {
                string sql = "DELETE FROM tblEmployee WHERE Emp_ID = @Id";
                connection.Execute(sql, new { Id = empId });
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

        public async Task<System.Collections.Generic.IEnumerable<Models.Customer>> GetCustomersForPosAsync()
        {
            using (var conn = GetConnection())
            {
                string sql = @"SELECT Cus_ID as Id, Cus_Code as Code, Cus_Name as Name, 
                                      Cus_Address as Address, Cus_Tel as Phone, Cus_Points as Points 
                               FROM tblCustomer";
                return await conn.QueryAsync<Models.Customer>(sql);
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

                try
                {
                    string delivSql = @"UPDATE tblDelivery 
                                       SET Recipient_Name = @Name, Recipient_Tel = @Phone, Recipient_Address = @Address 
                                       WHERE Sales_ID IN (SELECT Sales_ID FROM tblSales_H WHERE Cus_ID = @CusId)";
                    connection.Execute(delivSql, new { Name = customer.Name, Phone = customer.Phone, Address = customer.Address, CusId = customer.Id });
                }
                catch { }
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

                try
                {
                    connection.Execute(@"
                        INSERT INTO tblSupplier (SupplierName, ContactPerson, PhoneNumber)
                        SELECT @Partner_Name, @Partner_Contact, @Partner_Tel
                        WHERE NOT EXISTS (SELECT 1 FROM tblSupplier WHERE SupplierName = @Partner_Name);", partner);
                }
                catch { }
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

                try
                {
                    connection.Execute(@"
                        UPDATE tblSupplier 
                        SET SupplierName = @Partner_Name, ContactPerson = @Partner_Contact, PhoneNumber = @Partner_Tel
                        WHERE SupplierID = @Partner_ID OR SupplierName = @Partner_Name;", partner);
                }
                catch { }
            }
        }

        public void DeletePartner(int partnerId)
        {
            using (var connection = GetConnection())
            {
                string sql = "DELETE FROM tblPartner WHERE Partner_ID = @Id";
                connection.Execute(sql, new { Id = partnerId });

                try
                {
                    connection.Execute("DELETE FROM tblSupplier WHERE SupplierID = @Id;", new { Id = partnerId });
                }
                catch { }
            }
        }

        // --- Supplier Management Methods ---
        public System.Collections.Generic.IEnumerable<Models.Partner> GetSuppliers()
        {
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        SupplierID AS Partner_ID, SupplierID AS Id, SupplierID,
                        SupplierName AS Partner_Name, SupplierName AS Name, SupplierName,
                        ContactPerson AS Partner_Contact, ContactPerson,
                        PhoneNumber AS Partner_Tel, PhoneNumber AS Phone, PhoneNumber
                    FROM tblSupplier ORDER BY SupplierName ASC";
                try
                {
                    return connection.Query<Models.Partner>(sql);
                }
                catch
                {
                    connection.Execute(@"
                        CREATE TABLE IF NOT EXISTS tblSupplier (
                            SupplierID INTEGER PRIMARY KEY AUTOINCREMENT,
                            SupplierName TEXT NOT NULL,
                            ContactPerson TEXT,
                            PhoneNumber TEXT
                        );");
                    return connection.Query<Models.Partner>(sql);
                }
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
                        COALESCE(s.RefNo, printf('SALE-%06d', c.Sales_ID)) AS SalesOrderRefNo,
                        COALESCE(s.RefNo, printf('SALE-%06d', c.Sales_ID)) AS SalesID,
                        COALESCE(s.RefNo, printf('SALE-%06d', c.Sales_ID)) AS InvoiceNo,
                        COALESCE(cust.Cus_Name, 'ลูกค้าทั่วไป') AS CustomerName,
                        COALESCE(cust.Cus_Tel, '-') AS CustomerPhone,
                        COALESCE(cust.Cus_Tel, '-') AS PhoneNumber,
                        s.Sales_Date AS SaleDate,
                        c.Claim_Date AS Claim_Date, c.Claim_Reason AS Claim_Reason,
                        c.Claim_Status AS Claim_Status,
                        COALESCE(c.Claim_Action, 'เปลี่ยนสินค้าใหม่') AS Claim_Action,
                        COALESCE(c.Claim_Action, 'เปลี่ยนสินค้าใหม่') AS ClaimAction,
                        p.Pro_Name AS ProductName,
                        COALESCE(NULLIF(p.Pro_Barcode, ''), printf('P-%04d', p.Pro_ID)) AS ProductCode
                    FROM tblClaim c
                    LEFT JOIN tblSales_H s ON c.Sales_ID = s.Sales_ID
                    LEFT JOIN tblCustomer cust ON s.Cus_ID = cust.Cus_ID
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
                        COALESCE(NULLIF(p.Pro_Barcode, ''), printf('P-%04d', p.Pro_ID)) AS ProductCode,
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

        public System.Collections.Generic.List<Models.ClaimReceiptItemSelection> GetReceiptProductSelections(string refNo)
        {
            var list = new System.Collections.Generic.List<Models.ClaimReceiptItemSelection>();
            if (string.IsNullOrWhiteSpace(refNo)) return list;
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        soi.Pro_ID AS Pro_ID,
                        COALESCE(NULLIF(p.Pro_Barcode, ''), printf('P-%04d', p.Pro_ID)) AS ProductCode,
                        p.Pro_Name AS ProductName,
                        soi.Pro_Price AS UnitPrice,
                        soi.Sales_Qty AS ReceiptQty,
                        1 AS ClaimQty
                    FROM tblSales_H so
                    JOIN tblSalesDetail soi ON so.Sales_ID = soi.Sales_ID
                    JOIN tblProduct p ON soi.Pro_ID = p.Pro_ID
                    WHERE so.RefNo = @RefNo OR CAST(so.Sales_ID AS TEXT) = @RefNo";
                var items = connection.Query<Models.ClaimReceiptItemSelection>(sql, new { RefNo = refNo.Trim() });
                if (items != null)
                {
                    list.AddRange(items);
                }
            }
            return list;
        }

        public System.Collections.Generic.List<Models.ClaimReceiptItemSelection> GetStockInProductSelections(string refNo)
        {
            var list = new System.Collections.Generic.List<Models.ClaimReceiptItemSelection>();
            if (string.IsNullOrWhiteSpace(refNo)) return list;
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        d.Pro_ID AS Pro_ID,
                        COALESCE(NULLIF(p.Pro_Barcode, ''), printf('P-%04d', p.Pro_ID)) AS ProductCode,
                        p.Pro_Name AS ProductName,
                        0 AS UnitPrice,
                        (CASE WHEN d.StockIn_Qty < 0 THEN -d.StockIn_Qty ELSE d.StockIn_Qty END) AS ReceiptQty,
                        1 AS ClaimQty
                    FROM tblStockInDetail d
                    JOIN tblStockIn_H h ON d.StockIn_ID = h.StockIn_ID
                    JOIN tblProduct p ON d.Pro_ID = p.Pro_ID
                    WHERE h.StockIn_ID = @RefNo OR CAST(h.StockIn_ID AS TEXT) = @RefNo OR h.Note LIKE '%' || @RefNo || '%'";
                var items = connection.Query<Models.ClaimReceiptItemSelection>(sql, new { RefNo = refNo.Trim() });
                if (items != null)
                {
                    list.AddRange(items);
                }
            }
            return list;
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
                        p.Pro_Name AS ProductName, p.Pro_Barcode AS ProductCode,
                        COALESCE(pod.PO_Cost, p.Pro_Cost, 0) AS UnitCost
                    FROM tblStockInDetail d
                    JOIN tblStockIn_H h ON d.StockIn_ID = h.StockIn_ID
                    JOIN tblProduct p ON d.Pro_ID = p.Pro_ID
                    LEFT JOIN tblPODetail pod ON h.PO_ID = pod.PO_ID AND d.Pro_ID = pod.Pro_ID
                    ORDER BY d.Detail_ID DESC";
                return connection.Query<Models.StockTransaction>(sql);
            }
        }

        public (int TotalEntries, int TotalQty, decimal TotalCost) GetStockInKPIs()
        {
            using (var connection = GetConnection())
            {
                int stockInCount = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM tblStockIn_H;");
                int totalQty = connection.ExecuteScalar<int>("SELECT COALESCE(SUM(StockIn_Qty), 0) FROM tblStockInDetail;");

                string sqlCost = @"
                    SELECT COALESCE(SUM(d.StockIn_Qty * COALESCE(pod.PO_Cost, p.Pro_Cost, 0)), 0)
                    FROM tblStockInDetail d
                    JOIN tblStockIn_H h ON d.StockIn_ID = h.StockIn_ID
                    LEFT JOIN tblPODetail pod ON h.PO_ID = pod.PO_ID AND d.Pro_ID = pod.Pro_ID
                    LEFT JOIN tblProduct p ON d.Pro_ID = p.Pro_ID;";
                decimal totalCost = connection.ExecuteScalar<decimal>(sqlCost);

                if (stockInCount == 0)
                {
                    string sqlCompleted = @"
                        SELECT 
                            COUNT(1) AS TotalEntries,
                            COALESCE(SUM(PO_Total), 0) AS TotalCost
                        FROM tblPO_H 
                        WHERE PO_Status = 'ได้รับสินค้าแล้ว' OR PO_Status = 'ตรวจรับแล้ว' OR PO_Status = 'รับเข้าแล้ว';";
                    var completed = connection.QueryFirstOrDefault(sqlCompleted);

                    if (completed != null && completed.TotalEntries != null && (long)completed.TotalEntries > 0)
                    {
                        stockInCount = (int)(long)completed.TotalEntries;
                        totalCost = (decimal)(completed.TotalCost ?? 0m);

                        string sqlQty = @"
                            SELECT COALESCE(SUM(d.PO_Qty), 0)
                            FROM tblPODetail d
                            JOIN tblPO_H h ON d.PO_ID = h.PO_ID
                            WHERE h.PO_Status = 'ได้รับสินค้าแล้ว' OR h.PO_Status = 'ตรวจรับแล้ว' OR h.PO_Status = 'รับเข้าแล้ว';";
                        totalQty = connection.ExecuteScalar<int>(sqlQty);
                    }
                }

                return (stockInCount, totalQty, totalCost);
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
                        COALESCE(NULLIF(p.Pro_Barcode, ''), printf('P-%04d', p.Pro_ID)) AS ProductCode,
                        d.StockIn_Qty AS Quantity,
                        p.Pro_Name AS ProductName,
                        COALESCE(sup.SupplierName, pt.Partner_Name, '') AS SupplierName,
                        COALESCE(sup.PhoneNumber, pt.Partner_Tel, '') AS SupplierPhone
                    FROM tblStockInDetail d
                    JOIN tblStockIn_H h ON d.StockIn_ID = h.StockIn_ID
                    JOIN tblProduct p ON d.Pro_ID = p.Pro_ID
                    LEFT JOIN tblPO_H po ON h.PO_ID = po.PO_ID
                    LEFT JOIN tblSupplier sup ON po.Partner_ID = sup.SupplierID
                    LEFT JOIN tblPartner pt ON po.Partner_ID = pt.Partner_ID
                    WHERE h.StockIn_ID = @RefNo OR CAST(h.StockIn_ID AS TEXT) = @RefNo OR h.Note LIKE '%' || @RefNo || '%'
                    LIMIT 1";
                return connection.QueryFirstOrDefault<Models.ClaimedStockInDetails>(sql, new { RefNo = refNo.Trim() });
            }
        }

        public System.Collections.Generic.List<string> GetCachedDocumentNumbers()
        {
            // Strict case-sensitive collection
            var docNumbers = new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal);
            try
            {
                using (var connection = GetConnection())
                {
                    // 1) tblSales_H: Real receipt numbers matching ^SALE-\d{14}$ (no mock SALE-000001)
                    try
                    {
                        var salesRefs = connection.Query<string>(@"
                            SELECT DISTINCT RefNo FROM tblSales_H 
                            WHERE RefNo IS NOT NULL AND TRIM(RefNo) != ''
                            ORDER BY Sales_Date DESC;");
                        foreach (var r in salesRefs)
                        {
                            if (!string.IsNullOrWhiteSpace(r))
                            {
                                var trimmed = r.Trim();
                                if (ValidationHelper.IsSaleReference(trimmed))
                                {
                                    docNumbers.Add(trimmed);
                                }
                            }
                        }
                    }
                    catch { }

                    // 2) tblStockIn_H (Note e.g. RC-XXXXX and formatted RC-XXXXX from ID)
                    try
                    {
                        var stockRefs = connection.Query<string>(@"
                            SELECT DISTINCT Note FROM tblStockIn_H WHERE Note IS NOT NULL AND TRIM(Note) != ''
                            UNION
                            SELECT DISTINCT printf('RC-%05d', StockIn_ID) FROM tblStockIn_H;");
                        foreach (var r in stockRefs)
                        {
                            if (!string.IsNullOrWhiteSpace(r))
                            {
                                var trimmed = r.Trim();
                                if (trimmed.StartsWith("RC-", System.StringComparison.Ordinal))
                                {
                                    docNumbers.Add(trimmed);
                                }
                            }
                        }
                    }
                    catch { }

                    // 3) tblPO_H (Purchase Orders)
                    try
                    {
                        var poRefs = connection.Query<string>(@"
                            SELECT DISTINCT printf('PO-%05d', PO_ID) FROM tblPO_H WHERE PO_ID > 0;");
                        foreach (var r in poRefs)
                        {
                            if (!string.IsNullOrWhiteSpace(r))
                            {
                                var trimmed = r.Trim();
                                if (trimmed.StartsWith("PO-", System.StringComparison.Ordinal))
                                {
                                    docNumbers.Add(trimmed);
                                }
                            }
                        }
                    }
                    catch { }

                    // 4) tblGoodsReceived_H if table exists
                    try
                    {
                        int hasGr = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM sqlite_master WHERE type='table' AND name='tblGoodsReceived_H';");
                        if (hasGr > 0)
                        {
                            var grRefs = connection.Query<string>("SELECT DISTINCT RefNo FROM tblGoodsReceived_H WHERE RefNo IS NOT NULL AND TRIM(RefNo) != '';");
                            foreach (var r in grRefs)
                            {
                                if (!string.IsNullOrWhiteSpace(r))
                                {
                                    var trimmed = r.Trim();
                                    if (trimmed.StartsWith("RC-", System.StringComparison.Ordinal) || trimmed.StartsWith("GR-", System.StringComparison.Ordinal))
                                    {
                                        docNumbers.Add(trimmed);
                                    }
                                }
                            }
                        }
                    }
                    catch { }

                    // 5) tblPurchaseOrder_H if table exists
                    try
                    {
                        int hasPo = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM sqlite_master WHERE type='table' AND name='tblPurchaseOrder_H';");
                        if (hasPo > 0)
                        {
                            var poRefs = connection.Query<string>("SELECT DISTINCT PONumber FROM tblPurchaseOrder_H WHERE PONumber IS NOT NULL AND TRIM(PONumber) != '';");
                            foreach (var r in poRefs)
                            {
                                if (!string.IsNullOrWhiteSpace(r)) docNumbers.Add(r.Trim());
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
            return docNumbers.OrderBy(x => x).ToList();
        }

        public string? GetReceiptSuggestion(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return null;
            var trimmed = query.Trim();
            // Strict case-sensitive check: must start with uppercase "S"
            if (!trimmed.StartsWith("S", System.StringComparison.Ordinal)) return null;

            try
            {
                using (var connection = GetConnection())
                {
                    // Query real tblSales_H table ordered by Sales_Date DESC
                    string sql = @"
                        SELECT RefNo FROM tblSales_H 
                        WHERE RefNo IS NOT NULL 
                          AND (RefNo GLOB @query || '*' OR RefNo LIKE @query || '%')
                        ORDER BY Sales_Date DESC 
                        LIMIT 15;";
                    var results = connection.Query<string>(sql, new { query = trimmed });
                    foreach (var r in results)
                    {
                        if (!string.IsNullOrWhiteSpace(r))
                        {
                            var cand = r.Trim();
                            if (cand.StartsWith(trimmed, System.StringComparison.Ordinal) && 
                                ValidationHelper.IsSaleReference(cand))
                            {
                                return cand;
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        public string? GetStockInSuggestion(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return null;
            var trimmed = query.Trim();
            // Strict case-sensitive check: must start with uppercase "R"
            if (!trimmed.StartsWith("R", System.StringComparison.Ordinal)) return null;

            try
            {
                using (var connection = GetConnection())
                {
                    string sql = @"
                        SELECT Note FROM tblStockIn_H 
                        WHERE Note IS NOT NULL 
                          AND (Note GLOB @query || '*' OR Note LIKE @query || '%')
                        ORDER BY StockIn_Date DESC 
                        LIMIT 10;";
                    var results = connection.Query<string>(sql, new { query = trimmed });
                    foreach (var r in results)
                    {
                        if (!string.IsNullOrWhiteSpace(r))
                        {
                            var cand = r.Trim();
                            if (cand.StartsWith(trimmed, System.StringComparison.Ordinal))
                            {
                                return cand;
                            }
                        }
                    }

                    string formattedSql = @"
                        SELECT printf('RC-%05d', StockIn_ID) FROM tblStockIn_H 
                        ORDER BY StockIn_Date DESC 
                        LIMIT 10;";
                    var formattedList = connection.Query<string>(formattedSql);
                    foreach (var f in formattedList)
                    {
                        if (!string.IsNullOrWhiteSpace(f))
                        {
                            var cand = f.Trim();
                            if (cand.StartsWith(trimmed, System.StringComparison.Ordinal))
                            {
                                return cand;
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
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

            // Guard Clause: ตรวจสอบว่าต้องมีเลขที่ใบเสร็จหรือเลขที่ใบรับสินค้าก่อนบันทึกข้อมูล
            if (string.IsNullOrWhiteSpace(claim.SalesOrderRefNo) && string.IsNullOrWhiteSpace(claim.StockInRefNo))
            {
                errorMessage = "กรุณากรอกเลขที่ใบเสร็จหรือเลขที่ใบรับสินค้าก่อนบันทึกข้อมูล";
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

        public void DeductProductStockForClaim(int productId, int qty = 1)
        {
            if (productId <= 0 || qty <= 0) return;
            try
            {
                using (var connection = GetConnection())
                {
                    string sql = "UPDATE tblProduct SET Pro_Qty = MAX(0, Pro_Qty - @Qty) WHERE Pro_ID = @Id";
                    connection.Execute(sql, new { Qty = qty, Id = productId });
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeductProductStockForClaim failed: {ex.Message}");
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
                        po.Partner_ID AS Partner_ID,
                        COALESCE(NULLIF(s.SupplierName, ''), NULLIF(p.Partner_Name, ''), '') AS SupplierName
                    FROM tblPO_H po
                    LEFT JOIN tblPartner p ON po.Partner_ID = p.Partner_ID
                    LEFT JOIN tblSupplier s ON po.Partner_ID = s.SupplierID
                    ORDER BY po.PO_ID DESC";
                return connection.Query<Models.PurchaseOrder>(sql);
            }
        }

        public Models.PurchaseOrder? GetPurchaseOrderById(int poId)
        {
            using (var connection = GetConnection())
            {
                try
                {
                    string sql = @"
                        SELECT 
                            po.PO_ID AS PO_ID, po.PO_ID AS Id,
                            po.PO_Date AS PO_Date, po.PO_Total AS PO_Total, po.PO_Status AS PO_Status,
                            po.Partner_ID AS Partner_ID,
                            COALESCE(NULLIF(s.SupplierName, ''), NULLIF(p.Partner_Name, ''), '') AS SupplierName
                        FROM tblPO_H po
                        LEFT JOIN tblPartner p ON po.Partner_ID = p.Partner_ID
                        LEFT JOIN tblSupplier s ON po.Partner_ID = s.SupplierID
                        WHERE po.PO_ID = @POId";
                    var result = connection.QueryFirstOrDefault<Models.PurchaseOrder>(sql, new { POId = poId });
                    if (result != null) return result;
                }
                catch { }

                try
                {
                    int hasCustomTable = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM sqlite_master WHERE type='table' AND name='tblPurchaseOrder_H';");
                    if (hasCustomTable > 0)
                    {
                        string sql = @"
                            SELECT 
                                po.PO_ID AS PO_ID, po.PO_ID AS Id,
                                po.PO_Date AS PO_Date, po.PO_Total AS PO_Total, po.PO_Status AS PO_Status,
                                po.SupplierID AS Partner_ID,
                                COALESCE(NULLIF(s.SupplierName, ''), '') AS SupplierName
                            FROM tblPurchaseOrder_H po
                            LEFT JOIN tblSupplier s ON po.SupplierID = s.SupplierID
                            WHERE po.PO_ID = @POId";
                        return connection.QueryFirstOrDefault<Models.PurchaseOrder>(sql, new { POId = poId });
                    }
                }
                catch { }

                return null;
            }
        }

        public System.Collections.Generic.IEnumerable<Models.PurchaseOrderItem> GetPurchaseOrderItems(int poId)
        {
            using (var connection = GetConnection())
            {
                try
                {
                    string sql = @"
                        SELECT 
                            d.Detail_ID AS Detail_ID, d.Detail_ID AS Id,
                            d.PO_ID AS PO_ID, d.Pro_ID AS Pro_ID,
                            d.PO_Cost AS PO_Cost, d.PO_Qty AS PO_Qty, d.PO_Subtotal AS PO_Subtotal,
                            COALESCE(p.Pro_Name, CAST(d.Pro_ID AS TEXT)) AS ProductName,
                            COALESCE(NULLIF(p.Pro_Barcode, ''), printf('P-%04d', d.Pro_ID)) AS ProductCode
                        FROM tblPODetail d
                        LEFT JOIN tblProduct p ON d.Pro_ID = p.Pro_ID
                        WHERE d.PO_ID = @POId";
                    var list = connection.Query<Models.PurchaseOrderItem>(sql, new { POId = poId }).ToList();
                    if (list.Count > 0) return list;
                }
                catch { }

                try
                {
                    int hasCustomTable = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM sqlite_master WHERE type='table' AND name='tblPurchaseOrder_D';");
                    if (hasCustomTable > 0)
                    {
                        string sql = @"
                            SELECT 
                                d.Detail_ID AS Detail_ID, d.Detail_ID AS Id,
                                d.PO_ID AS PO_ID, d.ProductID AS Pro_ID,
                                d.CostPrice AS PO_Cost, d.QTY AS PO_Qty, d.TotalAmount AS PO_Subtotal,
                                COALESCE(p.Pro_Name, d.ProductName) AS ProductName,
                                COALESCE(NULLIF(p.Pro_Barcode, ''), printf('P-%04d', d.ProductID)) AS ProductCode
                            FROM tblPurchaseOrder_D d
                            LEFT JOIN tblProduct p ON d.ProductID = p.Pro_ID
                            WHERE d.PO_ID = @POId";
                        return connection.Query<Models.PurchaseOrderItem>(sql, new { POId = poId }).ToList();
                    }
                }
                catch { }

                return new System.Collections.Generic.List<Models.PurchaseOrderItem>();
            }
        }

        public System.Collections.Generic.IEnumerable<Models.SalesOrderItem> GetSalesOrderItems(int salesId)
        {
            using (var connection = GetConnection())
            {
                string sql = @"
                    SELECT 
                        d.Detail_ID AS Detail_ID, 
                        d.Detail_ID AS Id, 
                        d.Sales_ID, 
                        d.Pro_ID, 
                        COALESCE(p.Pro_Name, CAST(d.Pro_ID AS TEXT)) AS ProductName, 
                        d.Pro_Price AS UnitPrice, 
                        d.Sales_Qty AS Quantity, 
                        d.Sales_Subtotal AS Total 
                    FROM tblSalesDetail d
                    LEFT JOIN tblProduct p ON d.Pro_ID = p.Pro_ID
                    WHERE d.Sales_ID = @OrderId";
                return connection.Query<Models.SalesOrderItem>(sql, new { OrderId = salesId });
            }
        }

        private int? ResolvePartnerId(IDbConnection connection, string supplierName, int partnerId = 0)
        {
            if (partnerId > 0)
            {
                var existingSupp = connection.QueryFirstOrDefault<int?>("SELECT SupplierID FROM tblSupplier WHERE SupplierID = @Id LIMIT 1", new { Id = partnerId });
                if (existingSupp.HasValue && existingSupp.Value > 0) return existingSupp.Value;

                var existing = connection.QueryFirstOrDefault<int?>("SELECT Partner_ID FROM tblPartner WHERE Partner_ID = @Id LIMIT 1", new { Id = partnerId });
                if (existing.HasValue && existing.Value > 0) return existing.Value;
            }

            if (!string.IsNullOrWhiteSpace(supplierName))
            {
                var bySuppName = connection.QueryFirstOrDefault<int?>("SELECT SupplierID FROM tblSupplier WHERE SupplierName = @Name LIMIT 1", new { Name = supplierName.Trim() });
                if (bySuppName.HasValue && bySuppName.Value > 0) return bySuppName.Value;

                var byName = connection.QueryFirstOrDefault<int?>("SELECT Partner_ID FROM tblPartner WHERE Partner_Name = @Name LIMIT 1", new { Name = supplierName.Trim() });
                if (byName.HasValue && byName.Value > 0) return byName.Value;

                string sqlInsert = @"
                    INSERT INTO tblSupplier (SupplierName, ContactPerson, PhoneNumber) 
                    VALUES (@Name, '', '');
                    SELECT last_insert_rowid();";
                return connection.ExecuteScalar<int>(sqlInsert, new { Name = supplierName.Trim() });
            }

            var defaultSupp = connection.QueryFirstOrDefault<int?>("SELECT SupplierID FROM tblSupplier LIMIT 1");
            if (defaultSupp.HasValue && defaultSupp.Value > 0) return defaultSupp.Value;

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
                    WHERE 1=1";
                if (!string.IsNullOrWhiteSpace(searchKeyword))
                {
                    sql += @" AND (
                        CAST(po.PO_ID AS TEXT) LIKE @Search 
                        OR ('PO-' || printf('%05d', po.PO_ID)) LIKE @Search
                        OR ('PO' || printf('%05d', po.PO_ID)) LIKE @Search
                        OR p.Partner_Name LIKE @Search
                        OR po.PO_Status LIKE @Search
                    )";
                }
                sql += " ORDER BY po.PO_Date DESC, po.PO_ID DESC";
                return connection.Query<Models.PurchaseOrder>(sql, new { Search = "%" + searchKeyword.Trim() + "%" });
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

        #region Reports & Analytics Typed Methods

        public async Task<System.Collections.Generic.IEnumerable<Models.BestSellerItem>> GetBestSellersAsync(int limit = 5)
        {
            using (var conn = GetConnection())
            {
                string sql = @"
                    SELECT p.Pro_Name AS ProductName, SUM(d.Sales_Qty) AS TotalQuantity 
                    FROM tblSalesDetail d 
                    JOIN tblProduct p ON d.Pro_ID = p.Pro_ID 
                    GROUP BY p.Pro_Name 
                    ORDER BY TotalQuantity DESC 
                    LIMIT @Limit";
                return await conn.QueryAsync<Models.BestSellerItem>(sql, new { Limit = limit });
            }
        }

        public async Task<System.Collections.Generic.IEnumerable<Models.Product>> GetDeadStockAsync(int limit = 10)
        {
            using (var conn = GetConnection())
            {
                string sql = @"
                    SELECT Pro_ID AS Pro_ID, Pro_ID AS Id, Pro_Barcode AS Code, Pro_Name AS Name, Pro_Price AS Price, Pro_Cost AS Cost, Pro_Qty AS Stock 
                    FROM tblProduct 
                    WHERE Pro_Qty > 0 
                    AND Pro_ID NOT IN (
                        SELECT DISTINCT Pro_ID FROM tblSalesDetail
                    )
                    ORDER BY Pro_Qty DESC
                    LIMIT @Limit";
                return await conn.QueryAsync<Models.Product>(sql, new { Limit = limit });
            }
        }

        public async Task<decimal> GetReportTotalRevenueAsync(bool isAllPeriod, object dateParams)
        {
            using (var conn = GetConnection())
            {
                string sql = isAllPeriod
                    ? "SELECT COALESCE(SUM(Sales_Total), 0) FROM tblSales_H WHERE Sales_Status != 'ยกเลิก' OR Sales_Status IS NULL"
                    : "SELECT COALESCE(SUM(Sales_Total), 0) FROM tblSales_H WHERE (Sales_Status != 'ยกเลิก' OR Sales_Status IS NULL) AND ((Sales_Date >= @S AND Sales_Date <= @E) OR (Sales_Date >= @S_BE AND Sales_Date <= @E_BE))";
                var res = await conn.QueryAsync<decimal>(sql, dateParams);
                return res.FirstOrDefault();
            }
        }

        public async Task<decimal> GetReportTotalExpensesAsync(bool isAllPeriod, object dateParams)
        {
            using (var conn = GetConnection())
            {
                string sql = isAllPeriod
                    ? "SELECT COALESCE(SUM(Expense_Amount), 0) FROM tblExpense"
                    : "SELECT COALESCE(SUM(Expense_Amount), 0) FROM tblExpense WHERE (Expense_Date >= @S AND Expense_Date <= @E) OR (Expense_Date >= @S_BE AND Expense_Date <= @E_BE)";
                var res = await conn.QueryAsync<decimal>(sql, dateParams);
                return res.FirstOrDefault();
            }
        }

        public async Task<(IEnumerable<Models.ReportRow> Rows, int TotalCount, decimal TotalAmount, string SummaryText)> GetReportDataAsync(string reportType, bool isAllPeriod, object dateParams, DateTime start, DateTime end)
        {
            using (var conn = GetConnection())
            {
                IEnumerable<Models.ReportRow> rows = new List<Models.ReportRow>();
                int totalCount = 0;
                decimal totalAmount = 0;
                string summaryText = string.Empty;

                switch (reportType)
                {
                    case "รายงานข้อมูลพนักงาน":
                        var staff = await conn.QueryAsync("SELECT CAST(Emp_ID AS TEXT) as col1, Emp_Username as col2, Emp_Role as col3, Emp_Tel as col4, '' as col5 FROM tblEmployee ORDER BY Emp_Username");
                        rows = staff.Select(r => new Models.ReportRow { Col1 = r.col1, Col2 = r.col2, Col3 = r.col3, Col4 = r.col4 ?? "-", Col5 = r.col5 }).ToList();
                        totalCount = rows.Count();
                        summaryText = $"จำนวนพนักงานทั้งหมด: {totalCount} คน";
                        break;

                    case "รายงานข้อมูลลูกค้า":
                        var customers = await conn.QueryAsync("SELECT Cus_Name as col1, Cus_Tel as col2, Cus_Address as col3 FROM tblCustomer ORDER BY Cus_Name");
                        rows = customers.Select(r => new Models.ReportRow { Col1 = r.col1, Col2 = r.col2, Col3 = r.col3 }).ToList();
                        totalCount = rows.Count();
                        summaryText = $"จำนวนลูกค้าทั้งหมด: {totalCount} ราย";
                        break;

                    case "รายงานข้อมูลสินค้า":
                        var products = await conn.QueryAsync("SELECT Pro_Name as col1, CAST(Pro_Price as TEXT) as col2, CAST(Pro_Qty as TEXT) as col3, Pro_Category as col4 FROM tblProduct ORDER BY Pro_Name");
                        rows = products.Select(r => new Models.ReportRow { Col1 = r.col1, Col2 = r.col2, Col3 = r.col3, Col4 = r.col4 }).ToList();
                        totalCount = rows.Count();
                        summaryText = $"จำนวนสินค้าทั้งหมด: {totalCount} รายการ";
                        break;

                    case "รายงานข้อมูลบริษัทคู่ค้า":
                        var partners = await conn.QueryAsync("SELECT Partner_Name as col1, Partner_Contact as col2, Partner_Tel as col3, Partner_Address as col4 FROM tblPartner ORDER BY Partner_Name");
                        rows = partners.Select(r => new Models.ReportRow { Col1 = r.col1 ?? "-", Col2 = r.col2 ?? "-", Col3 = r.col3 ?? "-", Col4 = r.col4 ?? "-" }).ToList();
                        totalCount = rows.Count();
                        summaryText = $"จำนวนบริษัทคู่ค้าทั้งหมด: {totalCount} บริษัท";
                        break;

                    case "รายงานข้อมูลประเภทสินค้า":
                        var prods = await conn.QueryAsync("SELECT CAST(p.Pro_ID as TEXT) as col1, p.Pro_Category as col2, p.Pro_Name as col3, COALESCE(p.Pro_Image, '-') as col4 FROM tblProduct p ORDER BY p.Pro_Category, p.Pro_Name");
                        rows = prods.Select(r => new Models.ReportRow { Col1 = r.col1, Col2 = r.col2 ?? "-", Col3 = r.col3 ?? "-", Col4 = r.col4 ?? "-" }).ToList();
                        totalCount = rows.Count();
                        summaryText = $"จำนวนสินค้าทั้งหมด: {totalCount} รายการ";
                        break;

                    case "รายงานข้อมูลการสั่งซื้อสินค้า":
                        string sqlPO = isAllPeriod
                            ? "SELECT CAST(PO_ID as TEXT) as col1, CAST(PO_Total as TEXT) as col3, PO_Status as col4, PO_Date as col5 FROM tblPO_H ORDER BY PO_Date DESC"
                            : "SELECT CAST(PO_ID as TEXT) as col1, CAST(PO_Total as TEXT) as col3, PO_Status as col4, PO_Date as col5 FROM tblPO_H WHERE (PO_Date >= @S AND PO_Date <= @E) OR (PO_Date >= @S_BE AND PO_Date <= @E_BE) ORDER BY PO_Date DESC";
                        var pos = await conn.QueryAsync(sqlPO, dateParams);
                        rows = pos.Select(r => new Models.ReportRow { Col1 = r.col1, Col2 = "-", Col3 = r.col3, Col4 = r.col4, Col5 = r.col5 }).ToList();
                        totalCount = rows.Count();
                        summaryText = isAllPeriod ? $"จำนวนใบสั่งซื้อทั้งหมด: {totalCount} ใบ" : $"จำนวนใบสั่งซื้อ: {totalCount} ใบ  |  ช่วงเวลา: {start:dd/MM/yyyy} - {end:dd/MM/yyyy}";
                        break;

                    case "รายงานข้อมูลการรับเข้าสินค้า":
                        string sqlStockIn = isAllPeriod
                            ? "SELECT CAST(StockIn_ID as TEXT) as col1, Note as col2, StockIn_Date as col5 FROM tblStockIn_H ORDER BY StockIn_Date DESC"
                            : "SELECT CAST(StockIn_ID as TEXT) as col1, Note as col2, StockIn_Date as col5 FROM tblStockIn_H WHERE (StockIn_Date >= @S AND StockIn_Date <= @E) OR (StockIn_Date >= @S_BE AND StockIn_Date <= @E_BE) ORDER BY StockIn_Date DESC";
                        var stockIns = await conn.QueryAsync(sqlStockIn, dateParams);
                        rows = stockIns.Select(r => new Models.ReportRow { Col1 = r.col1, Col2 = r.col2 ?? "-", Col3 = "-", Col4 = "-", Col5 = r.col5 }).ToList();
                        totalCount = rows.Count();
                        summaryText = isAllPeriod ? $"จำนวนรายการรับเข้าทั้งหมด: {totalCount} รายการ" : $"จำนวนรายการรับเข้า: {totalCount} รายการ  |  ช่วงเวลา: {start:dd/MM/yyyy} - {end:dd/MM/yyyy}";
                        break;

                    case "รายงานข้อมูลการขายหน้าร้าน":
                        string sqlSales = isAllPeriod
                            ? "SELECT RefNo as col1, CAST(Sales_Total as TEXT) as col2, Sales_PaymentType as col3, Sales_Status as col4, Sales_Date as col5 FROM tblSales_H ORDER BY Sales_Date DESC"
                            : "SELECT RefNo as col1, CAST(Sales_Total as TEXT) as col2, Sales_PaymentType as col3, Sales_Status as col4, Sales_Date as col5 FROM tblSales_H WHERE (Sales_Date >= @S AND Sales_Date <= @E) OR (Sales_Date >= @S_BE AND Sales_Date <= @E_BE) ORDER BY Sales_Date DESC";
                        var sales = await conn.QueryAsync(sqlSales, dateParams);
                        rows = sales.Select(r => new Models.ReportRow { Col1 = r.col1, Col2 = r.col2, Col3 = r.col3, Col4 = r.col4, Col5 = r.col5 }).ToList();
                        totalCount = rows.Count();
                        totalAmount = rows.Sum(r => { decimal.TryParse(r.Col2, out decimal v); return v; });
                        summaryText = isAllPeriod ? $"จำนวนบิลทั้งหมด: {totalCount} ใบ  |  ยอดขายรวม: {totalAmount:N2} บาท" : $"จำนวนบิล: {totalCount} ใบ  |  ยอดขายรวม: {totalAmount:N2} บาท  |  ช่วงเวลา: {start:dd/MM/yyyy} - {end:dd/MM/yyyy}";
                        break;

                    case "รายงานข้อมูลรายรับ-รายจ่าย":
                        string sqlSalesInc = isAllPeriod
                            ? "SELECT 'รายรับ (ขาย)' as col1, RefNo as col2, CAST(Sales_Total as TEXT) as col3, Sales_PaymentType as col4, Sales_Date as col5 FROM tblSales_H"
                            : "SELECT 'รายรับ (ขาย)' as col1, RefNo as col2, CAST(Sales_Total as TEXT) as col3, Sales_PaymentType as col4, Sales_Date as col5 FROM tblSales_H WHERE (Sales_Date >= @S AND Sales_Date <= @E) OR (Sales_Date >= @S_BE AND Sales_Date <= @E_BE)";
                        string sqlExpRows = isAllPeriod
                            ? "SELECT 'รายจ่าย' as col1, Expense_Category as col2, CAST(Expense_Amount as TEXT) as col3, Expense_Note as col4, Expense_Date as col5 FROM tblExpense"
                            : "SELECT 'รายจ่าย' as col1, Expense_Category as col2, CAST(Expense_Amount as TEXT) as col3, Expense_Note as col4, Expense_Date as col5 FROM tblExpense WHERE (Expense_Date >= @S AND Expense_Date <= @E) OR (Expense_Date >= @S_BE AND Expense_Date <= @E_BE)";
                        var salesInc = (await conn.QueryAsync(sqlSalesInc, dateParams)).ToList();
                        var expRows = (await conn.QueryAsync(sqlExpRows, dateParams)).ToList();
                        rows = salesInc.Select(r => new Models.ReportRow { Col1 = r.col1, Col2 = r.col2, Col3 = r.col3, Col4 = r.col4, Col5 = r.col5 })
                               .Concat(expRows.Select(r => new Models.ReportRow { Col1 = r.col1, Col2 = r.col2, Col3 = r.col3, Col4 = r.col4, Col5 = r.col5 })).ToList();
                        var incSum = salesInc.Sum(r => { decimal.TryParse((string)r.col3, out decimal v); return v; });
                        var expSum = expRows.Sum(r => { decimal.TryParse((string)r.col3, out decimal v); return v; });
                        summaryText = $"รายรับ: {incSum:N2} บาท  |  รายจ่าย: {expSum:N2} บาท  |  กำไรสุทธิ: {(incSum - expSum):N2} บาท";
                        break;

                    case "รายงานข้อมูลการจัดส่งสินค้า":
                        var deliveries = await conn.QueryAsync("SELECT Tracking_No as col1, Recipient_Name as col2, Recipient_Tel as col3, Delivery_Status as col4 FROM tblDelivery");
                        rows = deliveries.Select(r => new Models.ReportRow { Col1 = r.col1 ?? "-", Col2 = r.col2 ?? "-", Col3 = r.col3 ?? "-", Col4 = r.col4 ?? "-", Col5 = "-" }).ToList();
                        totalCount = rows.Count();
                        summaryText = isAllPeriod ? $"จำนวนรายการจัดส่งทั้งหมด: {totalCount} รายการ" : $"จำนวนรายการจัดส่ง: {totalCount} รายการ  |  ช่วงเวลา: {start:dd/MM/yyyy} - {end:dd/MM/yyyy}";
                        break;

                    case "รายงานข้อมูลการเคลม":
                        string sqlClaims = isAllPeriod
                            ? @"SELECT CAST(c.Claim_ID as TEXT) as col1, 
                                       (p.Pro_Name || ' / ' || COALESCE(c.Claim_Reason, 'ไม่ระบุสาเหตุ')) as col2, 
                                       c.Claim_Status as col3, 
                                       COALESCE(c.Claim_Action, 'เปลี่ยนสินค้าใหม่') as col4, 
                                       c.Claim_Date as col5 
                                FROM tblClaim c 
                                LEFT JOIN tblProduct p ON c.Pro_ID = p.Pro_ID 
                                ORDER BY c.Claim_ID DESC"
                            : @"SELECT CAST(c.Claim_ID as TEXT) as col1, 
                                       (p.Pro_Name || ' / ' || COALESCE(c.Claim_Reason, 'ไม่ระบุสาเหตุ')) as col2, 
                                       c.Claim_Status as col3, 
                                       COALESCE(c.Claim_Action, 'เปลี่ยนสินค้าใหม่') as col4, 
                                       c.Claim_Date as col5 
                                FROM tblClaim c 
                                LEFT JOIN tblProduct p ON c.Pro_ID = p.Pro_ID 
                                WHERE (c.Claim_Date >= @S AND c.Claim_Date <= @E) OR (c.Claim_Date >= @S_BE AND c.Claim_Date <= @E_BE) 
                                 ORDER BY c.Claim_ID DESC";
                        var claims = await conn.QueryAsync(sqlClaims, dateParams);
                        rows = claims.Select(r => new Models.ReportRow { Col1 = r.col1, Col2 = r.col2, Col3 = r.col3, Col4 = r.col4, Col5 = r.col5 }).ToList();
                        totalCount = rows.Count();
                        summaryText = isAllPeriod ? $"จำนวนรายการเคลมสินค้าทั้งหมด: {totalCount} รายการ" : $"จำนวนรายการเคลมสินค้า: {totalCount} รายการ  |  ช่วงเวลา: {start:dd/MM/yyyy} - {end:dd/MM/yyyy}";
                        break;

                    default:
                        summaryText = "กรุณาเลือกประเภทรายงาน";
                        break;
                }

                return (rows, totalCount, totalAmount, summaryText);
            }
        }

        // --- Chart Report Methods ---
        public async Task<List<(string Name, string Tel, decimal Total, int Count)>> GetTopCustomerSpendersReportAsync(bool isAllPeriod, object chartParams)
        {
            using (var conn = GetConnection())
            {
                string sql = isAllPeriod
                    ? @"SELECT COALESCE(c.Cus_Name, 'ลูกค้าทั่วไป') as Name, 
                               COALESCE(c.Cus_Tel, '-') as Tel, 
                               COALESCE(SUM(s.Sales_Total), 0) as Total, 
                               COUNT(s.Sales_ID) as Count
                        FROM tblSales_H s
                        LEFT JOIN tblCustomer c ON s.Cus_ID = c.Cus_ID
                        GROUP BY s.Cus_ID
                        ORDER BY Total DESC
                        LIMIT 7"
                    : @"SELECT COALESCE(c.Cus_Name, 'ลูกค้าทั่วไป') as Name, 
                               COALESCE(c.Cus_Tel, '-') as Tel, 
                               COALESCE(SUM(s.Sales_Total), 0) as Total, 
                               COUNT(s.Sales_ID) as Count
                        FROM tblSales_H s
                        LEFT JOIN tblCustomer c ON s.Cus_ID = c.Cus_ID
                        WHERE (s.Sales_Date >= @startStr AND s.Sales_Date <= @endStr) OR (s.Sales_Date >= @startStrBE AND s.Sales_Date <= @endStrBE)
                        GROUP BY s.Cus_ID
                        ORDER BY Total DESC
                        LIMIT 7";
                var res = await conn.QueryAsync<(string Name, string Tel, decimal Total, int Count)>(sql, chartParams);
                return res.ToList();
            }
        }

        public async Task<List<(string Username, string Role, decimal Total, int Count)>> GetStaffPerformanceReportAsync(bool isAllPeriod, object chartParams)
        {
            using (var conn = GetConnection())
            {
                string sql = isAllPeriod
                    ? @"SELECT e.Emp_Username as Username, 
                               COALESCE(e.Emp_Role, 'พนักงาน') as Role, 
                               COALESCE(SUM(s.Sales_Total), 0) as Total, 
                               COUNT(s.Sales_ID) as Count
                        FROM tblEmployee e
                        LEFT JOIN tblSales_H s ON e.Emp_ID = s.Emp_ID
                        GROUP BY e.Emp_ID
                        ORDER BY Total DESC"
                    : @"SELECT e.Emp_Username as Username, 
                               COALESCE(e.Emp_Role, 'พนักงาน') as Role, 
                               COALESCE(SUM(s.Sales_Total), 0) as Total, 
                               COUNT(s.Sales_ID) as Count
                        FROM tblEmployee e
                        LEFT JOIN tblSales_H s ON e.Emp_ID = s.Emp_ID AND ((s.Sales_Date >= @startStr AND s.Sales_Date <= @endStr) OR (s.Sales_Date >= @startStrBE AND s.Sales_Date <= @endStrBE))
                        GROUP BY e.Emp_ID
                        ORDER BY Total DESC";
                var res = await conn.QueryAsync<(string Username, string Role, decimal Total, int Count)>(sql, chartParams);
                return res.ToList();
            }
        }

        public async Task<List<(string Name, int Qty, decimal Total)>> GetBestSellingProductsReportAsync(bool isAllPeriod, object chartParams)
        {
            using (var conn = GetConnection())
            {
                string sql = isAllPeriod
                    ? @"SELECT p.Pro_Name as Name, 
                               COALESCE(SUM(d.Sales_Qty), 0) as Qty, 
                               COALESCE(SUM(d.Sales_Subtotal), 0) as Total
                        FROM tblProduct p
                        JOIN tblSalesDetail d ON p.Pro_ID = d.Pro_ID
                        JOIN tblSales_H s ON d.Sales_ID = s.Sales_ID
                        GROUP BY p.Pro_ID
                        ORDER BY Qty DESC
                        LIMIT 5"
                    : @"SELECT p.Pro_Name as Name, 
                               COALESCE(SUM(d.Sales_Qty), 0) as Qty, 
                               COALESCE(SUM(d.Sales_Subtotal), 0) as Total
                        FROM tblProduct p
                        JOIN tblSalesDetail d ON p.Pro_ID = d.Pro_ID
                        JOIN tblSales_H s ON d.Sales_ID = s.Sales_ID
                        WHERE (s.Sales_Date >= @startStr AND s.Sales_Date <= @endStr) OR (s.Sales_Date >= @startStrBE AND s.Sales_Date <= @endStrBE)
                        GROUP BY p.Pro_ID
                        ORDER BY Qty DESC
                        LIMIT 5";
                var res = await conn.QueryAsync<(string Name, int Qty, decimal Total)>(sql, chartParams);
                return res.ToList();
            }
        }

        public async Task<List<(string Name, decimal Total, int Count)>> GetSupplierOrderVolumeReportAsync(bool isAllPeriod, object chartParams)
        {
            using (var conn = GetConnection())
            {
                string sql = isAllPeriod
                    ? @"SELECT COALESCE(p.Partner_Name, 'ซัพพลายเออร์ทั่วไป') as Name, 
                               COALESCE(SUM(po.PO_Total), 0) as Total, 
                               COUNT(po.PO_ID) as Count
                        FROM tblPartner p
                        LEFT JOIN tblPO_H po ON p.Partner_ID = po.Partner_ID
                        GROUP BY p.Partner_ID
                        ORDER BY Total DESC"
                    : @"SELECT COALESCE(p.Partner_Name, 'ซัพพลายเออร์ทั่วไป') as Name, 
                               COALESCE(SUM(po.PO_Total), 0) as Total, 
                               COUNT(po.PO_ID) as Count
                        FROM tblPartner p
                        LEFT JOIN tblPO_H po ON p.Partner_ID = po.Partner_ID AND ((po.PO_Date >= @startStr AND po.PO_Date <= @endStr) OR (po.PO_Date >= @startStrBE AND po.PO_Date <= @endStrBE))
                        GROUP BY p.Partner_ID
                        ORDER BY Total DESC";
                var res = await conn.QueryAsync<(string Name, decimal Total, int Count)>(sql, chartParams);
                return res.ToList();
            }
        }

        public async Task<List<(string Category, decimal Total)>> GetCategorySalesDistributionReportAsync(bool isAllPeriod, object chartParams)
        {
            using (var conn = GetConnection())
            {
                string sql = isAllPeriod
                    ? @"SELECT COALESCE(p.Pro_Category, 'ทั่วไป') as Category, 
                               COALESCE(SUM(d.Sales_Subtotal), 0) as Total
                        FROM tblProduct p
                        JOIN tblSalesDetail d ON p.Pro_ID = d.Pro_ID
                        JOIN tblSales_H s ON d.Sales_ID = s.Sales_ID
                        GROUP BY p.Pro_Category
                        ORDER BY Total DESC"
                    : @"SELECT COALESCE(p.Pro_Category, 'ทั่วไป') as Category, 
                               COALESCE(SUM(d.Sales_Subtotal), 0) as Total
                        FROM tblProduct p
                        JOIN tblSalesDetail d ON p.Pro_ID = d.Pro_ID
                        JOIN tblSales_H s ON d.Sales_ID = s.Sales_ID
                        WHERE (s.Sales_Date >= @startStr AND s.Sales_Date <= @endStr) OR (s.Sales_Date >= @startStrBE AND s.Sales_Date <= @endStrBE)
                        GROUP BY p.Pro_Category
                        ORDER BY Total DESC";
                var res = await conn.QueryAsync<(string Category, decimal Total)>(sql, chartParams);
                return res.ToList();
            }
        }

        public async Task<List<(string Status, int Count, decimal Total)>> GetPurchaseOrderStatusReportAsync(bool isAllPeriod, object chartParams)
        {
            using (var conn = GetConnection())
            {
                string sql = isAllPeriod
                    ? @"SELECT COALESCE(PO_Status, 'รอดำเนินการ') as Status, 
                               COUNT(PO_ID) as Count, 
                               COALESCE(SUM(PO_Total), 0) as Total
                        FROM tblPO_H
                        GROUP BY PO_Status
                        ORDER BY Count DESC"
                    : @"SELECT COALESCE(PO_Status, 'รอดำเนินการ') as Status, 
                               COUNT(PO_ID) as Count, 
                               COALESCE(SUM(PO_Total), 0) as Total
                        FROM tblPO_H
                        WHERE (PO_Date >= @startStr AND PO_Date <= @endStr) OR (PO_Date >= @startStrBE AND PO_Date <= @endStrBE)
                        GROUP BY PO_Status
                        ORDER BY Count DESC";
                var res = await conn.QueryAsync<(string Status, int Count, decimal Total)>(sql, chartParams);
                return res.ToList();
            }
        }

        public async Task<List<(string Date, int Qty)>> GetStockInDailyReportAsync(bool isAllPeriod, object chartParams)
        {
            using (var conn = GetConnection())
            {
                string sql = isAllPeriod
                    ? @"SELECT SUBSTR(h.StockIn_Date, 1, 10) as Date, 
                               COALESCE(SUM(d.StockIn_Qty), 0) as Qty
                        FROM tblStockIn_H h
                        JOIN tblStockInDetail d ON h.StockIn_ID = d.StockIn_ID
                        GROUP BY SUBSTR(h.StockIn_Date, 1, 10)
                        ORDER BY Date DESC
                        LIMIT 10"
                    : @"SELECT SUBSTR(h.StockIn_Date, 1, 10) as Date, 
                               COALESCE(SUM(d.StockIn_Qty), 0) as Qty
                        FROM tblStockIn_H h
                        JOIN tblStockInDetail d ON h.StockIn_ID = d.StockIn_ID
                        WHERE (h.StockIn_Date >= @startStr AND h.StockIn_Date <= @endStr) OR (h.StockIn_Date >= @startStrBE AND h.StockIn_Date <= @endStrBE)
                        GROUP BY SUBSTR(h.StockIn_Date, 1, 10)
                        ORDER BY Date DESC
                        LIMIT 10";
                var res = await conn.QueryAsync<(string Date, int Qty)>(sql, chartParams);
                return res.ToList();
            }
        }

        public async Task<List<(string Date, decimal Total, int Count)>> GetDailySalesTrendReportAsync(bool isAllPeriod, object chartParams)
        {
            using (var conn = GetConnection())
            {
                string sql = isAllPeriod
                    ? @"SELECT SUBSTR(Sales_Date, 1, 10) as Date, 
                               SUM(Sales_Total) as Total, 
                               COUNT(Sales_ID) as Count
                        FROM tblSales_H
                        GROUP BY SUBSTR(Sales_Date, 1, 10)
                        ORDER BY Date DESC
                        LIMIT 10"
                    : @"SELECT SUBSTR(Sales_Date, 1, 10) as Date, 
                               SUM(Sales_Total) as Total, 
                               COUNT(Sales_ID) as Count
                        FROM tblSales_H
                        WHERE (Sales_Date >= @startStr AND Sales_Date <= @endStr) OR (Sales_Date >= @startStrBE AND Sales_Date <= @endStrBE)
                        GROUP BY SUBSTR(Sales_Date, 1, 10)
                        ORDER BY Date DESC
                        LIMIT 10";
                var res = await conn.QueryAsync<(string Date, decimal Total, int Count)>(sql, chartParams);
                return res.ToList();
            }
        }

        public async Task<List<(string Reason, int Count)>> GetClaimReasonDistributionReportAsync(bool isAllPeriod, object chartParams)
        {
            using (var conn = GetConnection())
            {
                string sql = isAllPeriod
                    ? @"SELECT COALESCE(Claim_Reason, 'ไม่ระบุสาเหตุ') as Reason, 
                               COUNT(Claim_ID) as Count
                        FROM tblClaim
                        GROUP BY Claim_Reason
                        ORDER BY Count DESC"
                    : @"SELECT COALESCE(Claim_Reason, 'ไม่ระบุสาเหตุ') as Reason, 
                               COUNT(Claim_ID) as Count
                        FROM tblClaim
                        WHERE (Claim_Date >= @startStr AND Claim_Date <= @endStr) OR (Claim_Date >= @startStrBE AND Claim_Date <= @endStrBE)
                        GROUP BY Claim_Reason
                        ORDER BY Count DESC";
                var res = await conn.QueryAsync<(string Reason, int Count)>(sql, chartParams);
                return res.ToList();
            }
        }

        public async Task<List<(string Date, int Count)>> GetDailyDeliveryReportAsync(bool isAllPeriod, object chartParams)
        {
            using (var conn = GetConnection())
            {
                string sql = isAllPeriod
                    ? @"SELECT SUBSTR(s.Sales_Date, 1, 10) as Date, 
                               COUNT(d.Delivery_ID) as Count
                        FROM tblDelivery d
                        JOIN tblSales_H s ON d.Sales_ID = s.Sales_ID
                        WHERE d.Delivery_Status = 'จัดส่งสำเร็จ' OR d.Delivery_Status = 'สำเร็จ' OR d.Delivery_Status IS NOT NULL
                        GROUP BY SUBSTR(s.Sales_Date, 1, 10)
                        ORDER BY Date DESC
                        LIMIT 10"
                    : @"SELECT SUBSTR(s.Sales_Date, 1, 10) as Date, 
                               COUNT(d.Delivery_ID) as Count
                        FROM tblDelivery d
                        JOIN tblSales_H s ON d.Sales_ID = s.Sales_ID
                        WHERE (d.Delivery_Status = 'จัดส่งสำเร็จ' OR d.Delivery_Status = 'สำเร็จ' OR d.Delivery_Status IS NOT NULL)
                          AND ((s.Sales_Date >= @startStr AND s.Sales_Date <= @endStr) OR (s.Sales_Date >= @startStrBE AND s.Sales_Date <= @endStrBE))
                        GROUP BY SUBSTR(s.Sales_Date, 1, 10)
                        ORDER BY Date DESC
                        LIMIT 10";
                var res = await conn.QueryAsync<(string Date, int Count)>(sql, chartParams);
                return res.ToList();
            }
        }

        public async Task<(List<(string Month, decimal Total)> Revenue, List<(string Month, decimal Total)> Expenses)> GetMonthlyIncomeExpenseReportAsync(bool isAllPeriod, object chartParams)
        {
            using (var conn = GetConnection())
            {
                string sqlMonthlyRev = isAllPeriod
                    ? @"SELECT SUBSTR(Sales_Date, 1, 7) as Month, SUM(Sales_Total) as Total
                        FROM tblSales_H
                        GROUP BY SUBSTR(Sales_Date, 1, 7)
                        ORDER BY Month DESC
                        LIMIT 12"
                    : @"SELECT SUBSTR(Sales_Date, 1, 7) as Month, SUM(Sales_Total) as Total
                        FROM tblSales_H
                        WHERE (Sales_Date >= @startStr AND Sales_Date <= @endStr) OR (Sales_Date >= @startStrBE AND Sales_Date <= @endStrBE)
                        GROUP BY SUBSTR(Sales_Date, 1, 7)
                        ORDER BY Month DESC
                        LIMIT 12";

                string sqlMonthlyExp = isAllPeriod
                    ? @"SELECT SUBSTR(Expense_Date, 1, 7) as Month, SUM(Expense_Amount) as Total
                        FROM tblExpense
                        GROUP BY SUBSTR(Expense_Date, 1, 7)
                        ORDER BY Month DESC
                        LIMIT 12"
                    : @"SELECT SUBSTR(Expense_Date, 1, 7) as Month, SUM(Expense_Amount) as Total
                        FROM tblExpense
                        WHERE (Expense_Date >= @startStr AND Expense_Date <= @endStr) OR (Expense_Date >= @startStrBE AND Expense_Date <= @endStrBE)
                        GROUP BY SUBSTR(Expense_Date, 1, 7)
                        ORDER BY Month DESC
                        LIMIT 12";

                var revRaw = await conn.QueryAsync<(string Month, decimal Total)>(sqlMonthlyRev, chartParams);
                var expRaw = await conn.QueryAsync<(string Month, decimal Total)>(sqlMonthlyExp, chartParams);

                return (revRaw.ToList(), expRaw.ToList());
            }
        }

        #endregion
    }
}
