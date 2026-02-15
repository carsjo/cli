using System.CommandLine;
using System.Text.Json;
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
    protected bool IsDebugMode { get; private set; }
    
    /// <summary>
    /// Set before executing <see cref="ConfigureAction"/>
    /// </summary>
    protected bool ShouldDryRun { get; private set; }
    
    /// <summary>
    /// Set before executing <see cref="ConfigureAction"/>
    /// </summary>
    protected bool ShouldPrettyPrint  { get; private set; }
    
    /// <summary>
    /// Provides access to registered services. Available in <see cref="ConfigureAction"/>
    /// </summary>
    protected IServiceLocator ServiceProvider { get; private set; }
    
    /// <summary>
    /// Set before executing <see cref="ConfigureAction"/>
    /// </summary>
    protected ILogger<TCategoryName> Logger { get; private set; }
    
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
        JsonExtensions.DefaultOptions.WriteIndented = prettyPrint;
        await Output.WriteLineAsync(JsonSerializer.Serialize(obj, JsonExtensions.DefaultOptions));
        JsonExtensions.DefaultOptions.WriteIndented = false;
    }
    
    /// <inheritdoc/>
    protected BaseCommand(string name, string? description = null) : base(name, description)
    {
        ServiceProvider = null!;
        Logger = null!;
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
            
            ShouldDryRun = result.GetRequiredValue(Constants.Options.DryRun);
            ShouldPrettyPrint = result.GetRequiredValue(Constants.Options.PrettyPrint);
            IsDebugMode = result.GetRequiredValue(Constants.Options.Debug);

            ServiceProvider = result.ServiceProvider;
            Logger = result.ServiceProvider.GetRequiredService<ILogger<TCategoryName>>();
            Output = result.Output;
            Error = result.Error;

            return await ConfigureAction(result, cancellationToken);
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
    protected abstract Func<ParseResult, CancellationToken, Task<int>> ConfigureAction { get; }
}