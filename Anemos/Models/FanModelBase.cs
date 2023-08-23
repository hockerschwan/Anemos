using System.Diagnostics;
using Anemos.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using LibreHardwareMonitor.Hardware;

namespace Anemos.Models;

public enum FanControlModes
{
    Device, Constant, Curve
}

[DebuggerDisplay("{Name}, {Id}")]
public class FanProfile : ObservableObject
{
    public string Id { get; set; } = string.Empty;

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public IEnumerable<FanSettings_ProfileItem> Fans { get; set; } = Enumerable.Empty<FanSettings_ProfileItem>();
}

public class FanOptionsResult
{
    public int MaxSpeed;
    public int MinSpeed;
    public int DeltaLimitUp;
    public int DeltaLimitDown;
    public int RefractoryPeriodCyclesDown;
}

[DebuggerDisplay("{Name}")]
public abstract class FanModelBase : ObservableObject
{
    private protected readonly ILhwmService _lhwmService = App.GetService<ILhwmService>();

    private protected readonly ICurveService _curveService = App.GetService<ICurveService>();

    private protected readonly IFanService _fanService = App.GetService<IFanService>();

    private protected string _id = string.Empty;
    public string Id => _id;

    private protected string _name = string.Empty;
    public virtual string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                _fanService.Save();
            }
        }
    }

    private bool _isHidden;
    public bool IsHidden
    {
        get => _isHidden;
        set
        {
            if (SetProperty(ref _isHidden, value))
            {
                _fanService.Save();
            }
        }
    }

    private protected ISensor? Sensor => _lhwmService.GetSensor(Id);
    public int? CurrentRPM
    {
        get
        {
            if ((bool)(Sensor?.Value.HasValue)!)
            {
                return (int)float.Round(Sensor.Value!.Value);
            }
            return null;
        }
    }

    private protected ISensor? Control
    {
        get;
    }
    public virtual double? CurrentPercent
    {
        get
        {
            switch (ControlMode)
            {
                case FanControlModes.Constant:
                    return ConstantSpeed;
                case FanControlModes.Curve:
                    return TargetValue;
                default:
                    if ((bool)Control?.Value.HasValue!)
                    {
                        return double.Round(Control.Value!.Value, 1);
                    }
                    return null;
            }
        }
    }

    private protected FanControlModes _controlMode = FanControlModes.Device;
    public virtual FanControlModes ControlMode
    {
        get => _controlMode;
        set
        {
            if (SetProperty(ref _controlMode, value))
            {
                if (_controlMode == FanControlModes.Device)
                {
                    Control?.Control.SetDefault();
                }
                TargetValue = null;
                _refractoryPeriodCounter = 0;
                UpdateValue();
                UpdateProfile();
            }
        }
    }

    private protected string _curveId = string.Empty;
    public string CurveId
    {
        get => _curveId;
        set
        {
            if (SetProperty(ref _curveId, value))
            {
                OnPropertyChanged(nameof(CurveModel));
                if (CurveModel == null)
                {
                    Control?.Control.SetDefault();
                }

                UpdateProfile();
            }
        }
    }

    public CurveModelBase? CurveModel => _curveService.GetCurve(CurveId);

    private protected int _constantSpeed = 50;
    public int ConstantSpeed
    {
        get => _constantSpeed;
        set
        {
            if (SetProperty(ref _constantSpeed, value))
            {
                UpdateProfile();
            }
        }
    }

    private protected int _maxSpeed = 100;
    public int MaxSpeed
    {
        get => _maxSpeed;
        private protected set
        {
            if (SetProperty(ref _maxSpeed, value))
            {
                UpdateProfile();
            }
        }
    }

    private protected int _minSpeed = 0;
    public int MinSpeed
    {
        get => _minSpeed;
        private protected set
        {
            if (SetProperty(ref _minSpeed, value))
            {
                UpdateProfile();
            }
        }
    }

    private protected int _deltaLimitUp = 0;
    public int DeltaLimitUp
    {
        get => _deltaLimitUp;
        private protected set
        {
            if (SetProperty(ref _deltaLimitUp, value))
            {
                UpdateProfile();
            }
        }
    }

    private protected int _deltaLimitDown = 0;
    public int DeltaLimitDown
    {
        get => _deltaLimitDown;
        private protected set
        {
            if (SetProperty(ref _deltaLimitDown, value))
            {
                UpdateProfile();
            }
        }
    }

    private protected int _refractoryPeriodCounter = 0;

    private protected int _refractoryPeriodCyclesDown = 0;
    public int RefractoryPeriodCyclesDown
    {
        get => _refractoryPeriodCyclesDown;
        private protected set
        {
            if (SetProperty(ref _refractoryPeriodCyclesDown, value))
            {
                UpdateProfile();
            }
        }
    }

    private protected int? _targetValue;
    public int? TargetValue
    {
        get => _targetValue;
        private protected set => SetProperty(ref _targetValue, value);
    }

    private bool _updateProfile = true;

    public FanModelBase(string id, string name, bool isHidden)
    {
        if (id == string.Empty)
        {
            throw new ArgumentException("ID can't be empty.", nameof(id));
        }
        _id = id;

        if (Sensor == null)
        {
            throw new ArgumentException($"Could not find ISensor with ID:{Id}", nameof(id));
        }

        _name = name;
        _isHidden = isHidden;

        Control = FindControl(Sensor);
        Control?.Control.SetDefault();
    }

    private protected ISensor? FindControl(ISensor fan)
    {
        var id = fan.Identifier.ToString().Replace("fan", "control");
        return _lhwmService.GetSensor(id);
    }

    public virtual void Update()
    {
        UpdateValue();

        OnPropertyChanged(nameof(CurrentRPM));
        OnPropertyChanged(nameof(CurrentPercent));
    }

    private protected virtual void UpdateValue()
    {
    }

    private protected int? CalcTarget()
    {
        if (CurveModel == null || CurveModel.Output == null)
        {
            return null;
        }

        if (TargetValue == null)
        {
            return Math.Min(MaxSpeed, Math.Max(MinSpeed, (int)double.Round(CurveModel.Output.Value)));
        }

        if (RefractoryPeriodCyclesDown > 0)
        {
            ++_refractoryPeriodCounter;
            if (_refractoryPeriodCounter <= RefractoryPeriodCyclesDown && CurveModel.Output < TargetValue)
            {
                return Math.Min(MaxSpeed, Math.Max(MinSpeed, TargetValue.Value));
            }

            _refractoryPeriodCounter = 0;
        }

        if (CurveModel.Output > TargetValue)
        {
            var diff = CurveModel.Output - TargetValue.Value;
            if (DeltaLimitUp == 0 || diff <= DeltaLimitUp)
            {
                return Math.Min(MaxSpeed, Math.Max(MinSpeed, (int)double.Round(CurveModel.Output.Value)));
            }
            return Math.Min(MaxSpeed, Math.Max(MinSpeed, (int)double.Round(TargetValue.Value) + DeltaLimitUp));
        }
        else
        {
            var diff = TargetValue.Value - CurveModel.Output;
            if (DeltaLimitDown == 0 || diff <= DeltaLimitDown)
            {
                return Math.Min(MaxSpeed, Math.Max(MinSpeed, (int)double.Round(CurveModel.Output.Value)));
            }
            return Math.Min(MaxSpeed, Math.Max(MinSpeed, (int)double.Round(TargetValue.Value) - DeltaLimitDown));
        }
    }

    public void LoadProfile(FanSettings_ProfileItem item)
    {
        _updateProfile = false;

        CurveId = item.CurveId;
        ConstantSpeed = item.ConstantSpeed;
        MaxSpeed = item.MaxSpeed;
        MinSpeed = item.MinSpeed;
        DeltaLimitUp = item.DeltaLimitUp;
        DeltaLimitDown = item.DeltaLimitDown;
        RefractoryPeriodCyclesDown = item.RefractoryPeriodCyclesDown;
        ControlMode = item.Mode;

        _updateProfile = true;
    }

    public virtual void SetOptions(FanOptionsResult options)
    {
        MaxSpeed = options.MaxSpeed;
        MinSpeed = options.MinSpeed;
        DeltaLimitUp = options.DeltaLimitUp;
        DeltaLimitDown = options.DeltaLimitDown;
        RefractoryPeriodCyclesDown = options.RefractoryPeriodCyclesDown;
    }

    private protected void UpdateProfile()
    {
        if (_updateProfile)
        {
            _fanService.UpdateCurrentProfile();
        }
    }
}
