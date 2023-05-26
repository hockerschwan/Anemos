using Anemos.Models;
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
            Type = RuleConditionType.Process,
            ProcessName = NameTextBox.Text,
            MemoryType = MemoryType.SelectedIndex,
            MemoryLower = (int?)MemorySizeLow.Value,
            MemoryUpper = (int?)MemorySizeHigh.Value,
        }));
    }

    public void Set(ProcessRuleCondition cond)
    {
        NameTextBox.Text = cond.ProcessName ?? string.Empty;
        MemoryType.SelectedIndex = cond.MemoryType;

        if (cond.MemoryLower > 0)
        {
            MemorySizeLow.Value = (double)cond.MemoryLower;
        }
        else
        {
            MemorySizeLow.Text = string.Empty;
        }

        if (cond.MemoryUpper > 0)
        {
            MemorySizeHigh.Value = (double)cond.MemoryUpper;
        }
        else
        {
            MemorySizeHigh.Text = string.Empty;
        }

        NameTextBox.SelectAll();
    }
}
