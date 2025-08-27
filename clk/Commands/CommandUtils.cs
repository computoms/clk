
using System.Globalization;
using clk.Domain;

namespace clk.Commands;

public class CommandUtils(IRecordRepository repository, IDisplay display)
{
    public Domain.Task FindPartiallyMatchingTask(Domain.Task task)
    {
        var defaultTask = new Domain.Task(task.Title, task.Tags, task.Id);
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

public record CommandLineInput(List<string> Words, DateTime Time)
{
    public CommandLineInput ExtractAtOption(ProgramArguments pArgs)
    {
        if (!pArgs.HasOption(Args.At))
            return this;

        int index = Words.IndexOf("--at");
        var timeRaw = Words.Skip(index + 1).FirstOrDefault() ?? DateTime.Now.ToString("HH:mm");
        var lineWithoutTime = Words.Where(x => x != "--at" && x != timeRaw);
        var convertedTime = DateTime.TryParseExact(
                    SanitizeInput(timeRaw), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                ? date : DateTime.MinValue;
        var time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, convertedTime.Hour, convertedTime.Minute, 0);
        return new CommandLineInput([.. lineWithoutTime], time);
    }

    public CommandLineInput ExtractSettingsOption(ProgramArguments pArgs)
    {
        if (!pArgs.HasOption(Args.Settings))
            return this;

        var settingsValue = pArgs.GetValue(Args.Settings);
        return this with { Words = [.. Words.Where(w => w != $"--{Args.Settings}" && w != settingsValue)] };
    }

    public CommandLineInput IncludeDefaultTask(string defaultTask)
    {
        return Words.Count == 0 ? (this with { Words = [.. defaultTask.Split(' ')] }) : this;
    }

    private static string SanitizeInput(string line) => line.Replace("\r", "").Replace("\n", "").Trim();
}

