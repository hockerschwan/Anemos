using System.Collections.ObjectModel;
using ADLXWrapper;
using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LibreHardwareMonitor.Hardware;
using Serilog;

namespace Anemos.Services;

public class FanService : ObservableRecipient, IFanService
{
    private readonly ILhwmService _lhwmService;

    private readonly ISettingsService _settingsService;

    private IRuleService _ruleService => App.GetService<IRuleService>();

    private string _currentProfileId = string.Empty;
    public string CurrentProfileId
    {
        get => _currentProfileId;
        set
        {
            if (SetProperty(ref _currentProfileId, value))
            {
                var p = GetProfile(_currentProfileId);
                if (p == null)
                {
                    p = CreateEmptyFanProfile();
                    _currentProfileId = p.Id;

                    Save();
                }
                OnPropertyChanged(nameof(CurrentProfile));
                ApplyProfileToFans();
            }
        }
    }

    public FanProfile CurrentProfile => Profiles.Single(p => p.Id == CurrentProfileId);

    public RangeObservableCollection<FanProfile> Profiles { get; private set; } = new();

    public RangeObservableCollection<FanModelBase> Fans { get; } = new();

    private IEnumerable<ISensor> Sensors
        => _lhwmService.GetSensors(SensorType.Fan)
            .Where(s => s.Hardware.HardwareType != HardwareType.GpuNvidia || s.Identifier.ToString().Split("/").Last() == "1");

    private bool _useRules;
    public bool UseRules
    {
        get => _useRules;
        set
        {
            if (SetProperty(ref _useRules, value))
            {
                if (_useRules)
                {
                    _ruleService.Update();
                }
                else
                {
                    CurrentAutoProfileId = string.Empty;
                    ApplyProfileToFans(CurrentProfile);
                }

                if (_isLoaded)
                {
                    Save();
                }
            }
        }
    }

    public string CurrentAutoProfileId
    {
        get; set;
    } = string.Empty;

    private readonly ADLX? _adlx;
    public ADLX? ADLX => _adlx;

    private bool _isLoaded;

    private bool _isUpdating;

    public FanService(ISettingsService settingsService, ILhwmService lhwmService)
    {
        Messenger.Register<AppExitMessage>(this, AppExitMessageHandler);
        Messenger.Register<CurvesUpdateDoneMessage>(this, CurvesUpdateDoneMessageHandler);
        Messenger.Register<RuleSwitchedMessage>(this, RuleSwitchedMessageHandler);

        _lhwmService = lhwmService;
        _settingsService = settingsService;

        if (_lhwmService.Hardware.Where(hw => hw.HardwareType == HardwareType.GpuAmd).Any())
        {
            _adlx = new();
            Log.Debug("[FanService] ADLX Started");
        }

        Log.Information("[FanService] Started");
    }

    public async Task InitializeAsync()
    {
        await Task.Run(Load);
        Log.Information("[FanService] Loaded");
    }

    private async void AppExitMessageHandler(object recipient, AppExitMessage message)
    {
        Messenger.Unregister<CurvesChangedMessage>(this);
        while (true)
        {
            if (!_isUpdating) { break; }
            await Task.Delay(100);
        }

        foreach (var fm in Fans.Where(fm => fm.GetType() == typeof(GpuAmdFanModel)).Cast<GpuAmdFanModel>())
        {
            if (fm.ControlMode != FanControlModes.Device)
            {
                fm.RestoreSettings();
            }
        }

        Messenger.Send(new ServiceShutDownMessage(GetType().GetInterface("IFanService")!));
    }

    private void RuleSwitchedMessageHandler(object recipient, RuleSwitchedMessage message)
    {
        if (!UseRules || CurrentAutoProfileId == message.Value) { return; }

        var p = GetProfile(message.Value);
        if (p != null)
        {
            CurrentAutoProfileId = message.Value;
            ApplyProfileToFans(p);
            App.GetService<FansViewModel>().SelectedProfile = p;
        }
    }

    private void CurvesUpdateDoneMessageHandler(object recipient, CurvesUpdateDoneMessage message)
    {
        Update();
    }

    private void Update()
    {
        _isUpdating = true;
        foreach (var fm in Fans)
        {
            fm.Update();
        }
        _isUpdating = false;
    }

