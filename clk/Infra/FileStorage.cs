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

    public void AddLine(TaskLine line)
    {
        if (!isInitialized)
            GetLastDate();

        if (_lastDate != timeProvider.Now.Date)
        {
            stream.AddLine(timeProvider.Now.ToString("[yyyy-MM-dd]"));
            _lastDate = timeProvider.Now.Date;
        }

        AssertUniqueId(line);
        stream.AddLine(line.Raw);
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
        var path = string.Join("", task.Path.Select(x => $"/{x}"));
        var id = string.IsNullOrWhiteSpace(task.Id) ? "" : $".{task.Id}";
        var line = $"{startTime}"
            + task.Title.PrependSpaceIfNotNull()
            + path.PrependSpaceIfNotNull()
            + tags.PrependSpaceIfNotNull()
            + id.PrependSpaceIfNotNull();
        stream.AddLine(line);
    }

    public IEnumerable<TaskLine> GetTasks()
    {
        return ReadAll();
    }

    private IEnumerable<TaskLine> ReadAll()
    {
        var currentDate = DateTime.MinValue;
        TaskLine? previousTask = null;
        foreach (var line in stream.ReadAllLines())
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            var date = GetDate(Sanitize(line));
            if (date != DateTime.MinValue)
            {
                currentDate = date;
                continue;
            }
            var entry = new TaskLine(line, currentDate);

            if (previousTask != null)
            {
                previousTask.EndTime = entry.StartTime;
                yield return previousTask;
            }

            previousTask = entry;
        }
        if (previousTask != null)
        {
            yield return previousTask;
        }
    }

    private void AssertUniqueId(Domain.Task task)
    {
        if (GetTasks().Any(x => !string.IsNullOrWhiteSpace(x.Id) && x.Id == task.Id && x.Title != task.Title))
            throw new InvalidDataException($"Id {task.Id} already exists");
    }

    private void AssertUniqueId(TaskLine line)
    {
        if (GetTasks().Any(x => !string.IsNullOrWhiteSpace(x.Id) && x.Id == line.Id && x.Title != line.Title))
            throw new InvalidDataException($"Id {line.Id} already exists");
    }


    private DateTime ConcatDateAndTime(DateTime date, DateTime time) => new(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0);

    private record ParsedLine(DateTime StartTime, string Title, string[] Path, string[] Tags, string Id);

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
}

