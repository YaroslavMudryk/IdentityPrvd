namespace IdentityPrvd.Services.Security;

public interface IProtectionService
{
    string DecryptData(string cipherText);
    string EncryptData(string plainText);
}
