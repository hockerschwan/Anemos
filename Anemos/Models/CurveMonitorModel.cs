using Anemos.Contracts.Services;

namespace Anemos.Models;

public class CurveMonitorModel : MonitorModelBase
{
    private readonly ICurveService _curveService = App.GetService<ICurveService>();

    private CurveModelBase? Model;

    public override string SourceId
    {
        get => _sourceId;
        set
        {
            if (SetProperty(ref _sourceId, value))
            {
                Model = _curveService.GetCurve(_sourceId);
                History.Clear();
                Update(true);
                _monitorService.Save();
            }
        }
    }

    public override MonitorSourceType SourceType => MonitorSourceType.Curve;

    public CurveMonitorModel(MonitorArg arg) : base(arg)
    {
        Model = _curveService.GetCurve(_sourceId);
    }

    private protected override void UpdateTooltip()
    {
        _stringBuilder.Clear();

        if (Model == null) { return; }

        _stringBuilder.Append(Model.Name);
        _stringBuilder.Append(Value.HasValue ? $"\n{double.Round(Value.Value, 1)}%" : "\n---");
    }

    private protected override void UpdateValue()
    {
        if (Model == null)
        {
            Value = null;
            return;
        }

        Value = Model?.Output;
        History.Enqueue(Value ?? 0.0);
    }
}
