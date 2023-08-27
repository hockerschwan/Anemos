using LibreHardwareMonitor.Hardware.Gpu;
using Serilog;
using static ADLXWrapper.ADLXWrapper;

namespace Anemos.Models;

public class GpuAmdFanModel : FanModelBase
{
    private readonly int _adlxId;

    public bool IsSupported { get; } = false;

    private FanRange _fanRange = new();

    private FanSpeeds _originalFanSpeeds;

    private readonly bool _wasZeroRPMEnabled;

    //  ISensor.Value does not match the actual speed.
    public override double? CurrentPercent
    {
        get
        {
            return ControlMode switch
            {
                FanControlModes.Constant => ConstantSpeed,
                FanControlModes.Curve => TargetValue,
                _ => base.CurrentPercent,
            };
            ;
        }
    }

    public override FanControlModes ControlMode
    {
        get => base.ControlMode;
        set
        {
            if (base.ControlMode != value)
            {
                base.ControlMode = value;
                if (ControlMode == FanControlModes.Device && IsSupported)
                {
                    RestoreSettings();
                }
                TargetValue = null;
                _refractoryPeriodCounter = 0;
                UpdateValue();
                UpdateProfile();
            }
        }
    }

    public GpuAmdFanModel(string id, string name, bool isHidden) : base(id, name, isHidden)
    {
        var deviceId = (Sensor?.Hardware as GenericGpu)?.DeviceId ?? throw new Exception("DeviceId is null.");
        _adlxId = GetId(deviceId);
        if (_adlxId < 0)
        {
            throw new Exception($"Could not find GPU with PNPString:{deviceId}");
        }

        if (!IsSupported(_adlxId))
        {
            ControlMode = FanControlModes.Device;
            Log.Warning("[GpuAmdFan] This GPU does not support manual tuning. ID:{0}", _adlxId);
            return;
        }

        var range = GetFanRange(_adlxId);
        if (range != null)
        {
            _fanRange = range.Value;
        }

        var speeds = GetFanSpeeds(_adlxId);
        if (speeds != null)
        {
            _originalFanSpeeds = speeds.Value;
        }

        _wasZeroRPMEnabled = IsZeroRPMSupported(_adlxId) && IsZeroRPMEnabled(_adlxId);
        if (_wasZeroRPMEnabled)
        {
            SetZeroRPM(_adlxId, false);
        }

        IsSupported = true;

        Log.Debug(
            "[GpuAmdFan] GPU ID:{0}, Temp:{1}-{2}, Speed:{3}-{4}",
            _adlxId,
            _fanRange.minTemperature,
            _fanRange.maxTemperature,
            _fanRange.minSpeed,
            _fanRange.maxSpeed);
    }

    public override void Update()
    {
        if (IsSupported)
        {
            UpdateValue();
        }

        OnPropertyChanged(nameof(CurrentRPM));
        OnPropertyChanged(nameof(CurrentPercent));
    }

    private protected override void UpdateValue()
    {
        switch (ControlMode)
        {
            case FanControlModes.Device:
                break;
            case FanControlModes.Constant:
                if (TargetValue != ConstantSpeed)
                {
                    TargetValue = ConstantSpeed;
                    Write(ConstantSpeed);
                }
                break;
            case FanControlModes.Curve:
                var target = CalcTarget();
                if (target == null) { break; }

                target = Math.Max(_fanRange.minSpeed, Math.Min(_fanRange.maxSpeed, target.Value));
                if (TargetValue != target)
                {
                    TargetValue = target;
                    Write(TargetValue);
                }
                break;
        }
    }

    private void Write(int? value)
    {
        if (!value.HasValue) { return; }

        var spd = Math.Max(_fanRange.minSpeed, Math.Min(_fanRange.maxSpeed, (int)value));
        spd = Math.Max(MinSpeed, Math.Min(MaxSpeed, spd));
        SetFanSpeed(spd);
    }

    private void SetFanSpeed(int speed) => ADLXWrapper.ADLXWrapper.SetFanSpeed(_adlxId, speed);

    private void SetFanSpeeds(FanSpeeds speeds) => ADLXWrapper.ADLXWrapper.SetFanSpeeds(_adlxId, speeds);

    public unsafe void RestoreSettings()
    {
        SetFanSpeeds(_originalFanSpeeds);

        if (IsZeroRPMSupported(_adlxId) && _wasZeroRPMEnabled)
        {
            SetZeroRPM(_adlxId, true);
        }

        Log.Debug("[GpuAmdFan] Settings restored.");
    }
}
