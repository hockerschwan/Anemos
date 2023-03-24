using Microsoft.UI.Xaml.Data;

namespace Anemos.Converters;

public class TemperatureToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, string language)
    {
        if (value == null)
        {
            return $" --- ℃";
        }
        return string.Format("{0:##0.0} ℃", decimal.Round((decimal)value, 1));
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        var v = (value as string)![..^2];
        if (int.TryParse(v, out var i))
        {
            return i;
        }
        return null;
    }
}
