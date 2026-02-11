using System.Text.Json;
using System.Text.Json.Serialization;

namespace SampleCli.Extensions;

public static class JsonExtensions
{
    private static readonly JsonSerializerOptions DefaultOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
    
    extension(string json)
    {
        public T FromJson<T>(JsonSerializerOptions? options = null) where T : class, new()
            => JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions) ??
               throw new InvalidOperationException("Deserialization resulted in null.");
    }

    extension(JsonElement element)
    {
        public T ConvertTo<T>(JsonSerializerOptions? options = null) where T : class, new()
            => element.Deserialize<T>(options ?? DefaultOptions) ??
               throw new InvalidOperationException("Deserialization resulted in null.");
    }

    extension<T>(T json) where T : class, new()
    {
        public string ToJson(JsonSerializerOptions? options = null)
            => JsonSerializer.Serialize(json, options ?? DefaultOptions);
    }
}