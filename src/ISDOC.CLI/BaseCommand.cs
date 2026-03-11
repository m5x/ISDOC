using System.CommandLine;

namespace ISDOC.CLI;

public abstract class BaseCommand
{
  protected abstract Command CommandLineCommand { get; }

  public static implicit operator Command( BaseCommand command )
  {
    return command.CommandLineCommand;
  }
}
