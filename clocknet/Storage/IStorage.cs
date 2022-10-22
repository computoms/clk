namespace clocknet;

public interface IStream
{
    void AddLine(string line);
    void Write(string text);
    List<string> ReadAllLines();
}

public interface IStorage
{
    void AddEntry(Activity activity, Record record);
    List<Activity> GetActivities();
    List<Record> GetRecords();
}

