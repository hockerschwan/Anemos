using LibreHardwareMonitor.Hardware;

namespace Anemos.Models;

public class GpuNvidiaFanModel : FanModelBase
{
    private List<string> FanIds { get; } = new();

    private List<ISensor> Controls { get; } = new();

    public override FanControlModes ControlMode
    {
        get => base.ControlMode;
        set
        {
            if (base.ControlMode != value)
            {
                base.ControlMode = value;
                if (base.ControlMode == FanControlModes.Device)
                {
                    foreach (var c in Controls)
                    {
                        c.Control.SetDefault();
                    }
                }
                TargetValue = null;
                _refractoryPeriodCounter = 0;
                UpdateValue();
                UpdateProfile();
            }
        }
    }

    public GpuNvidiaFanModel(string id, string name, bool isHidden, int numFans) : base(id, name, isHidden)
    {
        var idDivided = id.Split("/");
        for (var i = 1; i <= numFans; ++i)
        {
            idDivided[^1] = i.ToString();
            FanIds.Add(string.Join("/", idDivided));
        }

        foreach (var fid in FanIds)
        {
            var s = _lhwmService.GetSensor(fid);
            if (s == null) { continue; }

            var c = FindControl(s);
            if (c == null) { continue; }

            Controls.Add(c);
            c.Control.SetDefault();
        }
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
                if (target == null || Control == null) { break; }

                target = Math.Max((int)Control.Control.MinSoftwareValue, Math.Min((int)Control.Control.MaxSoftwareValue, target.Value));
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

        foreach (var c in Controls)
        {
            c.Control.SetSoftware(value.Value);
        }
    }
}
