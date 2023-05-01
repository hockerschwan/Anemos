using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Globalization.NumberFormatting;

namespace Anemos.Views;

public sealed partial class LatchCurveEditorDialog : ContentDialog
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();

    public LatchCurveEditorViewModel ViewModel
    {
        get;
    }

    public LatchCurveEditorDialog()
    {
        ViewModel = App.GetService<LatchCurveEditorViewModel>();
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        App.MainWindow.SizeChanged += MainWindow_SizeChanged;
        Loaded += CurveEditorDialog_Loaded;
        Closing += CurveEditorDialog_Closing;
        InitializeComponent();

        Plot.SizeChanged += Plot_SizeChanged;

        SetNumberFormatter();
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (IsLoaded)
        {
            SetArrowLength();
            ViewModel.Plot.InvalidatePlot(true);
        }
    }

    private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        if (Visibility == Visibility.Visible)
        {
            SetPageSize(args.Size.Width, args.Size.Height);
        }
    }

    private async void Plot_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        await Task.Delay(100);
        SetArrowLength();
        ViewModel.Plot.InvalidatePlot(false);
    }

    private void CurveEditorDialog_Loaded(object sender, RoutedEventArgs e)
    {
        SetPageSize(App.MainWindow.Width, App.MainWindow.Height);
    }

    private void SetPageSize(double windowWidth, double windowHeight)
    {
        Page.Width = Math.Min(1000, windowWidth - 200);
        Page.Height = Math.Min(1000, windowHeight - 300);
    }

    private void CurveEditorDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        _messenger.Send(new LatchCurveEditorResultMessage(new(
            ViewModel.TemperatureThresholdLow,
            ViewModel.TemperatureThresholdHigh,
            ViewModel.OutputLowTemperature,
            ViewModel.OutputHighTemperature)));
    }

    private void SetNumberFormatter()
    {
        IncrementNumberRounder rounder = new()
        {
            Increment = 0.1,
            RoundingAlgorithm = RoundingAlgorithm.RoundHalfUp
        };

        DecimalFormatter formatter = new()
        {
            FractionDigits = 1,
            NumberRounder = rounder
        };
        NumberBoxOutputLow.NumberFormatter = NumberBoxOutputHigh.NumberFormatter
            = NumberBoxThresholdLow.NumberFormatter = NumberBoxThresholdHigh.NumberFormatter = formatter;
    }

    private void SetArrowLength()
    {
        var p1 = ViewModel.ArrowThresholdLow.Transform(ViewModel.ArrowThresholdLow.StartPoint);
        var p2 = ViewModel.ArrowThresholdLow.Transform(ViewModel.ArrowThresholdLow.EndPoint);
        var diff = Math.Abs((p1 - p2).Y);
        var len = Math.Min(10, Math.Max(0, (diff - 2) / ViewModel.ArrowThresholdLow.StrokeThickness));
        ViewModel.ArrowThresholdLow.HeadLength = ViewModel.ArrowThresholdHigh.HeadLength = len;
    }
}
