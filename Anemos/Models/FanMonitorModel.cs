using Anemos.Contracts.Services;

namespace Anemos.Models;

public sealed class FanMonitorModel : MonitorModelBase
{
    private readonly IFanService _fanService = App.GetService<IFanService>();

    private FanModelBase? Model;

    public override string SourceId
    {
        get => _sourceId;
        set
        {
            if (SetProperty(ref _sourceId, value))
            {
                Model = _fanService.GetFanModel(_sourceId);
                History.EnqueueRange(Enumerable.Repeat<double?>(0.0, History.Capacity));
                Update(true);
                _monitorService.Save();
            }
        }
    }

    public override MonitorSourceType SourceType => MonitorSourceType.Fan;

    public FanMonitorModel(MonitorArg arg) : base(arg)
    {
        Model = _fanService.GetFanModel(_sourceId);
    }

    private protected override void UpdateTooltip()
    {
        _stringBuilder.Clear();

        if (Model == null) { return; }

        _stringBuilder.Append(Model.Name);
        _stringBuilder.Append(Value.HasValue ? $"\n{double.Round(Value.Value, 1)}%" : "\n---");
        if (Model.CurrentRPM.HasValue)
        {
            _stringBuilder.Append($"\n{Model.CurrentRPM.Value}RPM");
        }
    }

    private protected override void UpdateValue()
    {
        if (Model == null)
        {
            Value = null;
            return;
        }

        Value = Model?.CurrentPercent;
        History.Enqueue(Value);
    }
}
