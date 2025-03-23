using Anemos.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Navigation;

namespace Anemos.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly IMessenger _messenger;

    [ObservableProperty]
    public partial object? Selected
    {
        get; set;
    }

    public INavigationService NavigationService
    {
        get;
    }

    public INavigationViewService NavigationViewService
    {
        get;
    }

    private readonly MessageHandler<object, WindowVisibilityChangedMessage> _windowVisibilityChangedMessageHandler;

    public ShellViewModel(
        IMessenger messenger,
        INavigationService navigationService,
        INavigationViewService navigationViewService)
    {
        _messenger = messenger;
        _windowVisibilityChangedMessageHandler = WindowVisibilityChangedMessageHandler;
        _messenger.Register(this, handler: _windowVisibilityChangedMessageHandler);

        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }

    private void WindowVisibilityChangedMessageHandler(object recipient, WindowVisibilityChangedMessage message)
    {
        if (NavigationService.Frame == null) { return; }

        if (Helpers.FrameExtensions.GetPageViewModel(NavigationService.Frame) is PageViewModelBase pageVM)
        {
            pageVM.IsVisible = message.Value;
        }
    }
}
