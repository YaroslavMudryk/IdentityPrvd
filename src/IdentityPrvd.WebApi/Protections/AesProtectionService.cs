using IdentityPrvd.WebApi.Options;
using System.Security.Cryptography;
using System.Text;

namespace IdentityPrvd.WebApi.Protections;

public class AesProtectionService(ProtectionOptions options) : IProtectionService
{
    private readonly byte[] EncryptionKey = ConvertHexStringToBytes(options.Key);

    public string DecryptData(string cipherText)
    {
        byte[] fullCipher = Convert.FromBase64String(cipherText);

        using Aes aesAlg = Aes.Create();
        aesAlg.Key = EncryptionKey;

        byte[] iv = new byte[aesAlg.BlockSize / 8];
        Array.Copy(fullCipher, 0, iv, 0, iv.Length);
        aesAlg.IV = iv;

        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msDecrypt = new();
        using (CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Write))
        {
            csDecrypt.Write(fullCipher, iv.Length, fullCipher.Length - iv.Length);
        }
        return Encoding.UTF8.GetString(msDecrypt.ToArray());
    }

    public string EncryptData(string plainText)
    {
        using Aes aesAlg = Aes.Create();
        aesAlg.Key = EncryptionKey;
        aesAlg.GenerateIV();

        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msEncrypt = new();
        msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
        using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (StreamWriter swEncrypt = new(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }
        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    public static byte[] ConvertHexStringToBytes(string hex)
    {
        if (hex.Length % 2 != 0) throw new ArgumentException("Hex string must have an even number of characters.");

        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < hex.Length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }
}
