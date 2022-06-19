using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FrisbeeDicomEditor.Converters
{
    public class Null2VisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Invert)
            {
                return value == null ? Visibility.Visible : Visibility.Collapsed;
            }
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
