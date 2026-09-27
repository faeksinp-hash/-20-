using System;
using System.Windows;
using Porjai20.Views;

namespace Porjai20.Services
{
    public interface IDialogService
    {
        void ShowInfo(string title, string message);
        void ShowSuccess(string title, string message);
        void ShowWarning(string title, string message);
        void ShowError(string title, string message);
        bool ShowConfirm(string title, string message);
    }

    public class DialogService : IDialogService
    {
        public static IDialogService Instance { get; } = new DialogService();

        public void ShowInfo(string title, string message)
        {
            CustomMessageBox.Show(
                messageBoxText: message,
                caption: string.IsNullOrWhiteSpace(title) ? "แจ้งเตือน" : title,
                button: MessageBoxButton.OK,
                icon: MessageBoxImage.Information);
        }

        public void ShowSuccess(string title, string message)
        {
            CustomMessageBox.Show(
                messageBoxText: message,
                caption: string.IsNullOrWhiteSpace(title) ? "สำเร็จ" : title,
                button: MessageBoxButton.OK,
                icon: MessageBoxImage.Information);
        }

        public void ShowWarning(string title, string message)
        {
            CustomMessageBox.Show(
                messageBoxText: message,
                caption: string.IsNullOrWhiteSpace(title) ? "แจ้งเตือน" : title,
                button: MessageBoxButton.OK,
                icon: MessageBoxImage.Warning);
        }

        public void ShowError(string title, string message)
        {
            CustomMessageBox.Show(
                messageBoxText: message,
                caption: string.IsNullOrWhiteSpace(title) ? "ข้อผิดพลาด" : title,
                button: MessageBoxButton.OK,
                icon: MessageBoxImage.Error);
        }

        public bool ShowConfirm(string title, string message)
        {
            var result = CustomMessageBox.Show(
                messageBoxText: message,
                caption: string.IsNullOrWhiteSpace(title) ? "ยืนยันการทำรายการ" : title,
                button: MessageBoxButton.YesNo,
                icon: MessageBoxImage.Question);

            return result == MessageBoxResult.Yes;
        }
    }
}
