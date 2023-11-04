using Anemos.ViewModels;

namespace Anemos.Views;

public sealed partial class SensorMonitorView : MonitorViewBase
{
    public new SensorMonitorViewModel ViewModel
    {
        get;
    }

    public SensorMonitorView(SensorMonitorViewModel viewModel) : base(viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }
}
