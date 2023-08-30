using System.ComponentModel;
using Anemos.Contracts.Services;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ScottPlot;
using ScottPlot.Control;
using ScottPlot.Plottables;

namespace Anemos.Views;

public sealed partial class FanView : UserControl
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();
    private readonly ISettingsService _settingsService = App.GetService<ISettingsService>();

    public FanViewModel ViewModel
    {
        get;
    }

    private static FansViewModel FansVM => App.GetService<FansViewModel>();

    private Plot Plot1 => WinUIPlot1.Plot;
    private readonly Signal Signal;

    private readonly Color LineColor = Color.FromARGB((uint)System.Drawing.Color.CornflowerBlue.ToArgb());
    private readonly Color AxisColor = Color.FromARGB((uint)System.Drawing.Color.DarkGray.ToArgb());
    private readonly Color BackgroundColor = Color.FromARGB((uint)System.Drawing.Color.Black.ToArgb());

    private bool _optionsDialogOpened;

    public FanView(FanViewModel viewModel)
    {
        _messenger.Register<FanOptionsChangedMessage>(this, FanOptionsChangedMessageHandler);

        ViewModel = viewModel;
        InitializeComponent();

        App.MainWindow.PositionChanged += MainWindow_PositionChanged;
        _settingsService.Settings.PropertyChanged += Settings_PropertyChanged;

        Plot1.SetAxisLimits(bottom: -25d, top: 525d);
        Plot1.Grids.Clear();
        Plot1.XAxis.TickGenerator = new ScottPlot.TickGenerators.NumericManual(Array.Empty<Tick>());
        Plot1.XAxes.ForEach(x => x.IsVisible = false);
        Plot1.YAxes[1].IsVisible = false;
        Plot1.Style.ColorAxes(AxisColor);
        Plot1.DataBackground = Plot1.FigureBackground = BackgroundColor;
        Plot1.Margins(horizontal: 0);
        Plot1.ScaleFactor = (float)App.MainWindow.DisplayScale;

        Signal = Plot1.Add.Signal(ViewModel.LineData, color: LineColor);
        Signal.LineStyle.Width = 2;
        Signal.Marker.IsVisible = false;

        WinUIPlot1.Interaction.Actions = PlotActions.NonInteractive();
        WinUIPlot1.Refresh();

        ViewModel.LineData.QueueChanged += LineData_QueueChanged;
    }

    private void FanOptionsChangedMessageHandler(object recipient, FanOptionsChangedMessage message)
    {
        if (!_optionsDialogOpened) { return; }

        _optionsDialogOpened = false;
        ViewModel.Model.SetOptions(message.Value);
    }

    private void LineData_QueueChanged(object? sender, EventArgs e)
    {
        var max = ViewModel.LineData.Max();
        var min = ViewModel.LineData.Min();
        var d = max - min;
        if (max <= 500d)
        {
            Plot1.SetAxisLimits(bottom: -25d, top: 525d);
        }
        else if (d < 500d)
        {
            var h = min + d / 2d;
            Plot1.SetAxisLimits(bottom: h - 275d, top: h + 275d);
        }
        else
        {
            Plot1.AutoScale();
        }

        if (FansVM.IsVisible && (!ViewModel.Model.IsHidden || FansVM.ShowHiddenFans))
        {
            WinUIPlot1.Refresh();
        }
    }

    private void MainWindow_PositionChanged(object? sender, Windows.Graphics.PointInt32 e)
    {
        var scale = (float)App.MainWindow.DisplayScale;
        if (Plot1.ScaleFactor != scale)
        {
            Plot1.ScaleFactor = scale;
            WinUIPlot1.Refresh();
        }
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_settingsService.Settings.FanHistory))
        {
            Signal.Axes.XAxis.Max = _settingsService.Settings.FanHistory;
            ViewModel.LineData.Capacity = _settingsService.Settings.FanHistory;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.IsVisible) && ViewModel.IsVisible)
        {
            WinUIPlot1.Refresh();
        }
    }

    private void EditNameTextBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not TextBox tb) { return; }

        tb.Focus(FocusState.Programmatic);
        tb.SelectionStart = tb.Text.Length;
    }

    private void EditNameTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case Windows.System.VirtualKey.Escape:
                (sender as TextBox)!.Text = ViewModel.Model.Name;
                ViewModel.EditingName = false;
                break;
            case Windows.System.VirtualKey.Enter:
                ViewModel.EditingName = false;
                break;
        }
    }

    private async void FanOptionsButton_Click(object sender, RoutedEventArgs e)
    {
        _optionsDialogOpened = await FansPage.OpenOptionsDialog(
            ViewModel.Model.MinSpeed,
            ViewModel.Model.MaxSpeed,
            ViewModel.Model.DeltaLimitUp,
            ViewModel.Model.DeltaLimitDown,
            ViewModel.Model.RefractoryPeriodCyclesDown);
    }

    private void Grid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        PlotOverlay.Visibility = Visibility.Visible;
    }

    private void Grid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        PlotOverlay.Visibility = Visibility.Collapsed;
    }
}
