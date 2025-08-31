using clk.Domain;
using clk.Domain.Filters;
using clk.Domain.Reports;
using clk.Utils;

namespace clk.Commands;

public class ShowCommand(ProgramArguments pArgs,
    FilterFactory filterFactory,
    IEnumerable<IReport> reports) : ICommand
{
    public static string Name { get; } = "show";

    public void Execute()
    {
        var filter = filterFactory.GetFilter();
        var report = GetReport() ?? throw new KeyNotFoundException("Specified report type could not be found");
        report.Print(filter.GetActivities());
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
}