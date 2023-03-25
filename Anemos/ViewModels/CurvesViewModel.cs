using System.Collections.ObjectModel;
using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using Anemos.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Anemos.ViewModels;

public partial class CurvesViewModel : ObservableRecipient
{
    private readonly ICurveService _curveService;

    private List<CurveModel> Models
    {
        get;
    }

    private ObservableCollection<CurveViewModel> ViewModels
    {
        get;
    }

    public ObservableCollection<CurveView> Views
    {
        get;
    }

    public CurveEditorDialog Editor
    {
        get;
    }

    private bool _isVisible = true;
    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public CurvesViewModel(ICurveService curveService)
    {
        _curveService = curveService;

        Messenger.Register<WindowVisibilityChangedMessage>(this, WindowVisibilityChangedMessageHandler);
        Messenger.Register<CurvesChangedMessage>(this, CurvesChangedMessageHandler);

        Models = _curveService.Curves.ToList();
        ViewModels = new(Models.Select(m => new CurveViewModel(m)));
        Views = new(ViewModels.Select(vm => new CurveView(vm)));
        Editor = new();
    }

    private void WindowVisibilityChangedMessageHandler(object recipient, WindowVisibilityChangedMessage message)
    {
        IsVisible = message.Value;
    }

    private void CurvesChangedMessageHandler(object recipient, CurvesChangedMessage message)
    {
        var removed = message.OldValue.Except(message.NewValue).ToList();
        if (removed.Any())
        {
            foreach (var model in removed.ToList())
            {
                Models.Remove(model);

                var vm = ViewModels.FirstOrDefault(vm => vm?.Model.Id == model.Id, null);
                if (vm != null)
                {
                    ViewModels.Remove(vm);
                }

                var v = Views.FirstOrDefault(v => (v?.ViewModel)?.Model.Id == model.Id, null);
                if (v != null)
                {
                    Views.Remove(v);
                }
            }
        }

        var added = message.NewValue.Except(message.OldValue).ToList();
        if (added.Any())
        {
            foreach (var model in added)
            {
                Models.Add(model);

                var vm = new CurveViewModel(model);
                ViewModels.Add(vm);
                Views.Add(new CurveView(vm));
                model.Update();
            }
        }
    }

    [RelayCommand]
    private void AddCurve()
    {
        _curveService.AddCurve(new()
        {
            Name = "Curves_NewCurveName".GetLocalized(),
            Points = new Point2[] { new(30, 30), new(70, 70) }
        });
    }
}
