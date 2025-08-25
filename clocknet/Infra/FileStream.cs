using System.IO;

namespace clocknet.Storage;

public class FileStream : IStream
{
    private readonly string _filename;

    public FileStream(Settings settings)
    {
        _filename = settings.File;
    }

    public void AddLine(string line)
    {
        EnsureFileExists();
        File.AppendAllLines(_filename, new List<string>() { line });
    }

    public List<string> ReadAllLines()
    {
        EnsureFileExists();
        return File.ReadAllLines(_filename).ToList();
    }

    public void WriteAllLines(string[] lines)
    {
        File.WriteAllLines(_filename, lines);
    }

    private void EnsureFileExists()
    { 
        if (!File.Exists(_filename))
        {
            File.WriteAllText(_filename, "");
	    }
    }
}

