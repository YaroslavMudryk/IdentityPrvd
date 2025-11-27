using System.Security.Claims;

namespace IdentityPrvd.Common.Constants;

public class IdentityClaims
{
    public static class Types
    {
        public const string UserId = ClaimTypes.NameIdentifier;
        public const string SessionId = "SessionId";
        public const string Identity = nameof(Identity);
        public const string Roles = ClaimTypes.Role;
        public const string AuthMethod = ClaimTypes.AuthenticationMethod;
    }
}
