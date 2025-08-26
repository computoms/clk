using clocknet.Domain;

namespace clocknet.Commands;

public class StopCommand(ProgramArguments pArgs, IRecordRepository recordRepository, CommandUtils commandUtils) : ICommand
{
    public static string Name { get; } = "stop";

    public void Execute()
    {
        var inputLine = ParseOptions();
        var activity = new InputTask(new Domain.Task("[Stop]", [], string.Empty), new Record(inputLine.Time));
        recordRepository.AddRecord(activity.Task, activity.Record);
        commandUtils.DisplayResult(activity);
    }

    private CommandLineInput ParseOptions()
    {
        var words = pArgs.Args.Skip(1).ToList();
        return new CommandLineInput(words, DateTime.Now)
            .ExtractAtOption(pArgs)
            .ExtractSettingsOption(pArgs);
    }
}