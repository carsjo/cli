using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace SampleCli.Commands;

public sealed class OpenAiCommand : BaseCommand<OpenAiCommand>
{
    private readonly Argument<string> _prompt = new("prompt")
    {
        Description = "The prompt to send to the OpenAI model"
    };

    public OpenAiCommand() : base("openai", "Commands for interacting with OpenAI services")
    {
        Arguments.Add(_prompt);
    }

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        => services.AddSingleton(new ChatClient(configuration["OpenAI:Model"], configuration["OpenAI:ApiKey"]));

    protected override Func<ParseResult, CancellationToken, Task<int>> ConfigureAction
        => async (parseResult, cancellationToken) =>
        {
            var client = ServiceProvider.GetRequiredService<ChatClient>();

            var prompt = parseResult.GetRequiredValue(_prompt);

            var result = await client.CompleteChatAsync([
                ChatMessage.CreateSystemMessage("You are a cooking master assistant. " +
                                                "You suggest meals that everyone can enjoy based on a couple of ingredients that the person has in their household. " +
                                                "You can suggest a list of additional ingredients to buy. " +
                                                "You always suggest easy to make meals that take between 60-90 minutes to prepare and cook. " +
                                                "You always provide step-by-step cooking instructions. "),
                ChatMessage.CreateUserMessage(prompt)
            ], null, cancellationToken);

            foreach (var part in result.Value.Content)
            {
                Console.WriteLine(part.Text);
            }

            return 0;
        };
}