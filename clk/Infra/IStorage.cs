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
    IEnumerable<TaskLine> GetTasks();
}

