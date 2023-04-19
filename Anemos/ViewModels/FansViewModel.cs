﻿using System.Collections.ObjectModel;
using Anemos.Contracts.Services;
using Anemos.Models;
using Anemos.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Anemos.ViewModels;

public partial class FansViewModel : ObservableRecipient
{
    private readonly IFanService _fanService;

    private List<FanModelBase> Models
    {
        get;
    }

    private ObservableCollection<FanViewModel> ViewModels
    {
        get;
    }

    public ObservableCollection<FanView> Views
    {
        get;
    }

    public IEnumerable<FanView> VisibleViews => Views.Where(v => !v.ViewModel.Model.IsHidden || ShowHiddenFans);

    public FanOptionsDialog OptionsDialog
    {
        get;
    }

    public FanProfileNameEditorDialog ProfileNameEditorDialog
    {
        get;
    }

    private bool _isVisible = true;
    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public RangeObservableCollection<FanProfile> FanProfiles => _fanService.Profiles;

    private FanProfile? _selectedProfile;
    public FanProfile? SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (SetProperty(ref _selectedProfile, value) && !UseRules)
            {
                _fanService.CurrentProfileId = _selectedProfile?.Id ?? string.Empty;
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
        }
    }

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

    public FansViewModel(IFanService fanService)
    {
        _fanService = fanService;

        Messenger.Register<WindowVisibilityChangedMessage>(this, WindowVisibilityChangedMessageHandler);
        Messenger.Register<FanProfileChangedMessage>(this, FanProfileChangedMessageHandler);

        Models = _fanService.Fans.ToList();
        ViewModels = new(Models.Select(m => new FanViewModel(m)));
        Views = new(ViewModels.Select(vm => new FanView(vm)));
        OptionsDialog = new();
        ProfileNameEditorDialog = new();

        _selectedProfile = _fanService.CurrentProfile;
    }

    private void WindowVisibilityChangedMessageHandler(object recipient, WindowVisibilityChangedMessage message)
    {
        IsVisible = message.Value;
        if (IsVisible)
        {
            foreach (var v in VisibleViews)
            {
                v.ViewModel.Plot.InvalidatePlot(true);
            }
        }
    }

    private void FanProfileChangedMessageHandler(object recipient, FanProfileChangedMessage message)
    {
        SelectedProfile = message.Value;
        OnPropertyChanged(nameof(FanProfiles));
    }

    public void UpdateView()
    {
        OnPropertyChanged(nameof(VisibleViews));
    }

    [RelayCommand]
    private void OpenProfileNameEditor()
    {
        Messenger.Send(new OpenFanProfileNameEditorMessage());
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
            _fanService.AddProfile(_fanService.CurrentProfileId);
        }
    }

    [RelayCommand]
    private void DeleteProfile()
    {
        _fanService.DeleteProfile(SelectedProfile?.Id!);
    }
}
