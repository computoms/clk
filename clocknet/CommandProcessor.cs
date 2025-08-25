using System.Diagnostics;
using clocknet.Display;
using clocknet.Reports;
using clocknet.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace clocknet;

public class CommandProcessor
{
    private readonly string[] arguments;
    private readonly IRecordRepository recordRepository;
    private readonly IDisplay display;
    private readonly ITimeProvider timeProvider;
    private readonly Settings settings;

    private readonly IServiceProvider sp;

    public CommandProcessor(
        ProgramArguments pArgs,
        IRecordRepository recordRepository,
        IDisplay display,
        ITimeProvider timeProvider,
        Settings settings,
        IServiceProvider sp)
    {
        this.arguments = pArgs.Args;
        this.recordRepository = recordRepository;
        this.display = display;
        this.timeProvider = timeProvider;
        this.settings = settings;
        this.sp = sp;
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
                ExecuteAdd();
                break;

            case Commands.Stop:
                recordRepository.AddRaw("[Stop]");
                break;

            case Commands.Restart:
                Restart();
                break;

            case Commands.Open:
                using (var p = new Process())
                {
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.FileName = settings.EditorCommand;
                    p.StartInfo.Arguments = settings.File;
                    p.StartInfo.CreateNoWindow = true;
                    p.Start();
                }
                break;
        }
    }

    private void ExecuteAdd()
    {
        var rawLine = string.Join(' ', arguments.Skip(1));
        bool parseTime = false;
        if (arguments.Count() == 1)
        {
            rawLine = settings.DefaultTask;
	    }
        else if (HasOption(Args.At))
        {
			int index = arguments.ToList().IndexOf("--at");
			var time =  arguments.Skip(index + 1).FirstOrDefault();
            rawLine = string.Join(' ', arguments.Skip(1).Where(x => x != "--at" && x != time).Prepend(time));
            parseTime = true;
	    }
        recordRepository.AddRaw(rawLine, parseTime);
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
            return sp.GetRequiredKeyedService<IReport>(Args.WorkTimes);
        }
        else if (HasOption(Args.BarGraphs))
        {
            return sp.GetRequiredKeyedService<IReport>(Args.BarGraphs);
        }
        return sp.GetRequiredKeyedService<IReport>(Args.Details);
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
            || (!string.IsNullOrEmpty(opt.Short) && arguments.Where(x => x.StartsWith("-") && !x.StartsWith("--") && x.Contains(opt.Short)).Any());
    }

    public static class Args
    {
        // Filters
        public readonly static Option All = new Option("all", "a");
        public readonly static Option Week = new Option("week", "w");
        public readonly static Option Yesterday = new Option("yesterday", "y");
        // Reports
        public readonly static Option WorkTimes = new Option("worktimes", "w");
        public readonly static Option BarGraphs = new Option("bar", "b");
        public readonly static Option Details = new Option("details", "d");
        // Others
        public readonly static Option At = new Option("at", string.Empty);
    }

    public static class Commands
    {
        public const string Show = "show";
        public const string Add = "add";
        public const string Stop = "stop";
        public const string Restart = "restart";
        public const string Open = "open";
    }

    public record Option(string Long, string Short);
}

