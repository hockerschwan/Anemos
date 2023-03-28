using System.Collections.ObjectModel;
using Anemos.Contracts.Services;
using Anemos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Serilog;

namespace Anemos.Services;
public class RuleService : ObservableRecipient, IRuleService
{
    private readonly ISettingsService _settingsService;

    private readonly IFanService _fanService;

    public RangeObservableCollection<RuleModel> Rules { get; } = new();

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
                Save();
            }
        }
    }

    public FanProfile DefaultProfile => _fanService.GetProfile(DefaultProfileId) ?? _fanService.CurrentProfile;

    private readonly DispatcherTimer _timer = new();

    private bool _shutdownRequested;

    private bool _isUpdating;

    public RuleService(ISettingsService settingsService, IFanService fanService)
    {
        Messenger.Register<AppExitMessage>(this, AppExitMessageHandler);

        _settingsService = settingsService;
        _fanService = fanService;

        Log.Debug("[ProfileRuleService] Started");
    }

    public async Task InitializeAsync()
    {
        Load();

        Update();

        _timer.Interval = new(0, 0, 10);
        _timer.Tick += Timer_Tick;
        _timer.Start();

        Log.Debug("[ProfileRuleService] Loaded");
        await Task.CompletedTask;
    }

    private async void AppExitMessageHandler(object recipient, AppExitMessage message)
    {
        _shutdownRequested = true;
        _timer.Stop();
        while (true)
        {
            if (!_isUpdating)
            {
                break;
            }
            await Task.Delay(100);
        }
        Messenger.Send(new ServiceShutDownMessage(GetType()));
    }

    private void Timer_Tick(object? sender, object e)
    {
        if (!_isUpdating)
        {
            Update();
        }
    }

    public void Update()
    {
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

        if (_fanService.UseRules)
        {
            var p = Rules.FirstOrDefault(r => r!.ConditionsSatisfied, null);
            if (p == null)
            {
                Messenger.Send(new RuleSwitchedMessage(DefaultProfileId));
            }
            else
            {
                Messenger.Send(new RuleSwitchedMessage(p.ProfileId));
            }
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

        if (save)
        {
            Save();
        }
    }

    public void RemoveRule(RuleModel rule)
    {
        if (!Rules.Contains(rule))
        {
            return;
        }

        var old = Rules.ToList();
        Rules.Remove(rule);
        Messenger.Send(new RulesChangedMessage(this, nameof(Rules), old, Rules));

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
                        ProcessName = c.ProcessName
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
                    return new();
                })
            });

        _settingsService.Save();
    }

    public void IncreasePriority(RuleModel rule)
    {
        var i = Rules.IndexOf(rule);
        if (i < 1)
        {
            return;
        }

        var old = Rules.ToList();

        Rules.RemoveAt(i);
        Rules.Insert(i - 1, rule);

        Messenger.Send(new RulesChangedMessage(this, nameof(Rules), old, Rules));

        Save();
    }

    public void DecreasePriority(RuleModel rule)
    {
        var i = Rules.IndexOf(rule);
        if (i == -1 || i == Rules.Count - 1)
        {
            return;
        }

        var old = Rules.ToList();

        Rules.RemoveAt(i);
        Rules.Insert(i + 1, rule);

        Messenger.Send(new RulesChangedMessage(this, nameof(Rules), old, Rules));

        Save();
    }
}
