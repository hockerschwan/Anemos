using Anemos.Helpers;
using Anemos.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class SensorsPage : Page
{
    public SensorsViewModel ViewModel
    {
        get;
    }

    public SensorsPage()
    {
        ViewModel = App.GetService<SensorsViewModel>();
        InitializeComponent();

        Loaded += SensorsPage_Loaded;
    }

    public static async Task<bool> OpenDeleteDialog(string name)
    {
        var dialog = new ContentDialog
        {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            PrimaryButtonStyle = Application.Current.Resources["DangerButtonStyle_"] as Style,
            Title = "Dialog_Delete_Title".GetLocalized(),
            PrimaryButtonText = "Dialog_Delete".GetLocalized(),
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "Dialog_Cancel".GetLocalized(),
            Content = "Dialog_Delete_Content".GetLocalized().Replace("$", name),
        };
        return await App.GetService<ShellPage>().OpenDialog(dialog);
    }

    public static async Task<bool> OpenSourceDialog(string id)
    {
        var dialog = new CustomSensorSourcesDialog(id)
        {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            PrimaryButtonStyle = Application.Current.Resources["AccentButtonStyle"] as Style,
            Title = "Select sources",
            PrimaryButtonText = "Dialog_OK".GetLocalized(),
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "Dialog_Cancel".GetLocalized(),
        };
        return await App.GetService<ShellPage>().OpenDialog(dialog);
    }

    private void SensorsPage_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= SensorsPage_Loaded;
        ViewModel.IsVisible = true;
    }
}
