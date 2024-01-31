using System.Diagnostics;
using Anemos.Activation;
using Anemos.Contracts.Services;
using Anemos.Services;
using Anemos.ViewModels;
using Anemos.Views;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Serilog;

namespace Anemos;

public partial class App : Application
{
    public readonly string AppId;

    public static string AppLocation => Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        $"{AppDomain.CurrentDomain.FriendlyName}.exe");

    public static UIElement? AppTitlebar
    {
        get; set;
    }

    public static new App Current => (App)Application.Current;

    public static DispatcherQueue DispatcherQueue { get; } = DispatcherQueue.GetForCurrentThread();

    public IHost Host
    {
        get;
    }

    public static bool ShutdownStarted
    {
        get; private set;
    }

    public static MainWindow MainWindow { get; } = new();

    private readonly IMessenger _messenger = WeakReferenceMessenger.Default;

    private readonly HashSet<Type> _servicesToShutDown = [];

    private readonly MessageHandler<App, ServiceStartupMessage> _serviceStartupMessageHandler;
    private readonly MessageHandler<App, ServiceShutDownMessage> _serviceShutDownMessageHandler;
    private readonly DispatcherQueueHandler _mainwindowCloseHandler;

    public App(string id) : base()
    {
        AppId = id;

        InitializeComponent();

        _serviceStartupMessageHandler = ServiceStartupMessageHandler;
        _serviceShutDownMessageHandler = ServiceShutDownMessageHandler;
        _messenger.Register(this, _serviceStartupMessageHandler);
        _messenger.Register(this, _serviceShutDownMessageHandler);

        Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices((context, services) =>
            {
                // Default Activation Handler
                services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

                // Services
                services.AddTransient<INavigationViewService, NavigationViewService>();

                services.AddSingleton<IActivationService, ActivationService>();
                services.AddSingleton<IPageService, PageService>();
                services.AddSingleton<INavigationService, NavigationService>();

                services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
                services.AddSingleton<IIpcService, IpcService>();
                services.AddSingleton<INotifyIconService, NotifyIconService>();
                services.AddSingleton<ISettingsService, SettingsService>();
                services.AddSingleton<ILhwmService, LhwmService>();
                services.AddSingleton<ISensorService, SensorService>();
                services.AddSingleton<ICurveService, CurveService>();
                services.AddSingleton<IFanService, FanService>();
                services.AddSingleton<IRuleService, RuleService>();
                services.AddSingleton<INotifyIconMonitorService, NotifyIconMonitorService>();

                // Views and ViewModels
                services.AddSingleton<ShellPage>();
                services.AddSingleton<ShellViewModel>();
                services.AddSingleton<FansPage>();
                services.AddSingleton<FansViewModel>();
                services.AddSingleton<CurvesPage>();
                services.AddSingleton<CurvesViewModel>();
                services.AddSingleton<SensorsPage>();
                services.AddSingleton<SensorsViewModel>();
                services.AddSingleton<RulesPage>();
                services.AddSingleton<RulesViewModel>();
                services.AddSingleton<MonitorsPage>();
                services.AddSingleton<MonitorsViewModel>();
                services.AddSingleton<SettingsPage>();
                services.AddSingleton<SettingsViewModel>();
            })
            .Build();

        UnhandledException += App_UnhandledException;

        _mainwindowCloseHandler = MainWindow.Close;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        if (e.Exception.Source == "SkiaSharp.Views.Windows"
            && e.Exception.TargetSite?.DeclaringType?.Name == "SKXamlCanvas"
            && e.Exception.TargetSite?.Name == "OnUnloaded"
            && e.Exception.HResult == -2147467261)
        {
            e.Handled = true;
            return;
        }

        Log.Fatal("[App] {0}\n{1}", e.Message, e.Exception);
        if (Debugger.IsAttached)
        {
            Debugger.Break();
        }
        if (!ShutdownStarted)
        {
            Shutdown(true);
        }
    }

    public static T GetService<T>() where T : class
    {
        if (Current!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await GetService<IActivationService>().ActivateAsync(args);
    }

    private void ServiceStartupMessageHandler(App recipient, ServiceStartupMessage message)
    {
        if (message.Value == null)
        {
            _messenger.Unregister<ServiceStartupMessage>(this);
            return;
        }

        _servicesToShutDown.Add(message.Value);
    }

    private void ServiceShutDownMessageHandler(App recipient, ServiceShutDownMessage message)
    {
        if (message.Value is Type type && _servicesToShutDown.Contains(type))
        {
            _servicesToShutDown.Remove(type);
            Log.Information("[App] Shut down: {0}", type.Name);
        }
    }

    public static void ShowWindow()
    {
        if (MainWindow == null) { return; }

        DispatcherQueue.TryEnqueue(() =>
        {
            if (MainWindow.Visible)
            {
                MainWindow.BringToFront();
                return;
            }

            if (MainWindow.IsMinimized)
            {
                if (MainWindow.IsMaximized)
                {
                    MainWindow.Maximize();
                }
                else
                {
                    MainWindow.Restore();
                    MainWindow.BringToFront();
                }
                return;
            }

            MainWindow.Minimize();
            MainWindow.Restore();
        });
    }

    public async void Shutdown(bool forceShutdown = false)
    {
        if (!forceShutdown)
        {
            var result = await DispatcherQueueExtensions.EnqueueAsync(
                DispatcherQueue,
                async () => await GetService<ShellPage>().OpenExitDialog(),
                DispatcherQueuePriority.High);
            if (!result) { return; }
        }

        Log.Information("[App] Shutting down...");
        ShutdownStarted = true;
        _messenger.Send(new AppExitMessage());
        DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, _mainwindowCloseHandler);

        while (true)
        {
            if (_servicesToShutDown.Count == 0) { break; }
            await Task.Delay(100);
        }

        Current?.Exit();
    }
}
