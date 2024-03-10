using Anemos.Models;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;

namespace Anemos.Views;

public class MonitorViewBase : UserControl
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();

    public MonitorViewModelBase ViewModel
    {
        get;
    }

    private bool _editorOpened;

    private readonly MessageHandler<object, MonitorColorChangedMessage> _monitorColorChangedMessageHandler;

    public MonitorViewBase(MonitorViewModelBase viewModel)
    {
        _monitorColorChangedMessageHandler = MonitorColorChangedMessageHandler;
        _messenger.Register(this, _monitorColorChangedMessageHandler);

        ViewModel = viewModel;
    }

    private protected void CreateMenuFlyout()
    {
        var menuAdd = (MenuFlyoutItem)XamlReader.Load("""
            <MenuFlyoutItem
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                x:Uid="Monitor_ContextFlyout_AddColor" />
            """);
        menuAdd.Click += AddColor_Click;
        menuAdd.Icon = new FontIcon() { Glyph = "\uE710" };

        if (Resources["ContextMenu"] is MenuFlyout menu1)
        {
            menu1.Items.Insert(0, menuAdd);
        }

        if (Resources["ContextMenu_Colors"] is MenuFlyout menu2)
        {
            menu2.Items.Insert(0, menuAdd);
        }
    }

    private protected void AddColor_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem)
        {
            ViewModel.AddColor();
        }
    }

    private protected void Border_ContextRequested(UIElement sender, ContextRequestedEventArgs e)
    {
        if (e.OriginalSource is not FrameworkElement elm || Resources["ContextMenu"] is not MenuFlyout menu) { return; }

        if (e.TryGetPosition(elm, out var point))
        {
            menu.ShowAt(elm, point);
        }
        else
        {
            menu.ShowAt(elm);
        }
    }

    private protected async void DeleteColor_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.CommandParameter is MonitorColorThreshold color)
        {
            if (await MonitorsPage.OpenDeleteDialog())
            {
                ViewModel.Model.RemoveColor(color);
            }
        }
    }

    private protected async void DeleteSelf_Click(object sender, RoutedEventArgs e)
    {
        if (await MonitorsPage.OpenDeleteDialog())
        {
            ViewModel.RemoveSelf();
        }
    }

    private protected void EditColor_Click(object sender, RoutedEventArgs e)
    {
        object? col = null;
        if (sender is MenuFlyoutItem item && item.CommandParameter is MonitorColorThreshold c)
        {
            col = c;
        }

        if (col is MonitorColorThreshold color)
        {
            OpenEditor(color);
        }
    }

    private protected void ListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is MonitorColorThreshold color)
        {
            OpenEditor(color);
        }
    }

    private protected void MonitorColorChangedMessageHandler(object recipient, MonitorColorChangedMessage message)
    {
        if (!_editorOpened) { return; }
        _editorOpened = false;

        if (!ViewModel.Model.Colors.Contains(message.Value.Item1)) { return; }

        ViewModel.Model.EditColor(message.Value.Item1, message.Value.Item2);
    }

    private protected async void OpenEditor(MonitorColorThreshold color)
    {
        _editorOpened = await MonitorsPage.OpenEditor(color);
    }
}
