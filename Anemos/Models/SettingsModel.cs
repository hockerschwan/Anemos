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

    public FanSettings FanSettings { get; set; } = new();

    public CurveSettings CurveSettings { get; set; } = new();

    public SensorSettings SensorSettings { get; set; } = new();

    public RuleSettings RuleSettings { get; set; } = new();

    public MonitorSettings MonitorSettings { get; set; } = new();
}

public class WindowSettings
{
    public bool Maximized { get; set; } = false;
    public int X { get; set; } = 100;
    public int Y { get; set; } = 100;
    public int Width { get; set; } = 1200;
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
    public string SelectedProfileId { get; set; } = string.Empty;
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
    public int MinSpeed { get; set; } = 0;
    public int MaxSpeed { get; set; } = 100;
    public int DeltaLimitUp { get; set; } = 0;
    public int DeltaLimitDown { get; set; } = 0;
    public int RefractoryPeriodCyclesDown { get; set; } = 0;
    public int Offset { get; set; } = 0;
}

public class CurveSettings
{
    public IEnumerable<CurveSettings_Curve> Curves { get; set; } = Enumerable.Empty<CurveSettings_Curve>();
}

public class CurveSettings_Curve
{
    public CurveType Type { get; set; } = CurveType.Chart;
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;

    // Chart
    public IEnumerable<Point2d>? Points { get; set; } = null;

    // Latch
    public double? OutputLowTemperature { get; set; } = null;
    public double? OutputHighTemperature { get; set; } = null;
    public double? TemperatureThresholdLow { get; set; } = null;
    public double? TemperatureThresholdHigh { get; set; } = null;
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

    public TimeOnly? TimeBeginning { get; set; } = null;
    public TimeOnly? TimeEnding { get; set; } = null;

    public string? ProcessName { get; set; } = null;
    public int? MemoryType { get; set; } = null;
    public int? MemoryLower { get; set; } = null;
    public int? MemoryUpper { get; set; } = null;

    public string? SensorId { get; set; } = null;
    public double? UpperValue { get; set; } = null;
    public double? LowerValue { get; set; } = null;
    public bool? IncludeUpper { get; set; } = null;
    public bool? IncludeLower { get; set; } = null;
}

public class MonitorSettings
{
    public IEnumerable<MonitorSettings_Monitor> Monitors { get; set; } = Enumerable.Empty<MonitorSettings_Monitor>();
}

public class MonitorSettings_Monitor
{
    public MonitorSourceType SourceType { get; set; } = MonitorSourceType.Fan;
    public MonitorDisplayType DisplayType { get; set; } = MonitorDisplayType.Current;
    public string Id { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public IEnumerable<Tuple<double, string>> Colors { get; set; } = Enumerable.Empty<Tuple<double, string>>();
}
