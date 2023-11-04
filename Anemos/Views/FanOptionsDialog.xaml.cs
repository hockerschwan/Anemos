using Anemos.Models;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Windows.Globalization.NumberFormatting;

namespace Anemos.Views;

public sealed partial class FanOptionsDialog : ContentDialog
{
    public FanOptionsDialog(FanOptionsResult args)
    {
        InitializeComponent();
        Loaded += FanOptionsDialog_Loaded;
        Unloaded += FanOptionsDialog_Unloaded;
        PrimaryButtonClick += FanOptionsDialog_PrimaryButtonClick;

        SetNumberFormatter();

        NB_Min.Value = args.MinSpeed;
        NB_Max.Value = args.MaxSpeed;
        NB_DeltaUp.Value = args.DeltaLimitUp;
        NB_DeltaDown.Value = args.DeltaLimitDown;
        NB_HoldCycleDown.Value = args.RefractoryPeriodCyclesDown;
        NB_Offset.Value = args.Offset;
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
            while (IsLoaded)
            {
                await Task.Delay(100);
            }
            await Task.Delay(100);

            App.GetService<IMessenger>().Send<FanOptionsChangedMessage>(new(new()
            {
                MinSpeed = (int)NB_Min.Value,
                MaxSpeed = (int)NB_Max.Value,
                DeltaLimitUp = (int)NB_DeltaUp.Value,
                DeltaLimitDown = (int)NB_DeltaDown.Value,
                RefractoryPeriodCyclesDown = (int)NB_HoldCycleDown.Value,
                Offset = (int)NB_Offset.Value
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
            = NB_DeltaUp.NumberFormatter = NB_DeltaDown.NumberFormatter
            = NB_HoldCycleDown.NumberFormatter = NB_Offset.NumberFormatter = formatter;
    }
}
