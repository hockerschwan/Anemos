using Anemos.Models;

namespace Anemos.Contracts.Services;

public interface IFanService
{
    List<FanProfile> Profiles
    {
        get;
    }

    List<FanModelBase> Fans
    {
        get;
    }

    FanProfile? CurrentProfile
    {
        get;
    }

    string ManualProfileId
    {
        get; set;
    }

    string AutoProfileId
    {
        get; set;
    }

    bool UseRules
    {
        get; set;
    }

    void AddProfile(string? idCopyFrom);

    FanModelBase? GetFanModel(string id);

    FanProfile? GetProfile(string id);

    Task LoadAsync();

    void RemoveProfile(string id);

    void Save();

    void UpdateCurrentProfile();
}
