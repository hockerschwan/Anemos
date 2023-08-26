using Anemos.Contracts.Services;
using Anemos.Models;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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

    public RuleView(RuleViewModel viewModel)
    {
        _messenger.Register<RuleTimeChangedMessage>(this, RuleTimeChangedMessageHandler);
        _messenger.Register<RuleProcessChangedMessage>(this, RuleProcessChangedMessageHandler);
        _messenger.Register<RuleSensorChangedMessage>(this, RuleSensorChangedMessageHandler);

        ViewModel = viewModel;
        InitializeComponent();
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

        _ruleService.Save();
    }

    private void AddConditionFlyoutButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) { return; }

        AddConditionFlyout.Hide();
        ViewModel.AddCondition((string)btn.CommandParameter);
    }

    private void DeleteConditionButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.RemoveCondition((Models.RuleConditionBase)((Button)sender).CommandParameter);
    }

    private async void DeleteSelfButton_Click(object sender, RoutedEventArgs e)
    {
        if (await RulesPage.OpenDeleteDialog(ViewModel.Model.Name))
        {
            ViewModel.RemoveSelf();
        }
    }

    private async void EditConditionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) { return; }

        if (btn.CommandParameter is TimeRuleCondition time)
        {
            _timeEditorOpened = await RulesPage.OpenTimeEditor(
                ViewModel.Model.Conditions.IndexOf(time),
                time.TimeBeginning,
                time.TimeEnding);
        }
        else if (btn.CommandParameter is ProcessRuleCondition process)
        {
            _processEditorOpened = await RulesPage.OpenProcessEditor(
                ViewModel.Model.Conditions.IndexOf(process),
                process.ProcessName,
                process.MemoryLower,
                process.MemoryUpper,
                process.MemoryType);
        }
        else if (btn.CommandParameter is SensorRuleCondition sensor)
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
