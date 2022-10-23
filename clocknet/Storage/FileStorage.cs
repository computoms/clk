using System.Globalization;
using clocknet.Storage;
using clocknet.Utils;

namespace clocknet;

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

    public void AddEntryRaw(string rawEntry)
    {
        var line = ParseLine(rawEntry, timeProvider.Now, false);
        var record = new Record(timeProvider.Now, null);
        var task = FindPartiallyMatchingTask(line);
        AddEntry(task, record);
    }

    public void AddEntry(Task task, Record record)
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
        stream.AddLine($"{startTime} {task.Title} {tags} {id}".Trim());
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

        var activity = activities.FirstOrDefault(x => x.Task.IsSameAs(parsedLine.Title, parsedLine.Tags, parsedLine.Id));
        if (activity == null)
        {
            activity = new Activity(new Task(parsedLine.Title, parsedLine.Tags, parsedLine.Id));
            activities.Add(activity);
        }
        return activity;
    }

    private Task FindPartiallyMatchingTask(ParsedLine line)
    {
        var defaultTask = new Task(line.Title, line.Tags, line.Id);
        if (string.IsNullOrWhiteSpace(line.Id))
            return defaultTask;

        var correspondingActivity = ReadAll().FirstOrDefault(x => x.Task.Id == line.Id);
        if (correspondingActivity == null)
            return defaultTask;

        if (!string.IsNullOrWhiteSpace(line.Title) && line.Title != correspondingActivity.Task.Title)
            throw new InvalidDataException($"Id {line.Id} already exists");

        return correspondingActivity.Task;
    }

    private ParsedLine ParseLine(string line, DateTime currentDate, bool parseHour = true)
    {
        line = Sanitize(line);
        var words = line.Split(' ');
        var time = currentDate;
        if (parseHour)
            time = GetStartTime(words.FirstOrDefault() ?? string.Empty);

        var tags = words.Where(x => x.StartsWith('+')).Select(x => x[1..]).ToArray();
        var number = words.FirstOrDefault(x => x.StartsWith('.') && x.Skip(1).All(char.IsDigit))?[1..];
        var title = string.Join(' ', (parseHour ? words.Skip(1) : words).Where(x => !x.StartsWith('+') && x != $".{number}")).Trim();
        return new ParsedLine(
	        ConcatDateAndTime(currentDate, time),
            title, tags, number ?? string.Empty);
    }

    private void AssertUniqueId(Task task)
    {
        if (GetActivities().Any(x => x.Task.Id == task.Id && x.Task.Title != task.Title))
            throw new InvalidDataException($"Id {task.Id} already exists");
    }

    private DateTime ConcatDateAndTime(DateTime date, DateTime time) => new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0);

    private record ParsedLine(DateTime StartTime, string Title, string[] Tags, string Id);

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

