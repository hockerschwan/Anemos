namespace Anemos.Models;

public class TimeRuleCondition : RuleConditionBase
{
    public override bool IsSatisfied
    {
        get
        {
            var c = TimeBeginning.CompareTo(TimeEnding);
            if (c < 0)
            {
                var now = TimeOnly.FromDateTime(DateTime.Now);
                return TimeBeginning.CompareTo(now) <= 0 && TimeEnding.CompareTo(now) >= 0;
            }
            else if (c > 0) // TimeBeginning is later than TimeEnding
            {
                var now = TimeOnly.FromDateTime(DateTime.Now);
                return !(TimeBeginning.CompareTo(now) <= 0 && TimeEnding.CompareTo(now) >= 0);
            }
            else // TimeBeginning == TimeEnding
            {
                return true;
            }
        }
    }

    public TimeOnly TimeBeginning
    {
        get;
        private set;
    }

    public TimeOnly TimeEnding
    {
        get;
        private set;
    }

    public override string Text => $"{TimeBeginning:HH:mm} - {TimeEnding:HH:mm}";

    public TimeRuleCondition(RuleModel parent, TimeOnly timeBegin, TimeOnly timeEnd) : base(parent)
    {
        TimeBeginning = timeBegin;
        TimeEnding = timeEnd;
    }

    public void SetBeginningTime(TimeOnly time)
    {
        TimeBeginning = time;
        OnPropertyChanged(nameof(Text));
    }

    public void SetEndingTime(TimeOnly time)
    {
        TimeEnding = time;
        OnPropertyChanged(nameof(Text));
    }
}
