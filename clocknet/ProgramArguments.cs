using System.Security.Cryptography.X509Certificates;

namespace clocknet;

public record ProgramArguments(string[] Args)
{
    public bool HasOption(Option opt)
    {
        if (Args.Contains($"--{opt.Long}"))
            return true;
        if (string.IsNullOrEmpty(opt.Short))
            return false;

        return Args.Any(x => x.StartsWith("-") && !x.StartsWith("--") && x.Contains(opt.Short));
    }

    public string GetValue(Option opt)
    {
        int index = GetIndex(opt);
        if (index == -1 || index == Args.Length - 1)
            return "";

        return Args[index + 1];
    }

    private int GetIndex(Option opt)
    {
        for (int i = 0; i < Args.Length; ++i)
        {
            if (Args[i] == $"--{opt.Long}")
            {
                return i;
            }
            if (Args[i].StartsWith("-") && !Args[i].StartsWith("--") && Args[i].Contains(opt.Short))
            {
                return i;
            }
        }
        return -1;
    }
}

public record Option(string Long, string Short);

public static class Args
{
    // FilterApartmentStates
    public readonly static Option All = new("all", "a");
    public readonly static Option Week = new("week", "w");
    public readonly static Option Yesterday = new("yesterday", "y");
    // Reports
    public readonly static Option WorkTimes = new("worktimes", "w");
    public readonly static Option BarGraphs = new("bar", "b");
    public readonly static Option Details = new("details", "d");
    // Others
    public readonly static Option At = new("at", string.Empty);
    public readonly static Option Settings = new("settings", string.Empty);
}