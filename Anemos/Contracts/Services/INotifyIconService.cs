using Anemos.Services;

namespace Anemos.Contracts.Services;

public interface INotifyIconService
{
    void SetMenuItems(List<MenuItem> items);
    void SetTooltip(string tooltip);
    void SetVisible(bool visible);
    void SetupMenu();
    void UpdateTooltip();
}
