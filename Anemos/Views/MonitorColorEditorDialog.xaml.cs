using Anemos.Models;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml.Controls;
using Windows.Globalization.NumberFormatting;

namespace Anemos.Views;

public sealed partial class MonitorColorEditorDialog : ContentDialog
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();

    public MonitorColorEditorViewModel ViewModel
    {
        get;
    }

    public MonitorColorEditorDialog(MonitorColorThreshold color)
    {
        ViewModel = new(color);

        InitializeComponent();
        Loaded += MonitorColorEditorDialog_Loaded;
        Unloaded += MonitorColorEditorDialog_Unloaded;
        PrimaryButtonClick += MonitorColorEditorDialog_PrimaryButtonClick;

        ColorPicker.Color = color.SolidColorBrush.Color;

        SetNumberFormatter();
    }

    private void MonitorColorEditorDialog_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Loaded -= MonitorColorEditorDialog_Loaded;
        if (!double.IsFinite(ViewModel.Threshold))
        {
            NB_Threshold.IsEnabled = false;
            NB_Threshold.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
        ColorPicker.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }

    private async void MonitorColorEditorDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        while (IsLoaded)
        {
            await Task.Delay(100);
        }
        await Task.Delay(100);

        _messenger.Send<MonitorColorChangedMessage>(new(new(
             ViewModel.OldColor,
             new()
             {
                 Threshold = ViewModel.Threshold,
                 Color = System.Drawing.Color.FromArgb(ColorPicker.Color.ToInt())
             })));
    }

    private void MonitorColorEditorDialog_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Unloaded -= MonitorColorEditorDialog_Unloaded;
        PrimaryButtonClick -= MonitorColorEditorDialog_PrimaryButtonClick;

        Bindings.StopTracking();
    }

    private void PreviewKeyDown_(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            e.Handled = true;

            if (sender is NumberBox nb)
            {
                var be = nb.GetBindingExpression(NumberBox.ValueProperty);
                be?.UpdateSource();
            }
        }
    }

    private void SetNumberFormatter()
    {
        if (!double.IsFinite(ViewModel.Threshold)) { return; }

        IncrementNumberRounder rounder = new()
        {
            Increment = 0.1,
            RoundingAlgorithm = RoundingAlgorithm.RoundHalfUp
        };

        DecimalFormatter formatter = new()
        {
            FractionDigits = 1,
            NumberRounder = rounder
        };

        NB_Threshold.NumberFormatter = formatter;
    }
}
