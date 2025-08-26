
using System.Globalization;
using clocknet.Domain;

namespace clocknet.Commands;

public class CommandUtils(IRecordRepository repository)
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
}

public record InputTask(clocknet.Domain.Task Task, clocknet.Domain.Record Record);

public record InputLine(List<string> Words, DateTime Time)
{
    public InputLine ExtractAtOption(ProgramArguments pArgs)
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
        return new InputLine([.. lineWithoutTime], time);
    }

    public InputLine ExtractSettingsOption(ProgramArguments pArgs)
    {
        if (!pArgs.HasOption(Args.Settings))
            return this;

        var settingsValue = pArgs.GetValue(Args.Settings);
        return this with { Words = [.. Words.Where(w => w != $"--{Args.Settings}" && w != settingsValue)] };
    }

    public InputLine IncludeDefaultTask(string defaultTask)
    {
        return Words.Count == 0 ? (this with { Words = [.. defaultTask.Split(' ')] }) : this;
    }

    private static string SanitizeInput(string line) => line.Replace("\r", "").Replace("\n", "").Trim();
}

