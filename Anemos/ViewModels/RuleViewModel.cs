using System.Collections.ObjectModel;
using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Anemos.ViewModels;

public partial class RuleViewModel : ObservableObject
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();
    private readonly IFanService _fanService = App.GetService<IFanService>();
    private readonly IRuleService _ruleService = App.GetService<IRuleService>();

    public RuleModel Model
    {
        get;
    }

    public ObservableCollection<FanProfile> Profiles
    {
        get;
    }

    public List<string> ConditionTypeNames
    {
        get;
    } =
    [
        "Rule_ConditionTypeNames_Time".GetLocalized(),
        "Rule_ConditionTypeNames_Process".GetLocalized(),
        "Rule_ConditionTypeNames_Sensor".GetLocalized()
    ];

    public List<string> RuleTypeNames
    {
        get;
    } =
    [
        "Rule_TypeNames_All".GetLocalized(),
        "Rule_TypeNames_Any".GetLocalized()
    ];

    private int _selectedRuleTypeIndex = -1;
    public int SelectedRuleTypeIndex
    {
        get => _selectedRuleTypeIndex;
        set
        {
            if (SetProperty(ref _selectedRuleTypeIndex, value))
            {
                Model.Type = (RuleType)value;
                _ruleService.Update();
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
                if (value < 0 || value >= _fanService.Profiles.Count)
                {
                    Model.ProfileId = string.Empty;
                }
                else
                {
                    Model.ProfileId = _fanService.Profiles.ElementAt(value).Id;
                }
                _ruleService.Update();
            }
        }
    }

    public bool CanIncreasePriority => _ruleService.Rules.First() != Model;
    public bool CanDecreasePriority => _ruleService.Rules.Last() != Model;

    public string IndexText => "Rule_IndexText".GetLocalized().Replace("$", (_ruleService.Rules.IndexOf(Model) + 1).ToString());

    private bool _editingName = false;
    public bool EditingName
    {
        get => _editingName;
        set => SetProperty(ref _editingName, value);
    }

    private readonly MessageHandler<object, FanProfilesChangedMessage> _fanProfilesChangedMessageHandler;

    public RuleViewModel(RuleModel model)
    {
        Model = model;

        _fanProfilesChangedMessageHandler = FanProfilesChangedMessageHandler;
        _messenger.Register(this, _fanProfilesChangedMessageHandler);

        Profiles = new(_fanService.Profiles);

        var profile = _fanService.GetProfile(Model.ProfileId);
        if (profile != null)
        {
            _selectedProfileIndex = Profiles.IndexOf(profile);
        }

        _selectedRuleTypeIndex = (int)Model.Type;
    }

    private void FanProfilesChangedMessageHandler(object recipient, FanProfilesChangedMessage message)
    {
        var removed = message.OldValue.Except(message.NewValue).ToList();
        foreach (var p in removed)
        {
            Profiles.Remove(p);
        }
        if (removed.Select(p => p.Id).Contains(Model.ProfileId))
        {
            SelectedProfileIndex = -1;
        }

        var added = message.NewValue.Except(message.OldValue).ToList();
        foreach (var p in added)
        {
            Profiles.Add(p);
        }
    }

    public void AddCondition(string param)
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
                    IncludeLower = true
                });
                break;
        }
    }

    public void RemoveCondition(RuleConditionBase condition)
    {
        Model.RemoveCondition(condition);
    }

    public void RemoveSelf()
    {
        _ruleService.RemoveRule(Model);
    }

    public void UpdatePriorityButtons()
    {
        OnPropertyChanged(nameof(CanIncreasePriority));
        OnPropertyChanged(nameof(CanDecreasePriority));
        OnPropertyChanged(nameof(IndexText));
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
