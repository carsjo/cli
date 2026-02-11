using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace SampleCli;

public sealed class LogScope : IDisposable
{
    private readonly Dictionary<string, object> _properties = new(StringComparer.OrdinalIgnoreCase);
    private readonly IDisposable? _innerScope;

    public LogScope(ILogger logger) 
        => _innerScope = logger.BeginScope(_properties);

    public object this[string name]
    {
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(value);
            _properties[name] = value;
        }
    }

    public LogScope Set(string name, object value)
    {
        this[name] = value;
        return this;
    }

    [ExcludeFromCodeCoverage(Justification = "Dispose pattern")]
    public void Dispose() 
        => _innerScope?.Dispose();
}