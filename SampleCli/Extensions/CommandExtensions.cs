using System.ComponentModel;
using SampleCli.Commands;

namespace SampleCli.Extensions;

public static class CommandExtensions
{
    extension<TCommand>(TCommand command) where TCommand : BaseCommand<TCommand>
    {
        /// <summary>
        /// Mainly for testing purposes: Invokes the command asynchronously and captures the output and error strings.
        /// </summary>
        /// <param name="args">String arguments to the command.</param>
        /// <returns>The resulting <see cref="Task{InvokeResult}"/></returns>
        public async Task<InvokeResult> InvokeAsync(string[] args)
        {
            await using var outputWriter = new StringWriter();
            await using var errorWriter = new StringWriter();
            var result = await command.Parse(args).InvokeAsync(outputWriter, errorWriter);
            return new InvokeResult(result, outputWriter.ToString(), errorWriter.ToString());
        }
    }

    public sealed record InvokeResult(
        [property: Description("The exit code resulting from the command invocation.")]
        int Result, 
        [property: Description("The captured output string from the command invocation.")]
        string Output,
        [property: Description("The captured error string from the command invocation.")]
        string Error);
}