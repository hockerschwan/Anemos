using Anemos.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class FansPage : Page
{
    public FansViewModel ViewModel
    {
        get;
    }

    public FansPage()
    {
        ViewModel = App.GetService<FansViewModel>();
        InitializeComponent();
    }
}
