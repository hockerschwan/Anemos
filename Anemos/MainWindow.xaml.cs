﻿using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;
using Windows.System;
using Windows.UI.ViewManagement;
using WinUIEx.Messaging;

namespace Anemos;

public sealed partial class MainWindow : WindowEx
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();

    private readonly ISettingsService _settingsService = App.GetService<ISettingsService>();

    private WindowSettings WindowSettings => _settingsService.Settings.Window;

    private readonly System.Timers.Timer _timer = new(250) { AutoReset = false };

    private readonly Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;

    private readonly UISettings settings;

    public bool IsMaximized => RuntimeHelper.IsMaximized(this);

    public bool IsMinimized => RuntimeHelper.IsMinimized(this);

    private readonly WindowMessageMonitor _messageMonitor;
    private bool _closeButtonClicked = false;

    public MainWindow()
    {
        InitializeComponent();
        Activated += MainWindow_Activated;
        Closed += MainWindow_Closed;

        _messageMonitor = new(this);
        _messageMonitor.WindowMessageReceived += MessageMonitor_WindowMessageReceived;

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();

        // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        settings = new UISettings();
        settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event

        this.MoveAndResize(WindowSettings.X, WindowSettings.Y, WindowSettings.Width, WindowSettings.Height);
        DisplayScale = RuntimeHelper.GetDpiForWindow(this) / 96.0;
        if (WindowSettings.Maximized)
        {
            this.Maximize();
            if (_settingsService.Settings.StartMinimized)
            {
                this.Hide();
            }
        }
    }

    public double DisplayScale
    {
        get; private set;
    }

    private bool IsWindowIdenticalToSettings(Helpers.PInvoke.RECT rect)
    {
        return IsMaximized == WindowSettings.Maximized &&
               rect.Left == WindowSettings.X &&
               rect.Top == WindowSettings.Y &&
               Width == WindowSettings.Width &&
               Height == WindowSettings.Height;
    }

    private void MainWindow_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
    {
        Activated -= MainWindow_Activated;

        if (_settingsService.Settings.StartMinimized)
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                await Task.Delay(1000);
                if (!App.MainWindow.Visible)
                {
                    _messenger.Send(new WindowVisibilityChangedMessage(false));
                }
            });
        }

        PositionChanged += MainWindow_PositionChanged;
        SizeChanged += MainWindow_SizeChanged;
        _timer.Elapsed += Timer_Elapsed;
    }

    private void MainWindow_Closed(object sender, Microsoft.UI.Xaml.WindowEventArgs args)
    {
        if (!App.HasShutdownStarted)
        {
            this.Hide();
            _messenger.Send<WindowVisibilityChangedMessage>(new(false));
            args.Handled = true;
        }
    }

    private void MainWindow_SizeChanged(object sender, Microsoft.UI.Xaml.WindowSizeChangedEventArgs args)
    {
        _timer.Stop();
        _timer.Start();
    }

    private void MainWindow_PositionChanged(object? sender, Windows.Graphics.PointInt32 e)
    {
        DisplayScale = RuntimeHelper.GetDpiForWindow(this) / 96.0;

        _timer.Stop();
        _timer.Start();
    }

    private void MessageMonitor_WindowMessageReceived(object? sender, WindowMessageEventArgs e)
    {
        switch (e.Message.MessageId)
        {
            case 0x10: // WM_CLOSE
                if (_closeButtonClicked)
                {
                    _closeButtonClicked = false;
                    break;
                }

                Log.Information("[MainWindow] WM_CLOSE received.");
                App.Current.Shutdown(true);
                return;
            case 0x112: // WM_SYSCOMMAND
                if (e.Message.WParam == 0xF060) // SC_CLOSE
                {
                    _closeButtonClicked = true;
                }
                break;
        }
    }

    private void OnPositionAndSizeChanged()
    {
        if (IsMinimized)
        {
            _messenger.Send(new WindowVisibilityChangedMessage(false));
            return;
        }

        _messenger.Send(new WindowVisibilityChangedMessage(true));

        if (IsMaximized)
        {
            if (WindowSettings.Maximized) { return; }

            WindowSettings.Maximized = true;
        }
        else
        {
            var rect = RuntimeHelper.GetWindowRect(this);

            if (IsWindowIdenticalToSettings(rect)) { return; }

            WindowSettings.Maximized = false;
            WindowSettings.X = rect.Left;
            WindowSettings.Y = rect.Top;
            WindowSettings.Width = (int)((rect.Right - rect.Left) / DisplayScale);
            WindowSettings.Height = (int)((rect.Bottom - rect.Top) / DisplayScale);
        }

        _settingsService.Save();
    }

    // this handles updating the caption button colors correctly when indows system theme is changed
    // while the app is open
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        dispatcherQueue.TryEnqueue(TitleBarHelper.ApplySystemThemeToCaptionButtons);
    }

    private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(OnPositionAndSizeChanged);
    }
}
