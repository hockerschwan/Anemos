using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Anemos.Converters;

public class InvertedBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, string language)
    {
        if (parameter != null)
        {
            if ((bool)value!)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        if ((bool)value!)
        {
            return Visibility.Collapsed;
        }
        return Visibility.Visible;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Visibility.Visible)
        {
            return false;
        }
        return true;
    }
}
