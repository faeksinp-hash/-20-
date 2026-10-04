using System.Windows;
using System.Windows.Controls;
using Porjai20.ViewModels;

namespace Porjai20.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContextChanged += MainWindow_DataContextChanged;
            this.AddHandler(UIElement.PreviewKeyDownEvent, new System.Windows.Input.KeyEventHandler(POSearchBox_PreviewKeyDown), true);
            this.AddHandler(UIElement.KeyDownEvent, new System.Windows.Input.KeyEventHandler(POSuggestionsListBox_KeyDown), true);
        }

        private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ProductViewModel oldVm)
                oldVm.PropertyChanged -= ViewModel_PropertyChanged;

            if (e.NewValue is ProductViewModel newVm)
                newVm.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProductViewModel.LoginPassword))
            {
                if (DataContext is ProductViewModel vm && string.IsNullOrEmpty(vm.LoginPassword))
                {
                    if (LoginPasswordBox != null && LoginPasswordBox.Password != string.Empty)
                        LoginPasswordBox.Password = string.Empty;
                }
            }
            else if (e.PropertyName == nameof(ProductViewModel.IsPOModalOpen) || e.PropertyName == "POItemAdded")
            {
                if (DataContext is ProductViewModel vm && vm.IsPOModalOpen)
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        var barcodeBox = FindChild<TextBox>(this, "POBarcodeBox");
                        if (barcodeBox != null)
                        {
                            barcodeBox.Focus();
                            barcodeBox.SelectAll();
                        }
                    }, System.Windows.Threading.DispatcherPriority.Input);
                }
            }
        }

        private T? FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            int childrenCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                        return typedChild;
                }

                var childOfChild = FindChild<T>(child, childName);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void LoginPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProductViewModel viewModel)
                viewModel.LoginPassword = ((PasswordBox)sender).Password;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void POSearchBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Down)
            {
                if (e.OriginalSource is TextBox tb && (tb.Name == "POBarcodeBox" || tb.Name == "PONameBox" || (string)tb.Tag == "สแกนบาร์โค้ด / รหัสสินค้า" || (string)tb.Tag == "ค้นหาชื่อสินค้า"))
                {
                    var popup = FindChild<System.Windows.Controls.Primitives.Popup>(this, "POSuggestionsPopup");
                    var listBox = FindChild<ListBox>(this, "POSuggestionsListBox");
                    if (popup != null && popup.IsOpen && listBox != null)
                    {
                        listBox.Focus();
                        if (listBox.Items.Count > 0 && listBox.SelectedIndex < 0)
                        {
                            listBox.SelectedIndex = 0;
                        }
                        e.Handled = true;
                    }
                }
            }
        }

        private void POSuggestionsListBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (sender is ListBox listBox && DataContext is ProductViewModel vm && listBox.SelectedItem is Porjai20.Models.Product selected)
                {
                    vm.SelectedPOSuggestedProduct = selected;
                    e.Handled = true;
                }
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                if (DataContext is ProductViewModel vm)
                {
                    vm.IsPOSuggestionsOpen = false;
                    e.Handled = true;
                }
            }
        }
    }
}