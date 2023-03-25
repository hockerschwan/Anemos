using Anemos.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class FanView : UserControl
{
    public FanViewModel ViewModel
    {
        get;
    }

    public FanView(FanViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }
}
