using System.Globalization;
using clk.Domain;
using clk.Utils;

namespace clk.Infra;

public class FileStorage : IStorage
{
    private readonly IStream stream;
    private readonly ITimeProvider timeProvider;
    private const int None = -1;
    private const int StopId = -2;
    private bool isInitialized = false;
    private DateTime _lastDate = DateTime.MinValue;

    public FileStorage(IStream stream, ITimeProvider timeProvider)
    {
        this.stream = stream;
        this.timeProvider = timeProvider;
    }

    public void AddEntry(Domain.Task task, Record record)
    {
        if (!isInitialized)
            GetLastDate();

        if (_lastDate != timeProvider.Now.Date)
        {
            stream.AddLine(timeProvider.Now.ToString("[yyyy-MM-dd]"));
            _lastDate = timeProvider.Now.Date;
        }

        AssertUniqueId(task);

        var startTime = record.StartTime.ToString("HH:mm");
        var tags = string.Join(" ", task.Tags.Select(x => $"+{x}"));
        var id = string.IsNullOrWhiteSpace(task.Id) ? "" : $".{task.Id}";
        var line = $"{startTime}"
            + task.Title.PrependSpaceIfNotNull()
            + tags.PrependSpaceIfNotNull()
            + id.PrependSpaceIfNotNull();
        stream.AddLine(line);
    }

    public List<Activity> GetActivities()
    {
        return ReadAll();
    }

    private List<Activity> ReadAll()
    {
        var activities = new List<Activity>();
        var currentDate = DateTime.MinValue;
        Activity? previousActivity = null;
        DateTime previousRecordStartTime = DateTime.MinValue;
        foreach (var line in stream.ReadAllLines())
        {
            var date = GetDate(Sanitize(line));
            if (date != DateTime.MinValue)
            {
                currentDate = date;
                continue;
            }
            var entry = ParseLine(line, currentDate);
            var currentActivity = GetActivity(entry, activities);

            if (previousActivity != null)
                previousActivity.AddRecord(new Record(previousRecordStartTime, entry.StartTime));

            previousActivity = currentActivity;
            previousRecordStartTime = entry.StartTime;
        }
        if (previousActivity != null)
            previousActivity.AddRecord(new Record(previousRecordStartTime, timeProvider.Now));

        return activities;
    }

    private Activity? GetActivity(ParsedLine parsedLine, List<Activity> activities)
    {
        if (parsedLine.Title == SpecialActivities.Stop)
            return null;

        var activity = activities.FirstOrDefault(x => x.Task.IsSameAs(parsedLine.Title, parsedLine.Tree, parsedLine.Tags, parsedLine.Id));
        if (activity == null)
        {
            activity = new Activity(new Domain.Task(parsedLine.Title, parsedLine.Tree, parsedLine.Tags, parsedLine.Id));
            activities.Add(activity);
        }
        return activity;
    }

    private ParsedLine ParseLine(string line, DateTime currentDate, bool parseHour = true)
    {
        line = Sanitize(line);
        var words = line.Split(' ');
        var time = currentDate;
        if (parseHour)
            time = GetStartTime(words.FirstOrDefault() ?? string.Empty);

        var tree = words.FirstOrDefault(x => x.Length > 1 && x.StartsWith('/'))?.Split('/', StringSplitOptions.RemoveEmptyEntries).ToArray() ?? [];
        var tags = words.Where(x => x.Length > 1 && x.StartsWith('+')).Select(x => x[1..]).ToArray();
        var number = words.FirstOrDefault(x => x.StartsWith('.') && x.Skip(1).All(char.IsDigit))?[1..];
        var title = string.Join(' ', (parseHour ? words.Skip(1) : words).Where(x => !x.StartsWith('+') && x != $".{number}")).Trim();
        return new ParsedLine(
	        ConcatDateAndTime(currentDate, time),
            title, tree, tags, number ?? string.Empty);
    }

    private void AssertUniqueId(Domain.Task task)
    {
        if (GetActivities().Any(x => !string.IsNullOrWhiteSpace(x.Task.Id) && x.Task.Id == task.Id && x.Task.Title != task.Title))
            throw new InvalidDataException($"Id {task.Id} already exists");
    }

    private DateTime ConcatDateAndTime(DateTime date, DateTime time) => new(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0);

    private record ParsedLine(DateTime StartTime, string Title, string[] Tree, string[] Tags, string Id);

    private void GetLastDate()
    {
        var lines = stream.ReadAllLines();
        DateTime currentDate = DateTime.MinValue;
        foreach (var line in lines)
        {
            var date = GetDate(line);
            if (currentDate.CompareTo(date) < 0)
            {
                currentDate = date;
            }
        }
        _lastDate = currentDate;
        isInitialized = true;
    }

    private DateTime GetStartTime(string line)
        => DateTime.TryParseExact(
                Sanitize(line), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
        ? date : DateTime.MinValue;

    private DateTime GetDate(string line)
        => DateTime.TryParseExact(
                   Sanitize(line), "[yyyy-MM-dd]",
                   CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date : DateTime.MinValue;

    private string Sanitize(string line) => line.Replace("\r", "").Replace("\n", "").Trim();

    private static class SpecialActivities
    {
        public static string Stop => "[Stop]";
    }
}

