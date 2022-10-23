using clocknet;
using clocknet.Display;
using clocknet.Reports;
using clocknet.Storage;
using clocknet.Utils;
using FileStream = clocknet.Storage.FileStream;

var timeProvider = new TimeProvider();
var file = new FileStream("/Users/thomas/clock.txt");
var storage = new FileStorage(file, timeProvider);
var repository = new RecordRepository(storage, timeProvider);
var display = new ConsoleDisplay();

var commandProcessor = new CommandProcessor(args, repository, display, timeProvider);
commandProcessor.Execute();