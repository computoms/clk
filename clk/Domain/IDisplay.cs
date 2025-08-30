using System.Runtime.InteropServices;

namespace clk.Domain;

public record FormattedText(string RawText)
{
    public FormattedText(string rawText, ConsoleColor color) : this(rawText)
    {
        Color = color;
    }

    public ConsoleColor Color { get; set; } = ConsoleColor.White;
}

public class FormattedLine
{
    public FormattedLine(IEnumerable<FormattedText> chunks)
    {
        Chunks = [.. chunks];
    }

    public FormattedLine(FormattedText chunk)
    {
        Chunks = [chunk];
    }

    public FormattedLine(string rawText)
    {
        Chunks = [new FormattedText(rawText)];
    }

    public FormattedLine Append(FormattedLine appended)
    {
        foreach (var chunk in appended.Chunks)
        {
            Chunks.Add(chunk);
        }
        return this;
    }

    public FormattedLine Prepend(FormattedLine prepended)
    {
        var newList = prepended.Chunks;
        newList.AddRange(Chunks);
        return new FormattedLine(newList);
    }

    public FormattedLine Substring(int startIndex, int count)
    {
        int currentCount = 0;
        var output = new List<FormattedText>();
        foreach (var chunk in Chunks)
        {
            var localIndexes = GetSubstringIndexes(chunk, currentCount, startIndex, count);
            if (IsFullChunk(chunk, localIndexes))
            {
                output.Add(chunk);
            }
            else if (IsPartOfChunk(chunk, localIndexes))
            {
                output.Add(new FormattedText(chunk.RawText.Substring(localIndexes.Item1, localIndexes.Item2), chunk.Color));
            }
            currentCount += chunk.RawText.Length;
        }

        return new FormattedLine(output);
    }

    private static bool IsFullChunk(FormattedText txt, Tuple<int, int> localIndexes)
        => localIndexes.Item1 == 0 && localIndexes.Item2 == txt.RawText.Length;
    private static bool IsPartOfChunk(FormattedText txt, Tuple<int, int> localIndexes)
        => localIndexes.Item1 >= 0 && localIndexes.Item2 >= 0 && localIndexes.Item2 <= txt.RawText.Length;

    private Tuple<int, int> GetSubstringIndexes(FormattedText txt, int currentCount, int startIndex, int count)
    {
        var localStart = startIndex - currentCount;
        if (localStart < 0)
            localStart = 0;
        if (localStart >= txt.RawText.Length)
            return new Tuple<int, int>(-1, 0);

        var localEnd = startIndex + count - currentCount;
        if (localEnd > txt.RawText.Length)
            localEnd = txt.RawText.Length;

        var localCount = localEnd - localStart;
        return new Tuple<int, int>(localStart, localCount);
    }

    public List<FormattedText> Chunks { get; private set; } = [];
}

public static class FormattedTextExtensions
{
    public static FormattedLine AsLine(this string text) => new FormattedLine(text);
    public static FormattedLine AsLine(this string text, ConsoleColor color) => new(new FormattedText(text, color));

    public static FormattedText FormatChunk(this string chunk) => new(chunk);

    public static FormattedText FormatChunk(this string chunk, ConsoleColor color) => new(chunk, color);

    public static IEnumerable<FormattedText> ToEnumerable(this FormattedText chunk) => [chunk];
}

public interface IDisplay
{
    FormattedLine Layout(IEnumerable<FormattedText> chunks, int tabs = 0);
    void Print(IEnumerable<FormattedLine> lines);
    void Print(FormattedLine line);
    void Error(string errorMessage);
}

