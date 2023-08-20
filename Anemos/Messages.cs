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
