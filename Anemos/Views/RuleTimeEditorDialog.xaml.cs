using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class RuleTimeEditorDialog : ContentDialog
{
    private readonly int _index;

    public RuleTimeEditorDialog(int index, TimeOnly beginning, TimeOnly ending)
    {
        _index = index;

        InitializeComponent();
        Loaded += RuleTimeEditorDialog_Loaded;
        Unloaded += RuleTimeEditorDialog_Unloaded;
        PrimaryButtonClick += RuleTimeEditorDialog_PrimaryButtonClick;

        TP_Begin.Time = beginning.ToTimeSpan();
        TP_End.Time = ending.ToTimeSpan();
    }

    private void RuleTimeEditorDialog_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Loaded -= RuleTimeEditorDialog_Loaded;

        TP_Begin.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }

    private void RuleTimeEditorDialog_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Unloaded -= RuleTimeEditorDialog_Unloaded;
        PrimaryButtonClick -= RuleTimeEditorDialog_PrimaryButtonClick;
    }

    private async void RuleTimeEditorDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        while (IsLoaded)
        {
            await Task.Delay(100);
        }
        await Task.Delay(100);

        App.GetService<IMessenger>().Send<RuleTimeChangedMessage>(new(new(
            _index,
            TimeOnly.FromTimeSpan(TP_Begin.Time),
            TimeOnly.FromTimeSpan(TP_End.Time))));
    }
}
