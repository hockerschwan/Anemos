using System.ComponentModel;
using Anemos.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LibreHardwareMonitor.Hardware;
using Microsoft.UI.Xaml;
using Serilog;

namespace Anemos.Services;

public class LhwmService : ObservableRecipient, ILhwmService
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

    private readonly ISettingsService _settingsService;

    private readonly Computer _computer;

    private readonly UpdateVisitor _updateVisitor;

    public List<IHardware> Hardware { get; } = new();

    public List<ISensor> Sensors { get; } = new();

    private readonly DispatcherTimer _timer = new();

    private bool _isUpdating;

    public LhwmService(ISettingsService settingsService)
    {
        _settingsService = settingsService;

        Messenger.Register<AppExitMessage>(this, AppExitMessageHandler);

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

        _updateVisitor = new();

        _computer.Open();
        _computer.Accept(_updateVisitor);
        Log.Information("[LHWM] Opened");

        Scan();
    }

    public async Task InitAsync()
    {
        _settingsService.Settings.PropertyChanged += Settings_PropertyChanged;

        var interval = _settingsService.Settings.UpdateInterval;
        SetInterval(interval);
        _timer.Tick += Timer_Tick;
        _timer.Start();

        await Task.CompletedTask;
        Log.Debug("[LHWM] Loaded");
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_settingsService.Settings.UpdateInterval))
        {
            var interval = _settingsService.Settings.UpdateInterval;
            SetInterval(interval);
        }
    }

    private void SetInterval(int interval_s)
    {
        _timer.Interval = new(interval_s / 3600, interval_s / 60, interval_s % 60);
    }

    private void Scan()
    {
        foreach (var hardware in _computer.Hardware)
        {
            foreach (var subHardware in hardware.SubHardware)
            {
                foreach (var sensor in subHardware.Sensors)
                {
                    sensor.ValuesTimeWindow = new(0);
                    if (IsSensorRelevant(sensor))
                    {
                        Sensors.Add(sensor);
                    }
                }
            }

            foreach (var sensor in hardware.Sensors)
            {
                sensor.ValuesTimeWindow = new(0);
                if (IsSensorRelevant(sensor))
                {
                    Sensors.Add(sensor);
                }
            }
        }

        HashSet<IHardware> hwSet = new();
        Sensors.ForEach(s => hwSet.Add(s.Hardware));
        Hardware.AddRange(hwSet);
    }

    private static bool IsSensorRelevant(ISensor sensor)
    {
        return sensor.SensorType switch
        {
            SensorType.Control or SensorType.Fan or SensorType.Temperature => true,
            _ => false,
        };
    }

    private void Timer_Tick(object? sender, object e)
    {
        Update();
        Messenger.Send(new LhwmUpdateDoneMessage());
    }

    private void Update()
    {
        _isUpdating = true;
        foreach (var hw in Hardware)
        {
            _updateVisitor.VisitHardware(hw);
        }
        _isUpdating = false;
    }

    public ISensor? GetSensor(string id)
    {
        return Sensors.SingleOrDefault(s => s?.Identifier.ToString() == id, null);
    }

    public IEnumerable<ISensor> GetSensors(SensorType sensorType)
    {
        return Sensors.Where(i => i.SensorType == sensorType);
    }

    private async void AppExitMessageHandler(object recipient, AppExitMessage message)
    {
        _timer.Stop();
        _timer.Tick -= Timer_Tick;

        while (true)
        {
            if (!_isUpdating)
            {
                break;
            }
            await Task.Delay(100);
        }

        Messenger.Send(new ServiceShutDownMessage(GetType()));
    }

    public void Close()
    {
        _computer.Close();
        Log.Information("[LHWM] Closed");
    }
}
