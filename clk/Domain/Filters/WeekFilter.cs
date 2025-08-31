
using clk.Utils;

namespace clk.Domain.Filters;

public class WeekFilter(IRecordRepository recordRepository, ITimeProvider timeProvider) : IFilter
{
    public string Name { get; } = nameof(WeekFilter);
    public IEnumerable<Activity> GetActivities()
    {
        return recordRepository.FilterByDate(timeProvider.Now.MondayOfTheWeek(), timeProvider.Now.MondayOfTheWeek().AddDays(4));
    }
}