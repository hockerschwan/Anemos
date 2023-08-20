using Microsoft.UI.Xaml.Data;

namespace Anemos.Converters;

internal class PercentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null)
        {
            return $"--- %";
        }
        return string.Format("{0:##0.0}%", double.Round((double)value, 1));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
