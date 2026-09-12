using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Porjai20.Views
{
    /// <summary>
    /// Interaction logic for CustomMessageBox.xaml.
    /// Provides a custom chromeless message box window with Thai button text and sleek styling.
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

        public CustomMessageBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Displays a custom message box dialog with standard parameters.
        /// </summary>
        public static MessageBoxResult Show(
            string messageBoxText,
            string caption = "แจ้งเตือน",
            MessageBoxButton button = MessageBoxButton.OK,
            MessageBoxImage icon = MessageBoxImage.Information)
        {
            return Show(null, messageBoxText, caption, button, icon);
        }

        /// <summary>
        /// Displays a custom message box dialog attached to a specified owner window.
        /// </summary>
        public static MessageBoxResult Show(
            Window owner,
            string messageBoxText,
            string caption = "แจ้งเตือน",
            MessageBoxButton button = MessageBoxButton.OK,
            MessageBoxImage icon = MessageBoxImage.Information)
        {
            var msgBox = new CustomMessageBox();

            // Set Title & Message
            msgBox.TxtTitle.Text = string.IsNullOrWhiteSpace(caption) ? "แจ้งเตือน" : caption;
            msgBox.TxtMessage.Text = messageBoxText ?? string.Empty;

            // Configure Icon Visual
            ConfigureIcon(msgBox, icon);

            // Configure Buttons
            ConfigureButtons(msgBox, button);

            // Set Owner Window & Center Position
            Window targetOwner = owner 
                ?? Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive && w.Visibility == Visibility.Visible) 
                ?? Application.Current?.MainWindow;

            if (targetOwner != null && targetOwner.IsVisible)
            {
                msgBox.Owner = targetOwner;
                msgBox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                msgBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            msgBox.ShowDialog();
            return msgBox.Result;
        }

        private static void ConfigureIcon(CustomMessageBox msgBox, MessageBoxImage icon)
        {
            switch (icon)
            {
                case MessageBoxImage.Warning: // Warning / Exclamation
                    msgBox.TxtIcon.Text = "⚠️";
                    msgBox.IconContainer.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF3C7"));
                    break;
                case MessageBoxImage.Error: // Error / Hand / Stop
                    msgBox.TxtIcon.Text = "❌";
                    msgBox.IconContainer.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2"));
                    break;
                case MessageBoxImage.Question:
                    msgBox.TxtIcon.Text = "❓";
                    msgBox.IconContainer.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0F2FE"));
                    break;
                case MessageBoxImage.Information: // Information / Asterisk
                default:
                    msgBox.TxtIcon.Text = "ℹ️";
                    msgBox.IconContainer.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0F2FE"));
                    break;
            }
        }

        private static void ConfigureButtons(CustomMessageBox msgBox, MessageBoxButton button)
        {
            msgBox.BtnOk.Visibility = Visibility.Collapsed;
            msgBox.BtnYes.Visibility = Visibility.Collapsed;
            msgBox.BtnNo.Visibility = Visibility.Collapsed;
            msgBox.BtnCancel.Visibility = Visibility.Collapsed;

            switch (button)
            {
                case MessageBoxButton.OK:
                    msgBox.BtnOk.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.OKCancel:
                    msgBox.BtnOk.Visibility = Visibility.Visible;
                    msgBox.BtnCancel.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNo:
                    msgBox.BtnYes.Visibility = Visibility.Visible;
                    msgBox.BtnNo.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNoCancel:
                    msgBox.BtnYes.Visibility = Visibility.Visible;
                    msgBox.BtnNo.Visibility = Visibility.Visible;
                    msgBox.BtnCancel.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            DialogResult = true;
            Close();
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            DialogResult = true;
            Close();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            DialogResult = false;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            DialogResult = false;
            Close();
        }

        private void BtnCloseCross_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            Close();
        }
    }
}
