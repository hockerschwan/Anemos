using System.Collections.ObjectModel;
using System.ComponentModel;
using Anemos.Contracts.Services;
using Anemos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Anemos.ViewModels;

internal delegate void CurveDataChangedEventHandler(object? sender, EventArgs e);
internal delegate void CurveMarkerChangedEventHandler(object? sender, EventArgs e);

public abstract partial class CurveViewModelBase : ObservableObject
{
    private protected readonly IMessenger _messenger = App.GetService<IMessenger>();
    private protected readonly ISettingsService _settingsService = App.GetService<ISettingsService>();
    private protected readonly ISensorService _sensorService = App.GetService<ISensorService>();
    private protected readonly ICurveService _curveService = App.GetService<ICurveService>();

    internal event CurveDataChangedEventHandler? CurveDataChanged;
    internal event CurveMarkerChangedEventHandler? CurveMarkerChanged;

    private protected static CurvesViewModel CurvesVM => App.GetService<CurvesViewModel>();

    public CurveModelBase Model
    {
        get;
    }

    public ObservableCollection<SensorModelBase> Sensors
    {
        get;
    }

    public SensorModelBase? Source
    {
        get => Model.SourceModel;
        set => Model.SourceId = value?.Id ?? string.Empty;
    }

    private bool _editingName;
    public bool EditingName
    {
        get => _editingName;
        set => SetProperty(ref _editingName, value);
    }

    public CurveViewModelBase(CurveModelBase model)
    {
        _messenger.Register<CustomSensorsChangedMessage>(this, CustomSensorsChangedMessageHandler);

        Model = model;
        Model.PropertyChanged += Model_PropertyChanged;

        Sensors = new(_sensorService.Sensors);
    }

    private void CustomSensorsChangedMessageHandler(object recipient, CustomSensorsChangedMessage message)
    {
        var removed = message.OldValue.Except(message.NewValue);
        foreach (var sensor in removed)
        {
            Sensors.Remove(sensor);
        }

        var added = message.NewValue.Except(message.OldValue);
        foreach (var sensor in added)
        {
            Sensors.Add(sensor);
        }
    }

    internal virtual void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Model.SourceId))
        {
            OnCurveMarkerChanged();
        }
        else if ((e.PropertyName == nameof(Model.Input) || e.PropertyName == nameof(Model.Output)) && CurvesVM.IsVisible)
        {
            OnCurveMarkerChanged();
        }
    }

    internal void OnCurveDataChanged()
    {
        CurveDataChanged?.Invoke(this, EventArgs.Empty);
    }

    internal void OnCurveMarkerChanged()
    {
        CurveMarkerChanged?.Invoke(this, EventArgs.Empty);
    }

    internal void RemoveSelf()
    {
        _messenger.UnregisterAll(this);
        Model.PropertyChanged -= Model_PropertyChanged;
        _curveService.RemoveCurve(Model.Id);
    }

    public void Update()
    {
        OnCurveMarkerChanged();
    }
}
