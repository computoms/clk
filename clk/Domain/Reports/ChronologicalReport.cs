using clk.Utils;

namespace clk.Domain.Reports;

internal class ChronologicalReport(IDisplay display, IRecordRepository recordRepository, ITimeProvider timeProvider) : IReport
{
    public string Name { get; } = Args.Chrono;

    public void Print(IEnumerable<Activity> activities)
    {
        var current = recordRepository.GetCurrent();

        var records = activities.SelectMany(a => a.Records.Select(r => new SingleRecord(r, a)))
            .OrderBy(r => r.Record.StartTime);
        display.Print(
            records.GroupBy(r => r.Record.StartTime.Date).SelectMany(g => LayoutRecordsOfTheDay(g.Key.Date, g))
                .Append(" ".AsLine())
                .Append(ReportUtils.TotalTime(activities))
                .Append(" ".AsLine())
                .Append(ReportUtils.Current(current, timeProvider))
        );
    }


    private IEnumerable<FormattedLine> LayoutRecordsOfTheDay(DateTime? date, IEnumerable<SingleRecord> records)
    {
        return records.Select(LayoutSingleRecord)
            .Prepend((date?.ToString("yyyy-MM-dd") ?? "").AsLine());
    }

    private FormattedLine LayoutSingleRecord(SingleRecord record)
    {
        var duration = Utilities.PrintDuration(record.Record.Duration);
        var path = (record.Activity.Task.Path.Length > 0 ? "/" : "") + string.Join('/', record.Activity.Task.Path);
        var tags = string.Join(' ', record.Activity.Task.Tags.Select(x => $"+{x}"));
        var id = string.IsNullOrWhiteSpace(record.Activity.Task.Id) ? "" : $".{record.Activity.Task.Id}";

        var line = new List<FormattedText>
        {
            $"{duration}".FormatChunk(ConsoleColor.DarkGreen),
            $" ({record.Record.StartTime:HH:mm} -> {record.Record.EndTime:HH:mm})".FormatChunk(ConsoleColor.DarkGray),
            record.Activity.Task.Title.PrependSpaceIfNotNull().FormatChunk(),
            path.PrependSpaceIfNotNull().FormatChunk(ConsoleColor.DarkBlue),
            tags.PrependSpaceIfNotNull().FormatChunk(ConsoleColor.DarkGreen),
            id.PrependSpaceIfNotNull().FormatChunk(ConsoleColor.DarkYellow)
        };

        return display.Layout(line);
    }

    private record SingleRecord(Record Record, Activity Activity);
}