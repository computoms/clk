namespace clk.Domain;

public record RepositoryQuery(DateTime? From = null, DateTime? To = null, List<string>? Path = null, List<string>? Tags = null, string? Id = null, int? Last = null);

public interface IRecordRepository
{
    void AddTask(TaskLine line);
    IEnumerable<TaskLine> GetAll();

    TaskLine? GetLast();

    IEnumerable<TaskLine> FilterByQuery(RepositoryQuery query);
}

