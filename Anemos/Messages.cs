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
