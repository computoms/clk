using clk.Domain;

namespace clk.Commands;

public class CommandUtils(IRecordRepository repository, IDisplay display)
{
    public TaskLine FindPartiallyMatchingTask(TaskLine task)
    {
        if (string.IsNullOrWhiteSpace(task.Id))
            return task;

        var correspondingTask = repository.GetAll().FirstOrDefault(x => x.Id == task.Id);
        if (correspondingTask == null)
            return task;

        if (!string.IsNullOrWhiteSpace(task.Title) && task.Title != correspondingTask.Title)
            throw new InvalidDataException($"Id {task.Id} already exists");

        return correspondingTask.Duplicate(task.StartTime);
    }

    public void DisplayTask(TaskLine line)
    {
        display.Print([
            display.Layout(
            [
                (line.StartTime.ToString("HH:mm") + " ").FormatChunk(ConsoleColor.DarkGreen),
                line.Title.FormatChunk(),
                line.Tags.Aggregate("", (t1, t2) => t1 + " +" + t2).FormatChunk(ConsoleColor.DarkBlue),
                line.Id != string.Empty ? $" .{line.Id}".FormatChunk(ConsoleColor.DarkYellow) : string.Empty.FormatChunk()
            ])
        ]);
    }
}
