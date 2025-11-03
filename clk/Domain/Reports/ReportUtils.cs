using clk.Domain;
using clk.Utils;

public static class ReportUtils
{
    public static IEnumerable<IGrouping<string?, Activity>> GroupByPath(IEnumerable<Activity> activities, string pathGroup)
    {
        var path = pathGroup.Split("/", StringSplitOptions.RemoveEmptyEntries).ToList();
        if (path.Count == 1 && path[0] == "*")
            return activities.GroupBy(a => a.Task.Path.FirstOrDefault())
                .OrderBy(g => g.Aggregate(TimeSpan.Zero, (total, a) => total + a.Duration));

        return activities
                // Remove all group-by path and take first child
                .GroupBy(a => a.Task.Path.Where(p => !path.Contains(p)).FirstOrDefault())
                .OrderBy(g => g.Aggregate(TimeSpan.Zero, (total, a) => total + a.Duration));
    }

    public static FormattedLine Current(Activity? current, ITimeProvider timeProvider)
    {
        var currentRecord = current?.Records.MaxBy(r => r.StartTime);
        if (currentRecord == null)
            return new FormattedLine(string.Empty);

        if (current!.IsStopped(timeProvider))
        {
            var stopDuration = (TimeSpan)(DateTime.Now - currentRecord.EndTime!);
            return new FormattedLine(new List<FormattedText> {
                    " --> ".FormatChunk(),
                    $"{Utilities.PrintDuration(stopDuration)} ".FormatChunk(ConsoleColor.DarkGreen),
                    "Stopped".FormatChunk(ConsoleColor.DarkYellow)
                });
        }

        return new FormattedLine(new List<FormattedText> {
                " --> ".FormatChunk(),
                $"{Utilities.PrintDuration(current!.Duration)} ".FormatChunk(ConsoleColor.DarkGreen),
                current.Task.Title.FormatChunk(ConsoleColor.DarkYellow)
            });
    }

    public static FormattedLine TotalTime(IEnumerable<Activity> activities)
        => $"{Utilities.PrintDuration(activities.Aggregate(TimeSpan.Zero, (curr, act) => curr + act.Duration))} Total".AsLine();
}