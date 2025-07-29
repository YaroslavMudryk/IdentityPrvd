namespace IdentityPrvd.Services.Security;

public interface IHasher
{
    string GetHash(string content);
    bool Verify(string hashContent, string content);
}
