namespace clocknet.Domain;

public interface IRecordRepository
{
    void AddRecord(Task activity, Record record);
    IEnumerable<Activity> FilterByTag(IList<string> tags);
    IEnumerable<Activity> GetAll();
    IEnumerable<Activity> FilterByDate(DateTime date);
    IEnumerable<Activity> FilterByDate(DateTime startDate, DateTime endDate);
}

