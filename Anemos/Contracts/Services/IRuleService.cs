using System.Collections.ObjectModel;
using Anemos.Models;

namespace Anemos.Contracts.Services;

public interface IRuleService
{
    RangeObservableCollection<RuleModel> Rules
    {
        get;
    }

    RuleModel? CurrentRule
    {
        get;
    }

    string DefaultProfileId
    {
        get; set;
    }

    FanProfile DefaultProfile
    {
        get;
    }

    Task InitializeAsync();

    void Update();

    void AddRule(RuleArg arg);

    void AddRules(IEnumerable<RuleArg> args, bool save);

    void RemoveRule(RuleModel rule);

    void Save();

    void IncreasePriority(RuleModel rule);

    void DecreasePriority(RuleModel rule);
}
