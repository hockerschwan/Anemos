using Anemos.Models;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class RuleView : UserControl
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();

    public RuleViewModel ViewModel
    {
        get;
    }

    public RuleView(RuleViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    private void CloseFlyout(object sender, RoutedEventArgs e)
    {
        ViewModel.AddConditionCommand.Execute((sender as Button)?.CommandParameter);
        AddConditionFlyout.Hide();
    }

    private void RemoveCondition(object sender, RoutedEventArgs e)
    {
        var cond = (Models.RuleConditionBase?)(sender as Button)?.CommandParameter;
        if (cond != null)
        {
            ViewModel.RemoveConditionCommand.Execute(cond);
        }
    }

    private void OpenEditor(object sender, RoutedEventArgs e)
    {
        var cond = (Models.RuleConditionBase?)(sender as Button)?.CommandParameter;
        if (cond == null)
        {
            return;
        }

        if (cond is TimeRuleCondition time)
        {
            _messenger.Send(new OpenRuleTimeEditorMessage(time));
        }
        else if (cond is ProcessRuleCondition process)
        {
            _messenger.Send(new OpenRuleProcessEditorMessage(process));
        }
    }
}
