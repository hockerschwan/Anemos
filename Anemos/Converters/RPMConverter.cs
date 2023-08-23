using Anemos.Helpers;
using Microsoft.UI.Xaml.Data;

namespace Anemos.Converters;

internal class RPMConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null)
        {
            return $"--- {"Fan_RPM".GetLocalized()}";
        }
        return string.Format("{0:###0}{1}", value, "Fan_RPM".GetLocalized());
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
