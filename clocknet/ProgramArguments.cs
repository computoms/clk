

public record ProgramArguments(string[] Args);

public record Option(string Long, string Short);

public static class Args
{
        // Filters
        public readonly static Option All = new("all", "a");
        public readonly static Option Week = new("week", "w");
        public readonly static Option Yesterday = new("yesterday", "y");
        // Reports
        public readonly static Option WorkTimes = new("worktimes", "w");
        public readonly static Option BarGraphs = new("bar", "b");
        public readonly static Option Details = new("details", "d");
        // Others
        public readonly static Option At = new("at", string.Empty);

}