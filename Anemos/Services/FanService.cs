using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using CommunityToolkit.Mvvm.Messaging;
using LibreHardwareMonitor.Hardware;
using Serilog;

namespace Anemos.Services;

internal class FanService : IFanService
{
    private readonly IMessenger _messenger;
    private readonly ISettingsService _settingsService;
    private readonly ILhwmService _lhwmService;

    public List<FanProfile> Profiles { get; } = [];
    public List<FanModelBase> Fans { get; } = [];

    public FanProfile? CurrentProfile { get; private set; } = null;

    private string _manualProfileId = string.Empty;
    public string ManualProfileId
    {
        get => _manualProfileId;
        set
        {
            if (_manualProfileId != value)
            {
                _manualProfileId = value;

                if (UseRules)
                {
                    UseRules = false;
                }

                var p = GetProfile(_manualProfileId);
                if (p == null)
                {
                    p = CreateEmptyFanProfile();
                    _manualProfileId = p.Id;

                    Save();
                }

                if (CurrentProfile != p)
                {
                    CurrentProfile = p;
                    ApplyProfileToFans(p, true);
                }
            }
        }
    }

    private string _autoProfileId = string.Empty;
    public string AutoProfileId
    {
        get => _autoProfileId;
        set
        {
            if (_autoProfileId != value)
            {
                _autoProfileId = value;

                if (UseRules)
                {
                    var p = GetProfile(value);
                    if (p != null && CurrentProfile != p)
                    {
                        CurrentProfile = p;
                        ApplyProfileToFans(p);
                    }
                }
            }
        }
    }

    private bool _useRules;
    public bool UseRules
    {
        get => _useRules;
        set
        {
            if (_useRules != value)
            {
                _useRules = value;

                ApplyProfileToFans(_useRules ? AutoProfileId : ManualProfileId, _isLoaded);
            }
        }
    }

    private bool _isLoaded;

    private bool _isUpdating;

    private readonly MessageHandler<object, AppExitMessage> _appExitMessageHandler;
    private readonly MessageHandler<object, CurvesUpdateDoneMessage> _curvesUpdatedMessageHandler;
    private readonly Action<FanModelBase> _updateAction;

    public FanService(
        IMessenger messenger,
        ISettingsService settingsService,
        ILhwmService lhwmService)
    {
        _messenger = messenger;
        _settingsService = settingsService;
        _lhwmService = lhwmService;

        _appExitMessageHandler = AppExitMessageHandler;
        _curvesUpdatedMessageHandler = CurvesUpdateDoneMessageHandler;
        _messenger.Register(this, _appExitMessageHandler);
        _messenger.Register(this, _curvesUpdatedMessageHandler);

        _updateAction = Update_;

        _messenger.Send<ServiceStartupMessage>(new(GetType()));
        Log.Information("[Fan] Started");
    }

    public void AddProfile(string? idCopyFrom = null)
    {
        var p = GetProfile(idCopyFrom ?? string.Empty);
        if (p == null)
        {
            ManualProfileId = string.Empty;
            return;
        }

        var old = Profiles.ToList();

        var clone = new FanProfile()
        {
            Id = GenerateId(),
            Name = "Fans_CloneName".GetLocalized().Replace("$", p.Name),
            Fans = p.Fans.ToList(),
        };
        clone.PropertyChanged += Profile_PropertyChanged;
        Profiles.Add(clone);
        ManualProfileId = clone.Id;
        CurrentProfile = clone;

        _messenger.Send<FanProfilesChangedMessage>(new(this, nameof(Profiles), old, Profiles));
        Log.Information("[Fan] Profile created: {name}", clone.Name);

        if (_isLoaded)
        {
            Save();
        }
    }

    private async void AppExitMessageHandler(object recipient, AppExitMessage message)
    {
        _messenger.Unregister<CurvesChangedMessage>(this);
        while (true)
        {
            if (!_isUpdating) { break; }
            await Task.Delay(100);
        }

        foreach (var fm in Fans.Where(fm => fm.GetType() == typeof(GpuAmdFanModel)).Cast<GpuAmdFanModel>().ToList())
        {
            if (fm.ControlMode != FanControlModes.Device)
            {
                fm.RestoreSettings();
            }
        }

        _messenger.Send<ServiceShutDownMessage>(new(GetType()));
    }

