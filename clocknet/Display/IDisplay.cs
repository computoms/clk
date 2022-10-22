namespace clocknet.Display;

public interface IDisplay
{
    string Layout(string line, int tabs = 0);
    void Print(IEnumerable<string> lines);
}

