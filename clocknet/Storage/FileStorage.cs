using System.Globalization;
using clocknet.Storage;

namespace clocknet;

public class FileStorage : IStorage
{
    private readonly IStream stream;
    private readonly ITimeProvider timeProvider;
    private const int None = -1;
    private const int StopId = -2;
    private bool isInitialized = false;
    private DateTime _lastDate = DateTime.MinValue;
    private long _currentId = 0;

    public FileStorage(IStream stream, ITimeProvider timeProvider)
    {
        this.stream = stream;
        this.timeProvider = timeProvider;
    }

    public void AddEntry(Activity activity, Record record)
    {
        if (!isInitialized)
            GetLastDate();

        if (_lastDate != timeProvider.Now.Date)
        {
            stream.AddLine(timeProvider.Now.ToString("[yyyy-MM-dd]"));
            _lastDate = timeProvider.Now.Date;
	    }

        var startTime = record.StartTime.ToString("HH:mm");
        var tags = string.Join(" ", activity.Tags.Select(x => $"+{x}"));
        stream.AddLine($"{startTime} {activity.Title} {tags} .{activity.Number}");
    }

    public List<Activity> GetActivities()
    {
        var result = ReadAll();
        return result.Activities;
    }

    public List<Record> GetRecords()
    {
        var result = ReadAll();
        return result.Records;
    }

    private (List<Activity> Activities, List<Record> Records) ReadAll()
    {
        var activities = new List<Activity>();
        var records = new List<Record>();
        var currentDate = DateTime.MinValue;
        long previousRecordActivityId = Ids.None;
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
            var activityId = GetActivityId(entry, activities);

            if (previousRecordActivityId != Ids.None && previousRecordActivityId != Ids.Stop)
                records.Add(new Record(previousRecordActivityId, previousRecordStartTime, entry.StartTime));
            previousRecordActivityId = activityId;
            previousRecordStartTime = entry.StartTime;
        }
        if (previousRecordActivityId != Ids.None && previousRecordActivityId != Ids.Stop)
            records.Add(new Record(previousRecordActivityId, previousRecordStartTime, timeProvider.Now));
        return (activities, records);
    }

    private long GetActivityId(ParsedLine parsedLine, List<Activity> activities)
    {
        if (parsedLine.Title == SpecialActivities.Stop)
            return Ids.Stop;

        var activity = activities.FirstOrDefault(x => x.IsSameAs(parsedLine.Title, parsedLine.Tags, parsedLine.Number));
        if (activity == null)
        {
            activity = new Activity(++_currentId, parsedLine.Title, parsedLine.Tags, parsedLine.Number);
            activities.Add(activity);
        }
        return activity.Id;
    }

    private ParsedLine ParseLine(string line, DateTime currentDate)
    {
        line = Sanitize(line);
        var words = line.Split(' ');
        var time = GetStartTime(words.FirstOrDefault() ?? string.Empty);
        var tags = words.Where(x => x.StartsWith('+')).Select(x => x[1..]).ToArray();
        var number = words.FirstOrDefault(x => x.StartsWith('.') && x.Skip(1).All(char.IsDigit))?[1..];
        var title = string.Join(' ', words.Skip(1).Where(x => !x.StartsWith('+') && x != $".{number}"));
        return new ParsedLine(new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, time.Hour, time.Minute, 0), 
	        title, tags, number ?? string.Empty);
    }

    private record ParsedLine(DateTime StartTime, string Title, string[] Tags, string Number);

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

    private string Sanitize(string line) => line.Replace("\r", "").Replace("\n", "");

    private static class Ids
    {
        public static long None => -1;
        public static long Stop => -2;
    }

    private static class SpecialActivities
    {
        public static string Stop => "[Stop]";
    }
}

