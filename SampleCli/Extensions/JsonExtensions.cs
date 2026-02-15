using System.Text.Json;
using System.Text.Json.Serialization;

namespace SampleCli.Extensions;

public static class JsonExtensions
{
    public static readonly JsonSerializerOptions DefaultOptions = new(JsonSerializerOptions.Web)
    {
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = false
    };
    
    extension(string json)
    {
        public T FromJson<T>(JsonSerializerOptions? options = null) where T : class, new()
            => JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions) ??
               throw new InvalidOperationException("Deserialization resulted in null.");
    }

    extension<T>(T json) where T : class, new()
    {
        public string ToJson(JsonSerializerOptions? options = null)
            => JsonSerializer.Serialize(json, options ?? DefaultOptions);
    }
}