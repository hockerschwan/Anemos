using Anemos.Activation;
using Anemos.Contracts.Services;
using Anemos.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Services;

public class ActivationService : IActivationService
{
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private readonly ILhwmService _lhwmService;
    private readonly ISensorService _sensorService;
    private readonly ICurveService _curveService;
    private readonly IFanService _fanService;
    private readonly IRuleService _ruleService;
    private UIElement? _shell = null;

    public ActivationService(
        ActivationHandler<LaunchActivatedEventArgs> defaultHandler,
        IEnumerable<IActivationHandler> activationHandlers,
        ILhwmService lhwmService,
        ISensorService sensorService,
        ICurveService curveService,
        IFanService fanService,
        IRuleService ruleService)
    {
        _defaultHandler = defaultHandler;
        _activationHandlers = activationHandlers;
        _lhwmService = lhwmService;
        _sensorService = sensorService;
        _curveService = curveService;
        _fanService = fanService;
        _ruleService = ruleService;
    }

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

    private async Task InitializeAsync()
    {
        await _lhwmService.InitializeAsync();
        await _sensorService.InitializeAsync();
        await _curveService.InitializeAsync();
        await _fanService.InitializeAsync();
        await _ruleService.InitializeAsync();
        await Task.CompletedTask;
    }

    private async Task StartupAsync()
    {
        await Task.CompletedTask;
    }
}
