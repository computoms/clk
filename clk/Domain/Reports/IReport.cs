namespace clk.Domain.Reports;

public interface IReport
{
    string Name { get; }
    void Print(IEnumerable<Activity> activities);
}

