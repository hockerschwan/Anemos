using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class RulesPage : Page
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();

    private readonly IRuleService _ruleService = App.GetService<IRuleService>();

    public RulesViewModel ViewModel
    {
        get;
    }

    private RuleConditionArg _result = new();

    private bool _isDialogShown = false;

    public RulesPage()
    {
        _messenger.Register<OpenRuleTimeEditorMessage>(this, OpenRuleTimeEditorMessageHandler);
        _messenger.Register<OpenRuleProcessEditorMessage>(this, OpenRuleProcessEditorMessageHandler);
        _messenger.Register<OpenRuleSensorEditorMessage>(this, OpenRuleSensorEditorMessageHandler);
        _messenger.Register<RuleEditorResultMessage>(this, RuleEditorResultMessageHandler);

        ViewModel = App.GetService<RulesViewModel>();
        Loaded += RulesPage_Loaded;
        Unloaded += RulesPage_Unloaded;
        InitializeComponent();
    }

    private void RulesPage_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.IsVisible = true;
    }

    private void RulesPage_Unloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.IsVisible = false;
    }

    private async void OpenRuleTimeEditorMessageHandler(object recipient, OpenRuleTimeEditorMessage message)
    {
        if (_isDialogShown) { return; }

        var cond = message.Value;

        var dialog = ViewModel.TimeEditorDialog;
        dialog.SetTime(
            new TimeSpan(cond.TimeBeginning.Hour, cond.TimeBeginning.Minute, 0),
            new TimeSpan(cond.TimeEnding.Hour, cond.TimeEnding.Minute, 0));

        SetDialogStyle(ViewModel.TimeEditorDialog);

        _isDialogShown = true;

        var result = await ViewModel.TimeEditorDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            cond.SetBeginningTime((TimeOnly)_result.TimeBeginning!);
            cond.SetEndingTime((TimeOnly)_result.TimeEnding!);

            var model = ViewModel.Models.Single(m => m == cond.Parent);
            model.Update();

            _ruleService.Update();
            _ruleService.Save();
        }

        _isDialogShown = false;
        _result = new();
    }

    private async void OpenRuleProcessEditorMessageHandler(object recipient, OpenRuleProcessEditorMessage message)
    {
        if (_isDialogShown) { return; }

        var cond = message.Value;
        ViewModel.ProcessEditorDialog.Set(cond);

        SetDialogStyle(ViewModel.ProcessEditorDialog);

        _isDialogShown = true;

        var result = await ViewModel.ProcessEditorDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            cond.ProcessName = _result.ProcessName!;
            cond.MemoryType = _result.MemoryType ?? 0;
            cond.MemoryLower = _result.MemoryLower;
            cond.MemoryUpper = _result.MemoryUpper;

            var model = ViewModel.Models.Single(m => m == cond.Parent);
            model.Update();

            _ruleService.Update();
            _ruleService.Save();
        }

        _isDialogShown = false;
        _result = new();
    }

    private async void OpenRuleSensorEditorMessageHandler(object recipient, OpenRuleSensorEditorMessage message)
    {
        if (_isDialogShown) { return; }

        var cond = message.Value;
        ViewModel.SensorEditorDialog.ViewModel.SensorId = cond.SensorId ?? string.Empty;
        ViewModel.SensorEditorDialog.ViewModel.LowerValue = cond.LowerValue ?? double.NaN;
        ViewModel.SensorEditorDialog.ViewModel.UpperValue = cond.UpperValue ?? double.NaN;
        ViewModel.SensorEditorDialog.ViewModel.IndexIncludeLower = cond.IncludeLower ? 1 : 0;
        ViewModel.SensorEditorDialog.ViewModel.IndexIncludeUpper = cond.IncludeUpper ? 1 : 0;

        SetDialogStyle(ViewModel.SensorEditorDialog);

        _isDialogShown = true;

        var result = await ViewModel.SensorEditorDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            cond.SensorId = _result.SensorId!;
            cond.LowerValue = _result.LowerValue;
            cond.UpperValue = _result.UpperValue;
            cond.IncludeLower = _result.IncludeLower!.Value;
            cond.IncludeUpper = _result.IncludeUpper!.Value;

            var model = ViewModel.Models.Single(m => m == cond.Parent);
            model.Update();

            _ruleService.Update();
            _ruleService.Save();
        }

        _isDialogShown = false;
        _result = new();
        ViewModel.SensorEditorDialog.ViewModel.Reset();
    }

    private void RuleEditorResultMessageHandler(object recipient, RuleEditorResultMessage message)
    {
        _result = message.Value;
    }

    private void SetDialogStyle(ContentDialog dialog)
    {
        dialog.XamlRoot = XamlRoot;
        dialog.Style = App.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.PrimaryButtonText = "Dialog_OK".GetLocalized();
        dialog.CloseButtonText = "Dialog_Cancel".GetLocalized();
        dialog.DefaultButton = ContentDialogButton.Primary;
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var cb = (ComboBox)sender;
        if (cb.SelectedIndex == -1)
        {
            cb.SelectedIndex = ViewModel.Profiles.ToList().IndexOf(ViewModel.DefaultProfile!);
        }
    }
}
