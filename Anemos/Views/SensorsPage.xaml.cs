using Anemos.ViewModels;
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
    }
}
