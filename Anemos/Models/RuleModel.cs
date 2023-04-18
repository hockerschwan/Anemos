using System.Collections.ObjectModel;
using Anemos.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Anemos.Models;

public enum RuleType
{
    All, Any
}

public class RuleArg
{
    public string Name = string.Empty;
    public RuleType Type = RuleType.All;
    public string ProfileId = string.Empty;
    public IEnumerable<RuleConditionArg> Conditions = Enumerable.Empty<RuleConditionArg>();
}

public class RuleModel : ObservableObject
{
    private readonly ISensorService _sensorService = App.GetService<ISensorService>();

    private readonly IRuleService _ruleService = App.GetService<IRuleService>();

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                _ruleService.Save();
            }
        }
    }

    private RuleType _type;
    public RuleType Type
    {
        get => _type;
        set
        {
            if (SetProperty(ref _type, value))
            {
                _ruleService.Save();
            }
        }
    }

    private string _profileId = string.Empty;
    public string ProfileId
    {
        get => _profileId;
        set
        {
            if (SetProperty(ref _profileId, value))
            {
                _ruleService.Save();
            }
        }
    }

    public ObservableCollection<RuleConditionBase> Conditions { get; } = new();

    public bool ConditionsSatisfied => Type switch
    {
        RuleType.All => Conditions.Any() && Conditions.All(c => c.IsSatisfied),
        RuleType.Any => Conditions.Any(c => c.IsSatisfied),
        _ => false,
    };

    public RuleModel(RuleArg arg)
    {
        _name = arg.Name;
        _profileId = arg.ProfileId;
        _type = arg.Type;

        var save = false;
        foreach (var cond in arg.Conditions)
        {
            if (cond.Type == RuleConditionType.Time && cond.TimeBeginning != null && cond.TimeEnding != null)
            {
                Conditions.Add(new TimeRuleCondition(this, cond.TimeBeginning.Value, cond.TimeEnding.Value));
            }
            else if (cond.Type == RuleConditionType.Process && cond.ProcessName != null)
            {
                Conditions.Add(new ProcessRuleCondition(this, cond.ProcessName));
            }
            else if (cond.Type == RuleConditionType.Sensor && !(cond.LowerValue == null && cond.UpperValue == null))
            {
                if (cond.SensorId != null && _sensorService.GetSensor(cond.SensorId) == null)
                {
                    cond.SensorId = string.Empty;
                    save = true;
                }
                Conditions.Add(new SensorRuleCondition(this, cond));
            }
        }

        if (save)
        {
            _ruleService.Save();
        }
    }

    public void Update()
    {
        foreach (var cond in Conditions)
        {
            cond.Update();
        }
        OnPropertyChanged(nameof(ConditionsSatisfied));
    }

    public void AddCondition(RuleConditionArg arg)
    {
        switch (arg.Type)
        {
            case RuleConditionType.Time:
                if (arg.TimeBeginning == null || arg.TimeEnding == null) { break; }
                Conditions.Add(new TimeRuleCondition(this, arg.TimeBeginning.Value, arg.TimeEnding.Value));
                break;
            case RuleConditionType.Process:
                if (arg.ProcessName == null) { break; }
                Conditions.Add(new ProcessRuleCondition(this, arg.ProcessName));
                break;
            case RuleConditionType.Sensor:
                if (arg.LowerValue == null && arg.UpperValue == null) { break; }
                Conditions.Add(new SensorRuleCondition(this, arg));
                break;
        }

        _ruleService.Save();
        _ruleService.Update();
    }

    public void RemoveCondition(RuleConditionBase item)
    {
        Conditions.Remove(item);
        _ruleService.Save();
        _ruleService.Update();
    }
}
