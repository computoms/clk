using clocknet;
using clocknet.Utils;
using FileStream = clocknet.Infra.FileStream;
using Microsoft.Extensions.DependencyInjection;
using clocknet.Commands;
using clocknet.Domain;
using clocknet.Infra;
using clocknet.Domain.Reports;

var settings = Settings.Read();
var serviceProvider = new ServiceCollection();
serviceProvider
    .AddSingleton<IRecordRepository, RecordRepository>()
    .AddSingleton<IStorage, FileStorage>()
    .AddSingleton<IStream, FileStream>()
    .AddSingleton<ITimeProvider, clocknet.Utils.TimeProvider>()
    .AddSingleton(sp => Settings.Read())
    .AddSingleton<IDisplay, ConsoleDisplay>(sp => new ConsoleDisplay(true))
    .AddSingleton(sp => new ProgramArguments(args))
    .AddSingleton(sp => new CommandProcessor(sp.GetRequiredKeyedService<ICommand>(args.FirstOrDefault())));

serviceProvider.AddSingleton<IReport, BarGraphReport>();
serviceProvider.AddSingleton<IReport, WorktimeReport>();
serviceProvider.AddSingleton<IReport, DetailsReport>();

serviceProvider.AddKeyedSingleton<ICommand, AddCommand>(AddCommand.Name);
serviceProvider.AddKeyedSingleton<ICommand, ShowCommand>(ShowCommand.Name);
serviceProvider.AddKeyedSingleton<ICommand, StopCommand>(StopCommand.Name);
serviceProvider.AddKeyedSingleton<ICommand, RestartCommand>(RestartCommand.Name);
serviceProvider.AddKeyedSingleton<ICommand, OpenCommand>(OpenCommand.Name);
serviceProvider.AddKeyedSingleton<ICommand, ListCommand>(ListCommand.Name);

var services = serviceProvider.BuildServiceProvider();
var commandProcessor = services.GetRequiredService<CommandProcessor>();
commandProcessor.Execute();