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

    private readonly System.Text.StringBuilder _stringBuilder = new();

    public override string Text
    {
        get
        {
            _stringBuilder.Clear();
            if (ProcessName == string.Empty)
            {
                _stringBuilder.Append("RuleProcess_ProcessAny".GetLocalized());
            }
            else
            {
                _stringBuilder.Append(ProcessName);
            }

            if (MemoryLower == null && MemoryUpper == null)
            {
                return _stringBuilder.ToString();
            }

            _stringBuilder.Append(", ");

            if (MemoryLower != null)
            {
                _stringBuilder.Append("RuleProcess_MemoryLowerText".GetLocalized().Replace("$", MemoryLower.ToString()));
            }

            switch (MemoryType)
            {
                case 0:
                    _stringBuilder.Append("RuleProcessEditor_PrivateBytes/Text".GetLocalized());
                    break;
                case 1:
                    _stringBuilder.Append("RuleProcessEditor_WorkingSet/Text".GetLocalized());
                    break;
            }

            if (MemoryUpper != null)
            {
                _stringBuilder.Append("RuleProcess_MemoryUpperText".GetLocalized().Replace("$", MemoryUpper.ToString()));
            }

            return _stringBuilder.ToString();
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
        if (pr.Length == 0 && MemoryLower == null && MemoryUpper == null)
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
