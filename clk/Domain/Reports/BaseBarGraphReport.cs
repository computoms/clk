
using clk.Utils;

namespace clk.Domain.Reports;

public class BaseBarGraphReport(IDisplay display)
{
    private readonly ConsoleColor[] _colors = { ConsoleColor.Blue, ConsoleColor.DarkGreen, ConsoleColor.DarkMagenta, ConsoleColor.Gray, ConsoleColor.DarkGray, ConsoleColor.Green, ConsoleColor.DarkCyan, ConsoleColor.DarkBlue };
    private int _colorIndex = 0;


    protected void PrintBarGraph(IEnumerable<BarInfo> infos, int minTitle = 10)
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

    protected static BarLayout GetLayout(int maxTitleLength)
    {
        var displayFullWidth = Console.WindowWidth > MaxTotalWidthChars ? MaxTotalWidthChars : Console.WindowWidth;
        const int durationLength = 7;
        const int extraLength = 5 + durationLength;
        var textAlignment = maxTitleLength + extraLength;
        if (textAlignment > MaxTextAlignmentRatio * displayFullWidth)
        {
            textAlignment = (int)(MaxTextAlignmentRatio * displayFullWidth) + extraLength;
        }

        return new BarLayout(textAlignment, displayFullWidth - textAlignment - extraLength);
    }

    private const int MaxTotalWidthChars = 150;
    private const double MaxTextAlignmentRatio = 0.7;

    protected FormattedLine DisplayBarGraph(string title, TimeSpan duration, TimeSpan maxDuration, int textAlignment, int maxBarLength)
    {
        var length = (int)(duration.Ticks * maxBarLength / maxDuration.Ticks);
        return AlignText(title, textAlignment).AsLine()
            .Append(new FormattedLine((Utilities.PrintDuration(duration) + "  ").FormatChunk(ConsoleColor.DarkGreen)))
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

    protected record BarInfo(string Title, TimeSpan Duration);
    protected record BarLayout(int TextAlignment, int MaxBarLength);
}
 