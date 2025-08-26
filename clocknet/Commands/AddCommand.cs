using clocknet.Domain;

namespace clocknet.Commands;

public class AddCommand(ProgramArguments pArgs, Settings settings, IRecordRepository recordRepository, CommandUtils commandUtils, IDisplay display) : ICommand
{
    public static string Name { get; } = "add";

    public void Execute()
    {
        var inputLine = ParseOptions();
        var activity = ParseInput(inputLine);
        activity = activity with { Task = commandUtils.FindPartiallyMatchingTask(activity.Task) };
        recordRepository.AddRecord(activity.Task, activity.Record);
        commandUtils.DisplayResult(activity);
    }

    private InputLine ParseOptions()
    {
        var words = pArgs.Args.Skip(1).ToList();
        return new InputLine(words, DateTime.Now)
            .ExtractAtOption(pArgs)
            .ExtractSettingsOption(pArgs)
            .IncludeDefaultTask(settings.Data.DefaultTask);
    }

    private static InputTask ParseInput(InputLine line)
    {
        var tags = line.Words.Where(x => x.StartsWith('+')).Select(x => x[1..]).ToArray();
        var number = line.Words.FirstOrDefault(x => x.StartsWith('.') && x.Skip(1).All(char.IsDigit))?[1..];
        var title = string.Join(' ', line.Words.Where(x => !x.StartsWith('+') && x != $".{number}")).Trim();
        return new InputTask(new Domain.Task(title, tags, number ?? string.Empty), new Record(line.Time));
    }

    private void DisplayResult(InputTask activity)
    {
        display.Print([
            display.Layout(
            [
                (activity.Record.StartTime.ToString("HH:mm") + " ").FormatChunk(ConsoleColor.DarkGreen),
                activity.Task.Title.FormatChunk(),
                activity.Task.Tags.Aggregate("", (t1, t2) => t1 + " +" + t2).FormatChunk(ConsoleColor.DarkBlue),
                $" .{activity.Task.Id}".FormatChunk(ConsoleColor.DarkYellow)
            ])
        ]);
    }
}