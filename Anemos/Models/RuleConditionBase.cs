using CommunityToolkit.Mvvm.ComponentModel;

namespace Anemos.Models;

public enum RuleConditionType
{
    Time, Process, Sensor
}

public class RuleConditionArg
{
    public RuleConditionType Type;

    public TimeOnly? TimeBeginning;
    public TimeOnly? TimeEnding;

    public string? ProcessName;

    public string? SensorId;
    public double? UpperValue;
    public double? LowerValue;
    public bool? UseUpperValue;
    public bool? UseLowerValue;
    public bool? IncludeUpper;
    public bool? IncludeLower;
}

public abstract class RuleConditionBase : ObservableObject
{
    public RuleModel Parent
    {
        get;
    }

    public RuleConditionType Type
    {
        get;
        private set;
    }

    private bool _isSatisfied;
    public bool IsSatisfied
    {
        get => _isSatisfied;
        private protected set => SetProperty(ref _isSatisfied, value);
    }

    public virtual string Text
    {
        get;
    } = string.Empty;

    public RuleConditionBase(RuleModel parent)
    {
        Parent = parent;
    }

    public virtual void Update()
    {
    }
}
