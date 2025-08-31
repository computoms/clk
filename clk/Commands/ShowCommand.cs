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
        var reportName = pArgs.HasOption(Args.Report) ? pArgs.GetValue(Args.Report) : Args.Details;
        if (pArgs.HasOption(Args.BarGraphs))
            reportName = Args.BarGraphs;
        else if (pArgs.HasOption(Args.Timesheet))
            reportName = Args.Timesheet;
        return reports.FirstOrDefault(r => r.Name == reportName);
    }
}