using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.TablerIcon;

public sealed class TablerIcon : FontIcon
{
    public static readonly DependencyProperty SymbolProperty = DependencyProperty.Register(
        nameof(Symbol),
        typeof(TablerIconGlyph),
        typeof(TablerIconGlyph),
        new PropertyMetadata(default, new PropertyChangedCallback(OnSymbolChanged)));

    public TablerIcon()
    {
        FontFamily = Application.Current.Resources["TablerIcons_"] as Microsoft.UI.Xaml.Media.FontFamily;
    }

    public TablerIconGlyph Symbol
    {
        get => (TablerIconGlyph)GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    private static void OnSymbolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is TablerIconGlyph symbol)
        {
            var instance = d as TablerIcon;
            if (instance != null)
            {
                instance.Glyph = ((char)symbol).ToString();
            }
        }
    }
}

public enum TablerIconGlyph
{
    ChartLine = '\uea5c',
    ChevronDown = '\uea5f',
    ChevronUp = '\uea62',
    CircleCheck = '\uea67',
    CircleX = '\uf2b0',
    Clock = '\uea70',
    Copy = '\uea7a',
    CPU = '\uef8e',
    Dots = '\uea95',
    Folder = '\ueaad',
    GitHub = '\uec1c',
    HeartRateMonitor = '\uef61',
    List = '\ueb6b',
    Minus = '\ueaf2',
    Pencil = '\ueb04',
    Plus = '\ueb0b',
    Repeat = '\ueb72',
    SettingsFilled = '\uf69e',
    Temperature = '\ueb38',
    TrashFilled = '\uf783',
    Wind = '\uec34',
    X = '\ueb55'
}
