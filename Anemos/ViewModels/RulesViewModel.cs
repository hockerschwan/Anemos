using System.Collections.ObjectModel;
using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using Anemos.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Anemos.ViewModels;

public partial class RulesViewModel : ObservableRecipient
{
    private readonly IFanService _fanService = App.GetService<IFanService>();

    private readonly IRuleService _ruleService = App.GetService<IRuleService>();

    public ObservableCollection<RuleModel> Models
    {
        get;
    }

    private ObservableCollection<RuleViewModel> ViewModels
    {
        get;
    }

    public ObservableCollection<RuleView> Views
    {
        get;
    }

    private bool _isVisible = true;
    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public RuleTimeEditorDialog TimeEditorDialog
    {
        get;
    }

    public RuleProcessEditorDialog ProcessEditorDialog
    {
        get;
    }

    public IEnumerable<FanProfile> Profiles => _fanService.Profiles;

    private FanProfile? _defaultProfile;
    public FanProfile? DefaultProfile
    {
        get => _defaultProfile;
        set
        {
            if (value == null)
            {
                value = _ruleService.DefaultProfile;
            }

            if (SetProperty(ref _defaultProfile, value))
            {
                _ruleService.DefaultProfileId = _defaultProfile?.Id ?? string.Empty;
            }
        }
    }

    public RulesViewModel()
    {
        Messenger.Register<WindowVisibilityChangedMessage>(this, WindowVisibilityChangedMessageHandler);
        Messenger.Register<RulesChangedMessage>(this, RulesChangedMessageHandler);

        Models = new(_ruleService.Rules);
        ViewModels = new(Models.Select(m => new RuleViewModel(m)));
        Views = new(ViewModels.Select(vm => new RuleView(vm)));

        TimeEditorDialog = new();
        ProcessEditorDialog = new();

        _defaultProfile = _ruleService.DefaultProfile;
    }

    private void WindowVisibilityChangedMessageHandler(object recipient, WindowVisibilityChangedMessage message)
    {
        IsVisible = message.Value;
    }

    private void RulesChangedMessageHandler(object recipient, RulesChangedMessage message)
    {
        var diff = message.NewValue.Count() - message.OldValue.Count();
        if (diff > 0)
        {
            var added = message.NewValue.Except(message.OldValue).ToList();
            foreach (var model in added)
            {
                Models.Add(model);

                var vm = new RuleViewModel(model);
                ViewModels.Add(vm);
                Views.Add(new RuleView(vm));
            }
        }
        else if (diff < 0)
        {
            var removed = message.OldValue.Except(message.NewValue).ToList();
            foreach (var model in removed.ToList())
            {
                Models.Remove(model);

                var vm = ViewModels.FirstOrDefault(vm => vm?.Model == model, null);
                if (vm != null)
                {
                    ViewModels.Remove(vm);
                }

                var v = Views.FirstOrDefault(v => (v?.ViewModel)?.Model == model, null);
                if (v != null)
                {
                    Views.Remove(v);
                }
            }
        }
        else
        {
            for (var i = 0; i < message.NewValue.Count(); ++i)
            {
                var j = Models.IndexOf(message.NewValue.ElementAt(i));
                if (j != -1 && i != j)
                {
                    var model = Models[i];
                    Models.RemoveAt(i);
                    Models.Insert(j, model);

                    var vm = ViewModels[i];
                    ViewModels.RemoveAt(i);
                    ViewModels.Insert(j, vm);

                    var view = Views[i];
                    Views.RemoveAt(i);
                    Views.Insert(j, view);

                    break;
                }
            }
        }

        foreach (var view in Views)
        {
            view.ViewModel.UpdateArrows();
        }
    }

    [RelayCommand]
    private void AddRule()
    {
        _ruleService.AddRule(new()
        {
            Name = "Rule_NewRuleName".GetLocalized(),
            ProfileId = _fanService.CurrentProfileId,
            Type = RuleType.All
        });
    }
}
