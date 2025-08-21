namespace IdentityPrvd.Services.Security;

public class FakeHasher : IHasher
{
    public string GetHash(string content)
    {
        return content;
    }

    public bool Verify(string hashContent, string content)
    {
        return hashContent == content;
    }
}
