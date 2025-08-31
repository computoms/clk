using clk;
using clk.Utils;
using FileStream = clk.Infra.FileStream;
using Microsoft.Extensions.DependencyInjection;
using clk.Commands;
using clk.Domain;
using clk.Infra;
using clk.Domain.Reports;
using clk.Domain.Filters;

try
{

    var serviceProvider = new ServiceCollection();
    serviceProvider
        .AddSingleton<IRecordRepository, RecordRepository>()
        .AddSingleton<IStorage, FileStorage>()
        .AddSingleton<IStream, FileStream>()
        .AddSingleton<ITimeProvider, clk.Utils.TimeProvider>()
        .AddSingleton<Settings>()
        .AddSingleton<CommandUtils>()
        .AddSingleton<IDisplay, ConsoleDisplay>(sp => new ConsoleDisplay(true))
        .AddSingleton(sp => new ProgramArguments(args))
        .AddSingleton(sp => new CommandProcessor(sp.GetRequiredKeyedService<ICommand>(args.FirstOrDefault())));

    // Reports
    serviceProvider.AddSingleton<IReport, BarGraphReport>();
    serviceProvider.AddSingleton<IReport, WorktimeReport>();
    serviceProvider.AddSingleton<IReport, DetailsReport>();

    // Commands
    serviceProvider.AddKeyedSingleton<ICommand, AddCommand>(AddCommand.Name);
    serviceProvider.AddKeyedSingleton<ICommand, ShowCommand>(ShowCommand.Name);
    serviceProvider.AddKeyedSingleton<ICommand, StopCommand>(StopCommand.Name);
    serviceProvider.AddKeyedSingleton<ICommand, RestartCommand>(RestartCommand.Name);
    serviceProvider.AddKeyedSingleton<ICommand, OpenCommand>(OpenCommand.Name);
    serviceProvider.AddKeyedSingleton<ICommand, ListCommand>(ListCommand.Name);
    serviceProvider.AddKeyedSingleton<ICommand, CurrentTaskCommand>(CurrentTaskCommand.Name);

    // Filters
    serviceProvider.AddSingleton<FilterFactory>();
    serviceProvider.AddSingleton<IFilter, AllFilter>();
    serviceProvider.AddSingleton<IFilter, WeekFilter>();
    serviceProvider.AddSingleton<IFilter, YesterdayFilter>();
    serviceProvider.AddSingleton<IFilter, TodayFilter>();

    var services = serviceProvider.BuildServiceProvider();
    var commandProcessor = services.GetRequiredService<CommandProcessor>();
    commandProcessor.Execute();

}
catch (InvalidOperationException ie) when (ie.Message.StartsWith("No service for type 'clk.Commands.ICommand'"))
{
    Console.WriteLine($"Unknown command {args.FirstOrDefault()}");
}
catch (Exception e)
{
    Console.WriteLine($"An error occurred: {e.Message}");
}