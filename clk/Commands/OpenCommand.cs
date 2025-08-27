using System.Diagnostics;

namespace clk.Commands;

public class OpenCommand(Settings settings) : ICommand
{
    public static string Name { get; } = "open";

    public void Execute()
    {
        using var p = new Process();
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.FileName = settings.Data.EditorCommand;
        p.StartInfo.Arguments = settings.Data.File;
        p.StartInfo.CreateNoWindow = true;
        p.Start();
    }
}