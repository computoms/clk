namespace clocknet;

public class RecordRepository
{
    private readonly IStorage storage;

    public RecordRepository(IStorage storage)
    {
        this.storage = storage;
    }

    public void AddRecord(Activity activity, Record record)
    {
        storage.AddEntry(activity, record);
    }
}

