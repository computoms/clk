using clk.Domain;
using Task = clk.Domain.Task;

namespace clk.Commands;

public class ListCommand(IRecordRepository recordRepository, IDisplay display, ProgramArguments pArgs) : ICommand
{
    public static string Name { get; } = "list";

    public void Execute()
    {
        var tasks = recordRepository.GetAll();
        if (pArgs.HasOption(Args.Path))
        {
            var paths = tasks.Select(x => GetPath(x)).Order().Distinct();
            display.Print(
                paths.Select(x => x.AsLine()).ToList()
                );

            // TODO display tree structure
            //display.Print(" ---- ".AsLine());
    
            //var resultToDisplay = new List<FormattedLine>();
            //var currentPath = paths.FirstOrDefault();
            //resultToDisplay.Add(currentPath.AsLine());
            //foreach (var p in paths.Skip(1))
            //{
            //    var level = FindLevel(p, currentPath);
            //    Console.WriteLine("Level: " + level);
            //    var remainder = RemoveFirstLevels(p, level);
            //    Console.WriteLine("Remainder: " + remainder);
            //    var prepend = level == 0 ? "" : Enumerable.Range(0, level).Select(x => "  ").Aggregate((r, t) => $"{r}{t}");
            //    resultToDisplay.Add((prepend + remainder).AsLine());
            //    currentPath = p;
            //}
            //display.Print(
            //    resultToDisplay
            //    );
            return;
        }

        display.Print(tasks
                .Select(x => x.Line.AsLine()).Distinct().ToList());
    }

    private string GetPath(TaskLine t) => t.Path.Aggregate("", (r, t) => $"{r}/{t}");

    // private int FindLevel(string path, string previousPath)
    // {
    //     var pathEntries = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
    //     var previousPathEntries = previousPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

    //     var level = 0;
    //     for (; level < pathEntries.Length && level < previousPathEntries.Length; level++)
    //     {
    //         if (pathEntries[level] != previousPathEntries[level])
    //             return level;
    //     }

    //     return level;
    // }

    // private string RemoveFirstLevels(string path, int level) => path.Split('/', StringSplitOptions.RemoveEmptyEntries)
    //     .Skip(level).Aggregate((r, t) => $"{r}/{t}");
}