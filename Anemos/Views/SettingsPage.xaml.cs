using Anemos.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel = App.GetService<SettingsViewModel>();

    public SettingsPage()
    {
        InitializeComponent();
    }
}
