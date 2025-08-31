using clk.Utils;
using clk.Domain;
using System.Security.Cryptography.X509Certificates;

namespace clk.Infra;

public class RecordRepository : IRecordRepository
{
    private readonly IStorage storage;
    private readonly ITimeProvider timeProvider;
    private readonly IDisplay display;

    public RecordRepository(IStorage storage, ITimeProvider timeProvider)
    {
        this.storage = storage;
        this.timeProvider = timeProvider;
        this.display = new ConsoleDisplay(true);
    }

    public void AddRecord(Domain.Task activity, Record record)
    {
        try
        {
            storage.AddEntry(activity, record);
        }
        catch (Exception e)
        {
            display.Error(e.Message);
        }
    }

    public IEnumerable<Activity> GetAll() => storage.GetActivities();

    public Activity? GetCurrent()
    {
        var currentActivity = FilterByDate(timeProvider.Now.Date)
            .OrderBy(a => a.Records.MaxBy(r => r.StartTime)?.StartTime ?? DateTime.MinValue)
            .LastOrDefault();

        if (currentActivity == null || currentActivity.IsStopped(timeProvider))
            return null;

        return currentActivity;
    }

    public IEnumerable<Activity> FilterByQuery(RepositoryQuery query)
    {
        var activities = GetAll();
        if (query.From != null && query.To != null)
        {
            activities = FilterByDate((DateTime)query.From, (DateTime)query.To);
        }
        if (query.Path != null)
        {
            activities = activities.Where(x => Enumerable.Range(0, query.Path.Count).All(i => query.Path[i] == x.Task.Path[i]));
        }
        if (query.Tags != null)
        {
            activities = activities.Where(x => query.Tags.All(y => x.Task.Tags.Contains(y)));
        }
        if (query.Id != null)
        {
            activities = activities.Where(x => x.Task.Id == query.Id);
        }
        return activities;
    }

    private IEnumerable<Activity> FilterByDate(DateTime date) => FilterByDate(date, date);

    public IEnumerable<Activity> FilterByDate(DateTime startDate, DateTime endDate)
        => storage.GetActivities()
            .Select(x => new Activity(x.Task, x.Records.Where(y => IsMatchingDate(y, startDate, endDate))))
            .Where(x => x.Records.Any());

    private bool IsMatchingDate(Record record, DateTime startDate, DateTime endDate)
        => record.StartTime.Date.CompareTo(startDate.Date) >= 0 && record.StartTime.Date.CompareTo(endDate.Date) <= 0;
}

