using Anemos.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class CustomSensorView : UserControl
{
    public CustomSensorViewModel ViewModel
    {
        get;
    }

    public CustomSensorView(CustomSensorViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    private void DeleteSourceClicked(object sender, RoutedEventArgs e)
    {
        ViewModel.RemoveSourceCommand.Execute((sender as Button)?.CommandParameter);
    }

    private void CloseFlyout(object sender, RoutedEventArgs e)
    {
        ViewModel.AddSourceCommand.Execute((sender as Button)?.CommandParameter);
        AddSourceFlyout.Hide();
    }
}
