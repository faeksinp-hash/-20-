using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Porjai20.Common
{
    /// <summary>
    /// Centralized helper for WPF PrintDialog interactions.
    /// Provides consistent visual printing, UI element hiding during capture, and error handling.
    /// </summary>
    public static class PrintHelper
    {
        /// <summary>
        /// Displays print dialog and prints the specified visual area.
        /// </summary>
        /// <param name="printArea">The visual element to print.</param>
        /// <param name="documentTitle">Job title displayed in print queue.</param>
        /// <param name="elementToHide">Optional UI control to temporarily hide during printing (e.g. action buttons).</param>
        /// <param name="windowToClose">Optional Window to close upon successful print completion.</param>
        /// <returns>True if printed successfully; false if cancelled or error.</returns>
        public static bool PrintVisual(
            Visual printArea,
            string documentTitle,
            FrameworkElement? elementToHide = null,
            Window? windowToClose = null)
        {
            if (printArea == null) return false;

            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    if (elementToHide != null)
                    {
                        elementToHide.Visibility = Visibility.Collapsed;
                    }

                    printDialog.PrintVisual(printArea, documentTitle);

                    if (elementToHide != null)
                    {
                        elementToHide.Visibility = Visibility.Visible;
                    }

                    windowToClose?.Close();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                if (elementToHide != null)
                {
                    elementToHide.Visibility = Visibility.Visible;
                }

                MessageBox.Show(
                    $"เกิดข้อผิดพลาดในการพิมพ์: {ex.Message}",
                    "ข้อผิดพลาดการพิมพ์",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }
        }
    }
}
