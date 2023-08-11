using Anemos.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class RulesPage : Page
{
    public RulesViewModel ViewModel
    {
        get;
    }

    public RulesPage()
    {
        ViewModel = App.GetService<RulesViewModel>();
        InitializeComponent();
    }
}
