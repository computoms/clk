using clocknet;
using clocknet.Display;

public class ListCommand : BaseCommand
{
    private readonly IRecordRepository recordRepository;
    private readonly IDisplay display;

    public ListCommand(ProgramArguments pArgs, IRecordRepository recordRepository, IDisplay display) : base(pArgs)
    {
        this.recordRepository = recordRepository;
        this.display = display;
    }

    public static string Name { get; } = "list";

    public override void Execute()
    {
        var activities = recordRepository.GetAll();
        display.Print(activities
                .Select(x => x.Task.Raw.FormatLine()).ToList());
        }
}