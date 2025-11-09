using System.Globalization;
using clk.Utils;

namespace clk.Domain;

public class TaskLine
{
    private DateTime _currentDate;

    public TaskLine(string raw, DateTime? currentDate = null)
    {
        Raw = raw;
        _currentDate = currentDate ?? DateTime.Today;
        Parse();
    }

    public string Raw { get; init; }
    public string Title { get; private set; } = string.Empty;
    public List<string> Path { get; private set; } = [];
    public List<string> Tags { get; private set; } = [];
    public string Id { get; private set; } = string.Empty;

    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; set; } = null;

    public TaskLine Duplicate(DateTime startTime)
    {
        return new TaskLine($"{startTime:HH:mm} {Line}", startTime.Date);
    }

    public bool IsSameAs(TaskLine line)
    { 
        return line.Title == Title && line.Id == Id
            && Enumerable.Range(0, Path.Count).All(i => Path[i] == line.Path[i])
            && Tags.All(x => line.Tags.Contains(x))
            && line.Tags.All(x => Tags.Contains(x));
    }

    public TimeSpan Duration => EndTime != null ? (TimeSpan)(EndTime - StartTime) : (DateTime.Now - StartTime);

    public string Line =>
        Title 
        + (Path.Count > 0 ? " " : "")
        + Path.Aggregate("", (s, p) => $"{s}/{p}") 
        + (Tags.Count > 0 ? " " : "")
        + string.Join(" ", Tags.Select(t => $"#{t}")) 
        + (string.IsNullOrEmpty(Id) ? "" : $" .{Id}");

    public bool IsStopped(ITimeProvider timeProvider)
    {
        return !IsNow(timeProvider, EndTime ?? DateTime.MinValue);
    }

    public bool IsStopTask()
    {
        return Title == "[Stop]";
    }

    private void Parse()
    {
        var words = Raw.Split(' ');
        if (words.Length > 0)
        {
            StartTime = ConcatDateAndTime(_currentDate, GetStartTime(words.First()));
        }
        EndTime = DateTime.Now;

        Title = string.Empty;
        Path = [];
        Tags = [];
        Id = string.Empty;
        foreach (string v in words.Skip(1))
        {
            if (v.Length > 1 && v.StartsWith("/"))
                Path.AddRange(v.Split('/', StringSplitOptions.RemoveEmptyEntries));
            else if (v.Length > 1 && v.StartsWith('#'))
                Tags.Add(v[1..]);
            else if (v.Length > 1 && v.StartsWith('.'))
                Id = v[1..];
            else
                Title += v + " ";
        }

        Title = Title.Trim();
    }

    private DateTime GetStartTime(string line)
        => DateTime.TryParseExact(
                Sanitize(line), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
        ? date : DateTime.MinValue;

    private DateTime ConcatDateAndTime(DateTime date, DateTime time) => new(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0);

    private string Sanitize(string line) => line.Replace("\r", "").Replace("\n", "").Trim();

   private bool IsNow(ITimeProvider timeProvider, DateTime recordTime) => recordTime.Hour == timeProvider.Now.Hour && recordTime.Minute == timeProvider.Now.Minute;
}
