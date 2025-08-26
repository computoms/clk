
using clocknet.Utils;

namespace clocknet.Domain.Reports;

public class BarGraphReport(IDisplay display, ProgramArguments pArgs) : IReport
{
    private readonly ConsoleColor[] _colors = { ConsoleColor.Blue, ConsoleColor.DarkGreen, ConsoleColor.DarkMagenta, ConsoleColor.Gray, ConsoleColor.DarkGray, ConsoleColor.Green, ConsoleColor.DarkCyan, ConsoleColor.DarkBlue };
    private int _colorIndex = 0;

    public Option Name { get; } = Args.BarGraphs;

    public void Print(IEnumerable<Activity> activities)
    {
        if (pArgs.HasOption(Args.Tags))
        {
            var groupBy = pArgs.GetValue(Args.Tags);
            PrintByTags(activities, groupBy);
            return;
        }

        PrintByTaskTitles(activities);
    }

    private void PrintByTags(IEnumerable<Activity> activities, string tagFilter)
    {
        var groups = FilterByTags(activities, tagFilter);

        var durations = groups.Select(g => g.Aggregate(TimeSpan.Zero, (total, a2) => total + a2.Duration)).ToList();
        if (durations.Count == 0)
        {
            Console.WriteLine("Nothing to show");
            return;
        }

        var maxActivityDuration = durations.Max();
        const string noCat = "No category";
        var maxTitle = groups.Where(g => g.Key != null).Select(g => g.Key).Append(noCat).Max(x => x!.Length);

        var layout = GetLayout(maxTitle);

        display.Print(
            groups
                .OrderBy(GetGroupDuration)
                .Select(g => DisplayBarGraph(
                    g.Key ?? noCat, g.Aggregate(TimeSpan.Zero, (total, a) => total + a.Duration),
                    maxActivityDuration, layout.TextAlignment, layout.MaxBarLength)));
    }

    private IEnumerable<IGrouping<string?, Activity>> FilterByTags(IEnumerable<Activity> activities, string tagFilter)
    {
        var tags = tagFilter.Split(",").Select(t => t[0] == '+' ? t.Substring(1) : t).ToList();
        if (tags.Count == 1 && tags[0] == "tags")
            return activities.GroupBy(a => a.Task.Tags.FirstOrDefault());

        return activities
                // Filter all activities that contain all the tags we want to filter on
                .Where(a => tags.All(t => a.Task.Tags.Contains(t)))
                // Remove filter tags before grouping by remaining tags
                .GroupBy(a => a.Task.Tags.Where(t => !tags.Contains(t)).FirstOrDefault());
    }

    private TimeSpan GetGroupDuration(IGrouping<string?, Activity> group)
        => group.Aggregate(TimeSpan.Zero, (ts, a) => ts + a.Duration);

    private void PrintByTaskTitles(IEnumerable<Activity> activities)
    {
        var maxActivityDuration = activities.Max(a => a.Duration);
        var maxTitle = activities.Max(a => a.Task.Title.Length);

        var layout = GetLayout(maxTitle);
        display.Print(
            activities
                .OrderBy(a => a.Duration)
                .Select(a => DisplayBarGraph(a.Task.Title, a.Duration, maxActivityDuration, layout.TextAlignment, layout.MaxBarLength)));
    }

    private static BarLayout GetLayout(int maxTitleLength)
    {
        var displayFullWidth = Console.WindowWidth > MaxTotalWidthChars ? MaxTotalWidthChars : Console.WindowWidth;
        var textAlignment = maxTitleLength + 5;
        if (textAlignment > MaxTextAlignmentRatio * displayFullWidth)
        {
            textAlignment = (int)(MaxTextAlignmentRatio * displayFullWidth) + 5;
        }

        return new BarLayout(textAlignment, displayFullWidth - textAlignment - 5);
    }

    private const int MaxTotalWidthChars = 150;
    private const double MaxTextAlignmentRatio = 0.7;

    private FormattedLine DisplayBarGraph(string title, TimeSpan duration, TimeSpan maxDuration, int textAlignment, int maxBarLength)
    {
        var length = (int)(duration.Ticks * maxBarLength / maxDuration.Ticks);
        return AlignText(title, textAlignment).FormatLine()
            .Append(BarGraph(length).FormatChunk(GetColor()));
    }

    private string BarGraph(int length) => length == 0 ? "" : Enumerable.Range(0, length).Select(i => "\u2588").Aggregate((a, b) => $"{a}{b}");

    private string AlignText(string text, int textAlignment) => text.Length >= textAlignment
            ? string.Concat(text.AsSpan(0, textAlignment - 8), "...").PadRight(textAlignment)
            : text.PadRight(textAlignment);

    private ConsoleColor GetColor()
    {
        return _colors[_colorIndex++ % _colors.Length];
    }

    private record BarLayout(int TextAlignment, int MaxBarLength);
}
 