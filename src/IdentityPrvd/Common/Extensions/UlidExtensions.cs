namespace IdentityPrvd.Common.Extensions;

public static class UlidExtensions
{
    public static string GetIdAsString(this Ulid sessionId)
    {
        return sessionId.ToString();
    }

    public static Ulid GetIdAsUlid(this string sessionId)
    {
        return Ulid.Parse(sessionId);
    }
}
