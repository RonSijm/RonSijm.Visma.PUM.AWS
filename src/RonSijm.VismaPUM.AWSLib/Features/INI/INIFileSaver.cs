namespace RonSijm.VismaPUM.AWSLib.Features.INI;

public static class INIFileSaver
{
    public static async Task SaveIniFile(string path, Dictionary<string, Dictionary<string, string>> data)
    {
        var lines = new List<string>();

        foreach (var section in data)
        {
            lines.Add($"[{section.Key}]");
            lines.AddRange(section.Value.Select(kvp => $"{kvp.Key} = {kvp.Value}"));
            lines.Add("");
        }

        await File.WriteAllLinesAsync(path, lines);
    }
}