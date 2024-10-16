using Fclp;

namespace RonSijm.VismaPUM.CLI.Features.CLI;

public static class FluentCommandLineParserFactory
{
    public static FluentCommandLineParser<ApplicationArgumentModel> CreateParser()
    {
        var parser = new FluentCommandLineParser<ApplicationArgumentModel>();

        parser.Setup(arg => arg.Load)
            .As('l', "load")
            .WithDescription("Load stored credentials for a specific profile.");

        parser.Setup(arg => arg.Save)
            .As('s', "save")
            .SetDefault(null)
            .WithDescription("Load stored credentials for a specific profile.");

        parser.Setup(arg => arg.Password)
            .As('w', "Password")
            .SetDefault(null)
            .WithDescription("Password for loading or saving credentials.");

        parser.Setup(arg => arg.OtpSecret)
            .As('o', "OtpKey")
            .SetDefault(null)
            .WithDescription("OTP Secret Key for creating 2FA tokens.")
            ;

        parser.Setup(arg => arg.Duration)
            .As('d', "duration")
            .SetDefault("1")
            .WithDescription("Token duration in hours");

        parser.Setup(arg => arg.Role)
            .As("role")
            .WithDescription("Role name");

        parser.Setup(arg => arg.Profile)
            .As('p', "profile")
            .SetDefault("default")
            .WithDescription("Store credentials for a non-default AWS profile");

        parser.Setup(arg => arg.Account)
            .As('a', "account")
            .WithDescription("Filter roles for the given AWS account");

        parser.Setup(arg => arg.Region)
            .As('r', "region")
            .WithDescription("Configure profile for the specified AWS region");

        parser.Setup(arg => arg.OpAccount)
            .As("op-account")
            .SetDefault(Environment.GetEnvironmentVariable("PUM_OP_ACCOUNT") ?? "visma")
            .WithDescription("Name of the 1Password account");

        parser.Setup(arg => arg.OpItem)
            .As("op-item")
            .SetDefault(Environment.GetEnvironmentVariable("PUM_OP_ITEM_NAME") ?? "Federation ADM")
            .WithDescription("Name of the 1Password item");

        return parser;
    }
}