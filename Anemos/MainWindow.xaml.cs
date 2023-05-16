using System.Timers;
using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Vanara.PInvoke;

namespace Anemos;

public sealed partial class MainWindow : WindowEx
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();

    private readonly ISettingsService _settingsService = App.GetService<ISettingsService>();

    private readonly Settings_Window _windowSettings;

    private readonly System.Timers.Timer _timer = new(250);

    public bool IsMaximized
    {
        get
        {
            User32.WINDOWPLACEMENT placement = new();
            if (User32.GetWindowPlacement(this.GetWindowHandle(), ref placement))
            {
                return placement.showCmd == ShowWindowCommand.SW_SHOWMAXIMIZED;
            }
            throw new Exception("User32.GetWindowPlacement failed.");
        }
    }

    public bool IsMinimized
    {
        get
        {
            User32.WINDOWPLACEMENT placement = new();
            if (User32.GetWindowPlacement(this.GetWindowHandle(), ref placement))
            {
                return placement.showCmd == ShowWindowCommand.SW_SHOWMINIMIZED;
            }
            throw new Exception("User32.GetWindowPlacement failed.");
        }
    }

    private readonly SolidColorBrush NavigationBackgroundColor = new();
    private readonly SolidColorBrush CommandBarBackgroundColor = new();

    public MainWindow()
    {
        _windowSettings = _settingsService.Settings.Window;

        InitializeComponent();

        var hicon = User32.LoadIcon(Kernel32.GetModuleHandle(), Macros.MAKEINTRESOURCE(32512)).DangerousGetHandle();
        AppWindow.SetIcon(Microsoft.UI.Win32Interop.GetIconIdFromIcon(hicon));
        Content = null;
        Title = "AppDisplayName".GetLocalized();

        this.MoveAndResize(_windowSettings.X, _windowSettings.Y, _windowSettings.Width, _windowSettings.Height);
        if (_windowSettings.Maximized)
        {
            this.Maximize();
            if (_settingsService.Settings.StartMinimized)
            {
                this.Hide();
            }
        }

        Closed += MainWindow_Closed;
        SizeChanged += MainWindow_SizeChanged;
        PositionChanged += MainWindow_PositionChanged;

        _settingsService.Settings.PropertyChanged += Settings_PropertyChanged;

        SetPrimaryNavColor();
        SetSecondaryNavColor();

        _timer.AutoReset = false;
        _timer.Elapsed += Timer_Elapsed;
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        if (!App.HasShutdownRequested)
        {
            this.Hide();
            _messenger.Send(new WindowVisibilityChangedMessage(false));
            args.Handled = true;
        }
    }

    private void MainWindow_PositionChanged(object? sender, Windows.Graphics.PointInt32 e)
    {
        _timer.Stop();
        _timer.Start();
    }

    private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        _timer.Stop();
        _timer.Start();
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        OnPositionAndSizeChanged();
    }

    private void OnPositionAndSizeChanged()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (IsMinimized)
            {
                _messenger.Send(new WindowVisibilityChangedMessage(false));
                return;
            }

            _messenger.Send(new WindowVisibilityChangedMessage(true));

            if (IsMaximized)
            {
                if (_windowSettings.Maximized) { return; }

                _windowSettings.Maximized = true;
            }
            else
            {
                User32.GetWindowRect(this.GetWindowHandle(), out var rect);

                if (IsWindowIdenticalToSettings(rect)) { return; }

                var dpi = GetDisplayScale();

                _windowSettings.Maximized = false;
                _windowSettings.X = rect.left;
                _windowSettings.Y = rect.top;
                _windowSettings.Width = (int)((rect.right - rect.left) / dpi);
                _windowSettings.Height = (int)((rect.bottom - rect.top) / dpi);
            }

            _settingsService.Save();
        });
    }

    private bool IsWindowIdenticalToSettings(RECT rect)
    {
        return IsMaximized == _windowSettings.Maximized &&
               rect.left == _windowSettings.X &&
               rect.top == _windowSettings.Y &&
               Width == _windowSettings.Width &&
               Height == _windowSettings.Height;
    }

    private double GetDisplayScale()
    {
        var dpi = User32.GetDpiForWindow(this.GetWindowHandle());
        return dpi / 96.0;
    }

    private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_settingsService.Settings.NavigationBackgroundColor))
        {
            SetPrimaryNavColor();
        }
        else if (e.PropertyName == nameof(_settingsService.Settings.CommandBarBackgroundColor))
        {
            SetSecondaryNavColor();
        }
    }

    private void SetPrimaryNavColor()
    {
        NavigationBackgroundColor.Color = ColorHelper.ToColor(_settingsService.Settings.NavigationBackgroundColor);
        App.Current.Resources["NavigationViewTopPaneBackground"] = NavigationBackgroundColor;
    }

    private void SetSecondaryNavColor()
    {
        CommandBarBackgroundColor.Color = ColorHelper.ToColor(_settingsService.Settings.CommandBarBackgroundColor);
        App.Current.Resources["CommandBarBackground"] = CommandBarBackgroundColor;
    }
}
