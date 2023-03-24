using Anemos.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class CurveView : UserControl
{
    public CurveViewModel ViewModel
    {
        get;
    }

    public CurveView(CurveViewModel viewModel)
    {
        ViewModel = viewModel;
        Loaded += CurveView_Loaded;
        InitializeComponent();
    }

    private void CurveView_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= CurveView_Loaded;
        ViewModel.IsVisible = true;
    }
}
