namespace clocknet.Display;

public class ConsoleDisplay : IDisplay
{
    public ConsoleDisplay()
    {
    }

    public string Layout(string line, int tabs = 0)
    {
        return string.Join(' ', Enumerable.Range(0, tabs).Select(x => "  ")) + line;
    }

    public void Print(IEnumerable<string> lines)
    {
        lines.ToList().ForEach(Console.WriteLine);
    }

    public void Error(string errorMessage) => Console.WriteLine($"Error: {errorMessage}");
}

