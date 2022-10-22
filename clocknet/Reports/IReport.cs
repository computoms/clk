namespace clocknet.Reports;

public interface IReport
{
    void Print(IEnumerable<Activity> activities);
}

