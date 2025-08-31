using clk.Utils;

namespace clk.Domain.Filters;

public class YesterdayFilter(IRecordRepository recordRepository, ITimeProvider timeProvider) : IFilter
{
    public string Name { get; } = nameof(YesterdayFilter);
    public IEnumerable<Activity> GetActivities()
    {
        return recordRepository.FilterByDate(timeProvider.Now.Date.AddDays(-1));
    }
}