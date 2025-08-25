using System.Diagnostics;
using clocknet;

public class OpenCommand : BaseCommand
{
    private readonly Settings settings;

    public OpenCommand(ProgramArguments pArgs, Settings settings) : base(pArgs)
    {
        this.settings = settings;
    }

    public static string Name { get; } = "open";

    public override void Execute()
    {
        using (var p = new Process())
        {
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = settings.EditorCommand;
            p.StartInfo.Arguments = settings.File;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
        }
    }
}