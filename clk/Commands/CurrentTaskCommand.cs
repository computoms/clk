using clk.Domain;

namespace clk.Commands;

public class CurrentTaskCommand(IRecordRepository recordRepository, IDisplay display) : ICommand
{
    public static string Name { get; } = "current";

    public void Execute()
    {
        var task = recordRepository.FilterByQuery(new RepositoryQuery(DateTime.Today, null, null, null, null, 1)).FirstOrDefault();
        var taskName = task?.Raw ?? "None";
        display.Print(taskName.AsLine());
    }
}