using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Anemos.ViewModels;

public partial class MonitorViewModelBase : ObservableObject
{
    private protected readonly IMessenger _messenger = App.GetService<IMessenger>();
    private protected readonly INotifyIconMonitorService _monitorService = App.GetService<INotifyIconMonitorService>();

    public MonitorModelBase Model
    {
        get;
    }

    public string[] TypeNames
    {
        get;
    } =
    [
        "Monitor_TypeNames_Current".GetLocalized(),
        "Monitor_TypeNames_History".GetLocalized()
    ];

    private protected int _displayTypeIndex = -1;
    public int DisplayTypeIndex
    {
        get => _displayTypeIndex;
        set
        {
            if (SetProperty(ref _displayTypeIndex, value))
            {
                Model.DisplayType = (MonitorDisplayType)value;
            }
        }
    }

    public MonitorViewModelBase(MonitorModelBase model)
    {
        Model = model;
        _displayTypeIndex = (int)Model.DisplayType;
    }

    public void AddColor()
    {
        Model.AddColor();
    }

    public void RemoveSelf()
    {
        _messenger.UnregisterAll(this);
        _monitorService.RemoveMonitor(Model.Id);
    }
}
