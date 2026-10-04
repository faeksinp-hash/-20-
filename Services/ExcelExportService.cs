using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using Microsoft.Win32;
using Porjai20.Models;

namespace Porjai20.Services
{
    /// <summary>
    /// Centralized Excel export service using ClosedXML.
    /// Standardizes spreadsheet formatting, headers, summary cards, and styling across the application.
    /// </summary>
    public static class ExcelExportService
    {
        public static readonly XLColor BrandDark = XLColor.FromHtml("#0F172A");
        public static readonly XLColor BrandBlue = XLColor.FromHtml("#0284C7");
        public static readonly XLColor BrandMuted = XLColor.FromHtml("#64748B");
        public static readonly XLColor HeaderBg = XLColor.FromHtml("#00A3E0");
        public static readonly XLColor CardBorder = XLColor.FromHtml("#BAE6FD");

        /// <summary>
        /// Shows a SaveFileDialog for Excel and returns the selected path, or null if cancelled.
        /// </summary>
        public static string? PromptSaveExcelPath(string defaultFileName, string title = "บันทึกรายงาน Excel")
        {
            var sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = defaultFileName,
                Title = title
            };

            return sfd.ShowDialog() == true ? sfd.FileName : null;
        }

        /// <summary>
        /// Attempts to open the exported file in the default associated application (e.g. MS Excel).
        /// </summary>
        public static bool TryOpenFile(string filePath, out string? errorMessage)
        {
            errorMessage = null;
            try
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Exports a list of SalesOrder items to a beautifully styled Excel workbook.
        /// </summary>
        public static void ExportSalesOrders(
            IEnumerable<SalesOrder> orders,
            string periodInfo,
            string filePath)
        {
            var orderList = orders.ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("ประวัติการขาย");
            ws.Style.Font.FontName = "Segoe UI";

            // Row 1: Header - Shop Name
            ws.Cell(1, 1).Value = "ร้านพอใจ ทุกอย่าง 20 บาท";
            ws.Cell(1, 1).Style.Font.FontSize = 16;
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontColor = BrandDark;

            // Row 2: Sub-header - Report Name
            ws.Cell(2, 1).Value = "รายงานประวัติการขาย (Sales History Report)";
            ws.Cell(2, 1).Style.Font.FontSize = 13;
            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(2, 1).Style.Font.FontColor = BrandBlue;

            // Row 3: Metadata
            ws.Cell(3, 1).Value = $"ช่วงเวลา: {periodInfo}  |  พิมพ์ออกรายงาน: {DateTime.Now:dd/MM/yyyy HH:mm:ss} น.";
            ws.Cell(3, 1).Style.Font.FontSize = 10.5;
            ws.Cell(3, 1).Style.Font.FontColor = BrandMuted;

            // Row 4: Summary KPIs
            decimal totalRevenue = orderList.Sum(o => o.TotalAmount);
            int totalCount = orderList.Count;
            decimal avgPerOrder = totalCount > 0 ? totalRevenue / totalCount : 0;

            ws.Range(4, 1, 4, 2).Merge();
            ws.Cell(4, 1).Value = $"จำนวนบิลรวม: {totalCount:N0} รายการ";
            ws.Cell(4, 1).Style.Font.Bold = true;
            ws.Cell(4, 1).Style.Font.FontSize = 11;
            ws.Cell(4, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E0F2FE");
            ws.Cell(4, 1).Style.Font.FontColor = XLColor.FromHtml("#0369A1");
            ws.Cell(4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(4, 1, 4, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(4, 1, 4, 2).Style.Border.OutsideBorderColor = CardBorder;

            ws.Range(4, 3, 4, 4).Merge();
            ws.Cell(4, 3).Value = $"ยอดขายรวมสุทธิ: {totalRevenue:N2} บาท";
            ws.Cell(4, 3).Style.Font.Bold = true;
            ws.Cell(4, 3).Style.Font.FontSize = 11;
            ws.Cell(4, 3).Style.Fill.BackgroundColor = XLColor.FromHtml("#DCFCE7");
            ws.Cell(4, 3).Style.Font.FontColor = XLColor.FromHtml("#15803D");
            ws.Cell(4, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(4, 3, 4, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(4, 3, 4, 4).Style.Border.OutsideBorderColor = XLColor.FromHtml("#86EFAC");

            ws.Range(4, 5, 4, 6).Merge();
            ws.Cell(4, 5).Value = $"เฉลี่ยต่อบิล: {avgPerOrder:N2} บาท";
            ws.Cell(4, 5).Style.Font.Bold = true;
            ws.Cell(4, 5).Style.Font.FontSize = 11;
            ws.Cell(4, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#FEF3C7");
            ws.Cell(4, 5).Style.Font.FontColor = XLColor.FromHtml("#B45309");
            ws.Cell(4, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(4, 5, 4, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(4, 5, 4, 6).Style.Border.OutsideBorderColor = XLColor.FromHtml("#FDE68A");

            // Row 6: Data Table Headers
            int headerRow = 6;
            ws.Row(headerRow).Height = 26;

            string[] headers = {
                "ลำดับ",
                "เลขที่ใบเสร็จ",
                "วันที่ทำรายการ",
                "ลูกค้า / สมาชิก",
                "ยอดรวม (บาท)",
                "ช่องทางชำระ",
                "สถานะรายการ",
                "ประเภทการขาย"
            };

            for (int col = 0; col < headers.Length; col++)
            {
                var cell = ws.Cell(headerRow, col + 1);
                cell.Value = headers[col];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontSize = 11;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = HeaderBg;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#0284C7");
            }

            // Populate rows
            int currentRow = headerRow + 1;
            for (int i = 0; i < orderList.Count; i++)
            {
                var order = orderList[i];
                ws.Row(currentRow).Height = 22;

                ws.Cell(currentRow, 1).Value = i + 1;
                ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(currentRow, 2).Value = order.RefNo ?? "-";
                ws.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(currentRow, 3).Value = order.Sales_Date ?? "-";
                ws.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(currentRow, 4).Value = string.IsNullOrWhiteSpace(order.CustomerName) ? "ลูกค้าทั่วไป" : order.CustomerName;
                ws.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                ws.Cell(currentRow, 5).Value = order.TotalAmount;
                ws.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(currentRow, 5).Style.Font.Bold = true;

                ws.Cell(currentRow, 6).Value = order.PaymentMethod ?? "-";
                ws.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(currentRow, 7).Value = order.Status ?? "-";
                ws.Cell(currentRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(currentRow, 8).Value = order.IsDelivery ? "จัดส่งถึงที่" : "รับหน้าร้าน";
                ws.Cell(currentRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Alternate row fill
                if (i % 2 == 1)
                {
                    ws.Range(currentRow, 1, currentRow, headers.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#F8FAFC");
                }

                ws.Range(currentRow, 1, currentRow, headers.Length).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(currentRow, 1, currentRow, headers.Length).Style.Border.OutsideBorderColor = XLColor.FromHtml("#E2E8F0");
                ws.Range(currentRow, 1, currentRow, headers.Length).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(currentRow, 1, currentRow, headers.Length).Style.Border.InsideBorderColor = XLColor.FromHtml("#E2E8F0");

                currentRow++;
            }

            // Summary row at bottom
            ws.Row(currentRow).Height = 24;
            ws.Range(currentRow, 1, currentRow, 4).Merge();
            ws.Cell(currentRow, 1).Value = $"รวมทั้งสิ้น ({orderList.Count} รายการ)";
            ws.Cell(currentRow, 1).Style.Font.Bold = true;
            ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F5F9");

            ws.Cell(currentRow, 5).Value = totalRevenue;
            ws.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(currentRow, 5).Style.Font.Bold = true;
            ws.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(currentRow, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#E0F2FE");
            ws.Cell(currentRow, 5).Style.Font.FontColor = XLColor.FromHtml("#0369A1");

            ws.Range(currentRow, 6, currentRow, headers.Length).Merge();
            ws.Cell(currentRow, 6).Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F5F9");

            ws.Range(currentRow, 1, currentRow, headers.Length).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.Range(currentRow, 1, currentRow, headers.Length).Style.Border.OutsideBorderColor = XLColor.FromHtml("#CBD5E1");

            // Auto-fit columns
            ws.Columns().AdjustToContents();
            foreach (var col in ws.Columns())
            {
                col.Width = Math.Max(col.Width + 4, 12);
            }

            workbook.SaveAs(filePath);
        }
    }
}
