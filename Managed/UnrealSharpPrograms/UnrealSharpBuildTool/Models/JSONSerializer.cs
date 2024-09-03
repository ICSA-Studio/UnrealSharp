using System.Text.Json;
using System.Text.Json.Serialization;

namespace UnrealSharpBuildTool.Models
{
    public static class JSONSerializer
    {
        public static JsonSerializerOptions Options { get; } = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            AllowTrailingCommas = true,
            Converters = {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };
        public static string Serialize<T>(params T[] instances)
        {
            return JsonSerializer.Serialize(instances, Options);
        }
        public static T? Deserialize<T>(string json) where T : class
        {
            return JsonSerializer.Deserialize<T>(json, Options);
        }
    }
}
