using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Windows.Globalization.NumberFormatting;

namespace Anemos.Views;

public sealed partial class RuleProcessEditorDialog : ContentDialog
{
    private readonly int _index;

    public RuleProcessEditorDialog(int index, string processName, int? memoryLow, int? memoryHigh, int type)
    {
        _index = index;

        InitializeComponent();
        Loaded += RuleProcessEditorDialog_Loaded;
        Unloaded += RuleProcessEditorDialog_Unloaded;
        PrimaryButtonClick += RuleProcessEditorDialog_PrimaryButtonClick;

        SetNumberFormatter();

        TB_ProcessName.Text = processName;
        NB_MemorySizeLow.Value = memoryLow ?? double.NaN;
        NB_MemorySizeHigh.Value = memoryHigh ?? double.NaN;
        RB_MemoryType.SelectedIndex = type;
    }

    private void RuleProcessEditorDialog_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Loaded -= RuleProcessEditorDialog_Loaded;

        TB_ProcessName.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
        TB_ProcessName.SelectionStart = TB_ProcessName.Text.Length;
    }

    private void RuleProcessEditorDialog_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Unloaded -= RuleProcessEditorDialog_Unloaded;
        PrimaryButtonClick -= RuleProcessEditorDialog_PrimaryButtonClick;
    }

    private void RuleProcessEditorDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
        {
            await Task.Delay(100);
            App.GetService<IMessenger>().Send<RuleProcessChangedMessage>(new(new(
                _index,
                TB_ProcessName.Text,
                double.IsNaN(NB_MemorySizeLow.Value) ? null : (int?)NB_MemorySizeLow.Value,
                double.IsNaN(NB_MemorySizeHigh.Value) ? null : (int?)NB_MemorySizeHigh.Value,
                RB_MemoryType.SelectedIndex)));
        });
    }

    private void NB_MemorySizeLow_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (NB_MemorySizeLow.Value >= NB_MemorySizeHigh.Value)
        {
            NB_MemorySizeLow.Value = NB_MemorySizeHigh.Value - 1;
        }
    }

    private void NB_MemorySizeHigh_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (NB_MemorySizeLow.Value <= NB_MemorySizeHigh.Value)
        {
            NB_MemorySizeHigh.Value = NB_MemorySizeLow.Value + 1;
        }
    }

    private void SetNumberFormatter()
    {
        IncrementNumberRounder rounder = new()
        {
            Increment = 1,
            RoundingAlgorithm = RoundingAlgorithm.RoundHalfUp
        };

        DecimalFormatter formatter = new()
        {
            FractionDigits = 0,
            NumberRounder = rounder
        };

        NB_MemorySizeLow.NumberFormatter = NB_MemorySizeHigh.NumberFormatter = formatter;
    }
}
