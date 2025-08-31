using clk.Domain;

namespace clk.Commands;

public class CommandUtils(IRecordRepository repository, IDisplay display)
{
    public Domain.Task FindPartiallyMatchingTask(Domain.Task task)
    {
        var defaultTask = new Domain.Task(task.Title, task.Path, task.Tags, task.Id);
        if (string.IsNullOrWhiteSpace(task.Id))
            return defaultTask;

        var correspondingActivity = repository.GetAll().FirstOrDefault(x => x.Task.Id == task.Id);
        if (correspondingActivity == null)
            return defaultTask;

        if (!string.IsNullOrWhiteSpace(task.Title) && task.Title != correspondingActivity.Task.Title)
            throw new InvalidDataException($"Id {task.Id} already exists");

        return correspondingActivity.Task;
    }

    public void DisplayResult(InputTask activity)
    {
        display.Print([
            display.Layout(
            [
                (activity.Record.StartTime.ToString("HH:mm") + " ").FormatChunk(ConsoleColor.DarkGreen),
                activity.Task.Title.FormatChunk(),
                activity.Task.Tags.Aggregate("", (t1, t2) => t1 + " +" + t2).FormatChunk(ConsoleColor.DarkBlue),
                activity.Task.Id != string.Empty ? $" .{activity.Task.Id}".FormatChunk(ConsoleColor.DarkYellow) : string.Empty.FormatChunk()
            ])
        ]);
    }
}

public record InputTask(clk.Domain.Task Task, clk.Domain.Record Record);
