using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using Anemos.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel;

namespace Anemos.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly ISettingsService _settingsService;

    public SettingsModel Settings => _settingsService.Settings;

    public readonly bool IsElevated;

    public bool IsNotElevated => !IsElevated;

    public ColorPickerDialog ColorPicker { get; } = new();

    private SolidColorBrush _navigationBackgroundColor = new();
    public SolidColorBrush NavigationBackgroundColor
    {
        get => _navigationBackgroundColor;
        set
        {
            if (SetProperty(ref _navigationBackgroundColor, value))
            {
                _settingsService.Settings.NavigationBackgroundColor = GetRGB(value);
            }
        }
    }

    private SolidColorBrush _commandBarBackgroundColor = new();
    public SolidColorBrush CommandBarBackgroundColor
    {
        get => _commandBarBackgroundColor;
        set
        {
            if (SetProperty(ref _commandBarBackgroundColor, value))
            {
                _settingsService.Settings.CommandBarBackgroundColor = GetRGB(value);
            }
        }
    }

    private SolidColorBrush _chartLineColor = new();
    public SolidColorBrush ChartLineColor
    {
        get => _chartLineColor;
        set
        {
            if (SetProperty(ref _chartLineColor, value))
            {
                _settingsService.Settings.ChartLineColor = GetRGB(value);
            }
        }
    }

    private SolidColorBrush _chartMarkerColor = new();
    public SolidColorBrush ChartMarkerColor
    {
        get => _chartMarkerColor;
        set
        {
            if (SetProperty(ref _chartMarkerColor, value))
            {
                _settingsService.Settings.ChartMarkerColor = GetRGB(value);
            }
        }
    }

    private SolidColorBrush _chartBGColor = new();
    public SolidColorBrush ChartBGColor
    {
        get => _chartBGColor;
        set
        {
            if (SetProperty(ref _chartBGColor, value))
            {
                _settingsService.Settings.ChartBGColor = GetRGB(value);
            }
        }
    }

    private SolidColorBrush _chartGridColor = new();
    public SolidColorBrush ChartGridColor
    {
        get => _chartGridColor;
        set
        {
            if (SetProperty(ref _chartGridColor, value))
            {
                _settingsService.Settings.ChartGridColor = GetRGB(value);
            }
        }
    }

    private SolidColorBrush _chartTextColor = new();
    public SolidColorBrush ChartTextColor
    {
        get => _chartTextColor;
        set
        {
            if (SetProperty(ref _chartTextColor, value))
            {
                _settingsService.Settings.ChartTextColor = GetRGB(value);
            }
        }
    }

    private string _versionDescription;
    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _versionDescription = GetVersionDescription();

        using var identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new(identity);
        IsElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);

        _navigationBackgroundColor.Color = ColorHelper.ToColor(_settingsService.Settings.NavigationBackgroundColor);
        _commandBarBackgroundColor.Color = ColorHelper.ToColor(_settingsService.Settings.CommandBarBackgroundColor);
        _chartLineColor.Color = ColorHelper.ToColor(_settingsService.Settings.ChartLineColor);
        _chartMarkerColor.Color = ColorHelper.ToColor(_settingsService.Settings.ChartMarkerColor);
        _chartBGColor.Color = ColorHelper.ToColor(_settingsService.Settings.ChartBGColor);
        _chartGridColor.Color = ColorHelper.ToColor(_settingsService.Settings.ChartGridColor);
        _chartTextColor.Color = ColorHelper.ToColor(_settingsService.Settings.ChartTextColor);
    }

    private static string GetRGB(SolidColorBrush brush)
    {
        return $"#{brush.Color.ToHex()[3..]}";
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return "VersionDescription".GetLocalized().Replace("$", $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}");
    }

    [RelayCommand]
    private static void OpenSettingsFolder()
    {
        Process.Start("explorer.exe", App.Current.SettingsFolder);
    }

    [RelayCommand]
    private void ExitApp()
    {
        App.Current.RequestShutdown();
    }

    [RelayCommand]
    private void CreateTask(bool createTask)
    {
        if (!IsElevated) { return; }

        var taskName = App.Current.AppName;
        var fullPath = Process.GetCurrentProcess().MainModule?.FileName!;

        var info = new ProcessStartInfo()
        {
            FileName = "schtasks",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
        };
        if (createTask)
        {
            info.Arguments = $"/create /sc onlogon /tn \"{taskName}\" /tr \"{fullPath}\" /rl highest";
        }
        else
        {
            info.Arguments = $"/delete /tn \"{taskName}\" /f";
        }

        var proc = new Process { StartInfo = info };
        proc.Start();
    }
}
