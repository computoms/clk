using clk.Domain;

namespace clk.Commands;

/**
* Switch to the n-1 activity.
*/
public class SwitchCommand(IRecordRepository recordRepository, IDisplay display, Utils.ITimeProvider timeProvider, CommandUtils commandUtils) : ICommand
{
    public static string Name { get; } = "switch";

    public void Execute()
    {
        var latestTask = recordRepository.FilterByQuery(new RepositoryQuery(DateTime.Today, null, null, null, null, 2))
            .OrderByDescending(x => x.StartTime)
            .Skip(1)
            .FirstOrDefault();
        if (latestTask == null)
        {
            display.Error("No activities to switch to");
            return;
        }

        var newTask = latestTask.Duplicate(timeProvider.Now);
        recordRepository.AddTask(newTask);
        commandUtils.DisplayTask(newTask);
    }
}
