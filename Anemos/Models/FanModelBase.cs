using Anemos.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using LibreHardwareMonitor.Hardware;

namespace Anemos.Models;

public enum FanControlModes
{
    Device, Constant, Curve
}

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
    public int RefractoryPeriodTicksDown;
}

public class FanModelBase : ObservableObject
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
                    return Value;
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
                Value = null;
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
                UpdateProfile();
            }
        }
    }

    public CurveModel? CurveModel => _curveService.GetCurve(CurveId);

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

    private protected int _refractoryPeriodTicksDown = 0;
    public int RefractoryPeriodTicksDown
    {
        get => _refractoryPeriodTicksDown;
        private protected set
        {
            if (SetProperty(ref _refractoryPeriodTicksDown, value))
            {
                UpdateProfile();
            }
        }
    }

    private protected int? _value;
    public int? Value
    {
        get => _value;
        private protected set => SetProperty(ref _value, value);
    }

    private bool _updateProfile = true;

    public FanModelBase(string id, string name)
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
        if (CurveModel == null || CurveModel.Value == null)
        {
            return null;
        }

        if (Value == null)
        {
            return Math.Min(MaxSpeed, Math.Max(MinSpeed, (int)double.Round(CurveModel.Value.Value)));
        }

        if (RefractoryPeriodTicksDown > 0)
        {
            ++_refractoryPeriodCounter;
            if (_refractoryPeriodCounter <= RefractoryPeriodTicksDown && CurveModel.Value < Value)
            {
                return Math.Min(MaxSpeed, Math.Max(MinSpeed, Value.Value));
            }

            _refractoryPeriodCounter = 0;
        }

        if (CurveModel.Value > Value)
        {
            var diff = CurveModel.Value - Value.Value;
            if (DeltaLimitUp == 0 || diff <= DeltaLimitUp)
            {
                return Math.Min(MaxSpeed, Math.Max(MinSpeed, (int)double.Round(CurveModel.Value.Value)));
            }
            return Math.Min(MaxSpeed, Math.Max(MinSpeed, (int)double.Round(Value.Value) + DeltaLimitUp));
        }
        else
        {
            var diff = Value.Value - CurveModel.Value;
            if (DeltaLimitDown == 0 || diff <= DeltaLimitDown)
            {
                return Math.Min(MaxSpeed, Math.Max(MinSpeed, (int)double.Round(CurveModel.Value.Value)));
            }
            return Math.Min(MaxSpeed, Math.Max(MinSpeed, (int)double.Round(Value.Value) - DeltaLimitDown));
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
        RefractoryPeriodTicksDown = item.RefractoryPeriodTicksDown;
        ControlMode = item.Mode;

        _updateProfile = true;
    }

    public virtual void SetOptions(FanOptionsResult options)
    {
        MaxSpeed = options.MaxSpeed;
        MinSpeed = options.MinSpeed;
        DeltaLimitUp = options.DeltaLimitUp;
        DeltaLimitDown = options.DeltaLimitDown;
        RefractoryPeriodTicksDown = options.RefractoryPeriodTicksDown;
    }

    private protected void UpdateProfile()
    {
        if (_updateProfile)
        {
            _fanService.UpdateCurrentProfile();
        }
    }
}
