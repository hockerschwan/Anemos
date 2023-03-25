using System.Collections.ObjectModel;
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

    Task InitializeAsync();

    void UpdateCurrentProfile();

    FanProfile? GetProfile(string id);

    FanModelBase? GetFanModel(string id);

    void AddProfile(string? idCopyFrom);

    void DeleteProfile(string id);

    void Save();
}
