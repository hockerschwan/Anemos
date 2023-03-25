using Anemos.Helpers;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Anemos.Views;

public sealed partial class SettingsPage : Page
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();

    private Windows.UI.Color _color;

    private bool _isDialogShown = false;

    public SettingsViewModel ViewModel = App.GetService<SettingsViewModel>();

    public SettingsPage()
    {
        _messenger.Register<ColorPickerResultMessage>(this, ColorPickerResultMessageHandler);

        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }

    private void ColorPickerResultMessageHandler(object recipient, ColorPickerResultMessage message)
    {
        _color = message.Value;
    }

    private void ColorBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case Windows.System.VirtualKey.Enter:
            case Windows.System.VirtualKey.Space:
                break;
            default: return;
        }

        var border = sender as Border;
        if (border == null) { return; }

        OpenColorPicker(border);
    }

    private void ColorBox_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var border = sender as Border;
        if (border == null) { return; }

        if (!e.GetCurrentPoint(border).Properties.IsLeftButtonPressed) { return; }

        border.Focus(FocusState.Pointer);
        OpenColorPicker(border);
    }

    private async void OpenColorPicker(Border sender)
    {
        if (_isDialogShown) { return; }

        var name = sender.Name[..^3];
        var propInfo = ViewModel.GetType().GetProperty(name);
        if (propInfo == null) { return; }

        var brush = (SolidColorBrush?)propInfo.GetValue(ViewModel);
        if (brush == null) { return; }

        ViewModel.ColorPicker.SetColor(brush.Color);

        SetDialogStyle();

        _isDialogShown = true;
        var result = await ViewModel.ColorPicker.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            propInfo.SetValue(ViewModel, new SolidColorBrush(_color));
        }

        _isDialogShown = false;
    }

    private void SetDialogStyle()
    {
        var dialog = ViewModel.ColorPicker;
        dialog.XamlRoot = XamlRoot;
        dialog.Style = App.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.PrimaryButtonText = "Dialog_OK".GetLocalized();
        dialog.CloseButtonText = "Dialog_Cancel".GetLocalized();
        dialog.DefaultButton = ContentDialogButton.Primary;
    }
}
