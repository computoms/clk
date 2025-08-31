using clk.Utils;

namespace clk.Domain.Reports;

public class WorktimeReport : IReport
{
    private readonly IDisplay display;
    private readonly bool perDay;

    public WorktimeReport(IDisplay display, bool perDay = true)
    {
        this.display = display;
        this.perDay = perDay;
    }

    public string Name { get; } = Args.Timesheet;

    public void Print(IEnumerable<Activity> activities)
    {
        var lines = activities
            .SelectMany(x => x.Records)
            .GroupBy(x => x.StartTime.Date)
            .Select(x => PrintDay(x.Key, x))
            .Append(" ".AsLine())
            .Append(TotalTime(activities))
            .Prepend("Daily Worktime Report".AsLine());

        if (!perDay)
        {
            lines = activities
                .SelectMany(x => x.Records)
                .GroupBy(x => Utilities.GetWeekNumber(x.StartTime))
                .Select(x => PrintWeek(x.Key, x.ToList()))
                .Append(" ".AsLine())
                .Append(TotalTime(activities))
                .Prepend("Weekly Report".AsLine());
        }

        display.Print(lines);
    }

    private FormattedLine TotalTime(IEnumerable<Activity> activities) =>
        $"{activities.Duration()}".AsLine(ConsoleColor.DarkBlue).Append(new FormattedLine(" Total"));

    private FormattedLine PrintWeek(int weekNumber, IEnumerable<Record> records) =>
        $"{records.Duration()}".AsLine(ConsoleColor.DarkGreen).Append(new FormattedLine($" Week {weekNumber}"));

    private FormattedLine PrintDay(DateTime day, IEnumerable<Record> records) =>
        $"{records.Duration()}".AsLine(ConsoleColor.DarkGreen).Append(new FormattedLine($" {day:yyyy-MM-dd}"));
}

