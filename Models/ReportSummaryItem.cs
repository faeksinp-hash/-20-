using System;

namespace Porjai20.Models
{
    public class ReportSummaryItem
    {
        public int Rank { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string ValueText { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public string Color { get; set; } = "#0284C7";
    }
}
