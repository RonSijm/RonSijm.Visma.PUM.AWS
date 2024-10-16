using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using RonSijm.VismaPUM.AWSLib.Features.AWS;
using RonSijm.VismaPUM.AWSLib.Features.RoleSelection;
using RonSijm.VismaPUM.CLI.Features.CLI;
using RonSijm.VismaPUM.CLI.Features.SAML;
using RonSijm.VismaPUM.CoreLib.Features.Encryption;
using RonSijm.VismaPUM.CoreLib.Features.FileExtension;
using RonSijm.VismaPUM.CoreLib.Features.OTP;
using RonSijm.VismaPUM.CoreLib.Features.Serialization;

namespace RonSijm.VismaPUM.CLI;

// ReSharper disable once ClassNeverInstantiated.Global - Justification: Entry point
public class Program
{
    public static async Task Main(string[] args)
    {
        ProgramSettings.ProgramFileExtension.CreateWindowsFileExtensionAssociation("PUM AWS Login", "Tool to automatically login to AWS though PUM.", "-l \"%1\"");
        ProgramSettings.ProgramFileExtension.CreateLinuxFileExtensionAssociation("PUM AWS Login", "Tool to automatically login to AWS though PUM.", "-l \"%1\"");

        var baseDirectory = Directory.GetCurrentDirectory().Replace(@"\bin\Debug\net8.0", string.Empty);
        baseDirectory = baseDirectory.Replace("src\\RonSijm.Visma.PUM.AWS.CLI", string.Empty);

        var parser = FluentCommandLineParserFactory.CreateParser();
        var result = parser.Parse(args);

        if (result.HasErrors)
        {
            Console.WriteLine(result.ErrorText);
            return;
        }

        var arguments = parser.Object;
        var tokenDuration = int.Parse(arguments.Duration) * 60 * 60;
        string samlAssertion;

        string otp;

        if (!string.IsNullOrWhiteSpace(arguments.Load))
        {
            var fileExists = File.Exists(arguments.Load);

            while (!fileExists)
            {
                Console.WriteLine($"Trying to load settings from Profile: '{arguments.Profile}'");
                arguments.Load = Path.Combine(baseDirectory, $"{arguments.Profile}.AWS-Pum-Profile");

                fileExists = File.Exists(arguments.Load);

                if (fileExists)
                {
                    continue;
                }

                Console.WriteLine($"Profile: '{arguments.Profile}' does not exist. Please specify a profile...");
                arguments.Profile = GetInput("Profile: ");
            }

            var encoded = await File.ReadAllTextAsync(arguments.Load);

            if (string.IsNullOrWhiteSpace(arguments.Password))
            {
                arguments.Password = GetInput($"Password to load file '{arguments.Load}': ", true);
            }

            var decoded = ObsidianEncryptV0.Decrypt(encoded, arguments.Password);

            arguments = decoded.Deserialize<ApplicationArgumentModel>();
            arguments.Load = "Loaded";

            tokenDuration = int.Parse(arguments.Duration) * 60 * 60;
            otp = OTPGenerator.Generate(arguments.OtpSecret);
            samlAssertion = await GetSamlAssertionHandler.GetSamlAssertion(arguments.AccountUsername, arguments.AccountPassword, otp);
        }
        else
        {
            // Not implemented.
            const string lastUsedUser = "";

            arguments.AccountUsername = string.IsNullOrWhiteSpace(lastUsedUser) ? GetInput("Privileged user (e.g. adm\\dev_ron): ") : GetInput($"Privileged user (e.g. adm\\dev_ron) [{lastUsedUser}]: ");
            arguments.AccountUsername = string.IsNullOrWhiteSpace(arguments.AccountUsername) ? lastUsedUser : arguments.AccountUsername;
            arguments.AccountPassword = GetInput("Domain password: ", true);

            HandleProfileParam(arguments);
            HandleSaveParams(arguments);

            otp = !string.IsNullOrWhiteSpace(arguments.OtpSecret) ? OTPGenerator.Generate(arguments.OtpSecret) : GetInput("Visma Google Auth 2FA Token: ");

            if (string.IsNullOrWhiteSpace(arguments.Region))
            {
                arguments.Region = GetInput("AWS Region: ");
            }

            samlAssertion = await GetSamlAssertionHandler.GetSamlAssertion(arguments.AccountUsername, arguments.AccountPassword, otp);
            var awsRoles = SamlAssertionRoleParser.ParseRolesFromSamlAssertion(samlAssertion);

            var roleArnAndPrincipalArn = AWSRoleSelector.SelectRole(awsRoles);

            arguments.RoleArn = roleArnAndPrincipalArn.RoleArn;
            arguments.PrincipalArn = roleArnAndPrincipalArn.PrincipalArn;
        }

        var credentialsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aws", "credentials");
        Console.WriteLine($"Warning: This script will overwrite your AWS credentials stored at {credentialsPath}, section [{arguments.Profile}]\n");

        var token = await HandleSignIn(arguments.RoleArn, arguments.PrincipalArn, samlAssertion, tokenDuration);

        var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aws", "config");
        await AWSCredentialSaver.SaveAwsCredentials(arguments.Profile, token.Credentials.AccessKeyId, token.Credentials.SecretAccessKey, token.Credentials.SessionToken, credentialsPath, configPath, arguments.Region);

        if (string.IsNullOrWhiteSpace(arguments.Load) && arguments.Save.HasValue && arguments.Save.Value)
        {
            if (string.IsNullOrWhiteSpace(arguments.Password))
            {
                arguments.Password = GetInput($"Password to save '{arguments.Profile} profile': ", true);
            }

            var toSave = arguments.Serialize();
            var encrypted = ObsidianEncryptV0.Encrypt(toSave, arguments.Password);

            var savePath = Path.Combine(baseDirectory, "Profiles");

            var username = arguments.AccountUsername.Split('@')[0];
            var role = arguments.RoleArn.Split('/')[^1];

            var outputFile = Path.Combine(savePath, $"{arguments.Profile} - {username} - {role}.AWS-Pum-Profile");
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            await File.WriteAllTextAsync(outputFile, encrypted.Value);
        }

        Console.WriteLine("\n----------------------------------------------------------------");
        Console.WriteLine($"Your AWS access key pair has been stored in the AWS configuration file {credentialsPath}");
        Console.WriteLine($"Note that it will expire at {token.Credentials.Expiration}");
        Console.WriteLine("----------------------------------------------------------------\n");

    }

