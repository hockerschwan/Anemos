using Anemos.Helpers;
using Anemos.Models;
using Anemos.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class MonitorsPage : Page
{
    public MonitorsViewModel ViewModel
    {
        get;
    }

    public MonitorsPage()
    {
        ViewModel = App.GetService<MonitorsViewModel>();
        InitializeComponent();

        Loaded += MonitorsPage_Loaded;
    }

    private void MonitorsPage_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= MonitorsPage_Loaded;
        ViewModel.IsVisible = true;
    }

    public static async Task<bool> OpenDeleteDialog()
    {
        var dialog = new ContentDialog
        {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            PrimaryButtonStyle = Application.Current.Resources["DangerButtonStyle_"] as Style,
            Title = "Dialog_DeleteMonitor_Title".GetLocalized(),
            PrimaryButtonText = "Dialog_Delete".GetLocalized(),
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "Dialog_Cancel".GetLocalized(),
            Content = "Dialog_Delete2_Content".GetLocalized(),
        };
        return await App.GetService<ShellPage>().OpenDialog(dialog);
    }

    public static async Task<bool> OpenEditor(MonitorColorThreshold color)
    {
        var dialog = new MonitorColorEditorDialog(color)
        {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            PrimaryButtonStyle = Application.Current.Resources["AccentButtonStyle"] as Style,
            Title = "MonitorColorEditor_Title".GetLocalized(),
            PrimaryButtonText = "Dialog_OK".GetLocalized(),
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "Dialog_Cancel".GetLocalized(),
        };
        return await App.GetService<ShellPage>().OpenDialog(dialog);
    }
}
