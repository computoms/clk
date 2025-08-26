using clocknet.Domain;

namespace clocknet.Commands;

public class StopCommand(ProgramArguments pArgs, IRecordRepository recordRepository) : ICommand
{
    public static string Name { get; } = "stop";

    public void Execute()
    {
        var inputLine = ParseOptions();
        var activity = new InputTask(new Domain.Task("[Stop]", [], string.Empty), new Record(inputLine.Time));
        recordRepository.AddRecord(activity.Task, activity.Record);
    }

    private InputLine ParseOptions()
    {
        var words = pArgs.Args.Skip(1).ToList();
        return new InputLine(words, DateTime.Now)
            .ExtractAtOption(pArgs)
            .ExtractSettingsOption(pArgs);
    }
}