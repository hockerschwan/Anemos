namespace Anemos.Models;

public class NormalFanModel : FanModelBase
{
    public override FanControlModes ControlMode
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

    public NormalFanModel(string id, string name) : base(id, name)
    {
    }

    public override void Update()
    {
        UpdateValue();

        OnPropertyChanged(nameof(CurrentRPM));
        OnPropertyChanged(nameof(CurrentPercent));
    }

    private void UpdateValue()
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
                var v = CalcTarget();
                if (v != Value)
                {
                    Value = v;
                    Write(Value);
                }
                break;
        }
    }

    private void Write(int? value)
    {
        if (!value.HasValue || Control == null) { return; }

        Control.Control.SetSoftware((float)value);
    }
}
