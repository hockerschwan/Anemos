using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
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
        Closing += RuleSensorEditorDialog_Closing;
        InitializeComponent();
    }

    private async void RuleSensorEditorDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        await Task.Delay(10);
        _messenger.Send(new RuleEditorResultMessage(new()
        {
            Type = Models.RuleConditionType.Sensor,
            SensorId = ViewModel.SensorId,
            LowerValue = double.IsNaN(ViewModel.LowerValue) ? null : ViewModel.LowerValue,
            UpperValue = double.IsNaN(ViewModel.UpperValue) ? null : ViewModel.UpperValue,
            IncludeLower = ViewModel.IncludeLower,
            IncludeUpper = ViewModel.IncludeUpper
        }));
        await Task.Delay(100);
    }
}
