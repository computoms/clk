using clocknet;
using clocknet.Display;
using clocknet.Reports;
using clocknet.Storage;
using FileStream = clocknet.Storage.FileStream;

var arguments = args;

var timeProvider = new TimeProvider();
var file = new FileStream("/Users/thomas/clock.txt");
var storage = new FileStorage(file, timeProvider);
var repository = new RecordRepository(storage, timeProvider);

IReport currentReport = new DetailsReport();
IEnumerable<Activity> activities = repository.FilterByDate(DateTime.Now);
if (arguments.Contains("--week"))
{
    activities = repository.FilterByDate(DateTime.Now.DayOfSameWeek(DayOfWeek.Monday), DateTime.Now.DayOfSameWeek(DayOfWeek.Friday));
    currentReport = new WorktimeReport();
}
else if (arguments.Contains("--month"))
{
    activities = repository.GetAll();
    currentReport = new WorktimeReport(false);
}
else if (arguments.Contains("--yesterday"))
{
    activities = repository.FilterByDate(DateTime.Now.Date.AddDays(-1));
}
else if (arguments.Contains("--all"))
{
    activities = repository.GetAll();
}

currentReport.Print(activities);
Console.ReadLine();

