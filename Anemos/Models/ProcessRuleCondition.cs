using System.Diagnostics;

namespace Anemos.Models;

public class ProcessRuleCondition : RuleConditionBase
{
    public string ProcessName
    {
        get;
        private set;
    }

    public override string Text => ProcessName;

    public ProcessRuleCondition(RuleModel parent, string processName) : base(parent)
    {
        ProcessName = new(processName);
    }

    public void SetProcessName(string processName)
    {
        ProcessName = processName;
        OnPropertyChanged(nameof(Text));
    }

    public override void Update()
    {
        IsSatisfied = Process.GetProcesses().Select(p => p.ProcessName).Contains(ProcessName);
    }
}
