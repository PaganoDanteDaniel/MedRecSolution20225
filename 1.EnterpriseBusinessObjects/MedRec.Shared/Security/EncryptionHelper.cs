using System.Security.Cryptography;
using System.Text;

namespace MedRec.Shared.Security;

public static class EncryptionHelper
{
    private const string EncryptedPrefix = "ENC:";

    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] encrypted = ProtectedData.Protect(
            plainBytes,
            null,
            DataProtectionScope.LocalMachine);

        return EncryptedPrefix + Convert.ToBase64String(encrypted);
    }

    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        if (!IsEncrypted(cipherText))
            throw new InvalidOperationException("El texto no está encriptado correctamente.");

        string base64 = cipherText.Substring(EncryptedPrefix.Length);
        byte[] encryptedBytes = Convert.FromBase64String(base64);
        byte[] decrypted = ProtectedData.Unprotect(
            encryptedBytes,
            null,
            DataProtectionScope.LocalMachine);

        return Encoding.UTF8.GetString(decrypted);
    }

    public static bool IsEncrypted(string s)
    {
        return !string.IsNullOrEmpty(s) && s.StartsWith(EncryptedPrefix);
    }
}


