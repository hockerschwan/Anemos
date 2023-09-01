using Anemos.Contracts.Services;
using CommunityToolkit.Mvvm.Messaging;
using PlainlyIpc.Interfaces;
using PlainlyIpc.IPC;
using Serilog;

namespace Anemos.Services;

internal class IpcService : IIpcService
{
    private readonly IMessenger _messenger;

    private readonly IIpcReceiver _receiver;

    public IpcService(IMessenger messenger)
    {
        _messenger = messenger;
        _messenger.Register<AppExitMessage>(this, AppExitMessageHandler);

        var ipcFactory = new IpcFactory();
        _receiver = ipcFactory.CreateNamedPipeIpcReceiver(App.Current.AppId).Result;
        _receiver.MessageReceived += Receiver_MessageReceived;

        _messenger.Send<ServiceStartupMessage>(new(GetType()));
        Log.Information("[IPC] Started");
    }

    private void AppExitMessageHandler(object recipient, AppExitMessage message)
    {
        _receiver.Dispose();
        _messenger.Send<ServiceShutDownMessage>(new(GetType()));
    }

    private void Receiver_MessageReceived(object? sender, PlainlyIpc.EventArgs.IpcMessageReceivedEventArgs e)
    {
        switch (e.Value)
        {
            case "SHOWWINDOW":
                App.ShowWindow();
                break;
        }
    }
}
