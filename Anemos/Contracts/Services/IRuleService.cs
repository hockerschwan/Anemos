using Anemos.Models;

namespace Anemos.Contracts.Services;

public interface IRuleService
{
    List<RuleModel> Rules
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

    void AddRule(RuleArg arg);

    void DecreasePriority(RuleModel rule);

    void IncreasePriority(RuleModel rule);

    Task LoadAsync();

    void RemoveRule(RuleModel rule);

    void Save();

    void Update();
}
