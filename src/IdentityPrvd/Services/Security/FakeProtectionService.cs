namespace IdentityPrvd.Services.Security;

public class FakeProtectionService : IProtectionService
{
    public string DecryptData(string cipherText)
    {
        return cipherText;
    }

    public string EncryptData(string plainText)
    {
        return plainText;
    }
}
