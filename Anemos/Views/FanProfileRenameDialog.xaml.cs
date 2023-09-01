using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class FanProfileRenameDialog : ContentDialog
{
    public FanProfileRenameDialog(string oldName)
    {
        InitializeComponent();
        Loaded += FanProfileRenameDialog_Loaded;
        Unloaded += FanProfileRenameDialog_Unloaded;
        PrimaryButtonClick += FanProfileRenameDialog_PrimaryButtonClick;

        TB_Name.Text = oldName;
    }

    private void FanProfileRenameDialog_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Loaded -= FanProfileRenameDialog_Loaded;

        TB_Name.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
        TB_Name.SelectionStart = TB_Name.Text.Length;
    }

    private void FanProfileRenameDialog_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Unloaded -= FanProfileRenameDialog_Unloaded;
        PrimaryButtonClick -= FanProfileRenameDialog_PrimaryButtonClick;
    }

    private void FanProfileRenameDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
        {
            while (IsLoaded || !FansPage.RenameDialogOpened)
            {
                await Task.Delay(100);
            }

            App.GetService<IMessenger>().Send<FanProfileRenamedMessage>(new(TB_Name.Text));
        });
    }
}
