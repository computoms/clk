using clk.Utils;

namespace clk.Domain.Reports;

public class DetailsReport(IDisplay display, IRecordRepository recordRepository, ProgramArguments pArgs, ITimeProvider timeProvider) : IReport
{
    public string Name { get; } = Args.Details;

    public void Print(IEnumerable<Activity> activities)
    {
        var current = recordRepository.GetCurrent();

        if (pArgs.HasOption(Args.GroupBy))
        {
            PrintByPath(activities, current);
            return;
        }

        PrintDetails(activities, current);
    }

    private void PrintByPath(IEnumerable<Activity> activities, Activity? current)
    {
        var groups = ReportUtils.GroupByPath(activities, pArgs.GetValue(Args.GroupBy));
        display.Print(
            groups
            .Select(g => new PathInfo(
                g.Key,
                g.OrderBy(a => a.Records.Min(r => r.StartTime))
                    .FirstOrDefault()?.Records
                    .FirstOrDefault()?.StartTime.Date ?? DateTime.MinValue,
                g.Aggregate(TimeSpan.Zero, (t, a) => t + a.Duration))
            )
            .GroupBy(e => e.Date)
            .SelectMany(g => g.SelectMany(a => LayoutTagInfo(a)))
            .Append(" ".AsLine())
            .Append(TotalTime(groups.SelectMany(g => g)))
            .Append(" ".AsLine())
            .Append(Current(current)));
    }

    private void PrintDetails(IEnumerable<Activity> activities, Activity? current)
    {
        display.Print(
            activities
                .Select(a => new { Date = a.Records.OrderBy(r => r.StartTime.Date).FirstOrDefault()?.StartTime.Date, Activity = a })
                .GroupBy(x => x.Date)
                .SelectMany(g => LayoutActivitiesOfTheDay(g.Key, g.Select(x => x.Activity)))
                .Append(" ".AsLine())
                .Append(TotalTime(activities))
                .Append(" ".AsLine())
                .Append(Current(current)));
    }

    private FormattedLine Current(Activity? current)
    {
        var currentRecord = current?.Records.MaxBy(r => r.StartTime);
        if (currentRecord == null)
            return new FormattedLine(string.Empty);

        if (current!.IsStopped(timeProvider))
        {
            TimeSpan stopDuration = (TimeSpan)(DateTime.Now - currentRecord.EndTime!);
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

    private FormattedLine TotalTime(IEnumerable<Activity> activities)
        => $"{Utilities.PrintDuration(activities.Aggregate(TimeSpan.Zero, (curr, act) => curr + act.Duration))} Total".AsLine();

    private IEnumerable<FormattedLine> LayoutActivitiesOfTheDay(DateTime? date, IEnumerable<Activity> activities)
    {
        return activities.OrderBy(a => a.Records.FirstOrDefault()?.StartTime ?? DateTime.Now)
            .SelectMany(LayoutActivity).Prepend((date?.ToString("yyyy-MM-dd") ?? "").AsLine());
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

    private IEnumerable<FormattedLine> LayoutActivity(Activity activity)
    {
        var duration = Utilities.PrintDuration(activity.Duration);
        var path = (activity.Task.Path.Length > 0 ? "/" : "") + string.Join('/', activity.Task.Path);
        var tags = string.Join(' ', activity.Task.Tags.Select(x => $"+{x}"));
        var id = string.IsNullOrWhiteSpace(activity.Task.Id) ? "" : $".{activity.Task.Id}";

        var line = new List<FormattedText>
        {
            $"{duration}".FormatChunk(ConsoleColor.DarkGreen),
            activity.Task.Title.PrependSpaceIfNotNull().FormatChunk(),
            path.PrependSpaceIfNotNull().FormatChunk(ConsoleColor.DarkBlue),
            tags.PrependSpaceIfNotNull().FormatChunk(ConsoleColor.DarkGreen),
            id.PrependSpaceIfNotNull().FormatChunk(ConsoleColor.DarkYellow)
        };

        return activity.Records.Select(LayoutRecords).Prepend(display.Layout(line, 1));
    }

    private FormattedLine LayoutRecords(Record record)
    {
        var recordDuration = Utilities.PrintDuration(record.Duration);
        var recordStart = record.StartTime.ToString("HH:mm");
        var recordEnd = record.EndTime?.ToString("HH:mm");
        var formattedRecords = new List<FormattedText> {
            $"{recordDuration}".FormatChunk(ConsoleColor.DarkCyan),
            $" ({recordStart} -> {recordEnd})".FormatChunk(ConsoleColor.DarkGray)
        };
        return display.Layout(formattedRecords, 2);
    }

    private record PathInfo(string? Name, DateTime Date, TimeSpan Duration);
}

