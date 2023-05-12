using System.Collections.ObjectModel;
using Anemos.Contracts.Services;
using Anemos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;

namespace Anemos.Services;
public class RuleService : ObservableRecipient, IRuleService
{
    private readonly ISettingsService _settingsService;

    private readonly IFanService _fanService;

    public RangeObservableCollection<RuleModel> Rules { get; } = new();

    public RuleModel? CurrentRule
    {
        get; private set;
    }

    private string _defaultProfileId = string.Empty;
    public string DefaultProfileId
    {
        get => _defaultProfileId;
        set
        {
            if (value == string.Empty)
            {
                value = _fanService.CurrentProfileId;
            }

            if (SetProperty(ref _defaultProfileId, value))
            {
                OnPropertyChanged(nameof(DefaultProfile));
                if (_isLoaded)
                {
                    Save();
                }
            }
        }
    }

    public FanProfile DefaultProfile => _fanService.GetProfile(DefaultProfileId) ?? _fanService.CurrentProfile;

    private int _updateCounter = 0;

    private int UpdateIntervalCycles => _settingsService.Settings.RulesUpdateIntervalCycles;

    private bool _shutdownRequested;

    private bool _isUpdating;

    private bool _isLoaded;

    public RuleService(ISettingsService settingsService, IFanService fanService)
    {
        Messenger.Register<AppExitMessage>(this, AppExitMessageHandler);
        Messenger.Register<LhwmUpdateDoneMessage>(this, LhwmUpdateDoneMessageHandler);

        _settingsService = settingsService;
        _fanService = fanService;

        _settingsService.Settings.PropertyChanged += Settings_PropertyChanged;

        Log.Debug("[RuleService] Started");
    }

    public async Task InitializeAsync()
    {
        Load();

        Update();

        Log.Debug("[RuleService] Loaded");
        await Task.CompletedTask;
    }

    private async void AppExitMessageHandler(object recipient, AppExitMessage message)
    {
        _shutdownRequested = true;
        Messenger.Unregister<LhwmUpdateDoneMessage>(this);
        while (true)
        {
            if (!_isUpdating) { break; }
            await Task.Delay(100);
        }
        Messenger.Send(new ServiceShutDownMessage(GetType().GetInterface("IRuleService")!));
    }

    private void LhwmUpdateDoneMessageHandler(object recipient, LhwmUpdateDoneMessage message)
    {
        ++_updateCounter;
        if (_updateCounter < UpdateIntervalCycles) { return; }

        if (!_isUpdating)
        {
            Update();
        }
    }

