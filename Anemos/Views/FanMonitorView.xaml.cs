using Anemos.ViewModels;

namespace Anemos.Views;

public sealed partial class FanMonitorView : MonitorViewBase
{
    public new FanMonitorViewModel ViewModel
    {
        get;
    }

    public FanMonitorView(FanMonitorViewModel viewModel) : base(viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }
}
