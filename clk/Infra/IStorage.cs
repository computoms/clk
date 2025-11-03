using clk.Domain;

namespace clk.Infra;

public interface IStream
{
    void AddLine(string line);
    void WriteAllLines(string[] lines);
    List<string> ReadAllLines();
}

public interface IStorage
{
    void AddLine(TaskLine line);
    void AddEntry(Domain.Task activity, Record record);
    IEnumerable<TaskLine> GetTasks();
}

