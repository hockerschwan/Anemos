using Anemos.Models;

namespace Anemos.Contracts.Services;

public interface INotifyIconMonitorService
{
    List<MonitorModelBase> Monitors
    {
        get;
    }

    void AddMonitor(MonitorArg arg);

    MonitorModelBase? GetMonitor(string id);

    Task LoadAsync();

    void RemoveMonitor(string id);

    void Save();

    void SetVisibility(bool visible);

    void Update();
}
