using clk.Domain;

namespace clk.Commands;

public class StopCommand(ProgramArguments pArgs, IRecordRepository recordRepository, CommandUtils commandUtils) : ICommand
{
    public static string Name { get; } = "stop";

    public void Execute()
    {
        var time = pArgs.Time.ToString("HH:mm");
        var task = new TaskLine($"{time} [Stop]", DateTime.Today);
        recordRepository.AddTask(task);
        commandUtils.DisplayTask(task);
    }
}