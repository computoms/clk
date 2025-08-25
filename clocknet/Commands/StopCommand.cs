using clocknet.Domain;

namespace clocknet.Commands;

public class StopCommand(ProgramArguments pArgs, IRecordRepository recordRepository) : ICommand
{
    public static string Name { get; } = "stop";

    public void Execute()
    {
        if (!pArgs.HasOption(Args.At))
        {
            recordRepository.AddRaw("[Stop]");
            return;
        }

        int index = pArgs.Args.ToList().IndexOf("--at");
        var time = pArgs.Args.Skip(index + 1).FirstOrDefault();
        recordRepository.AddRaw(time + " [Stop]", true);
    }
}