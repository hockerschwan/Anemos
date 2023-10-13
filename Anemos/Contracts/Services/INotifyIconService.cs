namespace Anemos.Contracts.Services;

internal interface INotifyIconService
{
    void SetTooltip(string tooltip);

    void SetVisibility(bool visible);

    void Update();
}
