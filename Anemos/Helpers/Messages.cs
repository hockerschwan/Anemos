using Anemos.Helpers;
using Anemos.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Anemos;

public class AppExitMessage : ValueChangedMessage<object?>
{
    public AppExitMessage() : base(null) { }
}

public class ServiceShutDownMessage : ValueChangedMessage<Type>
{
    public ServiceShutDownMessage(Type type) : base(type) { }
}

public class WindowVisibilityChangedMessage : ValueChangedMessage<bool>
{
    public WindowVisibilityChangedMessage(bool isOpen) : base(isOpen) { }
}

public class ColorPickerResultMessage : ValueChangedMessage<Windows.UI.Color>
{
    public ColorPickerResultMessage(Windows.UI.Color color) : base(color) { }
}

public class LhwmUpdateDoneMessage : ValueChangedMessage<object?>
{
    public LhwmUpdateDoneMessage() : base(null) { }
}

public class CustomSensorsUpdateDoneMessage : ValueChangedMessage<object?>
{
    public CustomSensorsUpdateDoneMessage() : base(null) { }
}

public class CustomSensorsChangedMessage : PropertyChangedMessage<IEnumerable<SensorModelBase>>
{
    public CustomSensorsChangedMessage(
        object sender,
        string? propertyName,
        IEnumerable<SensorModelBase> oldValue,
        IEnumerable<SensorModelBase> newValue)
        : base(sender, propertyName, oldValue, newValue) { }
}

public class CurvesUpdateDoneMessage : ValueChangedMessage<object?>
{
    public CurvesUpdateDoneMessage() : base(null) { }
}

public class CurvesChangedMessage : PropertyChangedMessage<IEnumerable<CurveModel>>
{
    public CurvesChangedMessage(
        object sender,
        string? propertyName,
        IEnumerable<CurveModel> oldValue,
        IEnumerable<CurveModel> newValue)
        : base(sender, propertyName, oldValue, newValue) { }
}

public class OpenCurveEditorMessage : ValueChangedMessage<string>
{
    public OpenCurveEditorMessage(string curveId) : base(curveId) { }
}

public class CurveEditorResultMessage : ValueChangedMessage<IEnumerable<Point2>>
{
    public CurveEditorResultMessage(IEnumerable<Point2> points) : base(points) { }
}
