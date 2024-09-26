namespace RonSijm.VismaPUM.CLI.Features.CLI;

// ReSharper disable UnusedAutoPropertyAccessor.Global - Justification: Set by <see cref="FluentCommandLineParserFactory"/>
public class ApplicationArgumentModel
{
    public string Duration { get; set; }
    public string Role { get; set; }
    public string Profile { get; set; }

    public string Account { get; set; }
    public string Region { get; set; }
    public string OpAccount { get; set; }
    public string OpItem { get; set; }

    public bool? Save { get; set; }
    public string Load { get; set; }
    public string Password { get; set; }

    public string AccountUsername { get; set; }
    public string AccountPassword { get; set; }
    public string OtpSecret { get; set; }
    public string RoleArn { get; set; }
    public string PrincipalArn { get; set; }
}