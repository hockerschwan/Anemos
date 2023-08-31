using Anemos.Models;

namespace Anemos.Contracts.Services;

public interface ISettingsService
{
    string SettingsFolder
    {
        get;
    }

    SettingsModel Settings
    {
        get;
    }

    void Save(bool immediate = false);
}
