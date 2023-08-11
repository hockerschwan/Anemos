using Anemos.ViewModels;
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
    }
}
