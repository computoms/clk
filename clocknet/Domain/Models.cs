namespace clocknet.Domain;

public class Activity
{
    private readonly List<Record> _records = new();

    public Activity(Task task)
    {
        Task = task;
    }

    public Activity(Task task, IEnumerable<Record> records)
    {
        Task = task;
        records.ToList().ForEach(x => _records.Add(x));
    }

    public IEnumerable<Record> Records => _records;

    public Task Task { get; }

    public TimeSpan Duration => Records.Aggregate<Record, TimeSpan>(TimeSpan.Zero, (currentDuration, record) => record.Duration + currentDuration);

    public void AddRecord(Record record)
    {
        _records.Add(record);
    }
}

public record Task(string Title, string[] Tags, string Id)
{
    public string Raw => (Title + Tags.Aggregate("", (r, t) => $"{r} +{t}") + (Id != string.Empty ? $" .{Id}" : "")).Trim();
    public bool IsSameAs(string title, string[] tags, string number)
    { 
        return title == Title && number == Id
            && Tags.All(x => tags.Contains(x))
            && tags.All(x => Tags.Contains(x));
    }
}

public record Record(DateTime StartTime, DateTime? EndTime = null)
{
    public TimeSpan Duration => EndTime != null ? (TimeSpan)(EndTime - StartTime) : (DateTime.Now - StartTime);
}
