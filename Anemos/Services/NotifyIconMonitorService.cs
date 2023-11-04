using Anemos.Contracts.Services;
using Anemos.Models;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;

namespace Anemos.Services;

internal class NotifyIconMonitorService : INotifyIconMonitorService
{
    private readonly IMessenger _messenger;

    private readonly ISettingsService _settingsService;

    public List<MonitorModelBase> Monitors { get; } = new();

    private bool _isUpdating;

    public NotifyIconMonitorService(IMessenger messenger, ISettingsService settingsService)
    {
        _messenger = messenger;
        _settingsService = settingsService;

        _messenger.Register<AppExitMessage>(this, AppExitMessageHandler);
        _messenger.Register<FansUpdateDoneMessage>(this, FansUpdateDoneMessageHandler);

        _messenger.Send<ServiceStartupMessage>(new(GetType()));
        Log.Information("[Monitor] Started");
    }

    public void AddMonitor(MonitorArg arg)
    {
        AddMonitors(new[] { arg });
    }

    private void AddMonitors(IEnumerable<MonitorArg> args, bool save = true)
    {
        var old = Monitors.ToList();
        var models = new List<MonitorModelBase>();
        foreach (var arg in args)
        {
            switch (arg.SourceType)
            {
                case MonitorSourceType.Fan:
                    models.Add(new FanMonitorModel(arg));
                    break;
                case MonitorSourceType.Curve:
                    models.Add(new CurveMonitorModel(arg));
                    break;
                case MonitorSourceType.Sensor:
                    models.Add(new SensorMonitorModel(arg));
                    break;
            }
        }
        Monitors.AddRange(models);
        _messenger.Send<MonitorsChangedMessage>(new(this, nameof(Monitors), old, Monitors));

        if (save)
        {
            Save();
        }
    }

    private async void AppExitMessageHandler(object recipient, AppExitMessage message)
    {
        _messenger.UnregisterAll(this);
        while (true)
        {
            if (!_isUpdating) { break; }
            await Task.Delay(100);
        }
        Monitors.ForEach(m => m.NotifyIcon.Destroy());
        _messenger.Send<ServiceShutDownMessage>(new(GetType()));
    }

    private void FansUpdateDoneMessageHandler(object recipient, FansUpdateDoneMessage message)
    {
        Update();
    }

    public MonitorModelBase? GetMonitor(string id) => Monitors.SingleOrDefault(m => m?.Id == id, null);

    private void Load()
    {
        AddMonitors(
            _settingsService.Settings.MonitorSettings.Monitors.Select(
                s => new MonitorArg()
                {
                    SourceType = s.SourceType,
                    DisplayType = s.DisplayType,
                    Id = s.Id,
                    SourceId = s.SourceId,
                    Colors = s.Colors
                }),
            false);
    }

    public async Task LoadAsync()
    {
        await Task.Run(Load);
        Log.Information("[Monitor] Loaded");
    }

    public void RemoveMonitor(string id)
    {
        var model = GetMonitor(id);
        if (model == null) { return; }

        var old = Monitors.ToList();
        Monitors.Remove(model);
        model.Destory();
        _messenger.Send<MonitorsChangedMessage>(new(this, nameof(Monitors), old, Monitors));

        Save();
    }

    public void Save()
    {
        var monitors = new List<MonitorSettings_Monitor>();
        foreach (var model in Monitors)
        {
            var s = new MonitorSettings_Monitor()
            {
                SourceType = model.SourceType,
                DisplayType = model.DisplayType,
                Id = model.Id,
                SourceId = model.SourceId,
                Colors = model.Colors.Select(p => new Tuple<double, string>(p.Threshold, (p.Color.ToArgb() & 0xFFFFFF).ToString("X6")))
            };
            monitors.Add(s);
        }
        _settingsService.Settings.MonitorSettings.Monitors = monitors;

        _settingsService.Save();
    }

    public void SetVisibility(bool visible) => Monitors.ForEach(m => m.NotifyIcon.SetVisibility(visible));

    public void Update()
    {
        if (_isUpdating) { return; }

        _isUpdating = true;
        Parallel.ForEach(Monitors, m =>
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(
                Microsoft.UI.Dispatching.DispatcherQueuePriority.High,
                () => m.Update());
        });
        _isUpdating = false;
    }
}
