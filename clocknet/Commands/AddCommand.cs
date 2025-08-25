
using clocknet;

public class AddCommand : BaseCommand
{
    private readonly Settings settings;
    private readonly IRecordRepository recordRepository;

    public AddCommand(ProgramArguments pArgs, Settings settings, IRecordRepository recordRepository) : base(pArgs)
    {
        this.settings = settings;
        this.recordRepository = recordRepository;
    }

    public static string Name { get; } = "add";

    public override void Execute()
    {
        var rawLine = string.Join(' ', pArgs.Args.Skip(1));
        bool parseTime = false;
        if (pArgs.Args.Count() == 1)
        {
            rawLine = settings.DefaultTask;
        }
        else if (HasOption(Args.At))
        {
            int index = pArgs.Args.ToList().IndexOf("--at");
            var time = pArgs.Args.Skip(index + 1).FirstOrDefault();
            rawLine = string.Join(' ', pArgs.Args.Skip(1).Where(x => x != "--at" && x != time).Prepend(time));
            parseTime = true;
        }
        recordRepository.AddRaw(rawLine, parseTime);
    }
}