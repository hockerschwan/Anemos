using System.Collections.ObjectModel;
using Anemos.Contracts.Services;
using Anemos.Models;
using Anemos.Views;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

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

    public FansViewModel(
        IMessenger messenger,
        ISettingsService settingsService,
        IFanService fanService)
    {
        _messenger = messenger;
        _settingsService = settingsService;
        _fanService = fanService;

        _messenger.Register<FanProfilesChangedMessage>(this, FanProfileChangedMessageHandler);
        _messenger.Register<FanProfileSwitchedMessage>(this, FanProfileSwitchedMessageHandler);

        _settingsService.Settings.FanSettings.PropertyChanged += FanSettings_PropertyChanged;

        Models = _fanService.Fans.ToList();
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
            UseRules = _settingsService.Settings.FanSettings.UseRules;
        }
    }

    private void FanProfileChangedMessageHandler(object recipient, FanProfilesChangedMessage message)
    {
        var removed = message.OldValue.Except(message.NewValue);
        foreach (var p in removed)
        {
            FanProfiles.Remove(p);
        }

        var added = message.NewValue.Except(message.OldValue);
        if (added.Any())
        {
            added.ToList().ForEach(FanProfiles.Add);
        }

        OnPropertyChanged(nameof(FanProfiles));
        SelectedProfile = _fanService.CurrentProfile;
    }

    private void FanProfileSwitchedMessageHandler(object recipient, FanProfileSwitchedMessage message)
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            SelectedProfile = message.Value;
        });
    }

    public void RemoveProfile(string id)
    {
        _fanService.RemoveProfile(id);
    }

    public void UpdateView()
    {
        var views = Views.Where(v => !v.ViewModel.Model.IsHidden || ShowHiddenFans).ToList();
        var viewsChanged = views.Union(VisibleViews).Except(views.Intersect(VisibleViews)).ToList();
        if (!viewsChanged.Any()) { return; }

        foreach (var v in viewsChanged)
        {
            if (VisibleViews.Contains(v))
            {
                VisibleViews.Remove(v);
            }
            else
            {
                VisibleViews.Insert(views.IndexOf(v), v);
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
