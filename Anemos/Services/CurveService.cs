using Anemos.Contracts.Services;
using Anemos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;

namespace Anemos.Services;

public class CurveService : ObservableRecipient, ICurveService
{
    private readonly ISensorService _sensorService;

    private readonly ISettingsService _settingsService;

    public List<CurveModel> Curves
    {
        get;
    } = new();

    private bool _isUpdating;

    public CurveService(ISettingsService settingsService, ISensorService thermometerService)
    {
        Messenger.Register<AppExitMessage>(this, AppExitMessageHandler);
        Messenger.Register<CustomSensorsUpdateDoneMessage>(this, CustomSensorsUpdateDoneMessageHandler);

        _settingsService = settingsService;
        _sensorService = thermometerService;

        Log.Information("[CurveService] Started");
    }

    public async Task InitializeAsync()
    {
        await Task.Run(Load);
        Log.Information("[CurveService] Loaded");
    }

    private async void AppExitMessageHandler(object recipient, AppExitMessage message)
    {
        Messenger.Unregister<CustomSensorsUpdateDoneMessage>(this);
        while (true)
        {
            if (!_isUpdating) { break; }
            await Task.Delay(100);
        }
        Messenger.Send(new ServiceShutDownMessage(GetType().GetInterface("ICurveService")!));
    }

    private void CustomSensorsUpdateDoneMessageHandler(object recipient, CustomSensorsUpdateDoneMessage message)
    {
        Update();
        Messenger.Send(new CurvesUpdateDoneMessage());
    }

    private void Update()
    {
        _isUpdating = true;
        foreach (var cm in Curves)
        {
            cm.Update();
        }
        _isUpdating = false;
    }

    public CurveModel? GetCurve(string id) => Curves.SingleOrDefault(cm => cm?.Id == id, null);

    public void AddCurve(CurveArg arg)
    {
        AddCurves(new List<CurveArg> { arg });
    }

    private void AddCurves(IEnumerable<CurveArg> args, bool save = true)
    {
        var old = Curves.ToList();
        var models = args.Select(arg => new CurveModel(this, _sensorService, arg));
        Curves.AddRange(models);
        Messenger.Send(new CurvesChangedMessage(this, nameof(Curves), old, Curves));

        if (save)
        {
            Save();
        }
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
        Messenger.Send(new CurvesChangedMessage(this, nameof(Curves), old, Curves));

        Save();
    }

    public void Save()
    {
        _settingsService.Settings.CurveSettings.Curves = Curves.Select(
            cm => new CurveSettings_Curve()
            {
                Id = cm.Id,
                Name = cm.Name,
                SourceId = cm.SourceId,
                Points = cm.Points
            });

        _settingsService.Save();
    }

    private void Load()
    {
        AddCurves(
            _settingsService.Settings.CurveSettings.Curves.Select(
                s => new CurveArg()
                {
                    Id = s.Id,
                    Name = s.Name,
                    SourceId = s.SourceId,
                    Points = s.Points
                }),
            false);
    }
}
