using ADLXWrapper;
using LibreHardwareMonitor.Hardware.Gpu;
using Serilog;

namespace Anemos.Models;

#pragma warning disable CS8500
public class GpuAmdFanModel : FanModelBase
{
    private readonly int _adlxId;

    public bool IsSupported
    {
        get;
    }

    public readonly FanRangeResult FanRange = new();

    private readonly List<Tuple<int, int>> _originalFanSpeed = new();

    private readonly bool _wasZeroRPMEnabled;

    //  ISensor.Value does not match the actual speed.
    public override double? CurrentPercent
    {
        get
        {
            switch (ControlMode)
            {
                case FanControlModes.Constant:
                    return ConstantSpeed;
                case FanControlModes.Curve:
                    return Value;
                default:
                    return base.CurrentPercent;
            };
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
                Value = null;
                _refractoryPeriodCounter = 0;
                UpdateValue();
                UpdateProfile();
            }
        }
    }

    public GpuAmdFanModel(string id, string name, bool isHidden) : base(id, name, isHidden)
    {
        if (_fanService.ADLX == null)
        {
            throw new Exception("ADLX is null");
        }
        var deviceId = (Sensor?.Hardware as GenericGpu)?.DeviceId ?? throw new Exception("DeviceId is null.");
        _adlxId = _fanService.ADLX.GetId(deviceId);
        if (_adlxId < 0)
        {
            throw new Exception($"Could not find GPU with PNPString:{deviceId}");
        }

        if (!_fanService.ADLX.IsSupported(_adlxId))
        {
            ControlMode = FanControlModes.Device;
            Log.Warning("[GpuAmdFanModel] This GPU does not support manual tuning. ID:{0}", _adlxId);
            return;
        }

        unsafe
        {
            fixed (FanRangeResult* ptr = &FanRange)
            {
                if (!_fanService.ADLX!.GetFanRange(_adlxId, &ptr))
                {
                    throw new Exception("Could not get FanTuningRanges.");
                }
                FanRange = *ptr;
            }

            fixed (List<Tuple<int, int>>* ptr = &_originalFanSpeed)
            {
                if (!_fanService.ADLX!.GetFanSpeeds(_adlxId, &ptr))
                {
                    throw new Exception("Could not get FanSpeed.");
                }
                _originalFanSpeed = *ptr;
            }
        }

        _wasZeroRPMEnabled = _fanService.ADLX.IsZeroRPMSupported(_adlxId) && _fanService.ADLX.IsZeroRPMEnabled(_adlxId);
        if (_wasZeroRPMEnabled)
        {
            _fanService.ADLX.SetZeroRPM(_adlxId, false);
        }

        IsSupported = true;

        Log.Debug(
            "[GpuAmdFanModel] GPU ID:{0}, Temp:{1}-{2}, Speed:{3}-{4}",
            _adlxId,
            FanRange.minTemperature,
            FanRange.maxTemperature,
            FanRange.minSpeed,
            FanRange.maxSpeed);
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
                if (Value != ConstantSpeed)
                {
                    Value = ConstantSpeed;
                    Write(ConstantSpeed);
                }
                break;
            case FanControlModes.Curve:
                var target = CalcTarget();
                if (target == null) { break; }

                target = Math.Max(FanRange.minSpeed, Math.Min(FanRange.maxSpeed, target.Value));
                if (Value != target)
                {
                    Value = target;
                    Write(Value);
                }
                break;
        }
    }

    private void Write(int? value)
    {
        if (!value.HasValue) { return; }

        var spd = Math.Max(FanRange.minSpeed, Math.Min(FanRange.maxSpeed, (int)value));
        spd = Math.Max(MinSpeed, Math.Min(MaxSpeed, spd));
        SetFanSpeed(spd);
    }

    private unsafe bool SetFanSpeed(int speed)
    {
        var res = _fanService.ADLX!.SetFanSpeed(_adlxId, speed);
        if (!res)
        {
            Log.Error("[GpuAmdFanModel] SetFanSpeed failed ID:{0}", _adlxId);
        }
        return res;
    }

    private unsafe bool SetFanSpeeds(List<Tuple<int, int>>* ptr)
    {
        var res = _fanService.ADLX!.SetFanSpeeds(_adlxId, ptr);
        if (!res)
        {
            Log.Error("[GpuAmdFanModel] SetFanSpeeds failed ID:{0}", _adlxId);
        }
        return res;
    }

    public unsafe void RestoreSettings()
    {
        if (_fanService.ADLX!.IsZeroRPMSupported(_adlxId) && _wasZeroRPMEnabled)
        {
            _fanService.ADLX.SetZeroRPM(_adlxId, true);
        }

        fixed (List<Tuple<int, int>>* ptr = &_originalFanSpeed)
        {
            SetFanSpeeds(ptr);
        }
        Log.Debug("[GpuAmdFanModel] Settings restored.");
    }
}
#pragma warning restore CS8500
