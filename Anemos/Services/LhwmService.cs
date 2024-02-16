using System.ComponentModel;
using Anemos.Contracts.Services;
using CommunityToolkit.Mvvm.Messaging;
using LibreHardwareMonitor.Hardware;
using Serilog;

namespace Anemos.Services;

internal class LhwmService : ILhwmService
{
    private class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (var subHardware in hardware.SubHardware)
            {
                subHardware.Accept(this);
            }
        }

        public void VisitSensor(ISensor sensor)
        {
        }

        public void VisitParameter(IParameter parameter)
        {
        }
    }

    private readonly IMessenger _messenger;

    private readonly ISettingsService _settingsService;

    private readonly Computer _computer;

    private readonly UpdateVisitor _updateVisitor = new();

    public List<IHardware> Hardware { get; } = [];

    public List<ISensor> Sensors { get; } = [];

    private readonly System.Timers.Timer _timer = new();

    private bool _isUpdating;

    private readonly MessageHandler<object, AppExitMessage> _appExitMessageHandler;
    private readonly Action<IHardware> _updateAction;

    public LhwmService(IMessenger messenger, ISettingsService settingsService)
    {
        _messenger = messenger;
        _appExitMessageHandler = AppExitMessageHandler;
        _messenger.Register(this, _appExitMessageHandler);

        _updateAction = _updateVisitor.VisitHardware;

        _settingsService = settingsService;
        _settingsService.Settings.PropertyChanged += Settings_PropertyChanged;

        _computer = new Computer()
        {
            IsMotherboardEnabled = true,
            IsCpuEnabled = true,
            IsMemoryEnabled = true,
            IsGpuEnabled = true,
            IsStorageEnabled = true,
            IsPsuEnabled = true,
            IsControllerEnabled = true,
            IsBatteryEnabled = false,
            IsNetworkEnabled = false
        };
        _computer.Open();
        _computer.Accept(_updateVisitor);

        Scan();

        _timer.Interval = 1000d * _settingsService.Settings.UpdateInterval;
        _timer.Elapsed += Timer_Tick;

        _messenger.Send<ServiceStartupMessage>(new(GetType()));
        Log.Information("[LHWM] Started");
    }

    private async void AppExitMessageHandler(object recipient, AppExitMessage message)
    {
        _timer.Stop();
        _timer.Elapsed -= Timer_Tick;

        while (true)
        {
            if (!_isUpdating) { break; }
            await Task.Delay(100);
        }

        GetSensors(SensorType.Control).ToList().ForEach(s => s.Control.SetDefault());
        Close();

        _messenger.Send<ServiceShutDownMessage>(new(GetType()));
    }

    public void Close()
    {
        _computer.Close();
        Log.Information("[LHWM] Closed");
    }

    public ISensor? GetSensor(string id)
    {
        return GetSensorImpl(this, id);

        static ISensor? GetSensorImpl(LhwmService @this, string id)
        {
            foreach (var sensor in @this.Sensors)
            {
                if (sensor.Identifier.ToString() == id)
                {
                    return sensor;
                }
            }
            return null;
        }
    }

    public IEnumerable<ISensor> GetSensors(SensorType sensorType)
    {
        return GetSensorsImpl(this, sensorType);

        static IEnumerable<ISensor> GetSensorsImpl(LhwmService @this, SensorType sensorType)
        {
            foreach (var sensor in @this.Sensors)
            {
                if (sensor.SensorType == sensorType)
                {
                    yield return sensor;
                }
            }
        }
    }

    private void Scan()
    {
        foreach (var hardware in _computer.Hardware.ToList())
        {
            Hardware.Add(hardware);

            foreach (var subHardware in hardware.SubHardware)
            {
                foreach (var sensor in subHardware.Sensors)
                {
                    sensor.ValuesTimeWindow = TimeSpan.Zero;
                    Sensors.Add(sensor);
                }
            }

            foreach (var sensor in hardware.Sensors)
            {
                sensor.ValuesTimeWindow = TimeSpan.Zero;
                Sensors.Add(sensor);
            }
        }
    }

    public void Start()
    {
        _timer.Start();
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_settingsService.Settings.UpdateInterval))
        {
            _timer.Interval = 1000d * _settingsService.Settings.UpdateInterval;
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        Update();
        _messenger.Send<LhwmUpdateDoneMessage>();
    }

    private void Update()
    {
        _isUpdating = true;
        foreach (var hw in Hardware)
        {
            ThreadPool.QueueUserWorkItem(_updateAction, hw, true);
        }
        _isUpdating = false;
    }
}
