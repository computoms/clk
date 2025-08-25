using clocknet;

public class StopCommand : BaseCommand
{
    private readonly IRecordRepository recordRepository;

    public StopCommand(ProgramArguments pArgs, IRecordRepository recordRepository) : base(pArgs)
    {
        this.recordRepository = recordRepository;
    }

    public static string Name { get; } = "stop";

    public override void Execute()
    {
        recordRepository.AddRaw("[Stop]");
    }
}