using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Anemos.ViewModels;

public partial class RuleViewModel : ObservableRecipient
{
    private readonly IFanService _fanService = App.GetService<IFanService>();

    private readonly IRuleService _ruleService = App.GetService<IRuleService>();

    public IEnumerable<FanProfile> Profiles => _fanService.Profiles;

    public List<string> ConditionTypeNames
    {
        get;
    } = new()
    {
        "Rule_ConditionType_Time".GetLocalized(),
        "Rule_ConditionType_Process".GetLocalized(),
        "Rule_ConditionType_Sensor".GetLocalized()
    };

    public List<string> RuleTypeNames
    {
        get;
    } = new() { "Rule_RuleType_All".GetLocalized(), "Rule_RuleType_Any".GetLocalized() };

    private int _selectedRuleTypeIndex = -1;
    public int SelectedRuleTypeIndex
    {
        get => _selectedRuleTypeIndex;
        set
        {
            if (SetProperty(ref _selectedRuleTypeIndex, value))
            {
                Model.Type = (RuleType)value;
                Model.Update();
                OnPropertyChanged(nameof(Model.ConditionsSatisfied));
            }
        }
    }

    private int _selectedProfileIndex = -1;
    public int SelectedProfileIndex
    {
        get => _selectedProfileIndex;
        set
        {
            if (SetProperty(ref _selectedProfileIndex, value))
            {
                Model.ProfileId = _fanService.Profiles.ElementAt(value).Id;
            }
        }
    }

    public RuleModel Model
    {
        get;
    }

    public bool CanIncreasePriority => _ruleService.Rules.First() != Model;

    public bool CanDecreasePriority => _ruleService.Rules.Last() != Model;

    public RuleViewModel(RuleModel model)
    {
        Model = model;

        var profile = _fanService.GetProfile(Model.ProfileId);
        if (profile != null)
        {
            _selectedProfileIndex = _fanService.Profiles.IndexOf(profile);
        }

        _selectedRuleTypeIndex = (int)Model.Type;
    }

    public void UpdateArrows()
    {
        OnPropertyChanged(nameof(CanIncreasePriority));
        OnPropertyChanged(nameof(CanDecreasePriority));
    }

    [RelayCommand]
    private void AddCondition(string param)
    {
        switch (ConditionTypeNames.IndexOf(param))
        {
            case 0:
                Model.AddCondition(new()
                {
                    Type = RuleConditionType.Time,
                    TimeBeginning = new TimeOnly(9, 0),
                    TimeEnding = new TimeOnly(18, 0)
                });
                break;
            case 1:
                Model.AddCondition(new()
                {
                    Type = RuleConditionType.Process,
                    ProcessName = "explorer"
                });
                break;
            case 2:
                Model.AddCondition(new()
                {
                    Type = RuleConditionType.Sensor,
                    SensorId = string.Empty,
                    LowerValue = 50,
                    UseLowerValue = true,
                    IncludeLower = true
                });
                break;
        }
    }

    [RelayCommand]
    private void RemoveCondition(RuleConditionBase condition)
    {
        Model.RemoveCondition(condition);
    }

    [RelayCommand]
    private void RemoveSelf()
    {
        _ruleService.RemoveRule(Model);
    }

    [RelayCommand]
    private void IncreasePriority()
    {
        _ruleService.IncreasePriority(Model);
    }

    [RelayCommand]
    private void DecreasePriority()
    {
        _ruleService.DecreasePriority(Model);
    }
}
