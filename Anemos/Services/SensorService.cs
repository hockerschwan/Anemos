using Anemos.Contracts.Models;
using Anemos.Contracts.Services;
using Anemos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LibreHardwareMonitor.Hardware;
using Serilog;

namespace Anemos.Services;

public class SensorService : ObservableRecipient, ISensorService
{
    private readonly ISettingsService _settingsService;

    private readonly ILhwmService _lhwmService;

    public List<ISensorModel> PhysicalSensors { get; } = new();

    public List<ISensorModel> CustomSensors { get; } = new();

    public List<ISensorModel> Sensors => PhysicalSensors.Concat(CustomSensors).ToList();

    private bool _isUpdating;

    public SensorService(ISettingsService settingsService, ILhwmService lhwmService)
    {
        Messenger.Register<AppExitMessage>(this, AppExitMessageHandler);
        Messenger.Register<LhwmUpdateDoneMessage>(this, (r, m) =>
        {
            Update();
            Messenger.Send(new CustomSensorsUpdateDoneMessage());
        });

        _settingsService = settingsService;
        _lhwmService = lhwmService;

        Scan();
        Log.Debug("[SensorService] Started");
    }

    public async Task InitializeAsync()
    {
        await Task.Run(Load);
        Update();
        Log.Debug("[SensorService] Loaded");
        await Task.CompletedTask;
    }

    private async void AppExitMessageHandler(object recipient, AppExitMessage message)
    {
        Messenger.Unregister<LhwmUpdateDoneMessage>(this);
        while (true)
        {
            if (!_isUpdating) { break; }
            await Task.Delay(100);
        }
        Messenger.Send(new ServiceShutDownMessage(GetType()));
    }

    private void Update()
    {
        _isUpdating = true;
        foreach (var sensor in Sensors)
        {
            sensor.Update();
        }
        _isUpdating = false;
    }

    private void Scan()
    {
        PhysicalSensors.AddRange(
            _lhwmService.GetSensors(SensorType.Temperature).Select(s => new HWSensorModel(s))
        );
    }

    public ISensorModel? GetSensor(string id)
    {
        return Sensors.SingleOrDefault(m => m?.Id == id, null);
    }

    public IEnumerable<ISensorModel> GetSensors(IEnumerable<string> idList)
    {
        return Sensors.Where(tm => idList.Contains(tm.Id));
    }

    public void AddCustomSensor(CustomSensorArg arg)
    {
        AddCustomSensors(new List<CustomSensorArg> { arg });
    }

    private void AddCustomSensors(IEnumerable<CustomSensorArg> args, bool save = true)
    {
        var old = Sensors.ToList();
        var models = args.Select(arg => new CustomSensorModel(arg));
        CustomSensors.AddRange(models);
        OnPropertyChanged(nameof(Sensors));
        Messenger.Send(new CustomSensorsChangedMessage(this, nameof(Sensors), old, Sensors));

        if (save)
        {
            Save();
        }
    }

    public void RemoveCustomSensor(string id)
    {
        var model = GetSensor(id);
        if (model == null) { return; }

        var old = Sensors.ToList();
        CustomSensors.Remove(model);
        OnPropertyChanged(nameof(Sensors));
        Messenger.Send(new CustomSensorsChangedMessage(this, nameof(Sensors), old, Sensors));

        Save();
    }

    public void Save()
    {
        _settingsService.Settings.SensorSettings.Sensors = CustomSensors.Select(
            sensor =>
            {
                var s = (CustomSensorModel)sensor!;
                return new SensorSettings_Sensor()
                {
                    Id = s.Id,
                    Name = s.Name,
                    CalcMethod = s.CalcMethod,
                    SampleSize = s.SampleSize,
                    SourceIds = s.SourceIds.Where(id => GetSensor(id) != null)
                };
            });

        _settingsService.Save();
    }

    private void Load()
    {
        AddCustomSensors(
            _settingsService.Settings.SensorSettings.Sensors.Select(
                s => new CustomSensorArg()
                {
                    Id = s.Id,
                    Name = s.Name,
                    CalcMethod = s.CalcMethod,
                    SampleSize = s.SampleSize,
                    SourceIds = s.SourceIds
                }),
            false);
    }
}
