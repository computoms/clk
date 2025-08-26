using clocknet.Domain;

namespace clocknet.Commands;

public class AddCommand(ProgramArguments pArgs, Settings settings, IRecordRepository recordRepository) : ICommand
{
    public static string Name { get; } = "add";

    public void Execute()
    {
        var rawLine = string.Join(' ', pArgs.Args.Skip(1));
        bool parseTime = false;
        if (pArgs.Args.Length == 1 || (pArgs.HasOption(Args.Settings) && pArgs.Args.Length == 3))
        {
            rawLine = settings.Data.DefaultTask;
        }
        else if (pArgs.HasOption(Args.At))
        {
            int index = pArgs.Args.ToList().IndexOf("--at");
            var time = pArgs.Args.Skip(index + 1).FirstOrDefault();
            rawLine = string.Join(' ', pArgs.Args.Skip(1).Where(x => x != "--at" && x != time).Prepend(time));
            parseTime = true;
        }
        recordRepository.AddRaw(rawLine, parseTime);
    }
}