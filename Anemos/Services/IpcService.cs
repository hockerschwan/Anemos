using Anemos.Contracts.Services;
using CommunityToolkit.Mvvm.Messaging;
using PlainlyIpc.Interfaces;
using PlainlyIpc.IPC;
using Serilog;

namespace Anemos.Services;

internal class IpcService : IIpcService
{
    private readonly IMessenger _messenger;

    private readonly IIpcHandler _server;

    public IpcService(IMessenger messenger)
    {
        _messenger = messenger;
        _messenger.Register<AppExitMessage>(this, AppExitMessageHandler);

        var ipcFactory = new IpcFactory();
        _server = ipcFactory.CreateNamedPipeIpcServer(App.Current.AppId).Result;
        _server.RegisterService<IIpcService>(this);

        _messenger.Send<ServiceStartupMessage>(new(GetType()));
        Log.Information("[IPC] Started");
    }

    private void AppExitMessageHandler(object recipient, AppExitMessage message)
    {
        _server.Dispose();
        _messenger.Send<ServiceShutDownMessage>(new(GetType()));
    }

    public void ShowWindow()
    {
        App.ShowWindow();
    }
}
