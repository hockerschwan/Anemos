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

    public List<SensorModelBase> PhysicalSensors { get; } = new();

    public List<SensorModelBase> CustomSensors { get; } = new();

    public List<SensorModelBase> Sensors { get; private set; } = new();

    private bool _isUpdating;

    public SensorService(
        IMessenger messenger,
        ISettingsService settingsService,
        ILhwmService lhwmService)
    {
        _messenger = messenger;
        _settingsService = settingsService;
        _lhwmService = lhwmService;

        _messenger.Register<AppExitMessage>(this, AppExitMessageHandler);
        _messenger.Register<LhwmUpdateDoneMessage>(this, LhwmUpdateDoneMessageHandler);

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
        return Sensors.SingleOrDefault(m => m?.Id == id, null);
    }

    public IEnumerable<SensorModelBase> GetSensors(IEnumerable<string> idList)
    {
        return Sensors.Where(tm => idList.Contains(tm.Id));
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
        await Task.Run(Load);
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

    private void Scan()
    {
        PhysicalSensors.AddRange(
            _lhwmService.GetSensors(SensorType.Temperature).Select(s => new HWSensorModel(s))
        );
    }

    private void Update()
    {
        _isUpdating = true;
        Parallel.ForEach(Sensors, s =>
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(
                Microsoft.UI.Dispatching.DispatcherQueuePriority.High,
                () => s.Update());
        });
        _isUpdating = false;
    }

    private void UpdateSensors()
    {
        Sensors = PhysicalSensors.Concat(CustomSensors).ToList();
    }
}
