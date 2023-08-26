using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Windows.Globalization.NumberFormatting;

namespace Anemos.Views;

public sealed partial class FanOptionsDialog : ContentDialog
{
    public FanOptionsDialog(int min, int max, int deltaUp, int deltaDown, int holdCycleDown)
    {
        InitializeComponent();
        Loaded += FanOptionsDialog_Loaded;
        Unloaded += FanOptionsDialog_Unloaded;
        PrimaryButtonClick += FanOptionsDialog_PrimaryButtonClick;

        SetNumberFormatter();

        NB_Min.Value = min;
        NB_Max.Value = max;
        NB_DeltaUp.Value = deltaUp;
        NB_DeltaDown.Value = deltaDown;
        NB_HoldCycleDown.Value = holdCycleDown;
    }

    private void FanOptionsDialog_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Loaded -= FanOptionsDialog_Loaded;

        NB_Min.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }

    private void FanOptionsDialog_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Unloaded -= FanOptionsDialog_Unloaded;
        PrimaryButtonClick -= FanOptionsDialog_PrimaryButtonClick;
    }

    private void FanOptionsDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
        {
            await Task.Delay(100);
            App.GetService<IMessenger>().Send<FanOptionsChangedMessage>(new(new()
            {
                MinSpeed = (int)NB_Min.Value,
                MaxSpeed = (int)NB_Max.Value,
                DeltaLimitUp = (int)NB_DeltaUp.Value,
                DeltaLimitDown = (int)NB_DeltaDown.Value,
                RefractoryPeriodCyclesDown = (int)NB_HoldCycleDown.Value
            }));
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

        NB_Min.NumberFormatter = NB_Max.NumberFormatter
            = NB_DeltaUp.NumberFormatter = NB_DeltaDown.NumberFormatter = NB_HoldCycleDown.NumberFormatter = formatter;
    }
}
