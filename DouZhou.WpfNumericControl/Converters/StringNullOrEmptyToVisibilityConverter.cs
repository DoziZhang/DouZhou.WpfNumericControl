using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DouZhou.WpfNumericControl
{
    /// <summary>
    /// 空字符串或null转换为Visibility.Visible
    /// </summary>
    public class StringNullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
