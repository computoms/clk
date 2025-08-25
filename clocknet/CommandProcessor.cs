namespace clocknet;

public class CommandProcessor(ICommand command)
{
    public void Execute()
    {
        command.Execute();
    }
}

