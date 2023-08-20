using Anemos.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class CurvesPage : Page
{
    public CurvesViewModel ViewModel
    {
        get;
    }

    public CurvesPage()
    {
        ViewModel = App.GetService<CurvesViewModel>();
        InitializeComponent();

        Loaded += CurvesPage_Loaded;
    }

    private void CurvesPage_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= CurvesPage_Loaded;
        ViewModel.IsVisible = true;
    }

    public static async Task<bool> OpenDeleteDialog(string name)
    {
        var dialog = new ContentDialog
        {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            PrimaryButtonStyle = Application.Current.Resources["DangerButtonStyle_"] as Style,
            Title = "Delete Curve",
            PrimaryButtonText = "Delete",
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "Cancel",
            Content = $"Are you sure to delete {name}?",
        };
        return await App.GetService<ShellPage>().OpenDialog(dialog);
    }
}
