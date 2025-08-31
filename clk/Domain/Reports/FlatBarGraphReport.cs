namespace clk.Domain.Reports;

public class FlatBarGraphReport(IDisplay display) : BaseBarGraphReport(display), IReport
{
    public string Name { get; } = nameof(FlatBarGraphReport);

    public void Print(IEnumerable<Activity> activities)
    {
        PrintBarGraph(
            activities.Select(a => new BarInfo(
                a.Task.Title,
                a.Duration)));
    }
}