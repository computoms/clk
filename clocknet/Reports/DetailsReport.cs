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
                .Append(" ")
                .Append(TotalTime(activities)));
    }

    private string TotalTime(IEnumerable<Activity> activities)
        => _display.Layout($"{Utilities.PrintDuration(activities.Aggregate<Activity, TimeSpan>(TimeSpan.Zero, (curr, act) => curr + act.Duration))} Total");

    private IEnumerable<string> LayoutActivitiesOfTheDay(DateTime? date, IEnumerable<Activity> activities)
    {
        return activities.SelectMany(LayoutActivity).Prepend(date?.ToString("yyyy-MM-dd") ?? "");
    }

    private IEnumerable<string> LayoutActivity(Activity activity)
    {
        var duration = Utilities.PrintDuration(activity.Duration);
        var tags = string.Join(' ', activity.Task.Tags.Select(x => $"+{x}"));
        var id = string.IsNullOrWhiteSpace(activity.Task.Id) ? "" : $".{activity.Task.Id}";
        var line = $"{duration}"
            + activity.Task.Title.PrependSpaceIfNotNull()
            + tags.PrependSpaceIfNotNull()
            + id.PrependSpaceIfNotNull();

        return activity.Records.Select(LayoutRecords).Prepend(_display.Layout(line.Trim(), 1));
    }

    private string LayoutRecords(Record record)
    {
        var recordDuration = Utilities.PrintDuration(record.Duration);
        var recordStart = record.StartTime.ToString("HH:mm");
        var recordEnd = record.EndTime?.ToString("HH:mm");
        return _display.Layout($"{recordDuration} ({recordStart} -> {recordEnd})", 2);
    }
}

