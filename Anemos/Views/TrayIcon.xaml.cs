using Anemos.Helpers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Anemos.Views;

public sealed partial class TrayIcon : UserControl
{
    public static string AppName => "AppDisplayName".GetLocalized();

    public TrayIcon()
    {
        InitializeComponent();
    }

    public void ShowWindowCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args)
    {
        App.ShowWindow();
    }

    public void ExitApplicationCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args)
    {
        TaskbarIcon.Dispose();
        App.Current.RequestShutdown();
    }
}
