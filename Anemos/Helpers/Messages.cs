using Anemos.Contracts.Models;
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

public class LhwmUpdateDoneMessage : ValueChangedMessage<object?>
{
    public LhwmUpdateDoneMessage() : base(null) { }
}

public class CustomSensorsUpdateDoneMessage : ValueChangedMessage<object?>
{
    public CustomSensorsUpdateDoneMessage() : base(null) { }
}

public class CustomSensorsChangedMessage : PropertyChangedMessage<IEnumerable<ISensorModel>>
{
    public CustomSensorsChangedMessage(
        object sender,
        string? propertyName,
        IEnumerable<ISensorModel> oldValue,
        IEnumerable<ISensorModel> newValue)
        : base(sender, propertyName, oldValue, newValue) { }
}
