using clk.Commands;

namespace clk;

public class CommandProcessor(ICommand command)
{
    public void Execute()
    {
        command.Execute();
    }
}

