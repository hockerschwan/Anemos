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
        Loaded += CurvesPage_Loaded;
        Unloaded += CurvesPage_Unloaded;
        InitializeComponent();
    }

    private void CurvesPage_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.IsVisible = true;
    }

    private void CurvesPage_Unloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.IsVisible = false;
    }
}
