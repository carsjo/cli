using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SampleCli.Extensions;

namespace SampleCli.Commands;

public abstract class BaseCommand<TCategoryName> : Command where TCategoryName : notnull
{
    /// <summary>
    /// Set before executing <see cref="ConfigureAction"/>
    /// </summary>
    protected bool PrettyPrint { get; private set; }
    
    /// <summary>
    /// Set before executing <see cref="ConfigureAction"/>
    /// </summary>
    protected bool DryRun { get; private set; }
    
    /// <summary>
    /// Set before executing <see cref="ConfigureAction"/>
    /// </summary>
    protected bool Debug { get; private set; }
    
    /// <summary>
    /// Set before executing <see cref="ConfigureAction"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; private set; }
    
    /// <summary>
    /// Set before executing <see cref="ConfigureAction"/>
    /// </summary>
    protected TextWriter Output { get; private set; }
    
    /// <summary>
    /// Set before executing <see cref="ConfigureAction"/>
    /// </summary>
    protected TextWriter Error { get; private set; }
    
    /// <summary>
    /// Writes the given object as JSON to the output writer. Available in <see cref="ConfigureAction"/>
    /// </summary>
    /// <param name="obj">Any object</param>
    /// <param name="prettyPrint">Write as indented or not</param>
    protected async Task WriteJsonAsync(object? obj, bool prettyPrint = false)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = prettyPrint,
            Converters = { new JsonStringEnumConverter() }
        };
        await Output.WriteLineAsync(JsonSerializer.Serialize(obj, options));
    }
    
    /// <inheritdoc/>
    protected BaseCommand(string name, string? description = null) : base(name, description)
    {
        ServiceProvider = null!;
        Output = TextWriter.Null;
        Error = TextWriter.Null;
        
        Options.Add(Constants.Options.Environment);
        Options.Add(Constants.Options.PrettyPrint);
        Options.Add(Constants.Options.Debug);
        Options.Add(Constants.Options.DryRun);
        
        SetAction(async (result, cancellationToken) =>
        {
            ConfigureServices(result.AppServices, result.AppConfiguration, result.HostEnvironment);
            ConfigureServices(result.AppServices, result.AppConfiguration);
            ConfigureServices(result.AppServices);

            PrettyPrint = result.GetRequiredValue(Constants.Options.PrettyPrint);
            DryRun = result.GetRequiredValue(Constants.Options.DryRun);
            Debug = result.GetRequiredValue(Constants.Options.Debug);
            
            ServiceProvider = result.ServiceProvider;
            Output = result.Output;
            Error = result.Error;

            return await ConfigureAction(result, cancellationToken, ServiceProvider.GetRequiredService<ILogger<TCategoryName>>());
        });
    }
    
    /// <summary>
    /// Register services to the DI container.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    protected virtual void ConfigureServices(
        IServiceCollection services) { }
    
    /// <summary>
    /// Register services to the DI container.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="configuration"><see cref="IConfiguration"/></param>
    protected virtual void ConfigureServices(
        IServiceCollection services, 
        IConfiguration configuration) { }
    
    /// <summary>
    /// Register services to the DI container.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="configuration"><see cref="IConfiguration"/></param>
    /// <param name="hostEnvironment"><see cref="IHostEnvironment"/></param>
    protected virtual void ConfigureServices(
        IServiceCollection services, 
        IConfiguration configuration, 
        IHostEnvironment hostEnvironment) { }

    /// <summary>
    /// Configure the action to be executed when the command is invoked.
    /// </summary>
    protected abstract Func<ParseResult, CancellationToken, ILogger<TCategoryName>, Task<int>> ConfigureAction { get; }
}