    private static void HandleProfileParam(ApplicationArgumentModel arguments)
    {
        if (arguments.Profile == "default")
        {
            arguments.Profile = GetInput("Profile: (Enter for Default)");
        }

        if (string.IsNullOrWhiteSpace(arguments.Profile))
        {
            arguments.Profile = "default";
        }
    }

    private static void HandleSaveParams(ApplicationArgumentModel arguments)
    {
        arguments.Save ??= bool.Parse(GetInput("Do you want to save input? (type 'true' or 'false', (lol))"));

        if (!arguments.Save.Value)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(arguments.OtpSecret))
        {
            arguments.OtpSecret = GetInput("Otp Secret Key: ", true);
        }
    }

    private static async Task<AssumeRoleWithSAMLResponse> HandleSignIn(string roleArn, string principalArn, string samlAssertion, int tokenDuration)
    {
        var client = new AmazonSecurityTokenServiceClient();
        var token = await client.AssumeRoleWithSAMLAsync(new AssumeRoleWithSAMLRequest
        {
            RoleArn = roleArn,
            PrincipalArn = principalArn,
            SAMLAssertion = samlAssertion,
            DurationSeconds = tokenDuration
        });
        return token;
    }

    private static string GetInput(string prompt, bool isPassword = false)
    {
        Console.Write(prompt);
        return isPassword ? HiddenPasswordCLIHelper.HidePassword() : Console.ReadLine();
    }
}