using System.Text.Json;

namespace RonSijm.VismaPUM.CoreLib.Features.Serialization;

public static class JsonSerializerHelper
{
    public static T Deserialize<T>(this string json)
    {
        var result = JsonSerializer.Deserialize<T>(json, ActualJsonSerializerSettingsContainer.Settings);

        return result;
    }

    public static string Serialize<T>(this T input, bool indented = false)
    {
        if (indented)
        {
            var result = JsonSerializer.Serialize(input, ActualJsonSerializerSettingsContainer.SettingsIndented);

            return result;
        }
        else
        {
            var result = JsonSerializer.Serialize(input, ActualJsonSerializerSettingsContainer.Settings);

            return result;
        }
    }
}