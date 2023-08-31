using System.ComponentModel;
using Anemos.Contracts.Services;
using Anemos.Models;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;

namespace Anemos.Services;

internal class RuleService : IRuleService
{
    private readonly IMessenger _messenger;
    private readonly ISettingsService _settingsService;
    private readonly IFanService _fanService;

    public List<RuleModel> Rules { get; } = new();

    public RuleModel? CurrentRule { get; private set; } = null;

    private string _defaultProfileId = string.Empty;
    public string DefaultProfileId
    {
        get => _defaultProfileId;
        set
        {
            if (value == string.Empty)
            {
                value = _fanService.ManualProfileId;
            }

            if (_defaultProfileId != value)
            {
                _defaultProfileId = value;

                if (_isLoaded)
                {
                    Save();
                }
            }
        }
    }

    private int _updateCounter = 0;

    private int UpdateIntervalCycles
    {
        get; set;
    }

    private bool _isUpdating;

    private bool _isLoaded;

    public RuleService(
        IMessenger messenger,
        ISettingsService settingsService,
        IFanService fanService)
    {
        _messenger = messenger;
        _settingsService = settingsService;
        _fanService = fanService;

        _messenger.Register<AppExitMessage>(this, AppExitMessageHandler);
        _messenger.Register<LhwmUpdateDoneMessage>(this, LhwmUpdateDoneMessageHandler);

        _settingsService.Settings.PropertyChanged += Settings_PropertyChanged;

        UpdateIntervalCycles = _settingsService.Settings.RulesUpdateIntervalCycles;

        _messenger.Send<ServiceStartupMessage>(new(GetType()));
        Log.Information("[Rule] Started");
    }

    private async void AppExitMessageHandler(object recipient, AppExitMessage message)
    {
        _messenger.UnregisterAll(this);
        while (true)
        {
            if (!_isUpdating) { break; }
            await Task.Delay(100);
        }
        _messenger.Send<ServiceShutDownMessage>(new(GetType()));
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
        _messenger.Send<RulesChangedMessage>(new(this, nameof(Rules), old, Rules));

        if (_isLoaded)
        {
            Update();
        }

        if (save)
        {
            Save();
        }
    }

    public void DecreasePriority(RuleModel rule)
    {
        var i = Rules.IndexOf(rule);
        if (i == -1 || i == Rules.Count - 1) { return; }

        var old = Rules.ToList();

        Rules.RemoveAt(i);
        Rules.Insert(i + 1, rule);

        _messenger.Send<RulesChangedMessage>(new(this, nameof(Rules), old, Rules));

        Update();

        Save();
    }

    public void IncreasePriority(RuleModel rule)
    {
        var i = Rules.IndexOf(rule);
        if (i < 1) { return; }

        var old = Rules.ToList();

        Rules.RemoveAt(i);
        Rules.Insert(i - 1, rule);

        _messenger.Send<RulesChangedMessage>(new(this, nameof(Rules), old, Rules));

        Update();

        Save();
    }

    private void LhwmUpdateDoneMessageHandler(object recipient, LhwmUpdateDoneMessage message)
    {
        ++_updateCounter;
        if (_updateCounter < UpdateIntervalCycles || _isUpdating) { return; }

        Update();
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
                        MemoryType = c.MemoryType,
                        MemoryLower = c.MemoryLower,
                        MemoryUpper = c.MemoryUpper,

                        SensorId = c.SensorId,
                        LowerValue = c.LowerValue,
                        UpperValue = c.UpperValue,
                        IncludeLower = c.IncludeLower,
                        IncludeUpper = c.IncludeUpper
                    })
                }),
        false);

        if (_fanService.GetProfile(_settingsService.Settings.RuleSettings.DefaultProfile) == null)
        {
            DefaultProfileId = _fanService.ManualProfileId;
            Save();
        }
        else
        {
            DefaultProfileId = _settingsService.Settings.RuleSettings.DefaultProfile;
        }

        _isLoaded = true;
    }

    public async Task LoadAsync()
    {
        await Task.Run(Load);
        Log.Debug("[Rule] Loaded");
    }

    public void RemoveRule(RuleModel rule)
    {
        if (!Rules.Contains(rule)) { return; }

        var old = Rules.ToList();
        Rules.Remove(rule);
        _messenger.Send<RulesChangedMessage>(new(this, nameof(Rules), old, Rules));

        Update();

        Save();
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
                            ProcessName = proc.ProcessName,
                            MemoryType = proc.MemoryLower == null && proc.MemoryUpper == null ? null : proc.MemoryType,
                            MemoryLower = proc.MemoryLower,
                            MemoryUpper = proc.MemoryUpper,
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
                            IncludeLower = sensor.IncludeLower,
                            IncludeUpper = sensor.IncludeUpper
                        };
                    }
                    return new();
                })
            });

        _settingsService.Save();
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_settingsService.Settings.RulesUpdateIntervalCycles))
        {
            UpdateIntervalCycles = _settingsService.Settings.RulesUpdateIntervalCycles;
            Update();
        }
    }

    public void Update()
    {
        _updateCounter = 0;
        _isUpdating = true;

        Parallel.ForEach(Rules, rule =>
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(
                Microsoft.UI.Dispatching.DispatcherQueuePriority.High,
                () => rule.Update());
        });

        var rule = Rules.FirstOrDefault(r => r!.ConditionsSatisfied && r!.ProfileId != string.Empty, null);
        if (rule != CurrentRule)
        {
            CurrentRule = rule;
        }

        if (rule?.ProfileId != _fanService.AutoProfileId)
        {
            _fanService.AutoProfileId = rule?.ProfileId ?? DefaultProfileId;
        }

        _isUpdating = false;
    }
}
