using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class RuleProcessEditorDialog : ContentDialog
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();

    public RuleProcessEditorDialog()
    {
        Closing += RuleProcessEditorDialog_Closing;
        InitializeComponent();
    }

    private void RuleProcessEditorDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        _messenger.Send(new RuleEditorResultMessage(new()
        {
            Type = Models.RuleConditionType.Process,
            ProcessName = NameTextBox.Text
        }));
    }

    public void SetText(string str)
    {
        NameTextBox.Text = str;
        NameTextBox.SelectAll();
    }
}
