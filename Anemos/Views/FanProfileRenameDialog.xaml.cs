using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class FanProfileRenameDialog : ContentDialog
{
    public FanProfileRenameDialog(string oldName)
    {
        InitializeComponent();
        Loaded += FanProfileRenameDialog_Loaded;
        Closing += FanProfileRenameDialog_Closing;

        TB_Name.Text = oldName;
    }

    private void FanProfileRenameDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        Closing -= FanProfileRenameDialog_Closing;

        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
        {
            await Task.Delay(100);
            App.GetService<IMessenger>().Send<FanProfileRenamedMessage>(new(TB_Name.Text));
        });
    }

    private void FanProfileRenameDialog_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Loaded -= FanProfileRenameDialog_Loaded;

        TB_Name.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
        TB_Name.SelectionStart = TB_Name.Text.Length;
    }
}
