using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Gauniv.WpfClient.Converters;

/// <summary>
/// 将字符串转换为 Visibility
/// 空字符串或 null -> Collapsed, 其他 -> Visible
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
        {
            return Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
