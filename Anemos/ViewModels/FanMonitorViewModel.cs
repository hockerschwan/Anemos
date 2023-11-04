using System.Collections.ObjectModel;
using Anemos.Contracts.Services;
using Anemos.Models;

namespace Anemos.ViewModels;

public class FanMonitorViewModel : MonitorViewModelBase
{
    private readonly IFanService _fanService = App.GetService<IFanService>();

    public new FanMonitorModel Model
    {
        get;
    }

    public ObservableCollection<FanModelBase> Fans
    {
        get;
    }

    private FanModelBase? _selectedFan;
    public FanModelBase? SelectedFan
    {
        get => _selectedFan;
        set
        {
            if (SetProperty(ref _selectedFan, value))
            {
                Model.SourceId = _selectedFan?.Id ?? string.Empty;
            }
        }
    }

    public FanMonitorViewModel(FanMonitorModel model) : base(model)
    {
        Model = model;
        Fans = new(_fanService.Fans);
        _selectedFan = _fanService.GetFanModel(Model.SourceId);
    }
}
