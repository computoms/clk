using clk.Domain;

public static class ReportUtils
{
    public static IEnumerable<IGrouping<string?, Activity>> GroupByPath(IEnumerable<Activity> activities, string pathGroup)
    {
        var path = pathGroup.Split("/", StringSplitOptions.RemoveEmptyEntries).ToList();
        if (path.Count == 1 && path[0] == "*")
            return activities.GroupBy(a => a.Task.Path.FirstOrDefault())
                .OrderBy(g => g.Aggregate(TimeSpan.Zero, (total, a) => total + a.Duration));

        return activities
                // Remove all group-by path and take first child
                .GroupBy(a => a.Task.Path.Where(p => !path.Contains(p)).FirstOrDefault())
                .OrderBy(g => g.Aggregate(TimeSpan.Zero, (total, a) => total + a.Duration));
    }

}