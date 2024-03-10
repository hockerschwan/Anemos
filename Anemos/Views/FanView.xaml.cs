using System.ComponentModel;
using Anemos.Contracts.Services;
using Anemos.Models;
using Anemos.Plot;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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

    private Plot.Plot Plot1 => PlotControl1.Plot;
    private readonly Signal History;

    private bool _optionsDialogOpened;

    private readonly MessageHandler<object, FanOptionsChangedMessage> _fanOptionsChangedMessageHandler;

    public FanView(FanViewModel viewModel)
    {
        _fanOptionsChangedMessageHandler = FanOptionsChangedMessageHandler;
        _messenger.Register(this, _fanOptionsChangedMessageHandler);

        ViewModel = viewModel;
        InitializeComponent();

        _settingsService.Settings.PropertyChanged += Settings_PropertyChanged;

        Plot1.BottomAxisIsVisible = Plot1.TopAxisIsVisible = Plot1.RightAxisIsVisible = false;
        Plot1.BottomAxisGridIsVisible = Plot1.LeftAxisGridIsVisible = false;

        Plot1.MinY = 0;
        Plot1.MaxY = 500;
        Plot1.MaxX = ViewModel.LineData.Capacity - 1;
        Plot1.AxisMargin = 6;

        History = new(ViewModel.LineData)
        {
            LineWidth = 2
        };
        Plot1.Plottables.Add(History);

        PlotControl1.Refresh();

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
            Plot1.MinY = -25;
            Plot1.MaxY = 525;
        }
        else if (d < 500d)
        {
            var h = min + d / 2d;
            Plot1.MinY = h - 275;
            Plot1.MaxY = h + 275;
        }
        else
        {
            Plot1.MinY = min - 25;
            Plot1.MaxY = max + 25;
        }

        if (FansVM.IsVisible && (!ViewModel.Model.IsHidden || FansVM.ShowHiddenFans))
        {
            PlotControl1.Refresh();
        }
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_settingsService.Settings.FanHistory))
        {
            Plot1.MaxX = _settingsService.Settings.FanHistory - 1;
            ViewModel.LineData.Capacity = _settingsService.Settings.FanHistory;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.IsVisible) && ViewModel.IsVisible)
        {
            PlotControl1.Refresh();
        }
    }

    private void Border_ContextRequested(UIElement sender, Microsoft.UI.Xaml.Input.ContextRequestedEventArgs e)
    {
        if (e.OriginalSource is not FrameworkElement elm) { return; }

        if (e.TryGetPosition(elm, out var point))
        {
            ContextMenu.ShowAt(elm, point);
        }
        else
        {
            ContextMenu.ShowAt(elm);
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
        var args = new FanOptionsResult
        {
            MinSpeed = ViewModel.Model.MinSpeed,
            MaxSpeed = ViewModel.Model.MaxSpeed,
            DeltaLimitUp = ViewModel.Model.DeltaLimitUp,
            DeltaLimitDown = ViewModel.Model.DeltaLimitDown,
            RefractoryPeriodCyclesDown = ViewModel.Model.RefractoryPeriodCyclesDown,
            Offset = ViewModel.Model.Offset
        };
        _optionsDialogOpened = await FansPage.OpenOptionsDialog(args);
    }
}
