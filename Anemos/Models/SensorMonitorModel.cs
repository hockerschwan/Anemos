using Anemos.Contracts.Services;

namespace Anemos.Models;

public class SensorMonitorModel : MonitorModelBase
{
    private readonly ISensorService _sensorService = App.GetService<ISensorService>();

    private SensorModelBase? Model;

    public override string SourceId
    {
        get => _sourceId;
        set
        {
            if (SetProperty(ref _sourceId, value))
            {
                Model = _sensorService.GetSensor(_sourceId);
                History.EnqueueRange(Enumerable.Repeat<double?>(0.0, History.Capacity));
                Update(true);
                _monitorService.Save();
            }
        }
    }

    public override MonitorSourceType SourceType => MonitorSourceType.Sensor;

    public SensorMonitorModel(MonitorArg arg) : base(arg)
    {
        Model = _sensorService.GetSensor(_sourceId);
    }

    private protected override void UpdateTooltip()
    {
        _stringBuilder.Clear();

        if (Model == null) { return; }

        _stringBuilder.Append(Model.Name);
        _stringBuilder.Append(Value.HasValue ? $"\n{double.Round(Value.Value, 1)}℃" : "\n---");
    }

    private protected override void UpdateValue()
    {
        if (Model == null)
        {
            Value = null;
            return;
        }

        Value = Model?.Value;
        History.Enqueue(Value);
    }
}
