using System.Collections.ObjectModel;
using System.ComponentModel;
using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;

namespace Anemos.ViewModels;

public partial class FanViewModel : ObservableObject
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();
    private readonly ISettingsService _settingsService = App.GetService<ISettingsService>();
    private readonly ICurveService _curveService = App.GetService<ICurveService>();
    private readonly IFanService _fanService = App.GetService<IFanService>();

    public FanModelBase Model
    {
        get;
    }

    public string[] ControlModeNames
    {
        get;
    } =
    [
        "Fan_ControlModeNames_Device".GetLocalized(),
        "Fan_ControlModeNames_Constant".GetLocalized(),
        "Fan_ControlModeNames_Curve".GetLocalized()
    ];

    private int _controlModeIndex = -1;
    public int ControlModeIndex
    {
        get => _controlModeIndex;
        set
        {
            if (SetProperty(ref _controlModeIndex, value))
            {
                Model.ControlMode = (FanControlModes)_controlModeIndex;
                OnPropertyChanged(nameof(ShowConstantControls));
                OnPropertyChanged(nameof(ShowCurveControls));
                OnPropertyChanged(nameof(UnlockCurveOption));
            }
        }
    }

    public bool ShowConstantControls => Model.ControlMode == FanControlModes.Constant;
    public bool ShowCurveControls => Model.ControlMode == FanControlModes.Curve;
    public bool UnlockControls => !_fanService.UseRules;
    public bool UnlockCurveOption => UnlockControls && Model.ControlMode == FanControlModes.Curve;

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
                Model.CurveId = _selectedCurve?.Id ?? string.Empty;
            }
        }
    }

    public readonly LimitedQueue LineData;

    private bool _isVisible;
    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    private bool _editingName;
    public bool EditingName
    {
        get => _editingName;
        set => SetProperty(ref _editingName, value);
    }

    private bool _isFlyoutOpened = false;
    public bool IsFlyoutOpened
    {
        get => _isFlyoutOpened;
        set => SetProperty(ref _isFlyoutOpened, value);
    }

    private readonly MessageHandler<object, CurvesChangedMessage> _curvesChangedMessageHandler;
    private readonly MessageHandler<object, FanProfileSwitchedMessage> _fanProfileSwitchedMessageHandler;
    private readonly DispatcherQueueHandler _updateControlsHandler;
    private readonly DispatcherQueueHandler _updateLockHandler;

    public FanViewModel(FanModelBase model)
    {
        _curvesChangedMessageHandler = CurvesChangedMessageHandler;
        _fanProfileSwitchedMessageHandler = FanProfileSwitchedMessageHandler;
        _messenger.Register(this, _curvesChangedMessageHandler);
        _messenger.Register(this, _fanProfileSwitchedMessageHandler);

        Model = model;
        Model.PropertyChanged += Model_PropertyChanged;

        _settingsService.Settings.FanSettings.PropertyChanged += FanSettings_PropertyChanged;

        _updateControlsHandler = UpdateControls;
        _updateLockHandler = UpdateLock;

        Curves = new(_curveService.Curves);

        _controlModeIndex = (int)Model.ControlMode;
        _selectedCurve = Model.CurveModel;

        LineData = new(_settingsService.Settings.FanHistory);
    }

    private void CurvesChangedMessageHandler(object recipient, CurvesChangedMessage message)
    {
        var removed = message.OldValue.Except(message.NewValue).ToList();
        foreach (var cm in removed)
        {
            Curves.Remove(cm);
        }

        var added = message.NewValue.Except(message.OldValue).ToList();
        foreach (var cm in added)
        {
            Curves.Add(cm);
        }
    }

    private void FanProfileSwitchedMessageHandler(object recipient, FanProfileSwitchedMessage message)
    {
        _controlModeIndex = (int)Model.ControlMode;
        _selectedCurve = Model.CurveModel;

        App.DispatcherQueue.TryEnqueue(_updateControlsHandler);
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Model.CurrentRPM))
        {
            if (Model.CurrentRPM != null)
            {
                LineData.Enqueue((double)Model.CurrentRPM);
            }
        }
        else if (e.PropertyName == nameof(Model.IsHidden))
        {
            App.GetService<FansViewModel>().UpdateView();
        }
    }

    private void FanSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        App.DispatcherQueue.TryEnqueue(_updateLockHandler);
    }

    private void UpdateControls()
    {
        OnPropertyChanged(nameof(ControlModeIndex));
        OnPropertyChanged(nameof(SelectedCurve));
        OnPropertyChanged(nameof(ShowConstantControls));
        OnPropertyChanged(nameof(ShowCurveControls));
        OnPropertyChanged(nameof(UnlockCurveOption));
    }

    private void UpdateLock()
    {
        OnPropertyChanged(nameof(UnlockControls));
        OnPropertyChanged(nameof(UnlockCurveOption));
    }
}
