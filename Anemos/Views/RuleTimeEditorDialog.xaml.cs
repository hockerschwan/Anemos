using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class RuleTimeEditorDialog : ContentDialog
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();

    public RuleTimeEditorDialog()
    {
        Closing += RuleTimeEditorDialog_Closing;
        InitializeComponent();
    }

    private void RuleTimeEditorDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        _messenger.Send(new RuleEditorResultMessage(new()
        {
            Type = Models.RuleConditionType.Time,
            TimeBeginning = TimeOnly.FromTimeSpan(TimePickerBeginning.Time),
            TimeEnding = TimeOnly.FromTimeSpan(TimePickerEnding.Time)
        }));
    }

    public void SetTime(TimeSpan begin, TimeSpan end)
    {
        TimePickerBeginning.Time = begin;
        TimePickerEnding.Time = end;
    }
}
