using System.Globalization;
using clocknet.Display;
using clocknet.Utils;

namespace clocknet.Reports;

public class WorktimeReport : IReport
{
    private readonly IDisplay display;
    private readonly bool perDay;

    public WorktimeReport(IDisplay display, bool perDay = true)
    {
        this.display = display;
        this.perDay = perDay;
    }

    public void Print(IEnumerable<Activity> activities)
    {
        var lines = activities
            .SelectMany(x => x.Records)
            .GroupBy(x => x.StartTime.Date)
            .Select(x => PrintDay(x.Key, x))
            .Append(" ")
            .Append(TotalTime(activities))
            .Prepend("Daily Worktime Report");

        if (!perDay)
        {
            lines = activities
                .SelectMany(x => x.Records)
                .GroupBy(x => Utilities.GetWeekNumber(x.StartTime))
                .Select(x => PrintWeek(x.Key, x.ToList()))
                .Append(" ")
                .Append(TotalTime(activities))
                .Prepend("Weekly Report");
	    }

        display.Print(lines);
    }

    private string TotalTime(IEnumerable<Activity> activities) => display.Layout($"{activities.Duration()} Total");

    private string PrintWeek(int weekNumber, IEnumerable<Record> records) => display.Layout($"{records.Duration()} Week {weekNumber}");

    private string PrintDay(DateTime day, IEnumerable<Record> records) => display.Layout($"{records.Duration()} {day:yyyy-MM-dd}");
}

