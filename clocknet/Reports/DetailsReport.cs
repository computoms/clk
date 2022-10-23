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
                .SelectMany(LayoutActivity)
                .Append(" ")
                .Append(TotalTime(activities)));
    }

    private string TotalTime(IEnumerable<Activity> activities)
        => _display.Layout($"{Utilities.PrintDuration(activities.Aggregate<Activity, TimeSpan>(TimeSpan.Zero, (curr, act) => curr + act.Duration))} Total");

    private IEnumerable<string> LayoutActivity(Activity activity)
    {
        var duration = Utilities.PrintDuration(activity.Duration);
	    var tags = string.Join(' ', activity.Task.Tags.Select(x => $"+{x}"));

        return activity.Records.Select(LayoutRecords)
            .Prepend(_display.Layout($"{duration} {activity.Task.Title} {tags} .{activity.Task.Id}"));
    }

    private string LayoutRecords(Record record)
    {
        var recordDuration = Utilities.PrintDuration(record.Duration);
        var recordStart = record.StartTime.ToString("HH:mm");
        var recordEnd = record.EndTime?.ToString("HH:mm");
        return _display.Layout($"{recordDuration} ({recordStart} -> {recordEnd})", 2);
    }
}

