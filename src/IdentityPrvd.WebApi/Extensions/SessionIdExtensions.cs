namespace IdentityPrvd.WebApi.Extensions;

public static class SessionIdExtensions
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
