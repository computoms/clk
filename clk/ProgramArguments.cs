using System.Diagnostics;
using System.Globalization;

namespace clk;

public record ProgramArguments
{
    public ProgramArguments(string[] rawArgs)
    {
        ParseArguments(rawArgs);
    }

    public List<SwitchOption> Switches { get; private set; } = [];
    public List<ValueOption> ValueOptions { get; private set; } = [];

    public string Title { get; private set; } = string.Empty;
    public DateTime Time { get; private set; } = DateTime.Now;
    public string Command { get; private set; } = string.Empty;

    public bool HasOption(string name) => Switches.Any(s => s.Name == name) || ValueOptions.Any(v => v.Name == name);

    public string GetValue(string name) => ValueOptions.FirstOrDefault(v => v.Name == name)?.Value ?? string.Empty;

    private void ParseArguments(string[] rawArgs)
    {
        Command = rawArgs.FirstOrDefault() ?? string.Empty;

        for (int i = 1; i < rawArgs.Length; i++)
        {
            if (GetSwitches(rawArgs[i]) is var switches && switches.Any())
            {
                Switches.AddRange(switches);
            }
            else if (GetValueOption(rawArgs[i]) is var valOpt && valOpt != null)
            {
                if (i < rawArgs.Length - 1)
                {
                    valOpt = valOpt with { Value = rawArgs[i + 1] };
                    ++i;
                }
                ValueOptions.Add(valOpt);
            }
            else
            {
                Title += rawArgs[i] + " ";
            }
        }

        Title = Title.Trim();

        if (HasOption(Args.At))
        {
            var atValue = GetValue(Args.At);
            var convertedTime = DateTime.TryParseExact(
                        SanitizeInput(atValue), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                    ? date : DateTime.MinValue;
            var now = DateTime.Now;
            Time = new DateTime(now.Year, now.Month, now.Day, convertedTime.Hour, convertedTime.Minute, 0);
        }
    }

    private static string SanitizeInput(string line) => line.Replace("\r", "").Replace("\n", "").Trim();

    private static IEnumerable<SwitchOption> GetSwitches(string arg)
    {
        var fullName = Args.Switches.FirstOrDefault(v => $"--{v.Long}" == arg);
        if (fullName != null)
            return [fullName];

        return IsShortArg(arg)
            ? Args.Switches.Where(v => arg.Contains(v.Short)) 
            : [];
    }

    private static ValueOption? GetValueOption(string arg) => Args.ValueOptions.FirstOrDefault(v => $"--{v.Long}" == arg);

    private static bool IsShortArg(string arg) => arg.Length >= 2 && arg.StartsWith('-') && arg[1] != '-';
}

/// <summary>
/// Switch option
/// </summary>
/// <param name="Long">Long description, such as all, which becomes --all</param>
/// <param name="Short">One letter description, such as a, which becomes -a (can be combined)</param>
public record SwitchOption(string Name, string Long, string Short);

/// <summary>
/// Option with value
/// </summary>
/// <param name="Long">Option name, such as group-by which becomes --group-by in command line args</param>
/// <param name="Value">Option value specified by user</param>
public record ValueOption(string Name, string Long, string Value);

public static class Args
{
    // Filters
    public const string All = "All";
    public const string Week = "Week";
    public const string Yesterday = "Yesterday";

    public const string Last = "Last";
    // Reports
    public const string Report = "Report";
    public const string Timesheet = "timesheet";
    public const string BarGraphs = "bars";
    public const string Details = "details";
    public const string Chrono = "chrono";
    public const string Json = "json";
    // Others
    public const string GroupBy = "GroupBy";
    public const string GroupByPath = "GroupByPath";
    public const string Tags = "Tags";
    public const string Path = "Path";
    public const string At = "At";
    public const string Settings = "Settings";

    public static List<SwitchOption> Switches { get; } = new List<SwitchOption>()
    {
        // Filters
        new (All, "all", "a"),
        new (Week, "week", "w"),
        new (Yesterday, "yesterday", "y"),
        // Shortcuts
        new (Chrono, "chrono", "c"),
        new (Json, "json", "j"),
        new (Details, "details", "d"),
        new (BarGraphs, "bars", "b"),
        new (Timesheet, "timesheet", "t"),
        new (GroupByPath, "group-by-path", "p")
    };

    public static List<ValueOption> ValueOptions { get; } = new List<ValueOption>()
    {
        // Reports (WorkTimes, BarGraphs, Details)
        new (Report, "report", string.Empty),
        // Filters
        new(GroupBy, "group-by", string.Empty),
        new(Tags, "tags", string.Empty),
        new(Path, "path", string.Empty),
        new (Last, "last", string.Empty),
        // Others
        new(At, "at", string.Empty),
        new(Settings, "settings", string.Empty)
    };
}