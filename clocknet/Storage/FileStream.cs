namespace clocknet.Storage;

public class FileStream : IStream
{
    private readonly string _filename;

    public FileStream(string filename)
    {
        _filename = filename;
    }

    public void AddLine(string line)
    {
        System.IO.File.AppendAllLines(_filename, new List<string>() { line });
    }

    public List<string> ReadAllLines()
    {
        return System.IO.File.ReadAllLines(_filename).ToList();
    }

    public void WriteAllLines(string[] lines)
    {
        System.IO.File.WriteAllLines(_filename, lines);
    }
}

