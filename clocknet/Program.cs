using clocknet;
using clocknet.Display;
using clocknet.Reports;
using clocknet.Storage;
using clocknet.Utils;
using FileStream = clocknet.Storage.FileStream;

var settings = Settings.Read();
var timeProvider = new TimeProvider();
var file = new FileStream(settings.File);
var storage = new FileStorage(file, timeProvider);
var repository = new RecordRepository(storage, timeProvider);
var display = new ConsoleDisplay();

var commandProcessor = new CommandProcessor(args, repository, display, timeProvider);
commandProcessor.Execute();