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

    private int _fanHistory = 120;
    public int FanHistory
    {
        get => _fanHistory;
        set => SetProperty(ref _fanHistory, value);
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

    public FanSettings FanSettings { get; set; } = new();

    public CurveSettings CurveSettings { get; set; } = new();

    public SensorSettings SensorSettings { get; set; } = new();

    public RuleSettings RuleSettings { get; set; } = new();
}

public class Settings_Window
{
    public bool Maximized { get; set; } = false;
    public int X { get; set; } = 100;
    public int Y { get; set; } = 100;
    public int Width { get; set; } = 900;
    public int Height { get; set; } = 720;
}

public class FanSettings : ObservableObject
{
    public IEnumerable<FanSettings_Fan> Fans { get; set; } = Enumerable.Empty<FanSettings_Fan>();
    private bool _useRules = false;
    public bool UseRules
    {
        get => _useRules;
        set => SetProperty(ref _useRules, value);
    }
    public string CurrentProfile { get; set; } = string.Empty;
    public IEnumerable<FanSettings_Profile> Profiles { get; set; } = Enumerable.Empty<FanSettings_Profile>();
}

public class FanSettings_Fan
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsHidden { get; set; } = false;
}

public class FanSettings_Profile
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public IEnumerable<FanSettings_ProfileItem> ProfileItems { get; set; } = Enumerable.Empty<FanSettings_ProfileItem>();
}

public class FanSettings_ProfileItem
{
    public string Id { get; set; } = string.Empty;
    public FanControlModes Mode { get; set; } = FanControlModes.Device;
    public string CurveId { get; set; } = string.Empty;
    public int ConstantSpeed { get; set; } = 50;
    public int MaxSpeed { get; set; } = 100;
    public int MinSpeed { get; set; } = 0;
    public int DeltaLimitUp { get; set; } = 0;
    public int DeltaLimitDown { get; set; } = 0;
    public int RefractoryPeriodTicksDown { get; set; } = 0;
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

public class RuleSettings
{
    public string DefaultProfile { get; set; } = string.Empty;
    public IEnumerable<RuleSettings_Rule> Rules { get; set; } = Enumerable.Empty<RuleSettings_Rule>();
}

public class RuleSettings_Rule
{
    public string Name { get; set; } = string.Empty;
    public RuleType Type { get; set; } = RuleType.All;
    public string ProfileId { get; set; } = string.Empty;
    public IEnumerable<RuleSettings_Condition> Conditions { get; set; } = Enumerable.Empty<RuleSettings_Condition>();
}

public class RuleSettings_Condition
{
    public RuleConditionType Type { get; set; } = RuleConditionType.Time;

    public TimeOnly? TimeBeginning
    {
        get; set;
    }
    public TimeOnly? TimeEnding
    {
        get; set;
    }

    public string? ProcessName
    {
        get; set;
    }

    public string? SensorId
    {
        get; set;
    }
    public double? UpperValue
    {
        get; set;
    }
    public double? LowerValue
    {
        get; set;
    }
    public bool? UseUpperValue
    {
        get; set;
    }
    public bool? UseLowerValue
    {
        get; set;
    }
    public bool? IncludeUpper
    {
        get; set;
    }
    public bool? IncludeLower
    {
        get; set;
    }
}
