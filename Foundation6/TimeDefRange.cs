﻿namespace Foundation;

public class TimeDefRange : TimeDefVisitor
{
    public Option<TimeDef> Greatest { get; private set; }

    protected bool SetGreatest(TimeDef? td)
    {
        ArgumentNullException.ThrowIfNull(td);

        if (!Greatest.TryGet(out var timeDef) || timeDef.CompareTo(td) == -1)
            Greatest = Option.Some(td);

        return true;
    }

    protected bool SetSmallest(TimeDef td)
    {
        if (!Smallest.TryGet(out var timeDef) || timeDef.CompareTo(td) == 1)
            Smallest = Option.Some(td);

        return true;
    }

    public Option<TimeDef> Smallest { get; private set; }

    protected override bool VisitDateSpan(TimeDef.DateSpan td)
    {
        SetGreatest(td);
        SetSmallest(td);

        return true;
    }

    protected override bool VisitDateTimeSpan(TimeDef.DateTimeSpan td)
    {
        SetGreatest(td);
        SetSmallest(td);

        return true;
    }

    protected override bool VisitDay(TimeDef.Day td)
    {
        SetGreatest(td);
        SetSmallest(td);

        return true;
    }

    protected override bool VisitDays(TimeDef.Days td)
    {
        SetGreatest(td);
        SetSmallest(td);

        return true;
    }

    protected override bool VisitHour(TimeDef.Hour td)
    {
        SetGreatest(td);
        SetSmallest(td);

        return true;
    }

    protected override bool VisitHours(TimeDef.Hours td)
    {
        SetGreatest(td);
        SetSmallest(td);

        return true;
    }

    protected override bool VisitMinute(TimeDef.Minute td)
    {
        SetGreatest(td);
        SetSmallest(td);

        return true;
    }

    protected override bool VisitMinutes(TimeDef.Minutes td)
    {
        SetGreatest(td);
        SetSmallest(td);

        return true;
    }

    protected override bool VisitMonth(TimeDef.Month td)
    {
        SetGreatest(td);
        SetSmallest(td);

        return true;
    }

    protected override bool VisitMonths(TimeDef.Months td)
    {
        SetGreatest(td);
        SetSmallest(td);

        return true;
    }

    protected override bool VisitTimespan(TimeDef.Timespan td)
    {
        SetGreatest(td);
        SetSmallest(td);

        return true;
    }

    protected override bool VisitWeekday(TimeDef.Weekday td)
    {
        SetGreatest(td);
        SetSmallest(td);

        return true;
    }

    protected override bool VisitWeekOfMonth(TimeDef.WeekOfMonth td)
    {
        SetGreatest(td);
        SetSmallest(td);

        return true;
    }

    protected override bool VisitWeeks(TimeDef.Weeks td)
    {
        SetGreatest(td);
        SetSmallest(td);

        return true;
    }

    protected override bool VisitYear(TimeDef.Year td)
    {
        SetGreatest(td);
        SetSmallest(td);

        return true;
    }

    protected override bool VisitYears(TimeDef.Years td)
    {
        SetGreatest(td);
        SetSmallest(td);

        return true;
    }
}

