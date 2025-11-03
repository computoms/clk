using clk.Utils;

namespace clk.Domain.Reports;

internal class ChronologicalReport(IDisplay display, ITimeProvider timeProvider) : IReport
{
    public string Name { get; } = Args.Chrono;

    public void Print(IEnumerable<TaskLine> tasks)
    {
        var records = tasks
            .OrderBy(t => t.StartTime);
        var current = records.LastOrDefault();
        display.Print(
            records.GroupBy(r => r.StartTime.Date).SelectMany(g => LayoutRecordsOfTheDay(g.Key.Date, g))
                .Append(" ".AsLine())
                .Append(ReportUtils.TotalTime(records))
                .Append(" ".AsLine())
                .Append(ReportUtils.Current(current, timeProvider))
        );
    }

    private IEnumerable<FormattedLine> LayoutRecordsOfTheDay(DateTime? date, IEnumerable<TaskLine> tasks)
    {
        return tasks.Select(LayoutSingleRecord)
            .Prepend((date?.ToString("yyyy-MM-dd") ?? "").AsLine());
    }

    private FormattedLine LayoutSingleRecord(TaskLine task)
    {
        var duration = Utilities.PrintDuration(task.Duration);
        var path = (task.Path.Count > 0 ? "/" : "") + string.Join('/', task.Path);
        var tags = string.Join(' ', task.Tags.Select(x => $"+{x}"));
        var id = string.IsNullOrWhiteSpace(task.Id) ? "" : $".{task.Id}";

        var line = new List<FormattedText>
        {
            $"{duration}".FormatChunk(ConsoleColor.DarkGreen),
            $" ({task.StartTime:HH:mm} -> {task.EndTime:HH:mm})".FormatChunk(ConsoleColor.DarkGray),
            task.Title.PrependSpaceIfNotNull().FormatChunk(),
            path.PrependSpaceIfNotNull().FormatChunk(ConsoleColor.DarkBlue),
            tags.PrependSpaceIfNotNull().FormatChunk(ConsoleColor.DarkGreen),
            id.PrependSpaceIfNotNull().FormatChunk(ConsoleColor.DarkYellow)
        };

        return display.Layout(line);
    }
}