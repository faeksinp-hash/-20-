using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Porjai20.Converters
{
    public class LowStockConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && 
                values[0] is int stock && 
                values[1] is int reorderPoint)
            {
                return stock <= reorderPoint;
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
