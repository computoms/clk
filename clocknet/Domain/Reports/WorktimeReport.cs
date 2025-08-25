using clocknet.Utils;

namespace clocknet.Domain.Reports;

public class WorktimeReport : IReport
{
    private readonly IDisplay display;
    private readonly bool perDay;

    public WorktimeReport(IDisplay display, bool perDay = true)
    {
        this.display = display;
        this.perDay = perDay;
    }

    public Option Name { get; } = Args.WorkTimes;

    public void Print(IEnumerable<Activity> activities)
    {
        var lines = activities
            .SelectMany(x => x.Records)
            .GroupBy(x => x.StartTime.Date)
            .Select(x => PrintDay(x.Key, x))
            .Append(" ".FormatLine())
            .Append(TotalTime(activities))
            .Prepend("Daily Worktime Report".FormatLine());

        if (!perDay)
        {
            lines = activities
                .SelectMany(x => x.Records)
                .GroupBy(x => Utilities.GetWeekNumber(x.StartTime))
                .Select(x => PrintWeek(x.Key, x.ToList()))
                .Append(" ".FormatLine())
                .Append(TotalTime(activities))
                .Prepend("Weekly Report".FormatLine());
        }

        display.Print(lines);
    }

    private FormattedLine TotalTime(IEnumerable<Activity> activities) =>
        $"{activities.Duration()}".FormatLine(ConsoleColor.DarkBlue).Append(" Total");

    private FormattedLine PrintWeek(int weekNumber, IEnumerable<Record> records) =>
        $"{records.Duration()}".FormatLine(ConsoleColor.DarkGreen).Append($" Week {weekNumber}");

    private FormattedLine PrintDay(DateTime day, IEnumerable<Record> records) =>
        $"{records.Duration()}".FormatLine(ConsoleColor.DarkGreen).Append($" {day:yyyy-MM-dd}");
}

