using Anemos.Helpers;
using Anemos.Models;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class FansPage : Page
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();

    public FansViewModel ViewModel
    {
        get;
    }

    internal static bool RenameDialogOpened
    {
        get; private set;
    }

    public FansPage()
    {
        _messenger.Register<FanProfileRenamedMessage>(this, FanProfileRenamedMessageHandler);

        ViewModel = App.GetService<FansViewModel>();
        InitializeComponent();

        Loaded += FansPage_Loaded;
    }

    private void FanProfileRenamedMessageHandler(object recipient, FanProfileRenamedMessage message)
    {
        if (!RenameDialogOpened || ViewModel.SelectedProfile == null || message.Value == string.Empty) { return; }

        RenameDialogOpened = false;
        ViewModel.SelectedProfile.Name = message.Value;
    }

    private void FansPage_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= FansPage_Loaded;
        ViewModel.IsVisible = true;
    }

    public static async Task<bool> OpenDeleteDialog(string name)
    {
        var dialog = new ContentDialog()
        {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            PrimaryButtonStyle = Application.Current.Resources["DangerButtonStyle_"] as Style,
            Title = "Dialog_DeleteFanProfile_Title".GetLocalized(),
            PrimaryButtonText = "Dialog_Delete".GetLocalized(),
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "Dialog_Cancel".GetLocalized(),
            Content = "Dialog_Delete_Content".GetLocalized().Replace("$", name),
        };
        return await App.GetService<ShellPage>().OpenDialog(dialog);
    }

    public static async Task<bool> OpenOptionsDialog(FanOptionsResult args)
    {
        var dialog = new FanOptionsDialog(args)
        {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            PrimaryButtonStyle = Application.Current.Resources["AccentButtonStyle"] as Style,
            Title = "FanOptions_Title".GetLocalized(),
            PrimaryButtonText = "Dialog_OK".GetLocalized(),
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "Dialog_Cancel".GetLocalized(),
        };
        return await App.GetService<ShellPage>().OpenDialog(dialog);
    }

    public static async Task<bool> OpenRenameDialog(string name)
    {
        var dialog = new FanProfileRenameDialog(name)
        {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            PrimaryButtonStyle = Application.Current.Resources["AccentButtonStyle"] as Style,
            Title = "FanProfileRename_Title".GetLocalized(),
            PrimaryButtonText = "Dialog_OK".GetLocalized(),
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "Dialog_Cancel".GetLocalized(),
        };
        return await App.GetService<ShellPage>().OpenDialog(dialog);
    }

    private async void ProfileRenameButton_Click(object sender, RoutedEventArgs e)
    {
        RenameDialogOpened = ViewModel.SelectedProfile != null && await OpenRenameDialog(ViewModel.SelectedProfile.Name);
    }

    private async void ProfileDeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedProfile != null && await OpenDeleteDialog(ViewModel.SelectedProfile.Name))
        {
            ViewModel.RemoveProfile(ViewModel.SelectedProfile.Id);
        }
    }
}
