using System.Configuration;
using System.Data;
using System.Windows;
using Porjai20.Views;       // เรียกใช้โฟลเดอร์เก็บหน้าต่าง UI
using Porjai20.ViewModels;  // เรียกใช้โฟลเดอร์เก็บ Logic เบื้องหลัง

namespace Porjai20
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // 🌟 บรรทัดสำคัญที่สุดที่ขาดไป: สั่งให้แอปพลิเคชันไปโหลดไฟล์ App.xaml เข้ามาใช้งาน
            InitializeComponent();

            // ตั้งค่าระบบเงินตราและฟอร์แมตภาษาให้เป็น th-TH (รองรับเครื่องหมาย ฿)
            var culture = new System.Globalization.CultureInfo("th-TH");
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    System.Windows.Markup.XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

            // ดักจับ Exception ป้องกันแอปปิดตัวเองเงียบๆ
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        /// <summary>
        /// จุดเริ่มต้นของโปรแกรม (Entry Point) ทำหน้าที่เหมือน Program.cs ของ WinForms
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. สร้างตัวควบคุมข้อมูลหลัก (ViewModel) 
            var vm = new ProductViewModel();

            // 2. จำลองสิทธิ์ล็อกอินอัตโนมัติ (Bypass Login Stage)
            vm.CurrentUser = new Models.User { Name = "Developer Mode", Role = "Admin" };

            // 3. บังคับให้ระบบเปิดมาแล้วเด้งไปที่หน้าจอระบบ "ขายหน้าร้าน (POS)" ทันที
            vm.SwitchToPosCommand.Execute(null);

            // 4. สั่งบิลด์หน้าต่างหลัก ผูกข้อมูลหลังบ้าน แล้วสั่งแสดงผลขึ้นจอภาพ
            var mainWindow = new MainWindow();
            mainWindow.DataContext = vm;
            mainWindow.Show();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"An unhandled exception occurred: {e.Exception.Message}\n\nStack Trace:\n{e.Exception.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}