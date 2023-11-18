using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Anemos.Models;

public enum RuleConditionType
{
    Time, Process, Sensor
}

[DebuggerDisplay("{Type}")]
public class RuleConditionArg
{
    public RuleConditionType Type;

    public TimeOnly? TimeBeginning;
    public TimeOnly? TimeEnding;

    public string? ProcessName;
    public int? MemoryType;
    public int? MemoryLower;
    public int? MemoryUpper;

    public string? SensorId;
    public double? UpperValue;
    public double? LowerValue;
    public bool? IncludeUpper;
    public bool? IncludeLower;
}

[DebuggerDisplay("{Text}")]
public abstract class RuleConditionBase(RuleModel parent) : ObservableObject
{
    public RuleModel Parent { get; } = parent;

    public RuleConditionType Type
    {
        get; private set;
    }

    private bool _isSatisfied;
    public bool IsSatisfied
    {
        get => _isSatisfied;
        private protected set => SetProperty(ref _isSatisfied, value);
    }

    public virtual string Text { get; } = string.Empty;

    public virtual void Update()
    {
    }
}
