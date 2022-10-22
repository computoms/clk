using clocknet.Storage;

namespace clocknet;

public class RecordRepository
{
    private readonly IStorage storage;
    private readonly ITimeProvider timeProvider;

    public RecordRepository(IStorage storage, ITimeProvider timeProvider)
    {
        this.storage = storage;
        this.timeProvider = timeProvider;
    }

    public void AddRecord(Task activity, Record record)
    {
        storage.AddEntry(activity, record);
    }

    public IEnumerable<Activity> FilterByTag(IList<string> tags)
    {
        return storage.GetActivities()
            .Where(x => tags.All(y => x.Task.Tags.Contains(y)));
    }

    public IEnumerable<Activity> GetAll() => storage.GetActivities();

    public IEnumerable<Activity> FilterByDate(DateTime date) => FilterByDate(date, date);

    public IEnumerable<Activity> FilterByDate(DateTime startDate, DateTime endDate) 
	    => storage.GetActivities()
            .Select(x => new Activity(x.Task, x.Records.Where(y => IsMatchingDate(y, startDate, endDate))))
            .Where(x => x.Records.Any());

    private bool IsMatchingDate(Record record, DateTime startDate, DateTime endDate) 
	    => record.StartTime.Date.CompareTo(startDate.Date) >= 0 && record.StartTime.Date.CompareTo(endDate.Date) <= 0;
}

