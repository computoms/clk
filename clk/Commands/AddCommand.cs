using clk.Domain;

namespace clk.Commands;

public class AddCommand(ProgramArguments pArgs, Settings settings, IRecordRepository recordRepository, CommandUtils commandUtils, IDisplay display) : ICommand
{
    public static string Name { get; } = "add";

    public void Execute()
    {
        var activity = ParseInput();
        activity = activity with { Task = commandUtils.FindPartiallyMatchingTask(activity.Task) };
        recordRepository.AddRecord(activity.Task, activity.Record);
        commandUtils.DisplayResult(activity);
    }

    private InputTask ParseInput()
    {
        var rawTitle = string.IsNullOrWhiteSpace(pArgs.Title) ? settings.Data.DefaultTask : pArgs.Title;
        var words = rawTitle.Split(' ');
        var title = string.Empty;
        var tags = new List<string>();
        var id = string.Empty;
        foreach (string v in words)
        {
            if (v.Length > 1 && v.StartsWith('+'))
                tags.Add(v[1..]);
            else if (v.Length > 1 && v.StartsWith('.'))
                id = v[1..];
            else
                title += v + " ";
        }

        title = title.Trim();
        return new InputTask(new Domain.Task(title, [.. tags], id), new Record(pArgs.Time));
    }
}