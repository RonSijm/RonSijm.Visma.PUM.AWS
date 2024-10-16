namespace RonSijm.VismaPUM.CoreLib.Features.Encryption;

public class ObsidianEncryptV0Result
{
    public string Value { get; init; }

    // ReSharper disable once UnusedMember.Global - Justification: Public Library Method
    public string ToObsidianValue(string hint)
    {
        return $"🔐ø 💡{hint}💡{Value} 🔐";
    }
}