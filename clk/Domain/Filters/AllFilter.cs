namespace clk.Domain.Filters;

public class AllFilter(IRecordRepository recordRepository) : IFilter
{
    public string Name { get; } = nameof(AllFilter);
    public IEnumerable<Activity> GetActivities()
    {
        return recordRepository.GetAll();
    }
}