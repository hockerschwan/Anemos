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

public class CurvesChangedMessage : PropertyChangedMessage<IEnumerable<CurveModelBase>>
{
    public CurvesChangedMessage(
        object sender,
        string? propertyName,
        IEnumerable<CurveModelBase> oldValue,
        IEnumerable<CurveModelBase> newValue)
        : base(sender, propertyName, oldValue, newValue) { }
}

public class OpenCurveEditorMessage : ValueChangedMessage<string>
{
    public OpenCurveEditorMessage(string curveId) : base(curveId) { }
}

public class ChartCurveEditorResultMessage : ValueChangedMessage<IEnumerable<Point2>>
{
    public ChartCurveEditorResultMessage(IEnumerable<Point2> points) : base(points) { }
}

public class FanProfileChangedMessage : ValueChangedMessage<FanProfile>
{
    public FanProfileChangedMessage(FanProfile newProfile) : base(newProfile) { }
}

public class OpenFanOptionsMessage : ValueChangedMessage<string>
{
    public OpenFanOptionsMessage(string fanId) : base(fanId) { }
}

public class FanOptionsResultMessage : ValueChangedMessage<FanOptionsResult>
{
    public FanOptionsResultMessage(FanOptionsResult result) : base(result) { }
}

public class OpenFanProfileNameEditorMessage : ValueChangedMessage<object?>
{
    public OpenFanProfileNameEditorMessage() : base(null) { }
}

public class FanProfileNameEditorResultMessage : ValueChangedMessage<string>
{
    public FanProfileNameEditorResultMessage(string newName) : base(newName) { }
}

public class RulesChangedMessage : PropertyChangedMessage<IEnumerable<RuleModel>>
{
    public RulesChangedMessage(
        object sender,
        string? propertyName,
        IEnumerable<RuleModel> oldValue,
        IEnumerable<RuleModel> newValue)
        : base(sender, propertyName, oldValue, newValue) { }
}

public class RuleSwitchedMessage : ValueChangedMessage<string>
{
    public RuleSwitchedMessage(string profileId) : base(profileId) { }
}

public class OpenRuleTimeEditorMessage : ValueChangedMessage<TimeRuleCondition>
{
    public OpenRuleTimeEditorMessage(TimeRuleCondition time) : base(time) { }
}

public class OpenRuleProcessEditorMessage : ValueChangedMessage<ProcessRuleCondition>
{
    public OpenRuleProcessEditorMessage(ProcessRuleCondition process) : base(process) { }
}

public class OpenRuleSensorEditorMessage : ValueChangedMessage<SensorRuleCondition>
{
    public OpenRuleSensorEditorMessage(SensorRuleCondition process) : base(process) { }
}

public class RuleEditorResultMessage : ValueChangedMessage<RuleConditionArg>
{
    public RuleEditorResultMessage(RuleConditionArg arg) : base(arg) { }
}
