using clk.Domain;

namespace clk.Commands;

public class ListCommand(IRecordRepository recordRepository, IDisplay display) : ICommand
{
    public static string Name { get; } = "list";

    public void Execute()
    {
        var activities = recordRepository.GetAll();
        display.Print(activities
                .Select(x => x.Task.Raw.AsLine()).ToList());
    }
}