﻿using Microsoft.UI.Xaml.Data;

namespace Anemos.Converters;

public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return !(bool)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return !(bool)value;
    }
}
