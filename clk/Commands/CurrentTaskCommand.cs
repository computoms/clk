using clk.Domain;

namespace clk.Commands;

public class CurrentTaskCommand(IRecordRepository recordRepository, IDisplay display) : ICommand
{
    public static string Name { get; } = "current";

    public void Execute()
    {
        var activity = recordRepository.GetCurrent();
        var taskName = activity?.Task.Raw ?? "None";
        display.Print(taskName.AsLine());
    }
}