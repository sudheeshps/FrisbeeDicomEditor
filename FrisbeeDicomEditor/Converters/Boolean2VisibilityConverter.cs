using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FrisbeeDicomEditor.Converters
{
    public class Boolean2VisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Invert)
            {
                return bool.Parse(value.ToString()) == false ? Visibility.Visible : Visibility.Collapsed;
            }
            return bool.Parse(value.ToString()) == false ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
