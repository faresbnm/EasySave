using System;
using System.Windows.Data;

namespace EasySaveWPF.Converters
{
    public class BackupTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Convert from model (1 or 2) to view (0 or 1)
            return (int)value - 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Convert from view (0 or 1) to model (1 or 2)
            return (int)value + 1;
        }
    }
}