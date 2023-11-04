using System.Collections.ObjectModel;
using Anemos.Contracts.Services;
using Anemos.Models;
using CommunityToolkit.Mvvm.Messaging;

namespace Anemos.ViewModels;

public class CurveMonitorViewModel : MonitorViewModelBase
{
    private readonly ICurveService _curveService = App.GetService<ICurveService>();

    public new CurveMonitorModel Model
    {
        get;
    }

    public ObservableCollection<CurveModelBase> Curves
    {
        get;
    }

    private CurveModelBase? _selectedCurve;
    public CurveModelBase? SelectedCurve
    {
        get => _selectedCurve;
        set
        {
            if (SetProperty(ref _selectedCurve, value))
            {
                Model.SourceId = _selectedCurve?.Id ?? string.Empty;
            }
        }
    }

    public CurveMonitorViewModel(CurveMonitorModel model) : base(model)
    {
        _messenger.Register<CurvesChangedMessage>(this, CurvesChangedMessageHandler);

        Model = model;
        Curves = new(_curveService.Curves);
        _selectedCurve = _curveService.GetCurve(Model.SourceId);
    }

    private void CurvesChangedMessageHandler(object recipient, CurvesChangedMessage message)
    {
        var removed = message.OldValue.Except(message.NewValue);
        foreach (var cm in removed)
        {
            Curves.Remove(cm);
        }

        var added = message.NewValue.Except(message.OldValue);
        foreach (var cm in added)
        {
            Curves.Add(cm);
        }
    }
}
