using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Porjai20.Converters
{
    /// <summary>
    /// แปลง string key เป็น Resource object จาก Application.Current.Resources
    /// ใช้สำหรับ bind ชื่อ key ของ SolidColorBrush เป็น Background/Foreground ใน DataTemplate
    /// 
    /// ตัวอย่าง: {Binding IconBgColorKey, Converter={StaticResource ResourceKeyToBrush}}
    /// </summary>
    public class ResourceKeyToBrushConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string key && !string.IsNullOrEmpty(key))
            {
                var resource = Application.Current?.TryFindResource(key);
                if (resource != null)
                    return resource;
            }
            // Fallback: สีเทาอ่อนถ้าไม่พบ key
            return System.Windows.Media.Brushes.LightGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
