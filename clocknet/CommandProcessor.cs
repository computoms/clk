using clocknet.Display;
using clocknet.Reports;
using clocknet.Utils;

namespace clocknet;

public class CommandProcessor
{
    private readonly string[] arguments;
    private readonly IRecordRepository recordRepository;
    private readonly IDisplay display;
    private readonly ITimeProvider timeProvider;

    public CommandProcessor(string[] arguments, IRecordRepository recordRepository, IDisplay display, ITimeProvider timeProvider)
    {
        this.arguments = arguments;
        this.recordRepository = recordRepository;
        this.display = display;
        this.timeProvider = timeProvider;
    }

    public void Execute()
    {
        var command = arguments.FirstOrDefault();
        switch (command)
        {
            case Commands.Show:
			    ExecuteShow();
			    break;

            case Commands.Add:
                recordRepository.AddRaw(string.Join(' ', arguments.Skip(1)));
                break;

            case Commands.Stop:
                recordRepository.AddRaw("[Stop]");
                break;

            case Commands.Restart:
                Restart();
                break;
        }
    }

    private void Restart()
    {
        var latestActivity = recordRepository.GetAll().Where(x => x.Records.Any()).OrderBy(x => x.Records.Max(y => y.StartTime)).LastOrDefault();
        if (latestActivity == null)
        {
            display.Error("No activities to restart");
            return;
	    }

        recordRepository.AddRecord(latestActivity.Task, new Record(timeProvider.Now, null));
    }

    private void ExecuteShow()
    {
        var activities = GetActivities(out bool isAll);
        IReport report = GetReport(isAll);
        report.Print(activities);
    }

    private IReport GetReport(bool isAll)
    { 
        if (HasOption(Args.WorkTimes))
        {
            return new WorktimeReport(display, !isAll);
        }
        return new DetailsReport(display);
    }

    private IEnumerable<Activity> GetActivities(out bool isAll)
    {
        isAll = false;
        if (HasOption(Args.All))
        {
            isAll = true;
            return recordRepository.GetAll();
        }
        else if (HasOption(Args.Week))
        {
            return recordRepository.FilterByDate(timeProvider.Now.MondayOfTheWeek(), timeProvider.Now.MondayOfTheWeek().AddDays(4));
        }
        else if (HasOption(Args.Yesterday))
        {
            return recordRepository.FilterByDate(timeProvider.Now.Date.AddDays(-1));
        }
        return recordRepository.FilterByDate(timeProvider.Now.Date);
    }

    private bool HasOption(Option opt)
    {
        return arguments.Contains($"--{opt.Long}")
            || arguments.Where(x => x.StartsWith("-") && !x.StartsWith("--") && x.Contains(opt.Short)).Any();
    }

    private static class Args
    {
        public readonly static Option All = new Option("all", "a");
        public readonly static Option Week = new Option("week", "w");
        public readonly static Option Yesterday = new Option("yesterday", "y");
        public readonly static Option WorkTimes = new Option("worktimes", "w");
    }

    private static class Commands
    {
        public const string Show = "show";
        public const string Add = "add";
        public const string Stop = "stop";
        public const string Restart = "restart";
    }

    private record Option(string Long, string Short);
}

