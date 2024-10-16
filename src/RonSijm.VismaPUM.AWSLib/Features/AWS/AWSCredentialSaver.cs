using System.Globalization;
using RonSijm.VismaPUM.AWSLib.Features.INI;

namespace RonSijm.VismaPUM.AWSLib.Features.AWS;

public static class AWSCredentialSaver
{

    public static async Task SaveAwsCredentials(string profile, string accessKeyId, string secretAccessKey, string sessionToken, string credentialsPath, string configPath, string region)
    {
        var credentialsFile = INIFileLoader.LoadIniFile(credentialsPath);

        if (!credentialsFile.TryGetValue(profile, out var credentials))
        {
            credentials = new Dictionary<string, string>();
            credentialsFile[profile] = credentials;
        }

        credentials["aws_access_key_id"] = accessKeyId;
        credentials["aws_secret_access_key"] = secretAccessKey;
        credentials["aws_session_token"] = sessionToken;
        credentials["meta_created"] = DateTime.Now.ToString(CultureInfo.InvariantCulture);

        await INIFileSaver.SaveIniFile(credentialsPath, credentialsFile);

        var profileKeyName = $"profile {profile}";

        // Read existing config or create a new one if the file doesn't exist
        var configFile = INIFileLoader.LoadIniFile(configPath);
        var configSection = profile == "default" ? profile : $"profile {profile}";
        if (!configFile.TryGetValue(configSection, out var value))
        {
            value = new Dictionary<string, string>();
            configFile[configSection] = value;
        }

        value["region"] = region;
        value["output"] = "json";

        await INIFileSaver.SaveIniFile(configPath, configFile);

        Console.WriteLine($"\nCredentials stored under profile [{profile}] in {credentialsPath}");
        Console.WriteLine($"Configuration stored under section [{configSection}] in {configPath}");
    }
}