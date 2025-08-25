using clocknet;
using clocknet.Display;

public class RestartCommand : BaseCommand
{
    private readonly IRecordRepository recordRepository;
    private readonly IDisplay display;
    private readonly clocknet.Utils.ITimeProvider timeProvider;

    public RestartCommand(ProgramArguments pArgs, IRecordRepository recordRepository, IDisplay display, clocknet.Utils.ITimeProvider timeProvider) : base(pArgs)
    {
        this.recordRepository = recordRepository;
        this.display = display;
        this.timeProvider = timeProvider;
    }

    public static string Name { get; } = "restart";

    public override void Execute()
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