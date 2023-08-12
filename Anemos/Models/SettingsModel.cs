using CommunityToolkit.Mvvm.ComponentModel;

namespace Anemos.Models;

public partial class SettingsModel : ObservableObject
{
    public WindowSettings Window { get; set; } = new();

    private bool _startMinimized = false;
    public bool StartMinimized
    {
        get => _startMinimized;
        set => SetProperty(ref _startMinimized, value);
    }

    private bool _startWithLogIn = false;
    public bool StartWithLogIn
    {
        get => _startWithLogIn;
        set => SetProperty(ref _startWithLogIn, value);
    }

    private int _updateInterval = 2;
    public int UpdateInterval
    {
        get => _updateInterval;
        set => SetProperty(ref _updateInterval, value);
    }

    private int _rulesUpdateIntervalCycles = 5;
    public int RulesUpdateIntervalCycles
    {
        get => _rulesUpdateIntervalCycles;
        set => SetProperty(ref _rulesUpdateIntervalCycles, value);
    }

    private int _fanHistory = 120;
    public int FanHistory
    {
        get => _fanHistory;
        set => SetProperty(ref _fanHistory, value);
    }

    private int _curveMinTemp = 20;
    public int CurveMinTemp
    {
        get => _curveMinTemp;
        set => SetProperty(ref _curveMinTemp, value);
    }

    private int _curveMaxTemp = 100;
    public int CurveMaxTemp
    {
        get => _curveMaxTemp;
        set => SetProperty(ref _curveMaxTemp, value);
    }
}

public class WindowSettings
{
    public bool Maximized { get; set; } = false;
    public int X { get; set; } = 100;
    public int Y { get; set; } = 100;
    public int Width { get; set; } = 1200;
    public int Height { get; set; } = 720;
}
