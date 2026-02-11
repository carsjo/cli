using System.CommandLine;
using Microsoft.Extensions.Logging;

namespace SampleCli.Commands;

public sealed class InJsonCommand : BaseCommand<InJsonCommand>
{
    public InJsonCommand() : base("json", "Reads JSON input from stdin and echoes it back.")
    {
        Arguments.Add(Constants.Arguments.Body);
    }

    protected override Func<ParseResult, CancellationToken, ILogger<InJsonCommand>, Task<int>> ConfigureAction =>
        async (parseResult, cancellationToken, logger) =>
        {
            if (Debug)
            {
                logger.LogInformation("Debug mode is enabled.");
            }
            
            var input = parseResult.GetRequiredValue(Constants.Arguments.Body);
            await Output.WriteLineAsync("Received JSON input:");
            await Output.WriteLineAsync(input.ToString());
            return 0;
        };
}