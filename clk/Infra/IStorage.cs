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
    void AddEntry(Domain.Task activity, Record record);
    List<Activity> GetActivities();
}

