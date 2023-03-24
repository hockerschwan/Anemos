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
        Loaded += SensorsPage_Loaded;
        Unloaded += SensorsPage_Unloaded;
        InitializeComponent();
    }

    private void SensorsPage_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.IsVisible = true;
    }

    private void SensorsPage_Unloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.IsVisible = false;
    }
}
