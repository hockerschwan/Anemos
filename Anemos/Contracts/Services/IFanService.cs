﻿using System.Collections.ObjectModel;
using ADLXWrapper;
using Anemos.Models;

namespace Anemos.Contracts.Services;

public interface IFanService
{
    string CurrentProfileId
    {
        get; set;
    }

    FanProfile CurrentProfile
    {
        get;
    }

    RangeObservableCollection<FanProfile> Profiles
    {
        get;
    }

    RangeObservableCollection<FanModelBase> Fans
    {
        get;
    }

    bool UseRules
    {
        get; set;
    }

    string CurrentAutoProfileId
    {
        get;
    }

    ADLX? ADLX
    {
        get;
    }

    Task InitializeAsync();

    void UpdateCurrentProfile();

    FanProfile? GetProfile(string id);

    FanModelBase? GetFanModel(string id);

    void AddProfile(string? idCopyFrom);

    void DeleteProfile(string id);

    void Save();
}
