using System.Runtime.InteropServices;
using clk.Utils;

namespace clk.Domain.Reports;

public class GroupedBarGraphReport(ProgramArguments pArgs, IDisplay display) : BaseBarGraphReport(display), IReport
{
    public string Name { get; } = nameof(GroupedBarGraphReport);

    public void Print(IEnumerable<Activity> activities)
    {
        var groupBy = pArgs.HasOption(Args.GroupByPath) ? "/*" : pArgs.GetValue(Args.GroupBy);
        var groups = ReportUtils.GroupByPath(activities, groupBy);
        const string noCat = "Others";
        var infos = groups.Select(g => new BarInfo(
            g.Key ?? noCat,
            g.Aggregate(TimeSpan.Zero, (total, a) => total + a.Duration)));

        if (!infos.Any())
        {
            display.Print("Nothing to show".AsLine());
        }

        var maxDuratin = infos.Max(i => i.Duration);
        var maxTitle = infos.Max(i => i.Title.Length);
        if (maxTitle < noCat.Length)
            maxTitle = noCat.Length;

        var layout = GetLayout(maxTitle);


        var currentPath = groupBy.Split("/", StringSplitOptions.RemoveEmptyEntries).ToList();

        display.Print(
            groups
                .SelectMany(g => PrintBarsOfLevel(g, 1, currentPath, noCat, maxDuratin, layout))
                .Prepend(new FormattedLine(groupBy))
            );
    }

    private IEnumerable<FormattedLine> PrintBarsOfLevel(
        IGrouping<string?, Activity> groups,
        int level,
        List<string> currentPath,
        string noCat,
        TimeSpan maxDuration,
        BarLayout layout)
    {
        // Do not go deeper than 5 levels
        if (level > 5)
        {
            return [];
        }

        List<FormattedLine> lines = [];
        lines.Add(DisplayBarGraph(
            "".PadRight(level * 2) + "/" + (groups.Key ?? noCat),
            groups.Aggregate(TimeSpan.Zero, (total, g) => total + g.Duration),
            maxDuration,
            layout.TextAlignment + 2 * level, layout.MaxBarLength));

        // Print deeper levels if any
        currentPath.Add(groups.Key ?? noCat);
        if (groups.Any(a => HasDeeperLevel(currentPath, a)))
        {
            lines.AddRange(
                ReportUtils.GroupByPath(groups, string.Join('/', currentPath))
                    .SelectMany(x => PrintBarsOfLevel(x, level + 1, currentPath, noCat, maxDuration, layout))
            );
        }

        return lines;
    }

    private bool HasDeeperLevel(List<string> currentPath, Activity activity)
        => activity.Task.Path.Except(currentPath).Any();
}