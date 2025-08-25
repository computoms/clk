using clocknet;
using clocknet.Reports;
using clocknet.Utils;
using Microsoft.Extensions.DependencyInjection;

public class ShowCommand : BaseCommand
{
    private readonly IRecordRepository recordRepository;
    private readonly ITimeProvider timeProvider;
    private readonly IEnumerable<IReport> reports;

    public ShowCommand(ProgramArguments pArgs,
        IRecordRepository recordRepository,
        ITimeProvider timeProvider,
        IEnumerable<IReport> reports) 
        : base(pArgs)
    {
        this.recordRepository = recordRepository;
        this.timeProvider = timeProvider;
        this.reports = reports;
    }

    public static string Name { get; } = "show";

    public override void Execute()
    {
        var activities = GetActivities(out bool isAll);
        var report = GetReport(isAll) ?? throw new KeyNotFoundException("Specified report type could not be found");
        report.Print(activities);
    }

    private IReport? GetReport(bool isAll)
    {
        if (HasOption(Args.WorkTimes))
        {
            return reports.FirstOrDefault(r => r.Name == Args.WorkTimes);
        }
        else if (HasOption(Args.BarGraphs))
        {
            return reports.FirstOrDefault(r => r.Name == Args.BarGraphs);
        }
        return reports.FirstOrDefault(r => r.Name == Args.Details);
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

}