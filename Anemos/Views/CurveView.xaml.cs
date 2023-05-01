using Anemos.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class CurveView : UserControl
{
    public CurveViewModelBase ViewModel
    {
        get;
    }

    public CurveView(CurveViewModelBase viewModel)
    {
        ViewModel = viewModel;
        Loaded += CurveView_Loaded;
        InitializeComponent();
    }

    private void CurveView_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= CurveView_Loaded;
        ViewModel.IsVisible = true;

        if (ViewModel is LatchCurveViewModel latch)
        {
            SetArrowLength(latch);
        }
    }

    internal void SetArrowLength(LatchCurveViewModel vm)
    {
        var p1 = vm.ArrowThresholdLow.Transform(vm.ArrowThresholdLow.StartPoint);
        var p2 = vm.ArrowThresholdLow.Transform(vm.ArrowThresholdLow.EndPoint);
        var diff = Math.Abs((p1 - p2).Y);
        var len = Math.Min(10, Math.Max(0, (diff - 2) / vm.ArrowThresholdLow.StrokeThickness));
        vm.ArrowThresholdLow.HeadLength = vm.ArrowThresholdHigh.HeadLength = len;
    }
}
