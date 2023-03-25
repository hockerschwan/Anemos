using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class ColorPickerDialog : ContentDialog
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();

    private readonly System.Timers.Timer _timer = new(500);

    public ColorPickerDialog()
    {
        Loaded += ColorPickerDialog_Loaded;
        Closing += ColorPickerDialog_Closing;
        InitializeComponent();

        _timer.AutoReset = false;
        _timer.Elapsed += Timer_Elapsed;
    }

    private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
        });
    }

    private void ColorPickerDialog_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        _timer.Start();
    }

    private void ColorPickerDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        _messenger.Send(new ColorPickerResultMessage(Picker.Color));
    }

    public void SetColor(Windows.UI.Color color)
    {
        Picker.Color = color;
        Picker.InvalidateArrange();
    }
}
