﻿namespace Anemos.Models;

public class TimeRuleCondition(RuleModel parent, TimeOnly timeBegin, TimeOnly timeEnd) : RuleConditionBase(parent)
{
    public TimeOnly TimeBeginning { get; private set; } = timeBegin;

    public TimeOnly TimeEnding { get; private set; } = timeEnd;

    public override string Text => $"{TimeBeginning:HH:mm} - {TimeEnding:HH:mm}";

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

    public override void Update()
    {
        var c = TimeBeginning.CompareTo(TimeEnding);
        if (c == 0) // TimeBeginning == TimeEnding
        {
            IsSatisfied = true;
            return;
        }

        var now = TimeOnly.FromDateTime(DateTime.Now);
        if (c < 0)
        {
            IsSatisfied = TimeBeginning.CompareTo(now) <= 0 && TimeEnding.CompareTo(now) > 0;
        }
        else // TimeBeginning is later than TimeEnding
        {
            IsSatisfied = !(TimeBeginning.CompareTo(now) > 0 && TimeEnding.CompareTo(now) <= 0);
        }
    }
}