    private void ApplyProfileToFans(string profileId, bool save = false)
    {
        var p = GetProfile(profileId);
        if (p == null || p == CurrentProfile)
        {
            if (save)
            {
                Save();
            }
            return;
        }

        ApplyProfileToFans(p, save);
        CurrentProfile = p;
    }

    private void ApplyProfileToFans(FanProfile profile, bool save = false)
    {
        foreach (var fm in Fans)
        {
            var item = FirstOrDefault(profile, fm);
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
        _messenger.Send<FanProfileSwitchedMessage>(new(profile));
        Log.Information("[Fan] Profile applied: {name}, Auto={auto}", profile.Name, UseRules);

        if (save && _isLoaded)
        {
            Save();
        }

        static FanSettings_ProfileItem? FirstOrDefault(FanProfile profile, FanModelBase? fm)
        {
            if (fm == null) { return null; }

            foreach (var pi in profile.Fans.ToList())
            {
                if (pi.Id == fm.Id) { return pi; }
            }
            return null;
        }
    }

    private FanProfile CreateEmptyFanProfile(bool notify = true)
    {
        var old = Profiles.ToList();

        var sensors = _lhwmService.GetSensors(SensorType.Fan);
        var p = new FanProfile()
        {
            Id = GenerateId(),
            Name = "Fans_NewProfileName".GetLocalized(),
            Fans = sensors.Select(s => new FanSettings_ProfileItem()
            {
                Id = s.Identifier.ToString(),
            })
        };
        Profiles.Add(p);
        p.PropertyChanged += Profile_PropertyChanged;
        CurrentProfile = p;
        ManualProfileId = p.Id;
        if (Profiles.Count == 1)
        {
            AutoProfileId = ManualProfileId;
        }

        if (notify)
        {
            _messenger.Send<FanProfilesChangedMessage>(new(this, nameof(Profiles), old, Profiles));
        }
        Log.Information("[Fan] Profile created: {name}", p.Name);

        return p;
    }

    private void CurvesUpdateDoneMessageHandler(object recipient, CurvesUpdateDoneMessage message)
    {
        if (_isUpdating) { return; }

        Update();
        _messenger.Send(new FansUpdateDoneMessage());
    }

    private string GenerateId()
    {
        while (true)
        {
            var id = Guid.NewGuid().ToString();
            if (!Profiles.Select(p => p.Id).Contains(id))
            {
                return id;
            }
        }
    }

    public FanModelBase? GetFanModel(string id)
    {
        return FirstOrDefault(this, id);

        static FanModelBase? FirstOrDefault(FanService @this, string id)
        {
            foreach (var f in @this.Fans)
            {
                if (f.Id == id) { return f; }
            }
            return null;
        }
    }

    public FanProfile? GetProfile(string id)
    {
        return FirstOrDefault(this, id);

        static FanProfile? FirstOrDefault(FanService @this, string id)
        {
            foreach (var p in @this.Profiles)
            {
                if (p.Id == id) { return p; }
            }
            return null;
        }
    }

    private void Load()
    {
        var save = false;
        var fanSettings = _settingsService.Settings.FanSettings;
        Profiles.AddRange(fanSettings.Profiles.Select(p => new FanProfile()
        {
            Id = p.Id,
            Name = p.Name,
            Fans = p.ProfileItems.ToList(),
        }));
        foreach (var p in Profiles)
        {
            p.PropertyChanged += Profile_PropertyChanged;
        }

        var sensors = _lhwmService
            .GetSensors(SensorType.Fan)
            .Where(s => s.Hardware.HardwareType != HardwareType.GpuNvidia || s.Identifier.ToString().Split("/").Last() == "1")
            .ToList();
        foreach (var sensor in sensors)
        {
            var settings = fanSettings.Fans.SingleOrDefault(f => f?.Id == sensor.Identifier.ToString(), null);

            string id, name;
            var hidden = false;
            if (settings == null)
            {
                id = sensor.Identifier.ToString();
                name = sensor.Name;

                save = true;
            }
            else
            {
                id = settings.Id;
                name = settings.Name;
                hidden = settings.IsHidden;
            }

            switch (sensor.Hardware.HardwareType)
            {
                case HardwareType.GpuAmd:
                    Fans.Add(new GpuAmdFanModel(id, name, hidden));
                    break;
                case HardwareType.GpuNvidia:
                    var idTerms = sensor.Identifier.ToString().Split("/");
                    if (!int.TryParse(idTerms.Last(), out var n) || n != 1) { break; }

                    var numFans = 0;
                    foreach (var fan in _lhwmService.GetSensors(SensorType.Fan).Where(f => f.Hardware.Identifier == sensor.Hardware.Identifier).ToList())
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
        }

        if (fanSettings.SelectedProfileId == string.Empty)
        {
            var p = CreateEmptyFanProfile();
            ManualProfileId = p.Id;

            save = true;
        }
        else
        {
            ManualProfileId = fanSettings.SelectedProfileId;
        }

        UseRules = fanSettings.UseRules;

        if (save)
        {
            Save();
        }

        _isLoaded = true;
    }

    public async Task LoadAsync()
    {
        if (_lhwmService.Sensors.Where(s => s.Hardware.HardwareType == HardwareType.GpuAmd).Any())
        {
            ADLXWrapper.ADLXWrapper.Initialize();
        }
        await Task.Run(Load);
        _messenger.Send<FanProfilesChangedMessage>(new(this, nameof(Profiles), Enumerable.Empty<FanProfile>(), Profiles));
        Log.Information("[Fan] Loaded");
    }

    private void Profile_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        Save();
    }

    public void RemoveProfile(string id)
    {
        var profile = GetProfile(id);
        if (profile == null) { return; }

        Log.Information("[Fan] Profile deleted: {name}", profile.Name);
        var old = Profiles.ToList();
        if (Profiles.Count == 1)
        {
            Profiles.Remove(profile);
            var np = CreateEmptyFanProfile(false);
            AutoProfileId = ManualProfileId = np.Id;
        }
        else
        {
            var np = First(this, id);
            if (np != null)
            {
                ManualProfileId = np.Id;
                Profiles.Remove(profile);
            }

            static FanProfile? First(FanService @this, string id)
            {
                foreach (var p in @this.Profiles)
                {
                    if (p.Id != id) { return p; }
                }
                return null;
            }
        }
        profile.PropertyChanged -= Profile_PropertyChanged;
        _messenger.Send<FanProfilesChangedMessage>(new(this, nameof(Profiles), old, Profiles));

        if (_isLoaded)
        {
            Save();
        }
    }

    public void Save()
    {
        var fanSettings = _settingsService.Settings.FanSettings;
        fanSettings.Fans = Fans.Select(fm => new FanSettings_Fan()
        {
            Id = fm.Id,
            Name = fm.Name,
            IsHidden = fm.IsHidden
        });

        fanSettings.SelectedProfileId = ManualProfileId;

        fanSettings.Profiles = Profiles.Select(p => new FanSettings_Profile()
        {
            Id = p.Id,
            Name = p.Name,
            ProfileItems = p.Fans.ToList(),
        });

        fanSettings.UseRules = UseRules;

        _settingsService.Save();
    }

    private void Update()
    {
        _isUpdating = true;
        Parallel.ForEach(Fans, _updateAction);
        _isUpdating = false;
    }

    private void Update_(FanModelBase fan)
    {
        fan.Update();
    }

    public void UpdateCurrentProfile()
    {
        if (CurrentProfile == null) { return; }

        var fans = CurrentProfile.Fans.ToList();
        foreach (var fan in fans)
        {
            var model = FirstOrDefault(this, fan);
            if (model != null)
            {
                fan.Mode = model.ControlMode;
                fan.CurveId = model.CurveId;
                fan.ConstantSpeed = model.ConstantSpeed;
                fan.MaxSpeed = model.MaxSpeed;
                fan.MinSpeed = model.MinSpeed;
                fan.DeltaLimitUp = model.DeltaLimitUp;
                fan.DeltaLimitDown = model.DeltaLimitDown;
                fan.RefractoryPeriodCyclesDown = model.RefractoryPeriodCyclesDown;
            }

            static FanModelBase? FirstOrDefault(FanService @this, FanSettings_ProfileItem? fan)
            {
                foreach (var f in @this.Fans)
                {
                    if (f.Id == fan?.Id) { return f; }
                }
                return null;
            }
        }

        CurrentProfile.Fans = fans;

        if (_isLoaded)
        {
            Save();
        }
    }
}
