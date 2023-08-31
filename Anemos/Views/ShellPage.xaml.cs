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

    private ContentDialog? _dialog;

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

    private void ExitButton_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case Windows.System.VirtualKey.Enter:
            case Windows.System.VirtualKey.Space:
                App.Current.Shutdown();
                break;
        }
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
            await Task.Delay(200);
            NavigationViewControl.Width = w;
        });
    }

    public async Task<bool> OpenExitDialog()
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            PrimaryButtonStyle = Application.Current.Resources["DangerButtonStyle_"] as Style,
            Title = "Dialog_Exit".GetLocalized(),
            PrimaryButtonText = "Dialog_Exit".GetLocalized(),
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "Dialog_Cancel".GetLocalized(),
            Content = "Dialog_Exit_Content".GetLocalized(),
        };
        return await OpenDialog(dialog);
    }

    public async Task<bool> OpenDialog(ContentDialog dialog)
    {
        if (_dialog != null)
        {
            _dialog.Hide();
            while (VisualTreeHelper.GetOpenPopupsForXamlRoot(XamlRoot).Any())
            {
                await Task.Delay(50);
            }
        }

        _dialog = dialog;
        _dialog.XamlRoot = XamlRoot;
        var result = await _dialog.ShowAsync();
        _dialog = null;
        return result == ContentDialogResult.Primary;
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState
            == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";

        AppTitleBarText.Foreground = (SolidColorBrush)Application.Current.Resources[resource];
        App.AppTitlebar = AppTitleBarText;
    }
}
