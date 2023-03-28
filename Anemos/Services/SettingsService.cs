using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Timers;
using Anemos.Contracts.Services;
using Anemos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;

namespace Anemos.Services;

public partial class SettingsService : ObservableRecipient, ISettingsService
{
    private readonly string _path;

    public SettingsModel Settings { get; private set; } = new();

    private readonly System.Timers.Timer _timer = new(1000);

    [JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = true)]
    [JsonSerializable(typeof(SettingsModel))]
    internal partial class SettingsModelSourceGenerationContext : JsonSerializerContext
    {
    }

    private readonly JsonTypeInfo<SettingsModel> _typeInfo = SettingsModelSourceGenerationContext.Default.SettingsModel;

    public SettingsService(string folder)
    {
        Messenger.Register<AppExitMessage>(this, AppExitMessageHandler);

        var fileNotFound = false;
        _path = Path.Combine(folder, "settings.json");
        if (!File.Exists(_path))
        {
            File.WriteAllText(_path, "{}");
            fileNotFound = true;
        }

        if (fileNotFound)
        {
            SaveToFile().Wait();
        }
        Log.Information("[Settings] Started");
    }

    public async Task LoadAsync()
    {
        var jsonString = File.ReadAllText(_path);
        Settings = JsonSerializer.Deserialize(jsonString, _typeInfo)!;
        Settings.PropertyChanged += Settings_PropertyChanged;

        await Task.CompletedTask;

        _timer.AutoReset = false;
        _timer.Elapsed += Timer_Elapsed;
        Log.Information("[Settings] Loaded");
    }

    private async void AppExitMessageHandler(object recipient, AppExitMessage message)
    {
        if (_timer.Enabled)
        {
            await SaveToFile();
        }
        Messenger.Send(new ServiceShutDownMessage(GetType().GetInterface("ISettingsService")!));
    }

    private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        _timer.Start();
    }

    private async void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        await SaveToFile();
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
            var jsonString = JsonSerializer.Serialize(Settings, _typeInfo);
            await File.WriteAllTextAsync(_path, jsonString).ConfigureAwait(false);
            Log.Debug("[Settings] Saved {path}", _path[(_path.LastIndexOf('\\') + 1)..]);
        }
        catch (Exception ex)
        {
            Log.Error("[Settings] {error}", ex.Message);
        }
    }
}
