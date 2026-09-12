using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Porjai20.Converters
{
    public class StockToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stock)
            {
                // return Red if stock < 5, else Gray
                return stock < 5 ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D32F2F")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#757575"));
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#757575"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
