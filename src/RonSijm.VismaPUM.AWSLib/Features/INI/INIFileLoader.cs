namespace RonSijm.VismaPUM.AWSLib.Features.INI;

public static class INIFileLoader
{
    public static Dictionary<string, Dictionary<string, string>> LoadIniFile(string path)
    {
        var result = new Dictionary<string, Dictionary<string, string>>();
        if (!File.Exists(path))
        {
            return result;
        }

        var lines = File.ReadAllLines(path);
        var currentSection = "default";

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
            {
                continue;
            }

            if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
            {
                currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2).Trim();
                if (!result.ContainsKey(currentSection))
                {
                    result[currentSection] = new Dictionary<string, string>();
                }
            }
            else
            {
                var separatorIndex = trimmedLine.IndexOf('=');
                if (separatorIndex < 0)
                {
                    continue;
                }

                var lineKey = trimmedLine[..separatorIndex].Trim();
                var lineValue = trimmedLine[(separatorIndex + 1)..].Trim();

                if (!result.TryGetValue(currentSection, out var value))
                {
                    value = new Dictionary<string, string>();
                    result[currentSection] = value;
                }

                value[lineKey] = lineValue;
            }
        }

        return result;
    }
}