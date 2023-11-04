using Anemos.Models;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public class MonitorViewBase : UserControl
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();

    public MonitorViewModelBase ViewModel
    {
        get;
    }

    private bool _editorOpened;

    public MonitorViewBase(MonitorViewModelBase viewModel)
    {
        _messenger.Register<MonitorColorChangedMessage>(this, MonitorColorChangedMessageHandler);

        ViewModel = viewModel;
    }

    private protected void DeleteColorButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Model.RemoveColor((MonitorColorThreshold)(sender as Button)?.CommandParameter!);
    }

    private protected async void DeleteSelfButton_Click(object sender, RoutedEventArgs e)
    {
        if (await MonitorsPage.OpenDeleteDialog())
        {
            ViewModel.RemoveSelf();
        }
    }

    private protected async void EditColorButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.CommandParameter is not MonitorColorThreshold oldColor) { return; }

        _editorOpened = await MonitorsPage.OpenEditor(oldColor);
    }

    private protected void MonitorColorChangedMessageHandler(object recipient, MonitorColorChangedMessage message)
    {
        if (!_editorOpened) { return; }
        _editorOpened = false;

        if (!ViewModel.Model.Colors.Contains(message.Value.Item1)) { return; }

        ViewModel.Model.EditColor(message.Value.Item1, message.Value.Item2);
    }
}
