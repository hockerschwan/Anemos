using System.Collections.ObjectModel;
using Anemos.Contracts.Services;
using Anemos.Models;
using Anemos.Views;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;

namespace Anemos.ViewModels;

public partial class FansViewModel : PageViewModelBase
{
    private readonly IMessenger _messenger;
    private readonly ISettingsService _settingsService;
    private readonly IFanService _fanService;

    private List<FanModelBase> Models
    {
        get;
    }

    public List<FanViewModel> ViewModels
    {
        get;
    }

    public List<FanView> Views
    {
        get;
    }

    public ObservableCollection<FanView> VisibleViews
    {
        get;
    }

    private bool _isVisible = false;
    public override bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (SetProperty(ref _isVisible, value))
            {
                foreach (var v in VisibleViews)
                {
                    v.ViewModel.IsVisible = value;
                }
            }
        }
    }

    public ObservableCollection<FanProfile> FanProfiles
    {
        get;
    }

    private FanProfile? _selectedProfile;
    public FanProfile? SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (SetProperty(ref _selectedProfile, value) && !UseRules && _selectedProfile != null)
            {
                _fanService.ManualProfileId = _selectedProfile.Id;
            }
        }
    }

    public bool UseRules
    {
        get => _fanService.UseRules;
        set
        {
            _fanService.UseRules = value;
            OnPropertyChanged(nameof(UseRules));
            OnPropertyChanged(nameof(UnlockControls));
        }
    }

    public bool UnlockControls => !UseRules;

    private bool _showHiddenFans;
    public bool ShowHiddenFans
    {
        get => _showHiddenFans;
        set
        {
            if (SetProperty(ref _showHiddenFans, value))
            {
                UpdateView();
            }
        }
    }

    private bool _isFlyoutOpened = false;
    public bool IsFlyoutOpened
    {
        get => _isFlyoutOpened;
        set => SetProperty(ref _isFlyoutOpened, value);
    }

    private readonly MessageHandler<object, FanProfilesChangedMessage> _fanProfilesChangedMessageHandler;
    private readonly MessageHandler<object, FanProfileSwitchedMessage> _fanProfileSwitchedMessageHandler;
    private readonly DispatcherQueueHandler _updateUseRulesHandler;

    public FansViewModel(
        IMessenger messenger,
        ISettingsService settingsService,
        IFanService fanService)
    {
        _messenger = messenger;
        _settingsService = settingsService;
        _fanService = fanService;

        _fanProfilesChangedMessageHandler = FanProfileChangedMessageHandler;
        _fanProfileSwitchedMessageHandler = FanProfileSwitchedMessageHandler;
        _messenger.Register(this, _fanProfilesChangedMessageHandler);
        _messenger.Register(this, _fanProfileSwitchedMessageHandler);

        _updateUseRulesHandler = UpdateUseRules;
        _settingsService.Settings.FanSettings.PropertyChanged += FanSettings_PropertyChanged;

        Models = [.. _fanService.Fans];
        ViewModels = new(Models.Select(m => new FanViewModel(m)));
        Views = new(ViewModels.Select(vm => new FanView(vm)));
        VisibleViews = new(Views.Where(v => !v.ViewModel.Model.IsHidden));
        FanProfiles = new(_fanService.Profiles);

        _selectedProfile = _fanService.CurrentProfile;
    }

    private void FanSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_settingsService.Settings.FanSettings.UseRules))
        {
            App.DispatcherQueue.TryEnqueue(_updateUseRulesHandler);
        }
    }

    private void FanProfileChangedMessageHandler(object recipient, FanProfilesChangedMessage message)
    {
        var removed = message.OldValue.Except(message.NewValue).ToList();
        foreach (var p in removed)
        {
            FanProfiles.Remove(p);
        }

        var added = message.NewValue.Except(message.OldValue).ToList();
        if (added.Count != 0)
        {
            foreach (var p in added)
            {
                FanProfiles.Add(p);
            }
        }

        OnPropertyChanged(nameof(FanProfiles));
        SelectedProfile = _fanService.CurrentProfile;
    }

    private void FanProfileSwitchedMessageHandler(object recipient, FanProfileSwitchedMessage message)
    {
        App.DispatcherQueue.TryEnqueue(() =>
        {
            SelectedProfile = message.Value;
        });
    }

    public void RemoveProfile(string id)
    {
        _fanService.RemoveProfile(id);
    }

    private void UpdateUseRules()
    {
        UseRules = _settingsService.Settings.FanSettings.UseRules;
    }

    public void UpdateView()
    {
        var views = GetViews(this).ToList();
        var viewsChanged = views.Union(VisibleViews).Except(views.Intersect(VisibleViews)).ToList();
        if (viewsChanged.Count == 0) { return; }

        foreach (var v in viewsChanged)
        {
            if (!VisibleViews.Remove(v))
            {
                VisibleViews.Insert(views.IndexOf(v), v);
            }
        }

        static IEnumerable<FanView> GetViews(FansViewModel @this)
        {
            foreach (var v in @this.Views)
            {
                if (@this.ShowHiddenFans || !v.ViewModel.Model.IsHidden) { yield return v; }
            }
        }
    }

    [RelayCommand]
    private void AddProfile(string? param)
    {
        if (param == null)
        {
            _fanService.AddProfile(null);
        }
        else
        {
            _fanService.AddProfile(_fanService.ManualProfileId);
        }
    }
}
