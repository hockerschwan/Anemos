using Anemos.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Anemos;

internal class AppExitMessage : ValueChangedMessage<object?>
{
    public AppExitMessage() : base(null) { }
}

internal class ServiceStartupMessage : ValueChangedMessage<object>
{
    public ServiceStartupMessage(object type) : base(type) { }
}

internal class ServiceShutDownMessage : ValueChangedMessage<object>
{
    public ServiceShutDownMessage(object type) : base(type) { }
}

internal class WindowVisibilityChangedMessage : ValueChangedMessage<bool>
{
    public WindowVisibilityChangedMessage(bool isOpen) : base(isOpen) { }
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
internal class CustomSensorsChangedMessage : PropertyChangedMessage<IEnumerable<SensorModelBase>>
{
    /// <inheritdoc cref="CustomSensorsChangedMessage"/>
    public CustomSensorsChangedMessage(
        object sender,
        string? propertyName,
        IEnumerable<SensorModelBase> oldValue,
        IEnumerable<SensorModelBase> newValue)
        : base(sender, propertyName, oldValue, newValue) { }
}

internal class CurvesUpdateDoneMessage : ValueChangedMessage<object?>
{
    public CurvesUpdateDoneMessage() : base(null) { }
}

/// <summary>
/// Curves added/removed
/// </summary>
internal class CurvesChangedMessage : PropertyChangedMessage<IEnumerable<CurveModelBase>>
{
    /// <inheritdoc cref="CurvesUpdateDoneMessage"/>
    public CurvesChangedMessage(
        object sender,
        string? propertyName,
        IEnumerable<CurveModelBase> oldValue,
        IEnumerable<CurveModelBase> newValue)
        : base(sender, propertyName, oldValue, newValue) { }
}

/// <summary>
/// Chart editor result
/// </summary>
internal class ChartCurveChangedMessage : ValueChangedMessage<IEnumerable<Point2d>>
{
    /// <inheritdoc cref="ChartCurveChangedMessage"/>
    public ChartCurveChangedMessage(IEnumerable<Point2d> result) : base(result) { }
}

/// <summary>
/// Latch editor result (X_Low, Y_Low, X_High, Y_High)
/// </summary>
internal class LatchCurveChangedMessage : ValueChangedMessage<Tuple<double, double, double, double>>
{
    /// <inheritdoc cref="LatchCurveChangedMessage"/>
    public LatchCurveChangedMessage(Tuple<double, double, double, double> result) : base(result) { }
}

internal class FanProfileSwitchedMessage : ValueChangedMessage<FanProfile>
{
    public FanProfileSwitchedMessage(FanProfile profile) : base(profile) { }
}

/// <summary>
/// Profiles added/removed
/// </summary>
internal class FanProfilesChangedMessage : PropertyChangedMessage<IEnumerable<FanProfile>>
{
    /// <inheritdoc cref="FanProfilesChangedMessage"/>
    public FanProfilesChangedMessage(
        object sender,
        string? propertyName,
        IEnumerable<FanProfile> oldValue,
        IEnumerable<FanProfile> newValue)
        : base(sender, propertyName, oldValue, newValue) { }
}

internal class FanProfileRenamedMessage : ValueChangedMessage<string>
{
    public FanProfileRenamedMessage(string newName) : base(newName) { }
}

internal class FanOptionsChangedMessage : ValueChangedMessage<FanOptionsResult>
{
    public FanOptionsChangedMessage(FanOptionsResult result) : base(result) { }
}

/// <summary>
/// Rules added/removed
/// </summary>
internal class RulesChangedMessage : PropertyChangedMessage<IEnumerable<RuleModel>>
{
    /// <inheritdoc cref="RulesChangedMessage"/>
    public RulesChangedMessage(
        object sender,
        string? propertyName,
        IEnumerable<RuleModel> oldValue,
        IEnumerable<RuleModel> newValue)
        : base(sender, propertyName, oldValue, newValue) { }
}

/// <summary>
/// Condition index, Beginning, Ending
/// </summary>
internal class RuleTimeChangedMessage : ValueChangedMessage<Tuple<int, TimeOnly, TimeOnly>>
{
    /// <inheritdoc cref="RuleTimeChangedMessage"/>
    public RuleTimeChangedMessage(Tuple<int, TimeOnly, TimeOnly> result) : base(result) { }
}

/// <summary>
/// Condition index, Process name, Memory low, Memory High, Type
/// </summary>
internal class RuleProcessChangedMessage : ValueChangedMessage<Tuple<int, string, int?, int?, int>>
{
    ///  <inheritdoc cref="RuleProcessChangedMessage"/>
    public RuleProcessChangedMessage(Tuple<int, string, int?, int?, int> result) : base(result) { }
}

/// <summary>
/// Condition index, Sensor ID, Lower, Include lower, Upper, Include upper
/// </summary>
internal class RuleSensorChangedMessage : ValueChangedMessage<Tuple<int, string, double?, bool, double?, bool>>
{
    ///  <inheritdoc cref="RuleSensorChangedMessage"/>
    public RuleSensorChangedMessage(Tuple<int, string, double?, bool, double?, bool> result) : base(result) { }
}
