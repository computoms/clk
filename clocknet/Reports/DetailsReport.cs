using clocknet.Display;
using clocknet.Utils;

namespace clocknet.Reports;

public class DetailsReport : IReport
{
    private readonly IDisplay _display;

    public DetailsReport(IDisplay display)
    {
        _display = display;
    }

    public void Print(IEnumerable<Activity> activities)
    {
        _display.Print(
            activities
                .Select(a => new {Date = a.Records.OrderBy(r => r.StartTime.Date).FirstOrDefault()?.StartTime.Date, Activity = a})
                .GroupBy(x => x.Date)
                .SelectMany(g => LayoutActivitiesOfTheDay(g.Key, g.Select(x => x.Activity)))
                .Append(" ".FormatLine())
                .Append(TotalTime(activities)));
    }

    private FormattedLine TotalTime(IEnumerable<Activity> activities)
        => _display.Layout($"{Utilities.PrintDuration(activities.Aggregate(TimeSpan.Zero, (curr, act) => curr + act.Duration))} Total");

    private IEnumerable<FormattedLine> LayoutActivitiesOfTheDay(DateTime? date, IEnumerable<Activity> activities)
    {
        return activities.SelectMany(LayoutActivity).Prepend((date?.ToString("yyyy-MM-dd") ?? "").FormatLine());
    }

    private IEnumerable<FormattedLine> LayoutActivity(Activity activity)
    {
        var duration = Utilities.PrintDuration(activity.Duration);
        var tags = string.Join(' ', activity.Task.Tags.Select(x => $"+{x}"));
        var id = string.IsNullOrWhiteSpace(activity.Task.Id) ? "" : $".{activity.Task.Id}";

        var line = new List<FormattedText>
        {
            $"{duration}".FormatChunk(ConsoleColor.DarkGreen),
            activity.Task.Title.PrependSpaceIfNotNull().FormatChunk(),
            tags.PrependSpaceIfNotNull().FormatChunk(ConsoleColor.DarkBlue),
            id.PrependSpaceIfNotNull().FormatChunk(ConsoleColor.DarkYellow)
        };

        return activity.Records.Select(LayoutRecords).Prepend(_display.Layout(line, 1));
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
        return _display.Layout(formattedRecords, 2);
    }
}

