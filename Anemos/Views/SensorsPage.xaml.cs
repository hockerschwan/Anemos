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
            Title = "Delete Sensor",
            PrimaryButtonText = "Delete",
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "Cancel",
            Content = $"Are you sure to delete {name}?",
        };
        return await App.GetService<ShellPage>().OpenDialog(dialog);
    }

    private void SensorsPage_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= SensorsPage_Loaded;
        ViewModel.IsVisible = true;
    }
}
