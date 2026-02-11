using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace SampleCli;

internal static class Constants
{
    internal static class Arguments
    {
        public static readonly Argument<JsonElement> Body = new("body")
        {
            Description = "Json content. Use '-' to read from stdin.",
            Arity = ArgumentArity.ExactlyOne,
            CustomParser = result =>
            {
                try
                {
                    var jsonValue = result.Tokens.Count > 0 ? result.Tokens[0].Value : null;
                    if (string.IsNullOrWhiteSpace(jsonValue))
                    {
                        result.AddError("Body argument is required.");
                        return default;
                    }
                    using var document = JsonDocument.Parse(jsonValue!);
                    return document.RootElement.Clone();
                }
                catch (Exception e) when (e is ArgumentException or JsonException)
                {
                    result.AddError(e.Message);
                    return default;
                }
            }
        };
    }
    
    internal static class Options
    {
        public static readonly Option<string> Environment = new("--environment", "-e")
        {
            Description = "Environment to run the application in.",
            DefaultValueFactory = _ => Environments.Development,
            Recursive = true
        };
        
        public static readonly Option<bool> PrettyPrint = new("--pretty", "-p")
        {
            Description = "Pretty print the output.",
            DefaultValueFactory = _ => false,
            Validators =
            {
                result => ValidateBoolean(result, "PrettyPrint must be 'true' or 'false'.")
            },
            Recursive = true
        };
        
        public static readonly Option<bool> Debug = new("--debug", "-dbg")
        {
            Description = "Print verbose error debugging output",
            DefaultValueFactory =  _ => false,
            Validators =
            {
                result => ValidateBoolean(result, "Debug must be 'true' or 'false'.")
            },
            Recursive = true
        };
        
        public static readonly Option<bool> DryRun = new("--dry-run", "-dry")
        {
            Description = "Display results without performing action",
            DefaultValueFactory =  _ => false,
            Validators =
            {
                result => ValidateBoolean(result, "DryRun must be 'true' or 'false'.")
            },
            Recursive = true
        };

        private static void ValidateBoolean(OptionResult result, string errorMessage)
        {
            var booleanValue = result.Tokens.Count > 0 ? result.Tokens[0].Value : null;
            if (!string.IsNullOrWhiteSpace(booleanValue) && !bool.TryParse(booleanValue, out _))
            {
                result.AddError(errorMessage);
            }
        }
    }
}