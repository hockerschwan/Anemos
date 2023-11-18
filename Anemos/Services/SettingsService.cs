using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;
using Anemos.Contracts.Services;
using Anemos.Models;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;

namespace Anemos.Services;

public class SettingsService : ISettingsService
{
    private readonly IMessenger _messenger;

    public string SettingsFolder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), // AppData/Local
        AppDomain.CurrentDomain.FriendlyName);

    private readonly string _path;

    public SettingsModel Settings { get; private set; } = new();

    private readonly JsonSerializerOptions _options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        WriteIndented = true
    };

    private readonly System.Timers.Timer _timer = new(1000);

    private readonly MessageHandler<object, AppExitMessage> _appExitMessageHandler;

    public SettingsService(IMessenger messenger)
    {
        _messenger = messenger;
        _appExitMessageHandler = AppExitMessageHandler;
        _messenger.Register(this, _appExitMessageHandler);

        var fileNotFound = false;
        if (!Directory.Exists(SettingsFolder))
        {
            Directory.CreateDirectory(SettingsFolder);
        }
        _path = Path.Combine(SettingsFolder, "settings.json");
        if (!File.Exists(_path))
        {
            File.WriteAllText(_path, "{}");
            fileNotFound = true;
        }

        if (fileNotFound)
        {
            SaveToFile().Wait();
        }

        Load();

        _messenger.Send<ServiceStartupMessage>(new(GetType()));
        Log.Information("[Settings] Started");
    }

    private async void AppExitMessageHandler(object recipient, AppExitMessage message)
    {
        if (_timer.Enabled)
        {
            await SaveToFile();
        }
        _messenger.Send<ServiceShutDownMessage>(new(GetType()));
    }

    private void Load()
    {
        var jsonString = File.ReadAllText(_path);

        Settings = JsonSerializer.Deserialize<SettingsModel>(jsonString, _options) ?? throw new Exception("Could not deserialize json");
        Settings.PropertyChanged += Settings_PropertyChanged;

        _timer.AutoReset = false;
        _timer.Elapsed += Timer_Elapsed;
        Log.Information("[Settings] Loaded");
    }

    public async void Save(bool immediate = false)
    {
        if (immediate)
        {
            await SaveToFile();
        }
        else
        {
            _timer.Stop();
            _timer.Start();
        }
    }

    private async Task SaveToFile()
    {
        _timer.Stop();
        try
        {
            var jsonString = JsonSerializer.Serialize(Settings, _options) ?? throw new Exception("Could not serialize json");
            await File.WriteAllTextAsync(_path, jsonString).ConfigureAwait(false);
            Log.Debug("[Settings] Saved {path}", _path[(_path.LastIndexOf('\\') + 1)..]);
        }
        catch (Exception ex)
        {
            Log.Error("[Settings] {error}", ex.Message);
        }
    }

    private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        _timer.Start();
    }

    private async void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        await SaveToFile();
    }
}
