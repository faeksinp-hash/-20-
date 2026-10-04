using System;

namespace Porjai20.Models
{
    public class PointsLedger
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string? BillId { get; set; }
        public string Type { get; set; } = "earn"; // 'earn', 'redeem', 'adjust', 'expire'
        public int Points { get; set; }
        public string? Note { get; set; }
        public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");

        public string TypeDisplay => Type switch
        {
            "earn" => "ได้รับแต้ม",
            "redeem" => "ใช้แต้มแลกส่วนลด",
            "adjust" => "ปรับปรุงแต้ม",
            "expire" => "แต้มหมดอายุ",
            _ => Type
        };

        public string PointsDisplay => Points > 0 ? $"+{Points}" : Points.ToString();
        public string PointsColor => Points > 0 ? "#16A34A" : (Points < 0 ? "#DC2626" : "#64748B");
    }
}
