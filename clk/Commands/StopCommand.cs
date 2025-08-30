using clk.Domain;

namespace clk.Commands;

public class StopCommand(ProgramArguments pArgs, IRecordRepository recordRepository, CommandUtils commandUtils) : ICommand
{
    public static string Name { get; } = "stop";

    public void Execute()
    {
        var activity = new InputTask(new Domain.Task("[Stop]", [], string.Empty), new Record(pArgs.Time));
        recordRepository.AddRecord(activity.Task, activity.Record);
        commandUtils.DisplayResult(activity);
    }
}