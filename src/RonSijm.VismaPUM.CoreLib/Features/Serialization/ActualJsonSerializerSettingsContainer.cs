using System.Text.Json;
using System.Text.Json.Serialization;

namespace RonSijm.VismaPUM.CoreLib.Features.Serialization;

public static class ActualJsonSerializerSettingsContainer
{
    public static JsonSerializerOptions Settings { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public static JsonSerializerOptions SettingsIndented { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        },
        WriteIndented = true
    };
}