using System.Diagnostics;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SampleCli.Commands.Aws;

public abstract class BaseAwsCommand<TCategoryName>(string name, string? description = null)
    : BaseCommand<TCategoryName>(name, description) where TCategoryName : notnull
{
    /// <summary>
    /// Sets up AWS options with SSO credentials.
    /// If you override this method, be sure to call base.ConfigureServices with the same parameters.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="configuration"><see cref="IConfiguration"/></param>
    /// <param name="environment"><see cref="IHostEnvironment"/></param>
    /// <exception cref="InvalidOperationException">Thrown when AWS Profile cannot be found.</exception>
    protected override void ConfigureServices(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var awsOptions = configuration.GetAWSOptions();

        var profileStoreChain = new CredentialProfileStoreChain(new SharedCredentialsFile().ConfigFilePath);
        if (!profileStoreChain.TryGetAWSCredentials(awsOptions.Profile, out var credentials))
        {
            throw new InvalidOperationException($"Unable to find AWS profile {awsOptions.Profile}");
        }

        var ssoCredentials = (SSOAWSCredentials)credentials;
        ssoCredentials.Options.ClientName = environment.ApplicationName;
        ssoCredentials.Options.SsoVerificationCallback = verificationArgs =>
        {
            Console.WriteLine($"Refreshing SSO token. Your code is: {verificationArgs.UserCode}");
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = verificationArgs.VerificationUriComplete,
                UseShellExecute = true
            });
        };

        awsOptions.Credentials = ssoCredentials;
        services.AddDefaultAWSOptions(awsOptions);
    }
}