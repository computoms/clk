using clocknet;
using clocknet.Display;
using clocknet.Storage;
using clocknet.Utils;
using FileStream = clocknet.Storage.FileStream;
using Microsoft.Extensions.DependencyInjection;
using clocknet.Reports;

var settings = Settings.Read();
var serviceProvider = new ServiceCollection()
    .AddSingleton<IRecordRepository, RecordRepository>()
    .AddSingleton<IStorage, FileStorage>()
    .AddSingleton<IStream, FileStream>()
    .AddSingleton<ITimeProvider, clocknet.Utils.TimeProvider>()
    .AddSingleton(sp => Settings.Read())
    .AddSingleton<IDisplay, ConsoleDisplay>(sp => new ConsoleDisplay(true))
    .AddSingleton(sp => new ProgramArguments(args))
    .AddSingleton<CommandProcessor>();

serviceProvider.AddKeyedSingleton<IReport, BarGraphReport>(CommandProcessor.Args.BarGraphs);
serviceProvider.AddKeyedSingleton<IReport, WorktimeReport>(CommandProcessor.Args.WorkTimes);
serviceProvider.AddKeyedSingleton<IReport, DetailsReport>(CommandProcessor.Args.Details);

var services = serviceProvider.BuildServiceProvider();
var commandProcessor = services.GetRequiredService<CommandProcessor>();
commandProcessor.Execute();