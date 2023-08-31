using System.Diagnostics;
using System.Reflection;
using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using CommunityToolkit.Mvvm.Input;
using Windows.ApplicationModel;

namespace Anemos.ViewModels;

public partial class SettingsViewModel : PageViewModelBase
{
    private readonly ISettingsService _settingsService;

    public SettingsModel Settings => _settingsService.Settings;

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
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return "VersionDescription".GetLocalized().Replace("$", $"{version.Major}.{version.Minor}.{version.Build}");
    }

    [RelayCommand]
    private static void CreateTask(bool createTask)
    {
        var taskName = AppDomain.CurrentDomain.FriendlyName;
        var info = new ProcessStartInfo()
        {
            FileName = "schtasks",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
        };

        if (createTask)
        {
            info.Arguments = $"/create /sc onlogon /tn \"{taskName}\" /tr \"{App.AppLocation}\" /rl highest";
        }
        else
        {
            info.Arguments = $"/delete /tn \"{taskName}\" /f";
        }

        var proc = new Process { StartInfo = info };
        proc.Start();
    }

    [RelayCommand]
    private void OpenSettingsFolder()
    {
        Process.Start("explorer.exe", _settingsService.SettingsFolder);
    }
}
