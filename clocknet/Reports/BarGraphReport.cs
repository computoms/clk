using System.ComponentModel;
using clocknet.Display;
using clocknet.Utils;

namespace clocknet.Reports;

public class BarGraphReport : IReport
{
    private readonly IDisplay _display;
    private readonly ConsoleColor[] _colors = { ConsoleColor.Blue, ConsoleColor.DarkGreen, ConsoleColor.DarkMagenta, ConsoleColor.Gray, ConsoleColor.DarkGray, ConsoleColor.Green, ConsoleColor.DarkCyan, ConsoleColor.DarkBlue };
    private int _colorIndex = 0;

    public BarGraphReport(IDisplay display)
    {
        _display = display;
    }

    public void Print(IEnumerable<Activity> activities)
    {
        const int maxTotalWidth = 150;
        const double maxTextAlignmentRatio = 0.7;

        var displayFullWidth = Console.WindowWidth > maxTotalWidth ? maxTotalWidth : Console.WindowWidth;
        var maxActivityDuration = activities.Max(a => a.Duration);
        var maxTitle = activities.Max(a => a.Task.Title.Length);

        // Align text on max title length, but do not overflow 80% of display width
        var textAlignment = maxTitle + 5;
        if (textAlignment > maxTextAlignmentRatio * displayFullWidth)
        {
            textAlignment = (int)(maxTextAlignmentRatio * displayFullWidth) + 5;
        }

        var maxBarLength = displayFullWidth - textAlignment - 5;
        _display.Print(activities.Select(a => DisplayBarGraph(a, maxActivityDuration, textAlignment, maxBarLength)));
    }

    private FormattedLine DisplayBarGraph(Activity activity, TimeSpan maxDuration, int textAlignment, int maxBarLength)
    {
        var length = (int)(activity.Duration.Ticks * maxBarLength / maxDuration.Ticks);
        return new FormattedLine
        {
            Chunks = new List<FormattedText>
            {
                new FormattedText { RawText = AlignText(activity.Task.Title, textAlignment), Color = Console.ForegroundColor },
                new FormattedText { RawText = BarGraph(length), Color = GetColor() }
            }
        };
    }

    private string BarGraph(int length) => length == 0 ? "" : Enumerable.Range(0, length).Select(i => "\u2588").Aggregate((a, b) => $"{a}{b}");

    private string AlignText(string text, int textAlignment) => text.Length >= textAlignment
            ? string.Concat(text.AsSpan(0, textAlignment - 8), "...").PadRight(textAlignment)
            : text.PadRight(textAlignment);

    private ConsoleColor GetColor()
    {
        return _colors[_colorIndex++ % _colors.Length];
    }
}
 