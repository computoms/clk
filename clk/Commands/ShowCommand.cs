using clk.Domain;
using clk.Domain.Reports;

namespace clk.Commands;

public class ShowCommand(ProgramArguments pArgs,
    FilterParser filterParser,
    IEnumerable<IReport> reports) : ICommand
{
    public static string Name { get; } = "show";

    public void Execute()
    {
        var report = GetReport() ?? throw new KeyNotFoundException("Specified report type could not be found");
        report.Print(filterParser.Filter());
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