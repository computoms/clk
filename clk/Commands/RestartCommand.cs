using clk.Domain;

namespace clk.Commands;

/**
* Restart the latest activity.
*/
public class RestartCommand(IRecordRepository recordRepository, IDisplay display, Utils.ITimeProvider timeProvider, CommandUtils commandUtils) : ICommand
{
    public static string Name { get; } = "restart";

    public void Execute()
    {
        var latestTask = recordRepository.GetAll()
            .OrderBy(x => x.StartTime)
            .LastOrDefault();
        if (latestTask == null)
        {
            display.Error("No activities to restart");
            return;
        }

        var newTask = latestTask.Duplicate(timeProvider.Now);
        recordRepository.AddTask(newTask);
        commandUtils.DisplayTask(newTask);
    }
}