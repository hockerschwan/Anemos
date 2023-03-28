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
        _messenger.Register<RuleEditorResultMessage>(this, RuleEditorResultMessageHandler);

        ViewModel = App.GetService<RulesViewModel>();
        InitializeComponent();
    }

    private async void OpenRuleTimeEditorMessageHandler(object recipient, OpenRuleTimeEditorMessage message)
    {
        if (_isDialogShown)
        {
            return;
        }

        var cond = message.Value;

        var dialog = ViewModel.TimeEditorDialog;
        dialog.SetTime(
            new TimeSpan(cond.TimeBeginning.Hour, cond.TimeBeginning.Minute, 0),
            new TimeSpan(cond.TimeEnding.Hour, cond.TimeEnding.Minute, 0));

        SetTimeEditorDialogStyle();

        _isDialogShown = true;

        var result = await ViewModel.TimeEditorDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            cond.SetBeginningTime((TimeOnly)_result.TimeBeginning!);
            cond.SetEndingTime((TimeOnly)_result.TimeEnding!);

            var model = ViewModel.Models.Single(m => m == cond.Parent);
            model.Update();

            _ruleService.Save();
        }

        _isDialogShown = false;
        _result = new();
    }

    private async void OpenRuleProcessEditorMessageHandler(object recipient, OpenRuleProcessEditorMessage message)
    {
        if (_isDialogShown)
        {
            return;
        }

        var cond = message.Value;
        ViewModel.ProcessEditorDialog.SetText(cond.ProcessName);

        SetProcessEditorDialogStyle();

        _isDialogShown = true;

        var result = await ViewModel.ProcessEditorDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            cond.SetProcessName(_result.ProcessName!);

            var model = ViewModel.Models.Single(m => m == cond.Parent);
            model.Update();

            _ruleService.Save();
        }

        _isDialogShown = false;
        _result = new();
    }

    private void RuleEditorResultMessageHandler(object recipient, RuleEditorResultMessage message)
    {
        _result = message.Value;
    }

    private void SetTimeEditorDialogStyle()
    {
        var dialog = ViewModel.TimeEditorDialog;
        dialog.XamlRoot = XamlRoot;
        dialog.Style = App.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.PrimaryButtonText = "Dialog_OK".GetLocalized();
        dialog.CloseButtonText = "Dialog_Cancel".GetLocalized();
        dialog.DefaultButton = ContentDialogButton.Primary;
    }

    private void SetProcessEditorDialogStyle()
    {
        var dialog = ViewModel.ProcessEditorDialog;
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
