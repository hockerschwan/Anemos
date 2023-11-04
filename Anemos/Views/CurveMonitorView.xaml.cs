using Anemos.ViewModels;

namespace Anemos.Views;

public sealed partial class CurveMonitorView : MonitorViewBase
{
    public new CurveMonitorViewModel ViewModel
    {
        get;
    }

    public CurveMonitorView(CurveMonitorViewModel viewModel) : base(viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }
}
