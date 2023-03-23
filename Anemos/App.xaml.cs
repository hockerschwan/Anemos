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
    public IHost Host
    {
        get;
    }

    public static T GetService<T>() where T : class
    {
        if (Current!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static new App Current => (App)Application.Current;

    public readonly string AppName = AppDomain.CurrentDomain.FriendlyName;

    public string SettingsFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), // AppData/Local
            AppName);

    public static MainWindow MainWindow { get; } = new MainWindow();

    private readonly IMessenger _messenger = WeakReferenceMessenger.Default;

    public static bool HasShutdownRequested
    {
        get; private set;
    }

    private readonly List<Type> _servicesToShutDown = new();

    public App()
    {
        InitializeComponent();

        CheckLocalFolder();

        var settingsService = new SettingsService(SettingsFolder);
        var lhwmService = new LhwmService(settingsService);
        var sensorService = new SensorService(settingsService, lhwmService);

        Host = Microsoft.Extensions.Hosting.Host.
            CreateDefaultBuilder().
            UseContentRoot(AppContext.BaseDirectory).
            ConfigureServices((context, services) =>
            {
                // Default Activation Handler
                services.AddSingleton<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

                // Services
                services.AddSingleton<INavigationViewService, NavigationViewService>();

                services.AddSingleton<IActivationService, ActivationService>();
                services.AddSingleton<IPageService, PageService>();
                services.AddSingleton<INavigationService, NavigationService>();

                services.AddSingleton(_messenger);
                services.AddSingleton<ISettingsService>(settingsService);
                services.AddSingleton<ILhwmService>(lhwmService);
                services.AddSingleton<ISensorService>(sensorService);

                // Views and ViewModels
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<MainPage>();
                services.AddSingleton<SettingsViewModel>();
                services.AddSingleton<SettingsPage>();
                services.AddSingleton<ShellPage>();
                services.AddSingleton<ShellViewModel>();

                // Configuration
            }).
            Build();

        UnhandledException += App_UnhandledException;

        _servicesToShutDown.AddRange(new Type[]
        {
            typeof(SettingsService),
            typeof(LhwmService),
        });
        _messenger.Register<ServiceShutDownMessage>(this, ServiceShutDownMessageHandler);
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Log.Fatal("[App] {0}\n{1}", e.Message, e.Exception);
        RequestShutdown();
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await GetService<ISettingsService>().LoadAsync();
        await GetService<IActivationService>().ActivateAsync(args);
    }

    private void CheckLocalFolder()
    {
        if (!Directory.Exists(SettingsFolder))
        {
            Directory.CreateDirectory(SettingsFolder);
        }
    }

    public static void OnIpcMessageReceived(string message)
    {
        Log.Information("[App] IPC message: {message}", message);
        if (message == "ACTIVATE")
        {
            ShowWindow();
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

    public async void RequestShutdown()
    {
        Log.Information("[App] Shut down requested");
        HasShutdownRequested = true;
        _messenger.Send(new AppExitMessage());
        MainWindow.Close();

        while (true)
        {
            if (!_servicesToShutDown.Any())
            {
                break;
            }
            await Task.Delay(100);
        }

        GetService<ILhwmService>().Close();

        Current.Exit();
    }

    private void ServiceShutDownMessageHandler(object recipient, ServiceShutDownMessage message)
    {
        _servicesToShutDown.Remove(message.Value);
        Log.Information("[App] Shut down: {0}", message.Value.ToString().Split(".").Last());
    }
}
