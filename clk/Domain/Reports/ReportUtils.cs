using clk.Domain;
using clk.Utils;

public static class ReportUtils
{
    public static IEnumerable<IGrouping<string?, TaskLine>> GroupByPath(IEnumerable<TaskLine> activities, string pathGroup)
    {
        var path = pathGroup.Split("/", StringSplitOptions.RemoveEmptyEntries).ToList();
        if (path.Count == 1 && path[0] == "*")
            return activities.GroupBy(a => a.Path.FirstOrDefault());

        return activities
                // Remove all group-by path and take first child
                .GroupBy(a => a.Path.Where(p => !path.Contains(p)).FirstOrDefault());
    }

    public static FormattedLine Current(TaskLine? current, ITimeProvider timeProvider)
    {
        if (current == null)
            return new FormattedLine(string.Empty);

        if (current!.IsStopped(timeProvider))
        {
            var stopDuration = (TimeSpan)(DateTime.Now - current.EndTime!);
            return new FormattedLine(new List<FormattedText> {
                    " --> ".FormatChunk(),
                    $"{Utilities.PrintDuration(stopDuration)} ".FormatChunk(ConsoleColor.DarkGreen),
                    "Stopped".FormatChunk(ConsoleColor.DarkYellow)
                });
        }

        return new FormattedLine(new List<FormattedText> {
                " --> ".FormatChunk(),
                $"{Utilities.PrintDuration(current!.Duration)} ".FormatChunk(ConsoleColor.DarkGreen),
                current.Title.FormatChunk(ConsoleColor.DarkYellow)
            });
    }

    public static FormattedLine TotalTime(IEnumerable<TaskLine> tasks)
        => $"{Utilities.PrintDuration(tasks.Aggregate(TimeSpan.Zero, (curr, act) => curr + act.Duration))} Total".AsLine();
}