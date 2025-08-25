namespace clocknet.Display;

public class FormattedText
{
    public string RawText { get; set; } = string.Empty;
    public ConsoleColor Color { get; set; } = ConsoleColor.White;
}

public class FormattedLine
{
    public IEnumerable<FormattedText> Chunks { get; set; } = Enumerable.Empty<FormattedText>();
}

public static class FormattedTextExtensions
{
    public static FormattedLine FormatLine(this string text) => new()
    {
        Chunks = new List<FormattedText> { new() { RawText = text, Color = Console.ForegroundColor } }
    };

    public static FormattedLine FormatLine(this string text, ConsoleColor color) => new()
    {
        Chunks = new List<FormattedText> { new() { RawText = text, Color = color } }
    };

    public static FormattedText FormatChunk(this string chunk) => new()
    {
        RawText = chunk,
        Color = Console.ForegroundColor
    };

    public static FormattedText FormatChunk(this string chunk, ConsoleColor color) => new()
    {
        RawText = chunk,
        Color = color
    };

    public static FormattedLine Append(this FormattedLine line, string chunk)
    {
        line.Chunks = line.Chunks.Append(chunk.FormatChunk());
        return line;
    }

    public static FormattedLine Append(this FormattedLine line, FormattedText chunk)
    {
        line.Chunks = line.Chunks.Append(chunk);
        return line;
    }
}

public interface IDisplay
{
    FormattedLine Layout(IEnumerable<FormattedText> chunks, int tabs = 0);
    FormattedLine Layout(string line, int tabs = 0, ConsoleColor? color = null);
    void Print(IEnumerable<FormattedLine> lines);
    void Print(IEnumerable<string> lines);
    void Error(string errorMessage);
}

