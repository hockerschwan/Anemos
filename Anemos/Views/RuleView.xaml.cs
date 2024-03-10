using Anemos.Contracts.Services;
using Anemos.Models;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;

namespace Anemos.Views;

public sealed partial class RuleView : UserControl
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();
    private readonly IRuleService _ruleService = App.GetService<IRuleService>();

    public RuleViewModel ViewModel
    {
        get;
    }

    private bool _timeEditorOpened;
    private bool _processEditorOpened;
    private bool _sensorEditorOpened;

    private readonly MessageHandler<object, RuleProcessChangedMessage> _ruleProcessChangedMessageHandler;
    private readonly MessageHandler<object, RuleSensorChangedMessage> _ruleSensorChangedMessageHandler;
    private readonly MessageHandler<object, RuleTimeChangedMessage> _ruleTimeChangedMessageHandler;

    public RuleView(RuleViewModel viewModel)
    {
        _ruleProcessChangedMessageHandler = RuleProcessChangedMessageHandler;
        _ruleSensorChangedMessageHandler = RuleSensorChangedMessageHandler;
        _ruleTimeChangedMessageHandler = RuleTimeChangedMessageHandler;
        _messenger.Register(this, _ruleProcessChangedMessageHandler);
        _messenger.Register(this, _ruleSensorChangedMessageHandler);
        _messenger.Register(this, _ruleTimeChangedMessageHandler);

        ViewModel = viewModel;
        InitializeComponent();

        CreateMenuFlyout();
    }

    // https://stackoverflow.com/questions/41376335/
    private void CreateMenuFlyout()
    {
        var menuAdd = (MenuFlyoutSubItem)XamlReader.Load("""
            <MenuFlyoutSubItem
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                x:Uid="Rule_ContextFlyout_AddCondition" />
            """);
        menuAdd.Icon = new FontIcon() { Glyph = "\uE710" };

        var menuProcess = (MenuFlyoutItem)XamlReader.Load("""
            <MenuFlyoutItem
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                x:Uid="Rule_ContextFlyout_Process" />
        """);
        menuProcess.CommandParameter = "Process";
        menuProcess.Click += AddCondition_Click;
        menuProcess.Icon = new TablerIcon.TablerIcon { Symbol = TablerIcon.TablerIconGlyph.CPU };
        menuAdd.Items.Add(menuProcess);

        var menuSensor = (MenuFlyoutItem)XamlReader.Load("""
            <MenuFlyoutItem
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                x:Uid="Rule_ContextFlyout_Sensor" />
        """);
        menuSensor.CommandParameter = "Sensor";
        menuSensor.Click += AddCondition_Click;
        menuSensor.Icon = new TablerIcon.TablerIcon { Symbol = TablerIcon.TablerIconGlyph.Temperature };
        menuAdd.Items.Add(menuSensor);

        var menuTime = (MenuFlyoutItem)XamlReader.Load("""
            <MenuFlyoutItem
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                x:Uid="Rule_ContextFlyout_Time" />
        """);
        menuTime.CommandParameter = "Time";
        menuTime.Click += AddCondition_Click;
        menuTime.Icon = new TablerIcon.TablerIcon { Symbol = TablerIcon.TablerIconGlyph.Clock };
        menuAdd.Items.Add(menuTime);

        if (Resources["ContextMenu"] is MenuFlyout menu1)
        {
            menu1.Items.Insert(0, menuAdd);
        }

        if (Resources["ContextMenu_Conditions"] is MenuFlyout menu2)
        {
            menu2.Items.Insert(0, menuAdd);
        }
    }

    private void RuleProcessChangedMessageHandler(object recipient, RuleProcessChangedMessage message)
    {
        if (!_processEditorOpened) { return; }

        _processEditorOpened = false;

        if (ViewModel.Model.Conditions.Count <= message.Value.Item1) { return; }

        var cond = ViewModel.Model.Conditions.ElementAt(message.Value.Item1);
        if (cond is not ProcessRuleCondition rule) { return; }

        rule.ProcessName = message.Value.Item2;
        rule.MemoryLower = message.Value.Item3;
        rule.MemoryUpper = message.Value.Item4;
        rule.MemoryType = message.Value.Item5;

        _ruleService.Update();
        _ruleService.Save();
    }

    private void RuleSensorChangedMessageHandler(object recipient, RuleSensorChangedMessage message)
    {
        if (!_sensorEditorOpened) { return; };

        _sensorEditorOpened = false;

        if (ViewModel.Model.Conditions.Count <= message.Value.Item1) { return; }

        var cond = ViewModel.Model.Conditions.ElementAt(message.Value.Item1);
        if (cond is not SensorRuleCondition sensor) { return; }

        sensor.SensorId = message.Value.Item2;
        sensor.LowerValue = message.Value.Item3;
        sensor.IncludeLower = message.Value.Item4;
        sensor.UpperValue = message.Value.Item5;
        sensor.IncludeUpper = message.Value.Item6;

        _ruleService.Update();
        _ruleService.Save();
    }

    private void RuleTimeChangedMessageHandler(object recipient, RuleTimeChangedMessage message)
    {
        if (!_timeEditorOpened) { return; }

        _timeEditorOpened = false;

        if (ViewModel.Model.Conditions.Count <= message.Value.Item1) { return; }

        var cond = ViewModel.Model.Conditions.ElementAt(message.Value.Item1);
        if (cond is not TimeRuleCondition time) { return; }

        time.SetBeginningTime(message.Value.Item2);
        time.SetEndingTime(message.Value.Item3);

        _ruleService.Update();
        _ruleService.Save();
    }

    private void AddCondition_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.CommandParameter is string param)
        {
            ViewModel.AddCondition(param);
        }
    }

    private async void DeleteCondition_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.CommandParameter is RuleConditionBase cond)
        {
            if (await RulesPage.OpenDeleteDialog(cond.Text))
            {
                ViewModel.RemoveCondition(cond);
            }
        }
    }

    private async void DeleteSelf_Click(object sender, RoutedEventArgs e)
    {
        if (await RulesPage.OpenDeleteDialog(ViewModel.Model.Name))
        {
            ViewModel.RemoveSelf();
        }
    }

    private void EditCondition_Click(object sender, RoutedEventArgs e)
    {
        object? cond = null;
        if (sender is MenuFlyoutItem item && item.CommandParameter is RuleConditionBase c)
        {
            cond = c;
        }

        if (cond is RuleConditionBase condition)
        {
            OpenEditor(condition);
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

    private void Border_ContextRequested(UIElement sender, ContextRequestedEventArgs e)
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

    private void ListView_ContextRequested(UIElement sender, ContextRequestedEventArgs e)
    {
        if (Resources["ContextMenu_Conditions"] is not MenuFlyout menu) { return; }
        if (e.OriginalSource is not FrameworkElement elm) { return; }

        if (elm.DataContext is RuleConditionBase cond1) // right click on item
        {
            DeleteCondition.IsEnabled = EditCondition.IsEnabled = true;
            DeleteCondition.CommandParameter = EditCondition.CommandParameter = cond1;

            if (e.TryGetPosition(elm, out var point))
            {
                menu.ShowAt(elm, point);
                goto End;
            }
        }
        else if (elm is ListViewItem lvi && lvi.Content is RuleConditionBase cond2) // menu key on item
        {
            DeleteCondition.IsEnabled = EditCondition.IsEnabled = true;
            DeleteCondition.CommandParameter = EditCondition.CommandParameter = cond2;
        }
        else // right click on empty space
        {
            DeleteCondition.IsEnabled = EditCondition.IsEnabled = false;
            DeleteCondition.CommandParameter = EditCondition.CommandParameter = null;

            if (e.TryGetPosition(sender, out var point))
            {
                menu.ShowAt(sender, point);
                goto End;
            }
        }

        menu.ShowAt(elm);

    End:
        e.Handled = true;
    }

    private void ListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is RuleConditionBase cond)
        {
            OpenEditor(cond);
        }
    }

    private async void OpenEditor(RuleConditionBase condition)
    {
        if (condition is TimeRuleCondition time)
        {
            _timeEditorOpened = await RulesPage.OpenTimeEditor(
                ViewModel.Model.Conditions.IndexOf(time),
                time.TimeBeginning,
                time.TimeEnding);
        }
        else if (condition is ProcessRuleCondition process)
        {
            _processEditorOpened = await RulesPage.OpenProcessEditor(
                ViewModel.Model.Conditions.IndexOf(process),
                process.ProcessName,
                process.MemoryLower,
                process.MemoryUpper,
                process.MemoryType);
        }
        else if (condition is SensorRuleCondition sensor)
        {
            _sensorEditorOpened = await RulesPage.OpenSensorEditor(
                ViewModel.Model.Conditions.IndexOf(sensor),
                sensor.SensorId,
                sensor.LowerValue,
                sensor.IncludeLower,
                sensor.UpperValue,
                sensor.IncludeUpper);
        }
    }
}
