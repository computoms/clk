using clk.Utils;
using clk.Domain;

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

    public void AddTask(TaskLine line)
    {
        try
        {
            storage.AddLine(line);
        }
        catch (Exception e)
        {
            display.Error(e.Message);
        }
    }

    public IEnumerable<TaskLine> GetAll() => storage.GetTasks().Where(t => !t.IsStopTask());

    public TaskLine? GetLast() => GetAll().OrderBy(t => t.StartTime).LastOrDefault();

    public IEnumerable<TaskLine> FilterByQuery(RepositoryQuery query)
    {
        var tasks = GetAll();
        if (query.From != null && query.To != null)
        {
            tasks = FilterByDate(tasks, (DateTime)query.From, (DateTime)query.To);
        }
        if (query.Path != null)
        {
            tasks = tasks
                .Where(x => query.Path.Count <= x.Path.Count)
                .Where(x => Enumerable.Range(0, query.Path.Count).All(i => query.Path[i] == x.Path[i]));
        }
        if (query.Tags != null)
        {
            tasks = tasks.Where(x => query.Tags.All(y => x.Tags.Contains(y)));
        }
        if (query.Id != null)
        {
            tasks = tasks.Where(x => x.Id == query.Id);
        }
        if (query.Last != null)
        {
            tasks = tasks.OrderBy(x => x.StartTime).TakeLast((int)query.Last);
        }
        return tasks;
    }

    public static IEnumerable<TaskLine> FilterByDate(IEnumerable<TaskLine> tasks, DateTime startDate, DateTime endDate)
        => tasks.Where(t => IsMatchingDate(t, startDate, endDate));

    private static bool IsMatchingDate(TaskLine task, DateTime startDate, DateTime endDate)
        => task.StartTime.Date.CompareTo(startDate.Date) >= 0 && task.StartTime.Date.CompareTo(endDate.Date) <= 0;

}

