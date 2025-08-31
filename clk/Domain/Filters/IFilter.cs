namespace clk.Domain.Filters;

public interface IFilter
{
    string Name { get; }
    IEnumerable<Activity> GetActivities();
}