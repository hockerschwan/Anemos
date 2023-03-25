using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class FanProfileNameEditorDialog : ContentDialog
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();

    public FanProfileNameEditorDialog()
    {
        Opened += FanProfileNameEditorDialog_Opened;
        Closing += FanProfileNameEditorDialog_Closing;
        InitializeComponent();
    }

    public void SetText(string name)
    {
        NewName.Text = name;
    }

    private void FanProfileNameEditorDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        NewName.Select(NewName.Text.Length, 0);
    }

    private void FanProfileNameEditorDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        _messenger.Send(new FanProfileNameEditorResultMessage(NewName.Text));
    }
}
