using clocknet.Domain;
using clocknet.Domain.Reports;
using clocknet.Utils;

namespace clocknet.Commands;

public class ShowCommand(ProgramArguments pArgs,
    IRecordRepository recordRepository,
    ITimeProvider timeProvider,
    IEnumerable<IReport> reports) : ICommand
{
    public static string Name { get; } = "show";

    public void Execute()
    {
        var activities = GetActivities(out bool isAll);
        var report = GetReport(isAll) ?? throw new KeyNotFoundException("Specified report type could not be found");
        report.Print(activities);
    }

    private IReport? GetReport(bool isAll)
    {
        if (pArgs.HasOption(Args.WorkTimes))
        {
            return reports.FirstOrDefault(r => r.Name == Args.WorkTimes);
        }
        else if (pArgs.HasOption(Args.BarGraphs))
        {
            return reports.FirstOrDefault(r => r.Name == Args.BarGraphs);
        }
        return reports.FirstOrDefault(r => r.Name == Args.Details);
    }


    private IEnumerable<Activity> GetActivities(out bool isAll)
    {
        isAll = false;
        if (pArgs.HasOption(Args.All))
        {
            isAll = true;
            return recordRepository.GetAll();
        }
        else if (pArgs.HasOption(Args.Week))
        {
            return recordRepository.FilterByDate(timeProvider.Now.MondayOfTheWeek(), timeProvider.Now.MondayOfTheWeek().AddDays(4));
        }
        else if (pArgs.HasOption(Args.Yesterday))
        {
            return recordRepository.FilterByDate(timeProvider.Now.Date.AddDays(-1));
        }
        return recordRepository.FilterByDate(timeProvider.Now.Date);
    }

}