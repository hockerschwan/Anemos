using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Windows.Globalization.NumberFormatting;

namespace Anemos.Views;

public sealed partial class RuleSensorEditorDialog : ContentDialog
{
    public RuleSensorEditorViewModel ViewModel
    {
        get;
    }

    private readonly int _index;

    public RuleSensorEditorDialog(
        int index,
        string sensorId,
        double? lower,
        bool includeLower,
        double? upper,
        bool includeUpper)
    {
        _index = index;
        ViewModel = new RuleSensorEditorViewModel()
        {
            SensorId = sensorId,
            LowerValue = lower ?? double.NaN,
            IndexIncludeLower = includeLower ? 1 : 0,
            UpperValue = upper ?? double.NaN,
            IndexIncludeUpper = includeUpper ? 1 : 0
        };

        InitializeComponent();
        Loaded += RuleSensorEditorDialog_Loaded;
        Unloaded += RuleSensorEditorDialog_Unloaded;
        PrimaryButtonClick += RuleSensorEditorDialog_PrimaryButtonClick;

        SetNumberFormatter();
    }

    private void RuleSensorEditorDialog_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Loaded -= RuleSensorEditorDialog_Loaded;

        CB_Source.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }

    private void RuleSensorEditorDialog_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Unloaded -= RuleSensorEditorDialog_Unloaded;
        PrimaryButtonClick -= RuleSensorEditorDialog_PrimaryButtonClick;

        Bindings.StopTracking();
    }

    private void RuleSensorEditorDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
        {
            while (IsLoaded)
            {
                await Task.Delay(100);
            }

            App.GetService<IMessenger>().Send<RuleSensorChangedMessage>(new(new(
                _index,
                ViewModel.SensorId,
                double.IsNaN(ViewModel.LowerValue) ? null : ViewModel.LowerValue,
                ViewModel.IncludeLower,
                double.IsNaN(ViewModel.UpperValue) ? null : ViewModel.UpperValue,
                ViewModel.IncludeUpper)));
        });
    }

    private void SetNumberFormatter()
    {
        IncrementNumberRounder rounder = new()
        {
            Increment = 1,
            RoundingAlgorithm = RoundingAlgorithm.RoundHalfUp
        };

        DecimalFormatter formatter = new()
        {
            FractionDigits = 0,
            NumberRounder = rounder
        };

        NB_Lower.NumberFormatter = NB_Upper.NumberFormatter = formatter;
    }
}
