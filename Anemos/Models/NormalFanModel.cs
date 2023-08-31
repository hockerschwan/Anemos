namespace Anemos.Models;

public class NormalFanModel : FanModelBase
{
    public NormalFanModel(string id, string name, bool isHidden) : base(id, name, isHidden)
    {
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
                var v = CalcTarget();
                if (v != TargetValue)
                {
                    TargetValue = v;
                    Write(TargetValue);
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
