namespace Anemos.Models;

public class ReadOnlyFanModel : FanModelBase
{
    public override FanControlModes ControlMode => FanControlModes.Device;

    public ReadOnlyFanModel(string id, string name, bool isHidden) : base(id, name, isHidden) { }
}
