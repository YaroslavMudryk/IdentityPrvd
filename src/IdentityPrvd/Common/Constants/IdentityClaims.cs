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
        public const string Role = nameof(Role);
        public const string Sessions = nameof(Sessions);
        public const string Devices = nameof(Devices);
        public const string Clients = nameof(Clients);
        public const string Claims = nameof(Claims);
    }

    public static class Values
    {
        public const string Create = nameof(Create);
        public const string Update = nameof(Update);
        public const string Delete = nameof(Delete);
        public const string SoftDelete = nameof(SoftDelete);
        public const string ViewAll = nameof(ViewAll);
        public const string View = nameof(View);
        public const string All = nameof(All);
    }
}
