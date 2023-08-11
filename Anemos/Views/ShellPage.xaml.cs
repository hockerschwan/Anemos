using Anemos.Helpers;
using Anemos.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Anemos.Views;

public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel
    {
        get;
    }

    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        ViewModel.NavigationService.Frame = NavigationFrame;
        ViewModel.NavigationViewService.Initialize(NavigationViewControl);

        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        App.MainWindow.Activated += MainWindow_Activated;
        AppTitleBarText.Text = "AppDisplayName".GetLocalized();
    }

    private void ExitButton_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        App.Current.Shutdown();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);

        // Hide scrollbar of FooterMenuItems
        DispatcherQueue.TryEnqueue(async () =>
        {
            var w = NavigationViewControl.Width + 0;
            NavigationViewControl.Width = 0;
            await Task.Delay(100);
            NavigationViewControl.Width = w;
        });
    }

    public async Task<bool> OpenExitDialog()
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            PrimaryButtonStyle = Application.Current.Resources["AccentButtonStyle"] as Style,
            Title = "Exit",
            PrimaryButtonText = "OK",
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "Cancel",
            Content = "Are you sure?",
            DefaultButton = ContentDialogButton.Primary,
        };

        dialog.Resources["AccentButtonBackground"] = "FireBrick";
        dialog.Resources["AccentButtonForeground"] = App.Current.Resources["TextFillColorPrimaryBrush"];
        dialog.Resources["AccentButtonBackgroundPointerOver"] = "Crimson";
        dialog.Resources["AccentButtonForegroundPointerOver"] = App.Current.Resources["TextFillColorPrimaryBrush"];
        dialog.Resources["AccentButtonBackgroundPressed"] = "DarkRed";
        dialog.Resources["AccentButtonForegroundPressed"] = App.Current.Resources["TextFillColorPrimaryBrush"];

        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState
            == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";

        AppTitleBarText.Foreground = (SolidColorBrush)Application.Current.Resources[resource];
        App.AppTitlebar = AppTitleBarText;
    }
}
