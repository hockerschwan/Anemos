using Anemos.Contracts.Services;
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
            }
        }
    }

    public SensorModelBase? Sensor => _sensorService.GetSensor(SensorId);

    private bool _useUpperValue;
    public bool UseUpperValue
    {
        get => _useUpperValue;
        set
        {
            if (SetProperty(ref _useUpperValue, value))
            {
                if (!_useUpperValue)
                {
                    UpperValue = null;
                }
            }
        }
    }

    private bool _useLowerValue;
    public bool UseLowerValue
    {
        get => _useLowerValue;
        set
        {
            if (SetProperty(ref _useLowerValue, value))
            {
                if (!_useLowerValue)
                {
                    LowerValue = null;
                }
            }
        }
    }

    public double? UpperValue
    {
        get; set;
    }

    public double? LowerValue
    {
        get; set;
    }

    public bool IncludeUpper
    {
        get; set;
    }

    public bool IncludeLower
    {
        get; set;
    }

    public override string Text
    {
        get
        {
            var str1 = $"{(UseLowerValue ? $"{LowerValue}℃ {(IncludeLower ? "≤" : "<")} " : string.Empty)}";
            var str2 = $"{(UseUpperValue ? $" {(IncludeUpper ? "≤" : "<")} {UpperValue}℃" : string.Empty)}";
            return $"{str1}T{str2}, {Sensor?.LongName}";
        }
    }

    public override bool IsSatisfied
    {
        get
        {
            if (Sensor?.Value == null)
            {
                return false;
            }

            if (!UseLowerValue && !UseUpperValue)
            {
                return false;
            }

            if (UseLowerValue)
            {
                if (IncludeLower && Sensor.Value < LowerValue)
                {
                    return false;
                }
                else if (Sensor.Value <= LowerValue)
                {
                    return false;
                }
            }

            if (UseUpperValue)
            {
                if (IncludeUpper && Sensor.Value > UpperValue)
                {
                    return false;
                }
                else if (Sensor.Value >= UpperValue)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public SensorRuleCondition(RuleModel parent, RuleConditionArg arg) : base(parent)
    {
        _messenger = App.GetService<IMessenger>();
        _sensorService = App.GetService<ISensorService>();

        _sensorId = arg.SensorId ?? string.Empty;
        LowerValue = arg.LowerValue;
        UpperValue = arg.UpperValue;
        _useLowerValue = arg.UseLowerValue ?? false;
        _useUpperValue = arg.UseUpperValue ?? false;
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

    public void UpdateText()
    {
        OnPropertyChanged(nameof(Text));
    }
}
