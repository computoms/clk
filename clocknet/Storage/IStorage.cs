namespace clocknet.Storage;

public interface IStream
{
    void AddLine(string line);
    void WriteAllLines(string[] lines);
    List<string> ReadAllLines();
}

public interface IStorage
{
    void AddEntry(Task activity, Record record);
    List<Activity> GetActivities();
}

