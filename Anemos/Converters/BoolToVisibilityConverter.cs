using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Anemos.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, string language)
    {
        if (parameter != null)
        {
            if ((bool)value!)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        if ((bool)value!)
        {
            return Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Visibility.Visible)
        {
            return true;
        }
        return false;
    }
}
