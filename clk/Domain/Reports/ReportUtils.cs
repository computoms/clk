using clk.Domain;

public static class ReportUtils
{
    public static IEnumerable<IGrouping<string?, Activity>> FilterByTags(IEnumerable<Activity> activities, string tagFilter)
    {
        var tags = tagFilter.Split(",").Select(t => t[0] == '+' ? t.Substring(1) : t).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
        if (tags.Count == 1 && tags[0] == "tags")
            return activities.GroupBy(a => a.Task.Tags.FirstOrDefault());

        return activities
                // Filter all activities that contain all the tags we want to filter on
                .Where(a => tags.All(t => a.Task.Tags.Contains(t)))
                // Remove filter tags before grouping by remaining tags
                .GroupBy(a => a.Task.Tags.Where(t => !tags.Contains(t)).FirstOrDefault());
    }
}