using System.Security.Cryptography;
using System.Text;

namespace RonSijm.VismaPUM.CoreLib.Features.Encryption;

public static class ObsidianEncryptV0
{
    private static byte[] IV { get; } = [196, 190, 240, 190, 188, 78, 41, 132, 15, 220, 84, 211];
    private static int TagLength => 16;

    public static ObsidianEncryptV0Result Encrypt(string text, string password)
    {
        var key = DeriveKey(password);
        var plaintextBytes = Encoding.UTF8.GetBytes(text);

        using var aesGcm = new AesGcm(key, TagLength);

        var cipherText = new byte[plaintextBytes.Length];
        var tag = new byte[TagLength];
        aesGcm.Encrypt(IV, plaintextBytes, cipherText, tag);

        var result = new byte[cipherText.Length + tag.Length];
        Buffer.BlockCopy(cipherText, 0, result, 0, cipherText.Length);
        Buffer.BlockCopy(tag, 0, result, cipherText.Length, tag.Length);

        var resultEncoded = Convert.ToBase64String(result);

        return new ObsidianEncryptV0Result { Value = resultEncoded };
    }

    public static string Decrypt(string base64Encoded, string password)
    {
        try
        {
            var key = DeriveKey(password);
            var cipherWithTag = Convert.FromBase64String(base64Encoded);
            var cipherText = new byte[cipherWithTag.Length - TagLength];
            var tag = new byte[TagLength];
            Buffer.BlockCopy(cipherWithTag, 0, cipherText, 0, cipherText.Length);
            Buffer.BlockCopy(cipherWithTag, cipherText.Length, tag, 0, tag.Length);

            using var aesGcm = new AesGcm(key, TagLength);

            var decryptedBytes = new byte[cipherText.Length];
            aesGcm.Decrypt(IV, cipherText, tag, decryptedBytes);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }

    private static byte[] DeriveKey(string password)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        return SHA256.HashData(passwordBytes);
    }
}