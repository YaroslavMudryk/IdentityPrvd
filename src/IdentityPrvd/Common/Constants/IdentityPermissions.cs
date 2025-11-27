namespace IdentityPrvd.Common.Constants;

public class IdentityPermissions
{
    public static class Credentials
    {
        public const string Manage = "identity:manage";
    }

    public static class Sessions
    {
        public const string Read = "sessions:read";
        public const string Manage = "sessions:manage";
    }

    public static class Mfas
    {
        public const string Manage = "mfas:manage";
    }

    public static class Contacts
    {
        public const string Read = "contacts:read";
        public const string Manage = "contacts:manage";
    }

    public static class Devices
    {
        public const string Read = "devices:read";
        public const string Manage = "devices:manage";
    }

    public static class Clients
    {
        public const string Read = "clients:read";
        public const string Manage = "clients:manage";
    }

    public static class Roles
    {
        public const string Read = "roles:read";
        public const string Manage = "roles:manage";
    }

    public static class Permissions
    {
        public const string Read = "permissions:read";
        public const string Manage = "permissions:manage";
    }

    public static class Qrs
    {
        public const string Read = "qrs:read";
        public const string Manage = "qrs:manage";
    }
}
