using Anemos.Contracts.Services;
using Anemos.Models;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;

namespace Anemos.Services;

internal class CurveService : ICurveService
{
    private readonly IMessenger _messenger;

    private readonly ISettingsService _settingsService;

    public List<CurveModelBase> Curves { get; } = new();

    private bool _isUpdating;

    public CurveService(
        IMessenger messenger,
        ISettingsService settingsService)
    {
        _messenger = messenger;
        _settingsService = settingsService;

        _messenger.Register<AppExitMessage>(this, AppExitMessageHandler);
        _messenger.Register<CustomSensorsUpdateDoneMessage>(this, CustomSensorsUpdateDoneMessageHandler);

        _messenger.Send<ServiceStartupMessage>(new(GetType()));
        Log.Information("[Curve] Started");
    }

    public void AddCurve(CurveArg arg)
    {
        AddCurves(new List<CurveArg> { arg });
    }

    private void AddCurves(IEnumerable<CurveArg> args, bool save = true)
    {
        var old = Curves.ToList();
        var models = new List<CurveModelBase>();
        foreach (var arg in args)
        {
            switch (arg.Type)
            {
                case CurveType.Chart:
                    models.Add(new ChartCurveModel(arg));
                    break;
                case CurveType.Latch:
                    models.Add(new LatchCurveModel(arg));
                    break;
            }
        }
        Curves.AddRange(models);
        _messenger.Send<CurvesChangedMessage>(new(this, nameof(Curves), old, Curves));

        if (save)
        {
            Save();
        }
    }

    private async void AppExitMessageHandler(object recipient, AppExitMessage message)
    {
        _messenger.Unregister<CustomSensorsUpdateDoneMessage>(this);
        while (true)
        {
            if (!_isUpdating) { break; }
            await Task.Delay(100);
        }
        _messenger.Send<ServiceShutDownMessage>(new(GetType()));
    }

    private void CustomSensorsUpdateDoneMessageHandler(object recipient, CustomSensorsUpdateDoneMessage message)
    {
        Update();
        _messenger.Send(new CurvesUpdateDoneMessage());
    }

    public CurveModelBase? GetCurve(string id) => Curves.SingleOrDefault(cm => cm?.Id == id, null);

    private void Load()
    {
        AddCurves(
            _settingsService.Settings.CurveSettings.Curves.Select(
                s => new CurveArg()
                {
                    Type = s.Type,
                    Id = s.Id,
                    Name = s.Name,
                    SourceId = s.SourceId,
                    Points = s.Points,
                    OutputLowTemperature = s.OutputLowTemperature,
                    OutputHighTemperature = s.OutputHighTemperature,
                    TemperatureThresholdLow = s.TemperatureThresholdLow,
                    TemperatureThresholdHigh = s.TemperatureThresholdHigh
                }),
            false);
    }

    public async Task LoadAsync()
    {
        await Task.Run(Load);
        Log.Information("[Curve] Loaded");
    }

    public void RemoveCurve(string id)
    {
        var model = GetCurve(id);
        if (model == null)
        {
            return;
        }

        var old = Curves.ToList();
        Curves.Remove(model);
        _messenger.Send<CurvesChangedMessage>(new(this, nameof(Curves), old, Curves));

        Save();
    }

    public void Save()
    {
        var curves = new List<CurveSettings_Curve>();
        foreach (var cm in Curves)
        {
            if (cm is ChartCurveModel chart)
            {
                curves.Add(new CurveSettings_Curve()
                {
                    Type = cm.Type,
                    Id = cm.Id,
                    Name = cm.Name,
                    SourceId = cm.SourceId,
                    Points = chart.Points
                });
            }
            else if (cm is LatchCurveModel latch)
            {
                curves.Add(new CurveSettings_Curve()
                {
                    Type = cm.Type,
                    Id = cm.Id,
                    Name = cm.Name,
                    SourceId = cm.SourceId,
                    OutputLowTemperature = latch.OutputLowTemperature,
                    OutputHighTemperature = latch.OutputHighTemperature,
                    TemperatureThresholdLow = latch.TemperatureThresholdLow,
                    TemperatureThresholdHigh = latch.TemperatureThresholdHigh
                });
            }
        }
        _settingsService.Settings.CurveSettings.Curves = curves;

        _settingsService.Save();
    }

    private void Update()
    {
        _isUpdating = true;
        Parallel.ForEach(Curves, c =>
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(
                Microsoft.UI.Dispatching.DispatcherQueuePriority.High,
                () => c.Update());
        });
        _isUpdating = false;
    }
}
