using clk.Utils;

namespace clk.Domain.Reports;

public class DetailsReport(IDisplay display, IRecordRepository recordRepository, ProgramArguments pArgs, ITimeProvider timeProvider) : IReport
{
    public string Name { get; } = Args.Details;

    public void Print(IEnumerable<TaskLine> activities)
    {
        var current = recordRepository.GetLast();

        if (pArgs.HasOption(Args.GroupBy))
        {
            PrintByPath(activities, current);
            return;
        }

        PrintDetails(activities, current);
    }

    private void PrintByPath(IEnumerable<TaskLine> tasks, TaskLine? current)
    {
        var groups = ReportUtils.GroupByPath(tasks, pArgs.GetValue(Args.GroupBy));
        display.Print(
            groups
            .Select(g => new PathInfo(
                g.Key,
                g.Min(t => t.StartTime).Date,
                g.Aggregate(TimeSpan.Zero, (t, a) => t + a.Duration))
            )
            .GroupBy(e => e.Date)
            .SelectMany(g => g.SelectMany(a => LayoutTagInfo(a)))
            .Append(" ".AsLine())
            .Append(ReportUtils.TotalTime(groups.SelectMany(g => g)))
            .Append(" ".AsLine())
            .Append(ReportUtils.Current(current, timeProvider)));
    }

    private void PrintDetails(IEnumerable<TaskLine> tasks, TaskLine? current)
    {
        throw new NotImplementedException();
        // TODO group by similar tasks using a dictionary and IsSameAs method?
    }

    private IEnumerable<FormattedLine> LayoutTagInfo(PathInfo info)
    {
        var duration = Utilities.PrintDuration(info.Duration);
        var line = new List<FormattedText>
        {
            $"{duration}".FormatChunk(ConsoleColor.DarkGreen),
            info.Name?.PrependSpaceIfNotNull()?.FormatChunk() ?? "".FormatChunk(),
        };

        return new List<FormattedLine> { display.Layout(line, 1) };
    }

    private record PathInfo(string? Name, DateTime Date, TimeSpan Duration);
}

