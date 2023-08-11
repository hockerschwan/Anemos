using System.Diagnostics;
using Anemos.Activation;
using Anemos.Contracts.Services;
using Anemos.Services;
using Anemos.ViewModels;
using Anemos.Views;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

    public IHost Host
    {
        get;
    }

    public static bool HasShutdownStarted
    {
        get; private set;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    private readonly IMessenger _messenger;

    private readonly HashSet<Type> _servicesToShutDown = new();

    public App(string id) : base()
    {
        AppId = id;

        InitializeComponent();

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
                services.AddSingleton<SettingsPage>();
                services.AddSingleton<SettingsViewModel>();
            })
            .Build();

        UnhandledException += App_UnhandledException;

        _messenger = GetService<IMessenger>();
        _messenger.Register<ServiceStartupMessage>(this, ServiceStartupMessageHandler);
        _messenger.Register<ServiceShutDownMessage>(this, ServiceShutDownMessageHandler);
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Log.Fatal("[App] {0}\n{1}", e.Message, e.Exception);
        if (Debugger.IsAttached)
        {
            Debugger.Break();
        }
        Shutdown(true);
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

        await App.GetService<IActivationService>().ActivateAsync(args);

        var icon = GetService<INotifyIconService>();
        icon.SetTooltip(AppDomain.CurrentDomain.FriendlyName);
        icon.SetVisibility(true);
    }

    private void ServiceStartupMessageHandler(object recipient, ServiceStartupMessage message)
    {
        _servicesToShutDown.Add(message.Value);
    }

    private void ServiceShutDownMessageHandler(object recipient, ServiceShutDownMessage message)
    {
        if (_servicesToShutDown.Contains(message.Value))
        {
            _servicesToShutDown.Remove(message.Value);
            Log.Information("[App] Shut down: {0}", message.Value.Name);
        }
    }

    public static void ShowWindow()
    {
        if (MainWindow == null) { return; }

        MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            if (MainWindow.Visible)
            {
                MainWindow.BringToFront();
                return;
            }

            if (Helpers.RuntimeHelper.IsMinimized(MainWindow))
            {
                if (Helpers.RuntimeHelper.IsMaximized(MainWindow))
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
            // todo: show dialog
            if (!await GetService<ShellPage>().OpenExitDialog()) { return; }
        }

        MainWindow.DispatcherQueue.TryEnqueue(async () =>
        {
            Log.Information("[App] Shutting down...");
            HasShutdownStarted = true;
            _messenger.Send(new AppExitMessage());
            MainWindow.Hide();

            while (true)
            {
                if (!_servicesToShutDown.Any()) { break; }
                await Task.Delay(100);
            }

            Current.Exit();
        });
    }
}
