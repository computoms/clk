using clocknet;
using clocknet.Display;
using clocknet.Reports;
using clocknet.Storage;
using FileStream = clocknet.Storage.FileStream;

var timeProvider = new TimeProvider();
var file = new FileStream("/Users/thomas/clock.txt");
var storage = new FileStorage(file, timeProvider);
var repository = new RecordRepository(storage, timeProvider);

var display = new ConsoleDisplay();
var report = new DetailsReport(display);

Console.WriteLine("Displaying Daily Report");
Console.WriteLine("=======================");
report.Print(repository.FilterByDate(DateTime.Now));
Console.ReadLine();

