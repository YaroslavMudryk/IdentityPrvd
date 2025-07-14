namespace IdentityPrvd.WebApi.Protections;

public interface IProtectionService
{
    string DecryptData(string cipherText);
    string EncryptData(string plainText);
}
