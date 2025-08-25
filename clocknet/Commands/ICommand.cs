using clocknet;

public interface ICommand
{
    static string Name { get; } = string.Empty;
    void Execute();
}

public abstract class BaseCommand : ICommand
{
    protected readonly ProgramArguments pArgs;

    protected BaseCommand(ProgramArguments pArgs)
    {
        this.pArgs = pArgs;
    }

    public abstract void Execute();

    protected bool HasOption(Option opt)
    {
        return pArgs.Args.Contains($"--{opt.Long}")
            || (!string.IsNullOrEmpty(opt.Short) && pArgs.Args.Any(x => x.StartsWith("-") && !x.StartsWith("--") && x.Contains(opt.Short)));
    }
}