using System.CommandLine;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;

namespace SampleCli.Extensions;

internal static class ParseResultExtensions
{
    extension(ParseResult parseResult)
    {
        private InvokeConfiguration InvokeConfiguration =>
            parseResult.InvocationConfiguration as InvokeConfiguration
            ?? throw new InvalidOperationException("InvocationConfiguration is not of expected type.");

        /// <summary>
        /// To use this, inherit your command from <see cref="Commands.BaseCommand{T}"/>
        /// </summary>
        public IServiceProvider ServiceProvider => parseResult.InvokeConfiguration.ServiceProvider;

        /// <summary>
        /// To use this, inherit your command from <see cref="Commands.BaseCommand{T}"/>
        /// </summary>
        public IServiceCollection AppServices => parseResult.InvokeConfiguration.AppServices;

        /// <summary>
        /// To use this, inherit your command from <see cref="Commands.BaseCommand{T}"/>
        /// </summary>
        public IConfiguration AppConfiguration => parseResult.InvokeConfiguration.AppConfiguration;
        
        /// <summary>
        /// To use this, inherit your command from <see cref="Commands.BaseCommand{T}"/>
        /// </summary>
        public IHostEnvironment HostEnvironment => parseResult.InvokeConfiguration.HostEnvironment;

        /// <summary>
        /// The output writer configured for this invocation.
        /// </summary>
        public TextWriter Output => parseResult.InvokeConfiguration.Output;

        /// <summary>
        /// The error writer configured for this invocation.
        /// </summary>
        public TextWriter Error => parseResult.InvokeConfiguration.Error;

        /// <summary>
        /// Invoke the command represented by this <see cref="ParseResult"/>.
        /// </summary>
        /// <param name="output">The output writer for this invocation, defaults to Console.Out.</param>
        /// <param name="error">The error writer for this invocation, defaults to Console.Error.</param>
        /// <returns>The resulting <see cref="Task{Int32}"/></returns>
        public async Task<int> InvokeAsync(TextWriter? output = null, TextWriter? error = null)
        {
            await using var configuration = new InvokeConfiguration(parseResult);
            configuration.ProcessTerminationTimeout = TimeSpan.FromSeconds(5);
            configuration.Output = output ?? Console.Out;
            configuration.Error = error ?? Console.Error;
            return await parseResult.InvokeAsync(configuration);
        }

        public T GetValueOrDefault<T>(Option<T> option, T fallbackValue)
            => option.HasDefaultValue && option.GetDefaultValue() is T defaultValue
                ? parseResult.GetValue(option) ?? defaultValue
                : parseResult.GetValue(option) ?? fallbackValue;

        public T GetValueOrDefault<T>(Argument<T> argument, T fallbackValue)
            => argument.HasDefaultValue && argument.GetDefaultValue() is T defaultValue
                ? parseResult.GetValue(argument) ?? defaultValue
                : parseResult.GetValue(argument) ?? fallbackValue;
    }
    
    private sealed class InvokeConfiguration : InvocationConfiguration, IAsyncDisposable
    {
        private readonly ServiceCollection _services = [];
        private ServiceProvider? _serviceProvider; 
        
        public InvokeConfiguration(ParseResult result)
        {
            var environment = result.GetValueOrDefault(Constants.Options.Environment, null!);

            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddUserSecrets(typeof(Program).Assembly, true, true);

            if (!string.IsNullOrWhiteSpace(environment))
            {
                configurationBuilder = configurationBuilder
                    .AddJsonFile($"appsettings.{environment}.json", true, true);
            }
            
            var configuration = configurationBuilder.Build();

            var hostEnvironment = new HostingEnvironment
            {
                EnvironmentName = environment,
                ApplicationName = Assembly.GetExecutingAssembly().GetName().Name!,
                ContentRootPath = Directory.GetCurrentDirectory()
            };
            
            AppServices = _services
                .AddLogging(loggingBuilder => loggingBuilder
                    .ClearProviders()
                    .AddConfiguration(configuration)
                    .AddConsole())
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton<IHostEnvironment>(hostEnvironment);
            AppConfiguration = configuration;
            HostEnvironment = hostEnvironment;
        }
        
        public IServiceCollection AppServices { get; }
        
        public IConfiguration AppConfiguration { get; }
        
        public IHostEnvironment HostEnvironment { get; }

        public IServiceProvider ServiceProvider => _serviceProvider ??= _services.BuildServiceProvider();

        public async ValueTask DisposeAsync()
            => await (_serviceProvider?.DisposeAsync() ?? new ValueTask(Task.CompletedTask));
    }
}