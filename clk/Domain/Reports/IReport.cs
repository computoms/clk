namespace clk.Domain.Reports;

public interface IReport
{
    string Name { get; }
    void Print(IEnumerable<TaskLine> tasks);
}

