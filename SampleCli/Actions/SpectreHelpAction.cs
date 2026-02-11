using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using Spectre.Console;

namespace SampleCli.Actions;

internal sealed class SpectreHelpAction(HelpAction action) : SynchronousCommandLineAction
{
    public override int Invoke(ParseResult parseResult)
    {
        AnsiConsole.Write(new FigletText(
            parseResult.RootCommandResult.Command.Description ??
            "No description provided for the option!"));
        return action.Invoke(parseResult);
    }
}