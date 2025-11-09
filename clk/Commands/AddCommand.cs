using clk.Domain;
using clk.Utils;

namespace clk.Commands;

public class AddCommand(ProgramArguments pArgs, Settings settings, IRecordRepository recordRepository, CommandUtils commandUtils, ITimeProvider timeProvider) : ICommand
{
    public static string Name { get; } = "add";

    public void Execute()
    {
        var task = ParseInput();
        task = commandUtils.FindPartiallyMatchingTask(task);
        recordRepository.AddTask(task);
        commandUtils.DisplayTask(task);
    }

    private TaskLine ParseInput()
    {
        var rawTitle = string.IsNullOrWhiteSpace(pArgs.Title) ? settings.Data.DefaultTask : pArgs.Title;
        var now = pArgs.Time.ToString("HH:mm");
        return new TaskLine($"{now} {rawTitle}");
    }
}