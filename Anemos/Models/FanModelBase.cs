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
    public decimal? CurrentRPM
    {
        get
        {
            if (Sensor == null || Sensor.Value == null)
            {
                return null;
            }
            return decimal.Round((decimal)Sensor.Value, 0);
        }
    }

    private protected ISensor? Control
    {
        get;
    }
    public decimal? CurrentPercent
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
                        return decimal.Round((decimal)Control.Value!, 1);
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

    private ISensor? FindControl(ISensor fan)
    {
        var id = fan.Identifier.ToString().Replace("fan", "control");
        return _lhwmService.GetSensor(id);
    }

    public virtual void Update()
    {
    }

    private protected int? CalcTarget()
    {
        if (CurveModel == null || CurveModel.Value == null)
        {
            return null;
        }

        if (CurrentPercent == null)
        {
            return (int)decimal.Round(CurveModel.Value.Value, 0);
        }

        if (CurveModel.Value > CurrentPercent)
        {
            var diff = CurveModel.Value - CurrentPercent;
            if (DeltaLimitUp == 0 || diff <= DeltaLimitUp)
            {
                return (int)decimal.Round(CurveModel.Value.Value, 0);
            }
            return (int)decimal.Round(CurrentPercent.Value, 0) + DeltaLimitUp;
        }
        else
        {
            var diff = CurrentPercent - CurveModel.Value;
            if (DeltaLimitDown == 0 || diff <= DeltaLimitDown)
            {
                return (int)decimal.Round(CurveModel.Value.Value, 0);
            }
            return (int)decimal.Round(CurrentPercent.Value, 0) - DeltaLimitDown;
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
        ControlMode = item.Mode;

        _updateProfile = true;
    }

    public virtual void SetOptions(FanOptionsResult options)
    {
        MaxSpeed = options.MaxSpeed;
        MinSpeed = options.MinSpeed;
        DeltaLimitUp = options.DeltaLimitUp;
        DeltaLimitDown = options.DeltaLimitDown;
    }

    private void UpdateProfile()
    {
        if (_updateProfile)
        {
            _fanService.UpdateCurrentProfile();
        }
    }
}