    public void UpdateCurrentProfile()
    {
        var fans = CurrentProfile.Fans.ToList();
        fans.ForEach(f =>
        {
            var model = Fans.SingleOrDefault(fm => fm?.Id == f.Id, null);
            if (model != null)
            {
                f.Mode = model.ControlMode;
                f.CurveId = model.CurveId;
                f.ConstantSpeed = model.ConstantSpeed;
                f.MaxSpeed = model.MaxSpeed;
                f.MinSpeed = model.MinSpeed;
                f.DeltaLimitUp = model.DeltaLimitUp;
                f.DeltaLimitDown = model.DeltaLimitDown;
                f.RefractoryPeriodCyclesDown = model.RefractoryPeriodCyclesDown;
            }
        });
        CurrentProfile.Fans = fans;

        if (_isLoaded)
        {
            Save();
        }
    }

    public FanProfile? GetProfile(string id)
    {
        return Profiles.SingleOrDefault(p => p?.Id == id, null);
    }

    private string GenerateId()
    {
        while (true)
        {
            var id = Guid.NewGuid().ToString();
            if (!Fans.Select(fm => fm.Id).Contains(id) && !Profiles.Select(p => p.Id).Contains(id))
            {
                return id;
            }
        }
    }

    public void AddProfile(string? idCopyFrom = null)
    {
        var p = GetProfile(idCopyFrom ?? string.Empty);
        if (p == null)
        {
            CurrentProfileId = string.Empty;
            return;
        }

        var clone = new FanProfile()
        {
            Id = GenerateId(),
            Name = "Fans_DuplicateName".GetLocalized().Replace("$", p.Name),
            Fans = p.Fans.Select(fi => new FanSettings_ProfileItem()
            {
                Id = fi.Id,
                Mode = fi.Mode,
                CurveId = fi.CurveId,
                ConstantSpeed = fi.ConstantSpeed,
                MaxSpeed = fi.MaxSpeed,
                MinSpeed = fi.MinSpeed,
                DeltaLimitUp = fi.DeltaLimitUp,
                DeltaLimitDown = fi.DeltaLimitDown,
                RefractoryPeriodCyclesDown = fi.RefractoryPeriodCyclesDown
            })
        };
        Profiles.Add(clone);
        CurrentProfileId = clone.Id;

        if (_isLoaded)
        {
            Save();
        }
    }

    public void DeleteProfile(string id)
    {
        var profile = GetProfile(id);
        if (profile == null)
        {
            return;
        }

        Log.Information("[FanService] Profile deleted: {name}", profile.Name);
        if (Profiles.Count == 1)
        {
            var np = CreateEmptyFanProfile();
            CurrentProfileId = np.Id;
            Profiles.Remove(profile);
        }
        else
        {
            var np = Profiles.First(p => p.Id != id);
            CurrentProfileId = np.Id;
            Profiles.Remove(profile);
        }

        if (_isLoaded)
        {
            Save();
        }
    }

    public FanModelBase? GetFanModel(string id)
    {
        return Fans.SingleOrDefault(f => f?.Id == id, null);
    }

