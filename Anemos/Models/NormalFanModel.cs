namespace Anemos.Models;

public class NormalFanModel : FanModelBase
{
    public NormalFanModel(string id, string name) : base(id, name)
    {
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
