using Anemos.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Anemos.Models;

public partial class SettingsModel : ObservableObject
{
    public Settings_Window Window { get; set; } = new();

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

    private int _curveMaxTemp = 100;
    public int CurveMaxTemp
    {
        get => _curveMaxTemp;
        set => SetProperty(ref _curveMaxTemp, value);
    }

    private int _curveMinTemp = 20;
    public int CurveMinTemp
    {
        get => _curveMinTemp;
        set => SetProperty(ref _curveMinTemp, value);
    }

    private string _chartLineColor = "#6495ED";
    public string ChartLineColor
    {
        get => _chartLineColor;
        set => SetProperty(ref _chartLineColor, value);
    }

    private string _chartMarkerColor = "#FFA500";
    public string ChartMarkerColor
    {
        get => _chartMarkerColor;
        set => SetProperty(ref _chartMarkerColor, value);
    }

    private string _chartBGColor = "#000000";
    public string ChartBGColor
    {
        get => _chartBGColor;
        set => SetProperty(ref _chartBGColor, value);
    }

    private string _chartGridColor = "#808080";
    public string ChartGridColor
    {
        get => _chartGridColor;
        set => SetProperty(ref _chartGridColor, value);
    }

    private string _chartTextColor = "#D3D3D3";
    public string ChartTextColor
    {
        get => _chartTextColor;
        set => SetProperty(ref _chartTextColor, value);
    }

    public CurveSettings CurveSettings { get; set; } = new();

    public SensorSettings SensorSettings { get; set; } = new();
}

public class Settings_Window
{
    public bool Maximized { get; set; } = false;
    public int X { get; set; } = 100;
    public int Y { get; set; } = 100;
    public int Width { get; set; } = 900;
    public int Height { get; set; } = 720;
}

public class CurveSettings
{
    public IEnumerable<CurveSettings_Curve> Curves { get; set; } = Enumerable.Empty<CurveSettings_Curve>();
}

public class CurveSettings_Curve
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public IEnumerable<Point2> Points { get; set; } = Enumerable.Empty<Point2>();
}

public class SensorSettings
{
    public IEnumerable<SensorSettings_Sensor> Sensors { get; set; } = Enumerable.Empty<SensorSettings_Sensor>();
}

public class SensorSettings_Sensor
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SampleSize { get; set; } = 1;
    public CustomSensorCalcMethod CalcMethod { get; set; } = CustomSensorCalcMethod.Max;
    public IEnumerable<string> SourceIds { get; set; } = Enumerable.Empty<string>();
}
