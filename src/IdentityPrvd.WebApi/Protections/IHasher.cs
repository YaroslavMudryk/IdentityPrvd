namespace IdentityPrvd.WebApi.Protections;

public interface IHasher
{
    string GetHash(string content);
    bool Verify(string hashContent, string content);
}
