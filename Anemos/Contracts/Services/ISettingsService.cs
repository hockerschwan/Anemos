using Anemos.Models;

namespace Anemos.Contracts.Services;

public interface ISettingsService
{
    SettingsModel Settings
    {
        get;
    }

    Task LoadAsync();

    void Save(bool immediate = false);
}