    private void Load()
    {
        var save = false;

        Profiles = new(_settingsService.Settings.FanSettings.Profiles.Select(p => new FanProfile()
        {
            Id = p.Id,
            Name = p.Name,
            Fans = p.ProfileItems.Select(pItem => new FanSettings_ProfileItem()
            {
                Id = pItem.Id,
                Mode = pItem.Mode,
                CurveId = pItem.CurveId,
                ConstantSpeed = pItem.ConstantSpeed,
                MaxSpeed = pItem.MaxSpeed,
                MinSpeed = pItem.MinSpeed,
                DeltaLimitUp = pItem.DeltaLimitUp,
                DeltaLimitDown = pItem.DeltaLimitDown,
                RefractoryPeriodCyclesDown = pItem.RefractoryPeriodCyclesDown
            })
        }));

        Sensors.ToList().ForEach(s =>
        {
            var settings = _settingsService.Settings.FanSettings.Fans.SingleOrDefault(f => f?.Id == s.Identifier.ToString(), null);

            string id, name;
            var hidden = false;
            if (settings == null)
            {
                id = s.Identifier.ToString();
                name = s.Name;

                save = true;
            }
            else
            {
                id = settings.Id;
                name = settings.Name;
                hidden = settings.IsHidden;
            }

            switch (s.Hardware.HardwareType)
            {
                case HardwareType.GpuAmd:
                    Fans.Add(new GpuAmdFanModel(id, name, hidden));
                    break;
                case HardwareType.GpuIntel:
                    Fans.Add(new ReadOnlyFanModel(id, name, hidden));
                    break;
                case HardwareType.GpuNvidia:
                    var idDivided = s.Identifier.ToString().Split("/");
                    if (!int.TryParse(idDivided.Last(), out var n) || n != 1) { break; }

                    var numFans = 0;
                    var partialId = string.Join("/", idDivided.Take(idDivided.Length - 1));
                    foreach (var fan in _lhwmService.GetSensors(SensorType.Fan).Where(f => f.Hardware.Identifier == s.Hardware.Identifier))
                    {
                        if (!int.TryParse(fan.Identifier.ToString().Split("/").Last(), out var k) || k <= numFans) { continue; }
                        numFans = k;
                    }
                    if (numFans > 0)
                    {
                        Fans.Add(new GpuNvidiaFanModel(id, name, hidden, numFans));
                    }
                    break;
                default:
                    Fans.Add(new NormalFanModel(id, name, hidden));
                    break;
            }
        });

        if (_settingsService.Settings.FanSettings.CurrentProfile != string.Empty)
        {
            CurrentProfileId = _settingsService.Settings.FanSettings.CurrentProfile;
        }
        else
        {
            var p = CreateEmptyFanProfile();
            CurrentProfileId = p.Id;

            save = true;
        }

        UseRules = _settingsService.Settings.FanSettings.UseRules;

        if (save)
        {
            Save();
        }

        _isLoaded = true;
    }

    public void Save()
    {
        _settingsService.Settings.FanSettings.Fans = Fans.Select(fm => new FanSettings_Fan()
        {
            Id = fm.Id,
            Name = fm.Name,
            IsHidden = fm.IsHidden
        });

        _settingsService.Settings.FanSettings.CurrentProfile = CurrentProfileId;

        _settingsService.Settings.FanSettings.Profiles = Profiles.Select(p => new FanSettings_Profile()
        {
            Id = p.Id,
            Name = p.Name,
            ProfileItems = p.Fans.Select(item => new FanSettings_ProfileItem()
            {
                Id = item.Id,
                Mode = item.Mode,
                CurveId = item.CurveId,
                ConstantSpeed = item.ConstantSpeed,
                MaxSpeed = item.MaxSpeed,
                MinSpeed = item.MinSpeed,
                DeltaLimitUp = item.DeltaLimitUp,
                DeltaLimitDown = item.DeltaLimitDown,
                RefractoryPeriodCyclesDown = item.RefractoryPeriodCyclesDown
            })
        });

        _settingsService.Settings.FanSettings.UseRules = UseRules;

        _settingsService.Save();
    }

    private void ApplyProfileToFans()
    {
        foreach (var fm in Fans)
        {
            var item = CurrentProfile.Fans.SingleOrDefault(pi => pi?.Id == fm.Id, null);
            if (item == null)
            {
                fm.LoadProfile(new FanSettings_ProfileItem()
                {
                    Id = fm.Id,
                });
            }
            else
            {
                fm.LoadProfile(item);
            }
        }
        Messenger.Send(new FanProfileChangedMessage(CurrentProfile));
        Log.Information("[FanService] Profile applied: {name}", CurrentProfile.Name);

        if (_isLoaded)
        {
            Save();
        }
    }

    private void ApplyProfileToFans(FanProfile profile)
    {
        foreach (var fm in Fans)
        {
            var item = profile.Fans.SingleOrDefault(pi => pi?.Id == fm.Id, null);
            if (item == null)
            {
                fm.LoadProfile(new FanSettings_ProfileItem()
                {
                    Id = fm.Id,
                });
            }
            else
            {
                fm.LoadProfile(item);
            }
        }
        Messenger.Send(new FanProfileChangedMessage(CurrentProfile));
        Log.Information("[FanService] Profile applied: {name}", profile.Name);
    }

    private FanProfile CreateEmptyFanProfile()
    {
        var p = new FanProfile()
        {
            Id = GenerateId(),
            Name = "Fans_NewProfileName".GetLocalized(),
            Fans = Sensors.Select(s => new FanSettings_ProfileItem()
            {
                Id = s.Identifier.ToString(),
            })
        };
        Profiles.Add(p);
        return p;
    }
}
