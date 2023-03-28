using CommunityToolkit.Mvvm.ComponentModel;

namespace Anemos.Models;

public enum RuleConditionType
{
    Time, Process
}

public class RuleConditionArg
{
    public RuleConditionType Type;

    public TimeOnly? TimeBeginning;
    public TimeOnly? TimeEnding;

    public string? ProcessName;
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

    public virtual bool IsSatisfied
    {
        get;
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
        OnPropertyChanged(nameof(IsSatisfied));
    }
}
