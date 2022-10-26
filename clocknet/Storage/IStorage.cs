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
    void AddEntryRaw(string rawEntry, bool parseTime = false);
    List<Activity> GetActivities();
}

