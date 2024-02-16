using Anemos.Contracts.Services;
using Anemos.Models;
using CommunityToolkit.Mvvm.Messaging;
using LibreHardwareMonitor.Hardware;
using Serilog;

namespace Anemos.Services;

internal class SensorService : ISensorService
{
    private readonly IMessenger _messenger;
    private readonly ISettingsService _settingsService;
    private readonly ILhwmService _lhwmService;

    public List<SensorModelBase> PhysicalSensors { get; } = [];

    public List<SensorModelBase> CustomSensors { get; } = [];

    public List<SensorModelBase> Sensors { get; private set; } = [];

    private bool _isUpdating;

    private readonly MessageHandler<object, AppExitMessage> _appExitMessageHandler;
    private readonly MessageHandler<object, LhwmUpdateDoneMessage> _lhwmUpdatedMessageHandler;
    private readonly Action _loadAction;
    private readonly Action<SensorModelBase> _updateAction;

    public SensorService(
        IMessenger messenger,
        ISettingsService settingsService,
        ILhwmService lhwmService)
    {
        _messenger = messenger;
        _settingsService = settingsService;
        _lhwmService = lhwmService;

        _appExitMessageHandler = AppExitMessageHandler;
        _lhwmUpdatedMessageHandler = LhwmUpdateDoneMessageHandler;
        _messenger.Register(this, _appExitMessageHandler);
        _messenger.Register(this, _lhwmUpdatedMessageHandler);

        _loadAction = Load;
        _updateAction = Update_;

        Scan();

        _messenger.Send<ServiceStartupMessage>(new(GetType()));
        Log.Information("[Sensor] Started");
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
        UpdateSensors();
        _messenger.Send(new CustomSensorsChangedMessage(this, nameof(Sensors), old, Sensors));

        if (save)
        {
            Save();
        }
    }

    private async void AppExitMessageHandler(object recipient, AppExitMessage message)
    {
        _messenger.UnregisterAll(this);
        while (true)
        {
            if (!_isUpdating) { break; }
            await Task.Delay(100);
        }
        _messenger.Send<ServiceShutDownMessage>(new(GetType()));
    }

    public SensorModelBase? GetSensor(string id)
    {
        return GetSensorImpl(this, id);

        static SensorModelBase? GetSensorImpl(SensorService @this, string id)
        {
            foreach (var sensor in @this.Sensors)
            {
                if (sensor.Id == id)
                {
                    return sensor;
                }
            }
            return null;
        }
    }

    public IEnumerable<SensorModelBase> GetSensors(IEnumerable<string> idList)
    {
        return GetSensorsImpl(this, idList);

        static IEnumerable<SensorModelBase> GetSensorsImpl(SensorService @this, IEnumerable<string> idList)
        {
            foreach (var sensor in @this.Sensors)
            {
                if (idList.Contains(sensor.Id))
                {
                    yield return sensor;
                }
            }
        }
    }

    private void LhwmUpdateDoneMessageHandler(object recipient, LhwmUpdateDoneMessage message)
    {
        Update();
        _messenger.Send<CustomSensorsUpdateDoneMessage>();
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

    public async Task LoadAsync()
    {
        UpdateSensors();
        await Task.Run(_loadAction);
        Log.Information("[Sensor] Loaded");
    }

    public void RemoveCustomSensor(string id)
    {
        var model = GetSensor(id);
        if (model == null) { return; }

        var old = Sensors.ToList();
        CustomSensors.Remove(model);
        UpdateSensors();
        _messenger.Send(new CustomSensorsChangedMessage(this, nameof(Sensors), old, Sensors));

        Save();
    }

    public void Save()
    {
        var array = new SensorSettings_Sensor[CustomSensors.Count];
        var sensors = new Span<SensorSettings_Sensor>(array);

        for (var i = 0; i < array.Length; ++i)
        {
            if (CustomSensors[i] is CustomSensorModel s)
            {
                sensors[i] = new SensorSettings_Sensor()
                {
                    Id = s.Id,
                    Name = s.Name,
                    CalcMethod = s.CalcMethod,
                    SampleSize = s.SampleSize,
                    SourceIds = Where(this, s)
                };

                static IEnumerable<string> Where(SensorService @this, CustomSensorModel s)
                {
                    if (s == null) { yield break; }

                    foreach (var id in s.SourceIds)
                    {
                        if (@this.GetSensor(id) != null)
                        {
                            yield return id;
                        }
                    }
                }
            }
        }

        _settingsService.Settings.SensorSettings.Sensors = sensors.ToArray();

        _settingsService.Save();
    }

    private void Scan()
    {
        PhysicalSensors.AddRange(
            _lhwmService.GetSensors(SensorType.Temperature).Select(s => new HWSensorModel(s))
        );
    }

    private void Update()
    {
        _isUpdating = true;
        foreach (var s in Sensors)
        {
            ThreadPool.QueueUserWorkItem(_updateAction, s, true);
        }
        _isUpdating = false;
    }

    private void Update_(SensorModelBase sensor)
    {
        sensor.Update();
    }

    private void UpdateSensors()
    {
        Sensors = [.. PhysicalSensors, .. CustomSensors];
    }
}
