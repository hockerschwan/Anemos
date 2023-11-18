using Anemos.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Anemos;

internal class AppExitMessage : ValueChangedMessage<object?>
{
    public AppExitMessage() : base(null) { }
}

internal class ServiceStartupMessage(Type? type) : ValueChangedMessage<Type?>(type)
{
}

internal class ServiceShutDownMessage(Type? type) : ValueChangedMessage<Type?>(type)
{
}

internal class WindowVisibilityChangedMessage(bool isOpen) : ValueChangedMessage<bool>(isOpen)
{
}

internal class LhwmUpdateDoneMessage : ValueChangedMessage<object?>
{
    public LhwmUpdateDoneMessage() : base(null) { }
}

internal class CustomSensorsUpdateDoneMessage : ValueChangedMessage<object?>
{
    public CustomSensorsUpdateDoneMessage() : base(null) { }
}

/// <summary>
/// Sensors added/removed
/// </summary>
internal class CustomSensorsChangedMessage(
    object sender,
    string? propertyName,
    IEnumerable<SensorModelBase> oldValue,
    IEnumerable<SensorModelBase> newValue)
    : PropertyChangedMessage<IEnumerable<SensorModelBase>>(sender, propertyName, oldValue, newValue)
{
}

internal class CurvesUpdateDoneMessage : ValueChangedMessage<object?>
{
    public CurvesUpdateDoneMessage() : base(null) { }
}

/// <summary>
/// Curves added/removed
/// </summary>
internal class CurvesChangedMessage(
    object sender,
    string? propertyName,
    IEnumerable<CurveModelBase> oldValue,
    IEnumerable<CurveModelBase> newValue)
    : PropertyChangedMessage<IEnumerable<CurveModelBase>>(sender, propertyName, oldValue, newValue)
{
}

/// <summary>
/// Chart editor result
/// </summary>
internal class ChartCurveChangedMessage(List<Point2d> result) : ValueChangedMessage<List<Point2d>>(result)
{
}

/// <summary>
/// Latch editor result (X_Low, Y_Low, X_High, Y_High)
/// </summary>
internal class LatchCurveChangedMessage(Tuple<double, double, double, double> result)
    : ValueChangedMessage<Tuple<double, double, double, double>>(result)
{
}

internal class FansUpdateDoneMessage : ValueChangedMessage<object?>
{
    public FansUpdateDoneMessage() : base(null) { }
}

internal class FanProfileSwitchedMessage(FanProfile profile) : ValueChangedMessage<FanProfile>(profile)
{
}

/// <summary>
/// Profiles added/removed
/// </summary>
internal class FanProfilesChangedMessage(
    object sender,
    string? propertyName,
    IEnumerable<FanProfile> oldValue,
    IEnumerable<FanProfile> newValue)
    : PropertyChangedMessage<IEnumerable<FanProfile>>(sender, propertyName, oldValue, newValue)
{
}

internal class FanProfileRenamedMessage(string newName) : ValueChangedMessage<string>(newName)
{
}

internal class FanOptionsChangedMessage(FanOptionsResult result) : ValueChangedMessage<FanOptionsResult>(result)
{
}

/// <summary>
/// Rules added/removed
/// </summary>
internal class RulesChangedMessage(
    object sender,
    string? propertyName,
    IEnumerable<RuleModel> oldValue,
    IEnumerable<RuleModel> newValue)
    : PropertyChangedMessage<IEnumerable<RuleModel>>(sender, propertyName, oldValue, newValue)
{
}

/// <summary>
/// Condition index, Beginning, Ending
/// </summary>
internal class RuleTimeChangedMessage(Tuple<int, TimeOnly, TimeOnly> result)
    : ValueChangedMessage<Tuple<int, TimeOnly, TimeOnly>>(result)
{
}

/// <summary>
/// Condition index, Process name, Memory low, Memory High, Type
/// </summary>
internal class RuleProcessChangedMessage(Tuple<int, string, int?, int?, int> result)
    : ValueChangedMessage<Tuple<int, string, int?, int?, int>>(result)
{
}

/// <summary>
/// Condition index, Sensor ID, Lower, Include lower, Upper, Include upper
/// </summary>
internal class RuleSensorChangedMessage(Tuple<int, string, double?, bool, double?, bool> result)
    : ValueChangedMessage<Tuple<int, string, double?, bool, double?, bool>>(result)
{
}

/// <summary>
/// Monitors added/removed
/// </summary>
internal class MonitorsChangedMessage(
    object sender,
    string? propertyName,
    IEnumerable<MonitorModelBase> oldValue,
    IEnumerable<MonitorModelBase> newValue)
    : PropertyChangedMessage<IEnumerable<MonitorModelBase>>(sender, propertyName, oldValue, newValue)
{
}

/// <summary>
/// Old, New
/// </summary>
internal class MonitorColorChangedMessage(Tuple<MonitorColorThreshold, MonitorColorThreshold> result)
    : ValueChangedMessage<Tuple<MonitorColorThreshold, MonitorColorThreshold>>(result)
{
}
