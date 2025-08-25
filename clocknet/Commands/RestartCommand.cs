using clocknet.Domain;

namespace clocknet.Commands;

public class RestartCommand(IRecordRepository recordRepository, IDisplay display, Utils.ITimeProvider timeProvider) : ICommand
{
    public static string Name { get; } = "restart";

    public void Execute()
    {
        var latestActivity = recordRepository.GetAll().Where(x => x.Records.Any()).OrderBy(x => x.Records.Max(y => y.StartTime)).LastOrDefault();
        if (latestActivity == null)
        {
            display.Error("No activities to restart");
            return;
        }

        recordRepository.AddRecord(latestActivity.Task, new Record(timeProvider.Now, null));
    }
}