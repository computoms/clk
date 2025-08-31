using clk.Utils;

namespace clk.Domain.Filters;

public class TodayFilter(IRecordRepository recordRepository, ITimeProvider timeProvider) : IFilter
{
    public string Name { get; } = nameof(TodayFilter);

    public IEnumerable<Activity> GetActivities()
    {
        return recordRepository.FilterByDate(timeProvider.Now.Date);
    }
}