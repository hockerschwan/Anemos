using Anemos.Helpers;
using Anemos.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class RulesPage : Page
{
    public RulesViewModel ViewModel
    {
        get;
    }

    public RulesPage()
    {
        ViewModel = App.GetService<RulesViewModel>();
        InitializeComponent();
        Loaded += RulesPage_Loaded;
    }

    private void RulesPage_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= RulesPage_Loaded;
        ViewModel.IsVisible = true;
    }

    public static async Task<bool> OpenDeleteDialog(string name)
    {
        var dialog = new ContentDialog
        {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            PrimaryButtonStyle = Application.Current.Resources["DangerButtonStyle_"] as Style,
            Title = "Dialog_DeleteRule_Title".GetLocalized(),
            PrimaryButtonText = "Dialog_Delete".GetLocalized(),
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "Dialog_Cancel".GetLocalized(),
            Content = "Dialog_Delete_Content".GetLocalized().Replace("$", name),
        };
        return await App.GetService<ShellPage>().OpenDialog(dialog);
    }

    public static async Task<bool> OpenProcessEditor(
        int index,
        string processName,
        int? memoryLow,
        int? memoryHigh,
        int type)
    {
        var dialog = new RuleProcessEditorDialog(index, processName, memoryLow, memoryHigh, type)
        {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            PrimaryButtonStyle = Application.Current.Resources["AccentButtonStyle"] as Style,
            Title = "RuleProcessEditor_Title".GetLocalized(),
            PrimaryButtonText = "Dialog_OK".GetLocalized(),
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "Dialog_Cancel".GetLocalized(),
        };
        return await App.GetService<ShellPage>().OpenDialog(dialog);
    }

    public static async Task<bool> OpenSensorEditor(
        int index,
        string id,
        double? lower,
        bool includeLower,
        double? upper,
        bool includeUpper)
    {
        var dialog = new RuleSensorEditorDialog(index, id, lower, includeLower, upper, includeUpper)
        {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            PrimaryButtonStyle = Application.Current.Resources["AccentButtonStyle"] as Style,
            Title = "RuleSensorEditor_Title".GetLocalized(),
            PrimaryButtonText = "Dialog_OK".GetLocalized(),
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "Dialog_Cancel".GetLocalized(),
        };
        return await App.GetService<ShellPage>().OpenDialog(dialog);
    }

    public static async Task<bool> OpenTimeEditor(int index, TimeOnly begin, TimeOnly end)
    {
        var dialog = new RuleTimeEditorDialog(index, begin, end)
        {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            PrimaryButtonStyle = Application.Current.Resources["AccentButtonStyle"] as Style,
            Title = "RuleTimeEditor_Title".GetLocalized(),
            PrimaryButtonText = "Dialog_OK".GetLocalized(),
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "Dialog_Cancel".GetLocalized(),
        };
        return await App.GetService<ShellPage>().OpenDialog(dialog);
    }
}
