namespace IdentityPrvd.Common.Extensions;

public static class GuidExtensions
{
    public static string GetIdAsString(this Guid sessionId)
    {
        return sessionId.ToString();
    }

    public static Guid GetIdAsGuid(this string sessionId)
    {
        return Guid.Parse(sessionId);
    }
}
