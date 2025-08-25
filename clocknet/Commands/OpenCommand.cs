using System.Diagnostics;

namespace clocknet.Commands;

public class OpenCommand(Settings settings) : ICommand
{
    public static string Name { get; } = "open";

    public void Execute()
    {
        using var p = new Process();
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.FileName = settings.EditorCommand;
        p.StartInfo.Arguments = settings.File;
        p.StartInfo.CreateNoWindow = true;
        p.Start();
    }
}