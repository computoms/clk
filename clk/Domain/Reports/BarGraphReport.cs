
using System.ComponentModel;
using clk.Utils;

namespace clk.Domain.Reports;

public class BarGraphReport(IDisplay display, ProgramArguments pArgs) : IReport
{
    private readonly ConsoleColor[] _colors = { ConsoleColor.Blue, ConsoleColor.DarkGreen, ConsoleColor.DarkMagenta, ConsoleColor.Gray, ConsoleColor.DarkGray, ConsoleColor.Green, ConsoleColor.DarkCyan, ConsoleColor.DarkBlue };
    private int _colorIndex = 0;

    public string Name { get; } = Args.BarGraphs;

    public void Print(IEnumerable<Activity> activities)
    {
        if (pArgs.HasOption(Args.GroupBy))
        {
            var groupBy = pArgs.GetValue(Args.GroupBy);
            var groups = ReportUtils.FilterByTags(activities, groupBy);
            const string noCat = "No category";
            PrintBarGraph(
                groups.Select(g => new BarInfo(
                g.Key ?? noCat,
                g.Aggregate(TimeSpan.Zero, (total, a) => total + a.Duration))),
                noCat.Length);
            return;
        }

        PrintBarGraph(
            activities.Select(a => new BarInfo(
                a.Task.Title,
                a.Duration)));
    }

    private void PrintBarGraph(IEnumerable<BarInfo> infos, int minTitle = 10)
    {
        if (!infos.Any())
        {
            display.Print("Nothing to show".AsLine());
        }

        var maxDuratin = infos.Max(i => i.Duration);
        var maxTitle = infos.Max(i => i.Title.Length);
        if (maxTitle < minTitle)
            maxTitle = minTitle;

        var layout = GetLayout(maxTitle);
        display.Print(
            infos
                .OrderBy(i => i.Duration)
                .Select(i => DisplayBarGraph(
                    i.Title, i.Duration, maxDuratin, layout.TextAlignment, layout.MaxBarLength)));
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
        return AlignText(title, textAlignment).AsLine()
            .Append(new FormattedLine(BarGraph(length).FormatChunk(GetColor())));
    }

    private string BarGraph(int length) => length == 0 ? "" : Enumerable.Range(0, length).Select(i => "\u2588").Aggregate((a, b) => $"{a}{b}");

    private string AlignText(string text, int textAlignment) => text.Length >= textAlignment
            ? string.Concat(text.AsSpan(0, textAlignment - 8), "...").PadRight(textAlignment)
            : text.PadRight(textAlignment);

    private ConsoleColor GetColor()
    {
        return _colors[_colorIndex++ % _colors.Length];
    }

    private record BarInfo(string Title, TimeSpan Duration);
    private record BarLayout(int TextAlignment, int MaxBarLength);
}
 