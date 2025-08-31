namespace clk.Domain.Filters;

public class FilterFactory(ProgramArguments pArgs, IFilter[] filters)
{
    public IFilter GetFilter()
    {
        if (pArgs.HasOption(Args.All))
            return GetFilter(nameof(AllFilter));
        if (pArgs.HasOption(Args.Week))
            return GetFilter(nameof(WeekFilter));
        if (pArgs.HasOption(Args.Yesterday))
            return GetFilter(nameof(YesterdayFilter));

        // Default filter: today
        return GetFilter(nameof(TodayFilter));
    }

    private IFilter GetFilter(string name)
    {
        return filters.FirstOrDefault(f => f.Name == name) ?? throw new KeyNotFoundException("Specified filter was not found");
    }
}