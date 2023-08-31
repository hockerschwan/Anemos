using Anemos.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class SensorView : UserControl
{
    public SensorViewModel ViewModel
    {
        get;
    }

    public SensorView(SensorViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    private void CloseFlyout(object sender, RoutedEventArgs e)
    {
        ViewModel.AddSourceCommand.Execute((sender as Button)?.CommandParameter);
        AddSourceFlyout.Hide();
    }

    private async void DeleteSelfButton_Click(object sender, RoutedEventArgs e)
    {
        if (await SensorsPage.OpenDeleteDialog(ViewModel.Model.Name))
        {
            ViewModel.RemoveSelf();
        }
    }

    private void DeleteSourceButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.RemoveSource((string)((sender as Button)?.CommandParameter!));
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
}
