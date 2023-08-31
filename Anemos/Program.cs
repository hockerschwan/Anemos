using Microsoft.UI.Dispatching;
using PlainlyIpc.IPC;
using Serilog;

namespace Anemos;

public static class Program
{
    private const string Id = "2474C51B-9ABB-4B5D-8909-7EAA97A9DE25";

    [STAThread]
    public static void Main()
    {
        Mutex mutex = new(false, Id);
        try
        {
            if (mutex.WaitOne(0, false))
            {
                GC.KeepAlive(mutex);

                SetupLogger();
                Log.Information("[Program] START");

                Microsoft.UI.Xaml.Application.Start((p) =>
                {
                    var context = new DispatcherQueueSynchronizationContext(
                        DispatcherQueue.GetForCurrentThread());
                    SynchronizationContext.SetSynchronizationContext(context);
                    _ = new App(Id);
                });

                Log.Information("[Program] END");
            }
            else // already running
            {
                var ipcFactory = new IpcFactory();
                var client = ipcFactory.CreateNamedPipeIpcClient(Id).Result;
                client.ExecuteRemote<Contracts.Services.IIpcService>(x => x.ShowWindow());
            }
        }
        catch (Exception ex)
        {
            Log.Fatal("[Program] {0}\n{1}\n{2}", ex.Message, ex.Source, ex.StackTrace);
        }
        finally
        {
            Log.CloseAndFlush();
            mutex.Close();
        }
    }

    private static void SetupLogger()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), // AppData/Local
            AppDomain.CurrentDomain.FriendlyName,
            "logs");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        RollLogFiles(folder, 10);

        var template = "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        var config = new LoggerConfiguration();
#if DEBUG
        config.MinimumLevel.Debug();
        config.WriteTo.Debug(outputTemplate: template);
#endif

        var fileName = $"log{DateTime.Now:yyyyMMdd-HHmmss}.txt";

        config.WriteTo.File(
            Path.Combine(folder, fileName),
            outputTemplate: template);

        Log.Logger = config.CreateLogger();
    }

    private static void RollLogFiles(string folder, ushort count)
    {
        var files = new List<string>(Directory.GetFiles(folder, "log*.txt"));
        if (files.Count < count)
        {
            return;
        }

        foreach (var file in files.GetRange(0, files.Count - count + 1))
        {
            File.Delete(file);
        }
    }
}
