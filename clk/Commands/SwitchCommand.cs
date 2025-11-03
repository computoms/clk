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
        var latestActivity = recordRepository.GetAll()
            .Where(x => x.Records.Any())
            .OrderByDescending(x => x.Records.Max(y => y.StartTime))
            .Skip(1)
            .FirstOrDefault();
        if (latestActivity == null)
        {
            display.Error("No activities to switch to");
            return;
        }

        recordRepository.AddRecord(latestActivity.Task, new Record(timeProvider.Now, null));
        commandUtils.DisplayResult(latestActivity.Task, new Record(timeProvider.Now, null));
    }
}
