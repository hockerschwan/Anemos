using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class RuleSensorEditorDialog : ContentDialog
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();

    public RuleSensorEditorViewModel ViewModel
    {
        get;
    }

    public RuleSensorEditorDialog()
    {
        ViewModel = App.GetService<RuleSensorEditorViewModel>();
        Opened += RuleSensorEditorDialog_Opened;
        Closing += RuleSensorEditorDialog_Closing;
        InitializeComponent();
    }

    private void RuleSensorEditorDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        SetButtonState();
    }

    private void RuleSensorEditorDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        _messenger.Send(new RuleEditorResultMessage(new()
        {
            Type = Models.RuleConditionType.Sensor,
            SensorId = ViewModel.SensorId,
            LowerValue = ViewModel.UseLower ? ViewModel.LowerValue : null,
            UpperValue = ViewModel.UseUpper ? ViewModel.UpperValue : null,
            UseLowerValue = ViewModel.UseLower,
            UseUpperValue = ViewModel.UseUpper,
            IncludeLower = ViewModel.IncludeLower,
            IncludeUpper = ViewModel.IncludeUpper
        }));
    }

    private void CheckBox_Checked(object sender, RoutedEventArgs e)
    {
        SetButtonState();
    }

    private void SetButtonState()
    {
        if (!CheckBoxUseLower.IsChecked!.Value && !CheckBoxUseUpper.IsChecked!.Value)
        {
            IsPrimaryButtonEnabled = false;
        }
        else
        {
            IsPrimaryButtonEnabled = true;
        }
    }
}
