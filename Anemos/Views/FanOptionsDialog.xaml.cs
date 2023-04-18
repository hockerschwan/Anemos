using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Windows.Globalization.NumberFormatting;

namespace Anemos.Views;

public sealed partial class FanOptionsDialog : ContentDialog
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();

    public FanOptionsViewModel ViewModel
    {
        get;
    }

    public FanOptionsDialog()
    {
        ViewModel = App.GetService<FanOptionsViewModel>();
        Opened += FanOptionsDialog_Opened;
        Closing += FanOptionsDialog_Closing;
        InitializeComponent();
    }

    private void FanOptionsDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        IncrementNumberRounder rounder = new()
        {
            RoundingAlgorithm = RoundingAlgorithm.RoundHalfUp,
            Increment = 1
        };
        DecimalFormatter formatter = new()
        {
            IntegerDigits = 1,
            FractionDigits = 0,
            NumberRounder = rounder
        };
        MaxSpeed.NumberFormatter = MinSpeed.NumberFormatter
            = DeltaLimitUp.NumberFormatter = DeltaLimitDown.NumberFormatter = formatter;
    }

    private void FanOptionsDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        _messenger.Send(new FanOptionsResultMessage(new()
        {
            MaxSpeed = ViewModel.MaxSpeed,
            MinSpeed = ViewModel.MinSpeed,
            DeltaLimitUp = ViewModel.DeltaLimitUp,
            DeltaLimitDown = ViewModel.DeltaLimitDown,
            RefractoryPeriodCyclesDown = ViewModel.RefractoryPeriodCyclesDown
        }));
    }
}
