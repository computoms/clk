using clk.Domain;

namespace clk.Infra;

public class ConsoleDisplay : IDisplay
{
    private bool _enableColors = false;

    public ConsoleDisplay(bool enableColors)
    {
        _enableColors = enableColors;
    }

    public FormattedLine Layout(IEnumerable<FormattedText> chunks, int tabs = 0)
    {
        return new FormattedLine
        {
            Chunks = chunks.Prepend(new()
            {
                RawText = string.Join(' ', Enumerable.Range(0, tabs).Select(x => " ")),
                Color = Console.ForegroundColor
            })
        };
    }

    public void Print(IEnumerable<FormattedLine> lines)
    {
        foreach (var line in lines)
        {
            foreach (var chunk in line.Chunks)
            {
                Console.ForegroundColor = chunk.Color;
                Console.Write(chunk.RawText);
                Console.ResetColor();
            }
            Console.WriteLine("");
        }
    }

    public void Error(string errorMessage) => Console.WriteLine($"Error: {errorMessage}");
}

