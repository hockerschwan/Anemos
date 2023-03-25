namespace Anemos.Models;

public class NormalFanModel : FanModelBase
{
    public NormalFanModel(string id, string name) : base(id, name)
    {
    }

    public override void Update()
    {
        switch (ControlMode)
        {
            case FanControlModes.Device:
                break;
            case FanControlModes.Constant:
                Write(ConstantSpeed);
                break;
            case FanControlModes.Curve:
                Value = CalcTarget();
                Write(Value);
                break;
        }

        OnPropertyChanged(nameof(CurrentRPM));
        OnPropertyChanged(nameof(CurrentPercent));
    }

    private void Write(int? value)
    {
        if (!value.HasValue || Control == null) { return; }

        Control.Control.SetSoftware((float)value);
    }
}
