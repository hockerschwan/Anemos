﻿using Anemos.Activation;
using Anemos.Contracts.Services;
using Anemos.Views;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Services;

public class ActivationService(
    ActivationHandler<LaunchActivatedEventArgs> defaultHandler,
    IEnumerable<IActivationHandler> activationHandlers) : IActivationService
{
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler = defaultHandler;
    private readonly IEnumerable<IActivationHandler> _activationHandlers = activationHandlers;
    private UIElement? _shell = null;

    public async Task ActivateAsync(object activationArgs)
    {
        // Execute tasks before activation.
        await InitializeAsync();

        // Set the MainWindow Content.
        if (App.MainWindow.Content == null)
        {
            _shell = App.GetService<ShellPage>();
            App.MainWindow.Content = _shell ?? new Frame();
        }

        // Handle activation via ActivationHandlers.
        await HandleActivationAsync(activationArgs);

        if (!App.GetService<ISettingsService>().Settings.StartMinimized)
        {
            // Activate the MainWindow.
            App.MainWindow.Activate();
        }

        // Execute tasks after activation.
        await StartupAsync();
    }

    private async Task HandleActivationAsync(object activationArgs)
    {
        var activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));

        if (activationHandler != null)
        {
            await activationHandler.HandleAsync(activationArgs);
        }

        if (_defaultHandler.CanHandle(activationArgs))
        {
            await _defaultHandler.HandleAsync(activationArgs);
        }
    }

    private static async Task InitializeAsync()
    {
        _ = App.GetService<IIpcService>();
        _ = App.GetService<ISettingsService>();
        _ = App.GetService<INotifyIconService>();
        await App.GetService<ISensorService>().LoadAsync();
        await App.GetService<ICurveService>().LoadAsync();
        await App.GetService<IFanService>().LoadAsync();
        await App.GetService<IRuleService>().LoadAsync();
        await App.GetService<INotifyIconMonitorService>().LoadAsync();

        App.GetService<IMessenger>().Send<ServiceStartupMessage>(new(null));
    }

    private static async Task StartupAsync()
    {
        App.GetService<ILhwmService>().Start();
        App.GetService<IRuleService>().Update();
        App.GetService<INotifyIconService>().Update();
        App.GetService<INotifyIconService>().SetVisibility(true);
        App.GetService<INotifyIconMonitorService>().Update();
        App.GetService<INotifyIconMonitorService>().SetVisibility(true);

        await Task.CompletedTask;
    }
}
