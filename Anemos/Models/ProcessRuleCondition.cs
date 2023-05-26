using System.Diagnostics;
using Anemos.Helpers;

namespace Anemos.Models;

public class ProcessRuleCondition : RuleConditionBase
{
    private string _processName = string.Empty;
    public string ProcessName
    {
        get => _processName;
        set
        {
            if (SetProperty(ref _processName, value))
            {
                OnPropertyChanged(nameof(Text));
            }
        }
    }

    private int _memoryType = 0;
    public int MemoryType
    {
        get => _memoryType;
        set
        {
            if (SetProperty(ref _memoryType, value))
            {
                OnPropertyChanged(nameof(Text));
            }
        }
    }

    private int? _memoryLower;
    public int? MemoryLower
    {
        get => _memoryLower;
        set
        {
            if (value <= 0)
            {
                value = null;
            }

            if (SetProperty(ref _memoryLower, value))
            {
                OnPropertyChanged(nameof(Text));
            }
        }
    }

    private int? _memoryUpper;
    public int? MemoryUpper
    {
        get => _memoryUpper;
        set
        {
            if (value <= 0)
            {
                value = null;
            }

            if (SetProperty(ref _memoryUpper, value))
            {
                OnPropertyChanged(nameof(Text));
            }
        }
    }

    public override string Text
    {
        get
        {
            var str = ProcessName;
            if (ProcessName == string.Empty)
            {
                str += "Rule_AnyProcess".GetLocalized();
            }

            if (MemoryLower == null && MemoryUpper == null)
            {
                return str;
            }

            str += ", ";

            if (MemoryLower != null)
            {
                str += $"{MemoryLower} {"Unit_MegaByte".GetLocalized()} < ";
            }

            str += $"{(MemoryType == 0 ? "Rule_MemoryType_PrivateBytes".GetLocalized() : "Rule_MemoryType_WorkingSet".GetLocalized())}";

            if (MemoryUpper != null)
            {
                str += $" < {MemoryUpper} {"Unit_MegaByte".GetLocalized()}";
            }

            return str;
        }
    }

    public ProcessRuleCondition(RuleModel parent, RuleConditionArg arg) : base(parent)
    {
        ProcessName = new(arg.ProcessName);
        MemoryType = arg.MemoryType ?? 0;
        MemoryLower = arg.MemoryLower;
        MemoryUpper = arg.MemoryUpper;
    }

    public override void Update()
    {
        var pr = ProcessName == string.Empty ? Process.GetProcesses() : Process.GetProcessesByName(ProcessName);
        if (!pr.Any() && MemoryLower == null && MemoryUpper == null)
        {
            IsSatisfied = false;
            return;
        }

        foreach (var p in pr)
        {
            var mem = MemoryType == 0 ? p.PrivateMemorySize64 : p.WorkingSet64;
            mem /= 1024 * 1024;

            if (MemoryLower != null && mem <= MemoryLower) { continue; }
            if (MemoryUpper != null && mem >= MemoryUpper) { continue; }

            IsSatisfied = true;
            return;
        }

        IsSatisfied = false;
    }
}
