using Microsoft.UI.Xaml.Data;

namespace Anemos.Converters;

public class RPMToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, string language)
    {
        if (value == null)
        {
            return $" --- RPM";
        }
        return string.Format("{0:###0} RPM", (int)value);
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        var v = (value as string)![..^4];
        if (int.TryParse(v, out var i))
        {
            return i;
        }
        return null;
    }
}