    private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_settingsService.Settings.RulesUpdateIntervalCycles))
        {
            Update();
            OnPropertyChanged(nameof(UpdateIntervalCycles));
        }
    }

    public void Update()
    {
        _updateCounter = 0;

        _isUpdating = true;

        foreach (var rule in Rules)
        {
            if (_shutdownRequested)
            {
                _isUpdating = false;
                return;
            }
            rule.Update();
        }

        if (_fanService.UseRules || !_isLoaded)
        {
            var rule = Rules.FirstOrDefault(r => r!.ConditionsSatisfied, null);
            if (rule?.ProfileId != CurrentRule?.ProfileId || !_isLoaded)
            {
                if (rule != CurrentRule)
                {
                    CurrentRule = rule;
                }

                if (rule == null)
                {
                    Messenger.Send(new RuleSwitchedMessage(DefaultProfileId));
                }
                else
                {
                    Messenger.Send(new RuleSwitchedMessage(rule.ProfileId));
                }
            }
        }
        else if (CurrentRule != null)
        {
            CurrentRule = null;
        }

        _isUpdating = false;
    }

    public void AddRule(RuleArg arg)
    {
        AddRules(new RuleArg[] { arg });
    }

    public void AddRules(IEnumerable<RuleArg> args, bool save = true)
    {
        var old = Rules.ToList();
        var models = args.Select(arg => new RuleModel(arg));
        Rules.AddRange(models);
        Messenger.Send(new RulesChangedMessage(this, nameof(Rules), old, Rules));

        Update();

        if (save)
        {
            Save();
        }
    }

    public void RemoveRule(RuleModel rule)
    {
        if (!Rules.Contains(rule)) { return; }

        var old = Rules.ToList();
        Rules.Remove(rule);
        Messenger.Send(new RulesChangedMessage(this, nameof(Rules), old, Rules));

        Update();

        Save();
    }

    private void Load()
    {
        AddRules(
            _settingsService.Settings.RuleSettings.Rules.Select(
                r => new RuleArg()
                {
                    Name = r.Name,
                    ProfileId = r.ProfileId,
                    Type = r.Type,
                    Conditions = r.Conditions.Select(c => new RuleConditionArg()
                    {
                        Type = c.Type,
                        TimeBeginning = c.TimeBeginning,
                        TimeEnding = c.TimeEnding,
                        ProcessName = c.ProcessName,
                        SensorId = c.SensorId,
                        LowerValue = c.LowerValue,
                        UpperValue = c.UpperValue,
                        UseLowerValue = c.UseLowerValue,
                        UseUpperValue = c.UseUpperValue,
                        IncludeLower = c.IncludeLower,
                        IncludeUpper = c.IncludeUpper
                    })
                }),
        false);

        if (_fanService.GetProfile(_settingsService.Settings.RuleSettings.DefaultProfile) == null)
        {
            DefaultProfileId = _fanService.CurrentProfileId;
            Save();
        }
        else
        {
            DefaultProfileId = _settingsService.Settings.RuleSettings.DefaultProfile;
        }

        _isLoaded = true;
    }

    public void Save()
    {
        _settingsService.Settings.RuleSettings.DefaultProfile = DefaultProfileId;

        _settingsService.Settings.RuleSettings.Rules = Rules.Select(
            rm => new RuleSettings_Rule()
            {
                Name = rm.Name,
                ProfileId = rm.ProfileId,
                Type = rm.Type,
                Conditions = rm.Conditions.Select(c =>
                {
                    if (c is TimeRuleCondition time)
                    {
                        return new RuleSettings_Condition()
                        {
                            Type = RuleConditionType.Time,
                            TimeBeginning = time.TimeBeginning,
                            TimeEnding = time.TimeEnding
                        };
                    }
                    else if (c is ProcessRuleCondition proc)
                    {
                        return new RuleSettings_Condition()
                        {
                            Type = RuleConditionType.Process,
                            ProcessName = proc.ProcessName
                        };
                    }
                    else if (c is SensorRuleCondition sensor)
                    {
                        return new RuleSettings_Condition()
                        {
                            Type = RuleConditionType.Sensor,
                            SensorId = sensor.SensorId,
                            LowerValue = sensor.LowerValue,
                            UpperValue = sensor.UpperValue,
                            UseLowerValue = sensor.UseLowerValue,
                            UseUpperValue = sensor.UseUpperValue,
                            IncludeLower = sensor.IncludeLower,
                            IncludeUpper = sensor.IncludeUpper
                        };
                    }
                    return new();
                })
            });

        _settingsService.Save();
    }

    public void IncreasePriority(RuleModel rule)
    {
        var i = Rules.IndexOf(rule);
        if (i < 1) { return; }

        var old = Rules.ToList();

        Rules.RemoveAt(i);
        Rules.Insert(i - 1, rule);

        Messenger.Send(new RulesChangedMessage(this, nameof(Rules), old, Rules));

        Update();

        Save();
    }

    public void DecreasePriority(RuleModel rule)
    {
        var i = Rules.IndexOf(rule);
        if (i == -1 || i == Rules.Count - 1) { return; }

        var old = Rules.ToList();

        Rules.RemoveAt(i);
        Rules.Insert(i + 1, rule);

        Messenger.Send(new RulesChangedMessage(this, nameof(Rules), old, Rules));

        Update();

        Save();
    }
}
