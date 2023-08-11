using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Anemos;

internal class AppExitMessage : ValueChangedMessage<object?>
{
    public AppExitMessage() : base(null) { }
}

internal class ServiceStartupMessage : ValueChangedMessage<Type>
{
    public ServiceStartupMessage(Type type) : base(type) { }
}

internal class ServiceShutDownMessage : ValueChangedMessage<Type>
{
    public ServiceShutDownMessage(Type type) : base(type) { }
}

internal class WindowVisibilityChangedMessage : ValueChangedMessage<bool>
{
    public WindowVisibilityChangedMessage(bool isOpen) : base(isOpen) { }
}
