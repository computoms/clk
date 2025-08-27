using clk.Domain;
using clk.Domain.Reports;
using clk.Utils;

namespace clk.Commands;

public class ShowCommand(ProgramArguments pArgs,
    IRecordRepository recordRepository,
    ITimeProvider timeProvider,
    IEnumerable<IReport> reports) : ICommand
{
    public static string Name { get; } = "show";

    public void Execute()
    {
        var activities = GetActivities();
        var report = GetReport() ?? throw new KeyNotFoundException("Specified report type could not be found");
        report.Print(activities);
    }

    private IReport? GetReport()
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


    private IEnumerable<Activity> GetActivities()
    {
        if (pArgs.HasOption(Args.All))
        {
            return recordRepository.GetAll();
        }
        if (pArgs.HasOption(Args.Week))
        {
            return recordRepository.FilterByDate(timeProvider.Now.MondayOfTheWeek(), timeProvider.Now.MondayOfTheWeek().AddDays(4));
        }
        if (pArgs.HasOption(Args.Yesterday))
        {
            return recordRepository.FilterByDate(timeProvider.Now.Date.AddDays(-1));
        }
        if (GetFilterArgs().Count > 0)
        {
            return recordRepository.FilterByTag(GetFilterArgs());
        }
        return recordRepository.FilterByDate(timeProvider.Now.Date);
    }

    private List<string> GetFilterArgs()
    {
        return [.. pArgs.Args.Skip(1).Where(a => !a.StartsWith("-")).Select(a => a.First() == '+' ? a[1..] : a)];
    }

}