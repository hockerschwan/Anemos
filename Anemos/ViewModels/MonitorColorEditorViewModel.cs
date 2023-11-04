using Anemos.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Anemos.ViewModels;

public class MonitorColorEditorViewModel : ObservableObject
{
    private double _threshold;
    public double Threshold
    {
        get => _threshold;
        set => SetProperty(ref _threshold, value);
    }

    public MonitorColorThreshold OldColor
    {
        get; init;
    }

    public MonitorColorEditorViewModel(MonitorColorThreshold color)
    {
        OldColor = color;
        Threshold = color.Threshold;
    }
}
