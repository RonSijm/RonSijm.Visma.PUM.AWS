namespace RonSijm.VismaPUM.CLI;

public static class ProgramSettings
{
    public const string ProgramFileExtension = ".AWS-Pum-Profile";
    public const string SamlAWSAssertionUrl = $"{SamlBaseAssertionUrl}?loginToRp=urn:amazon:webservices";
    private const string SamlBaseAssertionUrl = "https://federation.visma.com/adfs/ls/idpinitiatedsignon.aspx";
}