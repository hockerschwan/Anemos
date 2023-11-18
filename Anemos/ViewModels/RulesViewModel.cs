using System.Collections.ObjectModel;
using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using Anemos.Views;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Anemos.ViewModels;
public partial class RulesViewModel : PageViewModelBase
{
    private readonly IMessenger _messenger;
    private readonly IFanService _fanService;
    private readonly IRuleService _ruleService;

    public ObservableCollection<RuleViewModel> ViewModels { get; } = [];
    public ObservableCollection<RuleView> Views { get; } = [];

    public ObservableCollection<FanProfile> Profiles
    {
        get;
    }

    private FanProfile? _defaultProfile;
    public FanProfile? DefaultProfile
    {
        get => _defaultProfile;
        set
        {
            if (SetProperty(ref _defaultProfile, value) && _defaultProfile != null)
            {
                _ruleService.DefaultProfileId = _defaultProfile.Id;
            }
        }
    }

    private bool _isVisible;
    public override bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    private readonly MessageHandler<object, FanProfilesChangedMessage> _fanProfilesChangedMessageHandler;
    private readonly MessageHandler<object, RulesChangedMessage> _rulesChangedMessageHandler;

    public RulesViewModel(IMessenger messenger, IFanService fanService, IRuleService ruleService)
    {
        _messenger = messenger;
        _fanService = fanService;
        _ruleService = ruleService;

        _fanProfilesChangedMessageHandler = FanProfilesChangedMessageHandler;
        _rulesChangedMessageHandler = RulesChangedMessageHandler;
        _messenger.Register(this, _fanProfilesChangedMessageHandler);
        _messenger.Register(this, _rulesChangedMessageHandler);

        Profiles = new(_fanService.Profiles);

        foreach (var m in _ruleService.Rules)
        {
            var vm = new RuleViewModel(m);
            ViewModels.Add(vm);
            Views.Add(new RuleView(vm));
        }

        _defaultProfile = _fanService.GetProfile(_ruleService.DefaultProfileId);
    }

    private void FanProfilesChangedMessageHandler(object recipient, FanProfilesChangedMessage message)
    {
        var removed = message.OldValue.Except(message.NewValue).ToList();
        foreach (var p in removed)
        {
            Profiles.Remove(p);
        }

        var added = message.NewValue.Except(message.OldValue).ToList();
        foreach (var p in added)
        {
            Profiles.Add(p);
        }

        if (DefaultProfile == null || removed.Contains(DefaultProfile))
        {
            DefaultProfile = _fanService.CurrentProfile;
            _ruleService.DefaultProfileId = DefaultProfile!.Id;
        }
    }

    private void RulesChangedMessageHandler(object recipient, RulesChangedMessage message)
    {
        var removed = message.OldValue.Except(message.NewValue).ToList();
        foreach (var model in removed)
        {
            foreach (var vm in ViewModels)
            {
                if (vm.Model == model)
                {
                    ViewModels.Remove(vm);
                    break;
                }
            }

            foreach (var v in Views)
            {
                if (v.ViewModel.Model == model)
                {
                    Views.Remove(v);
                    break;
                }
            }
        }

        var added = message.NewValue.Except(message.OldValue).ToList();
        foreach (var model in added)
        {
            var vm = new RuleViewModel(model);
            ViewModels.Add(vm);
            Views.Add(new RuleView(vm));
        }

        if (removed.Count == 0 && added.Count == 0) // priority changed
        {
            var models = ViewModels.Select(vm => vm.Model).ToList();
            for (var i = 0; i < message.NewValue.Count(); ++i)
            {
                var j = models.IndexOf(message.NewValue.ElementAt(i));
                if (j != -1 && i != j)
                {
                    var model = models[i];
                    models.RemoveAt(i);
                    models.Insert(j, model);

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

        foreach (var vm in ViewModels)
        {
            vm.UpdatePriorityButtons();
        }
    }

    [RelayCommand]
    private void AddRule()
    {
        _ruleService.AddRule(new()
        {
            Name = "Rules_NewRuleName".GetLocalized(),
            ProfileId = _fanService.ManualProfileId,
            Type = RuleType.All
        });
    }
}
