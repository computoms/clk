namespace clk.Domain.Reports;

public interface IReport
{
    Option Name { get; }
    void Print(IEnumerable<Activity> activities);
}

