using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Anemos.Views;

public sealed partial class SensorView : UserControl
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();

    public SensorViewModel ViewModel
    {
        get;
    }

    private readonly MessageHandler<object, CustomSensorSourceEditedMessage> _customSensorSourceEditedMessageHandler;

    private bool _sourceEditorDialogOpened;

    public SensorView(SensorViewModel viewModel)
    {
        _customSensorSourceEditedMessageHandler = CustomSensorsSourceEditedMessageHandler;
        _messenger.Register(this, _customSensorSourceEditedMessageHandler);

        ViewModel = viewModel;
        InitializeComponent();
    }

    private void CustomSensorsSourceEditedMessageHandler(object recipient, CustomSensorSourceEditedMessage message)
    {
        if (!_sourceEditorDialogOpened) { return; }

        _sourceEditorDialogOpened = false;
        if (!ViewModel.Model.SourceIds.SequenceEqual(message.Value))
        {
            ViewModel.Model.SourceIds = message.Value;
        }
    }

    private void Border_ContextRequested(UIElement sender, ContextRequestedEventArgs e)
    {
        if (Resources["ContextMenu"] is not MenuFlyout menu) { return; }
        if (e.OriginalSource is not FrameworkElement elm) { return; }

        if (e.TryGetPosition(elm, out var point))
        {
            menu.ShowAt(elm, point);
        }
        else
        {
            menu.ShowAt(elm);
        }
    }

    private async void DeleteSelf_Click(object sender, RoutedEventArgs e)
    {
        if (await SensorsPage.OpenDeleteDialog(ViewModel.Model.Name))
        {
            ViewModel.RemoveSelf();
        }
    }

    private void EditNameTextBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not TextBox tb) { return; }

        tb.Focus(FocusState.Programmatic);
        tb.SelectionStart = tb.Text.Length;
    }

    private void EditNameTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
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

    private void EditSource_Click(object sender, RoutedEventArgs e)
    {
        OpenSourceDialog();
    }

    private void ListViewSources_ItemClick(object sender, ItemClickEventArgs e)
    {
        OpenSourceDialog();
    }

    private async void OpenSourceDialog()
    {
        _sourceEditorDialogOpened = await SensorsPage.OpenSourceDialog(ViewModel.Model.Id);
    }
}
