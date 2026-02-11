using System.CommandLine;
using System.CommandLine.Help;
using SampleCli.Actions;
using SampleCli.Extensions;

namespace SampleCli.Commands;

public sealed class RedMarbleRootCommand : RootCommand
{
    private RedMarbleRootCommand() : base("Example for a basic command line tool")
    {
        Subcommands.Add(new GreetCommand());
        Subcommands.Add(new OpenAiCommand());
        Subcommands.Add(new InJsonCommand());
        
        Options.Add(Constants.Options.Environment);
        Options.Add(Constants.Options.PrettyPrint);
        Options.Add(Constants.Options.Debug);
        Options.Add(Constants.Options.DryRun);

        if (Options.FirstOrDefault(x => x is HelpOption) is HelpOption { Action: HelpAction helpAction } helpOption)
        {
            helpOption.Action = new SpectreHelpAction(helpAction);
        }
    }

    public static Task<int> InvokeAsync(string[] args, TextWriter? output = null, TextWriter? error = null)
        => new RedMarbleRootCommand().Parse(args).InvokeAsync(output, error);
}