using clocknet.Display;

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
        _display.Print(activities.SelectMany(LayoutActivity));
    }

    private IEnumerable<string> LayoutActivity(Activity activity)
    {
        var duration = PrintDuration(activity.Duration);
	    var tags = string.Join('+', activity.Task.Tags.Select(x => $"+{x}"));

        return activity.Records.Select(LayoutRecords)
	        .Prepend(_display.Layout($"{duration} {activity.Task.Title} {tags} .{activity.Task.Number}"));
    }

    private string LayoutRecords(Record record)
    {
        var recordDuration = PrintDuration(record.Duration);
        var recordStart = record.StartTime.ToString("HH:mm");
        var recordEnd = record.EndTime?.ToString("HH:mm");
        return _display.Layout($"{recordDuration} ({recordStart} -> {recordEnd})", 2);
    }

    private static string PrintDuration(TimeSpan duration) => duration.ToString(@"hh\:mm");
}

