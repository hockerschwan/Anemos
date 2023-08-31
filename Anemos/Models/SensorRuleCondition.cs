using Anemos.Contracts.Services;
using Anemos.Helpers;
using CommunityToolkit.Mvvm.Messaging;

namespace Anemos.Models;

public class SensorRuleCondition : RuleConditionBase
{
    private readonly IMessenger _messenger;

    private readonly ISensorService _sensorService;

    public string _sensorId;
    public string SensorId
    {
        get => _sensorId;
        set
        {
            if (SetProperty(ref _sensorId, value))
            {
                OnPropertyChanged(nameof(Sensor));
                OnPropertyChanged(nameof(Text));
            }
        }
    }

    public SensorModelBase? Sensor => _sensorService.GetSensor(SensorId);

    private double? _upperValue;
    public double? UpperValue
    {
        get => _upperValue;
        set
        {
            if (SetProperty(ref _upperValue, value))
            {
                OnPropertyChanged(nameof(Text));
            }
        }
    }

    private double? _lowerValue;
    public double? LowerValue
    {
        get => _lowerValue;
        set
        {
            if (SetProperty(ref _lowerValue, value))
            {
                OnPropertyChanged(nameof(Text));
            }
        }
    }

    private bool _includeUpper;
    public bool IncludeUpper
    {
        get => _includeUpper;
        set
        {
            if (SetProperty(ref _includeUpper, value))
            {
                OnPropertyChanged(nameof(Text));
            }
        }
    }

    private bool _includeLower;
    public bool IncludeLower
    {
        get => _includeLower;
        set
        {
            if (SetProperty(ref _includeLower, value))
            {
                OnPropertyChanged(nameof(Text));
            }
        }
    }

    private readonly System.Text.StringBuilder _stringBuilder = new();

    public override string Text
    {
        get
        {
            _stringBuilder.Clear();

            if (LowerValue != null)
            {
                _stringBuilder.Append("RuleSensor_ValueText".GetLocalized().Replace("$", LowerValue.ToString()));
                _stringBuilder.Append(IncludeLower ? $" {"RuleSensorEditor_Signs_LEQ".GetLocalized()} " : $" {"RuleSensorEditor_Signs_LEQ".GetLocalized()} ");
            }

            _stringBuilder.Append("RuleSensor_T".GetLocalized());

            if (UpperValue != null)
            {
                _stringBuilder.Append(IncludeUpper ? $" {"RuleSensorEditor_Signs_LEQ".GetLocalized()} " : $" {"RuleSensorEditor_Signs_LEQ".GetLocalized()} ");
                _stringBuilder.Append("RuleSensor_ValueText".GetLocalized().Replace("$", UpperValue.ToString()));
            }

            if (Sensor != null)
            {
                _stringBuilder.Append($", {Sensor.LongName}");
            }

            return _stringBuilder.ToString();
        }
    }

    public SensorRuleCondition(RuleModel parent, RuleConditionArg arg) : base(parent)
    {
        _messenger = App.GetService<IMessenger>();
        _sensorService = App.GetService<ISensorService>();

        _sensorId = arg.SensorId ?? string.Empty;
        LowerValue = arg.LowerValue;
        UpperValue = arg.UpperValue;
        IncludeLower = arg.IncludeLower ?? false;
        IncludeUpper = arg.IncludeUpper ?? false;

        _messenger.Register<CustomSensorsChangedMessage>(this, CustomSensorsChangedMessageHandler);
    }

    private void CustomSensorsChangedMessageHandler(object recipient, CustomSensorsChangedMessage message)
    {
        var removed = message.OldValue.Except(message.NewValue);
        if (removed.Any() && SensorId != string.Empty && removed.Select(s => s.Id).Contains(SensorId))
        {
            SensorId = string.Empty;
        }
    }

    public override void Update()
    {
        if (Sensor?.Value == null || (LowerValue == null && UpperValue == null))
        {
            IsSatisfied = false;
            return;
        }

        if (LowerValue != null && ((IncludeLower && Sensor.Value < LowerValue) || (!IncludeLower && Sensor.Value <= LowerValue)))
        {
            IsSatisfied = false;
            return;
        }

        if (UpperValue != null && ((IncludeUpper && Sensor.Value > UpperValue) || (!IncludeUpper && Sensor.Value >= UpperValue)))
        {
            IsSatisfied = false;
            return;
        }

        IsSatisfied = true;
    }
}
