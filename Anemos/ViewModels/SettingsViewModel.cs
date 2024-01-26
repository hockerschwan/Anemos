using System.Diagnostics;
using System.Reflection;
using System.Xml;
using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using Windows.ApplicationModel;

namespace Anemos.ViewModels;

public partial class SettingsViewModel : PageViewModelBase
{
    private static ISettingsService SettingsService => App.GetService<ISettingsService>();
    public static SettingsModel Settings => SettingsService.Settings;

    private string _versionDescription = GetVersionDescription();
    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
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

    public static async Task<bool> TaskExists()
    {
        var info = new ProcessStartInfo()
        {
            FileName = "schtasks",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            Arguments = $"/query /tn \"{AppDomain.CurrentDomain.FriendlyName}\" /fo csv"
        };

        var source = new CancellationTokenSource();
        var token = source.Token;
        token.ThrowIfCancellationRequested();

        var proc = new Process
        {
            StartInfo = info
        };
        proc.Exited += Proc_Exited;
        proc.Start();
        await proc.WaitForExitAsync();

        try
        {
            await Task.Delay(1000, token);
        }
        catch (TaskCanceledException) { }
        finally
        {
            proc.Exited -= Proc_Exited;
            source.Dispose();
        }

        var output = proc.StandardOutput.ReadToEnd();
        return output.StartsWith("\"TaskName\"");

        void Proc_Exited(object? sender, EventArgs e)
        {
            source.Cancel();
        }
    }

    public static async Task CreateTask(bool createTask)
    {
        var taskName = AppDomain.CurrentDomain.FriendlyName;
        var proc = new Process();
        var info = new ProcessStartInfo()
        {
            FileName = "schtasks",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
        };

        if (!createTask)
        {
            info.Arguments = $"/delete /tn \"{taskName}\" /f";
            proc.StartInfo = info;
            proc.Start();
            await proc.WaitForExitAsync();
            return;
        }

        // create (with 3days limit)
        info.Arguments = $"/create /sc onlogon /tn \"{taskName}\" /tr \"{App.AppLocation}\" /rl highest";
        proc.StartInfo = info;
        proc.Start();
        await proc.WaitForExitAsync();

        // export
        info.Arguments = $"/query /xml /tn \"{taskName}\"";
        proc = new()
        {
            StartInfo = info
        };
        proc.Start();
        await proc.WaitForExitAsync();

        var output = proc.StandardOutput.ReadToEnd().Replace("\r\r\n", "\r\n");
        output = output.Remove(0, output.IndexOf('\n') + 1);

        // delete task
        info.Arguments = $"/delete /tn \"{taskName}\" /f";
        proc = new()
        {
            StartInfo = info
        };
        proc.Start();
        await proc.WaitForExitAsync();

        // edit xml
        var xml = new XmlDocument();
        xml.LoadXml(output);

        {
            var settingsNode = xml.GetElementsByTagName("Settings").Item(0);
            if (settingsNode == null)
            {
                Log.Error("\"Settings\" not found in exported XML");
                return;
            }

            var etlNode = xml.GetElementsByTagName("ExecutionTimeLimit").Item(0);
            if (etlNode == null)
            {
                etlNode = xml.CreateElement("ExecutionTimeLimit", xml.DocumentElement?.NamespaceURI);
                settingsNode.AppendChild(etlNode);
            }
            etlNode.InnerText = "PT0S";

            var ahtNode = xml.CreateElement("AllowHardTerminate", xml.DocumentElement?.NamespaceURI);
            ahtNode.InnerText = "false";
            settingsNode.AppendChild(ahtNode);
        }

        // save xml
        var xmlPath = Path.Combine(SettingsService.SettingsFolder, "Task.xml");
        {
            using var xmlFile = File.CreateText(xmlPath);
            using var writer = new XmlTextWriter(xmlFile);
            writer.Formatting = Formatting.Indented;

            xml.WriteContentTo(writer);

            writer.Close();
            xmlFile.Close();
        }

        // import
        info.Arguments = $"/create /tn \"{taskName}\" /xml \"{xmlPath}\"";
        proc = new()
        {
            StartInfo = info
        };
        proc.Start();
        await proc.WaitForExitAsync();

        // delete xml
        File.Delete(xmlPath);
    }

    [RelayCommand]
    private static void OpenSettingsFolder()
    {
        Process.Start("explorer.exe", SettingsService.SettingsFolder);
    }
}
