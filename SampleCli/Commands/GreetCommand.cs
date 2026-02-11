using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SampleCli.Extensions;

namespace SampleCli.Commands;

public sealed class GreetCommand : BaseCommand<GreetCommand>
{
    private readonly Argument<string> _firstNameArg = new("f")
    {
        Description = "First name"
    };
    private readonly Argument<string> _lastNameArg = new("l")
    {
        Description = "Last name"
    };

    public GreetCommand() : base("greet", "Greet a person")
    {
        Arguments.Add(_firstNameArg);
        Arguments.Add(_lastNameArg);
    }

    protected override void ConfigureServices(IServiceCollection services)
        => services.AddHttpClient();

    protected override Func<ParseResult, CancellationToken, ILogger<GreetCommand>, Task<int>> ConfigureAction
        => async (result, cancellationToken, logger) =>
        {
            var serviceScopeFactory = ServiceProvider.GetService<IServiceScopeFactory>();
            var httpClientFactory = ServiceProvider.GetService<IHttpClientFactory>();
            var hostEnvironment = ServiceProvider.GetRequiredService<IHostEnvironment>();

            if (Debug)
            {
                logger.LogInformation("Debug mode is enabled.");
            }

            await Output.WriteLineAsync("Waiting for 1 second...");
            await Task.Delay(1000, cancellationToken);

            await Output.WriteLineAsync(serviceScopeFactory is not null
                ? "IServiceScopeFactory resolved successfully."
                : "Failed to resolve IServiceScopeFactory.");

            await Output.WriteLineAsync(httpClientFactory is not null
                ? "IHttpClientFactory resolved successfully."
                : "Failed to resolve IHttpClientFactory.");

            var firstName = result.GetRequiredValue(_firstNameArg);
            var lastName = result.GetRequiredValue(_lastNameArg);

            var profile = result.AppConfiguration.GetValue<string>("AWS:Profile");
            await Output.WriteLineAsync($"AWS Profile from configuration: {profile}");

            await Output.WriteLineAsync($"Hello, {firstName} {lastName}! {hostEnvironment.EnvironmentName}");
            return 0;
        };
}