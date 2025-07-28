using IdentityPrvd.Services.Security;
using System.Security.Cryptography;

namespace IdentityPrvd.Services.Security;

public class Sha512Hasher : IHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 64;
    private const int Iterations = 10000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA512;

    public string GetHash(string content)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(content, salt, Iterations, HashAlgorithm, HashSize);

        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    public bool Verify(string hashContent, string content)
    {
        string[] parts = hashContent.Split('-');
        byte[] hash = Convert.FromHexString(parts[0]);
        byte[] salt = Convert.FromHexString(parts[1]);

        byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(content, salt, Iterations, HashAlgorithm, HashSize);

        return CryptographicOperations.FixedTimeEquals(hash, inputHash);
    }
}