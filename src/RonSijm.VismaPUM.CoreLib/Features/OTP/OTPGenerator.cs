using OtpNet;

namespace RonSijm.VismaPUM.CoreLib.Features.OTP;

public static class OTPGenerator
{
    public static string Generate(string secretkey)
    {
        var secretKeyBytes = Base32Encoding.ToBytes(secretkey);
        var totp = new Totp(secretKeyBytes);

        var result = totp.ComputeTotp();

        return result;
    }
}