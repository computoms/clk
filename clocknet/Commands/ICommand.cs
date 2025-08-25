namespace clocknet.Commands;

public interface ICommand
{
    static string Name { get; } = string.Empty;
    void Execute();
}