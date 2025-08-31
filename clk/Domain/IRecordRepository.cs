namespace clk.Domain;

public record RepositoryQuery(DateTime? From = null, DateTime? To = null, List<string>? Tree = null, List<string>? Tags = null, string? Id = null);

public interface IRecordRepository
{
    void AddRecord(Task activity, Record record);
    IEnumerable<Activity> GetAll();
    Activity? GetCurrent();

    IEnumerable<Activity> FilterByQuery(RepositoryQuery query);
}

