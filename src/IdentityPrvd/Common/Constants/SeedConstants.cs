using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Services.Security;

namespace IdentityPrvd.Common.Constants;

public class SeedConstants
{
    public static IEnumerable<IdentityPermission> GetPermissions()
    {
        yield return new IdentityPermission
        {
            Id = Guid.CreateVersion7(),
            Value = IdentityPermissions.Credentials.Manage
        };


        yield return new IdentityPermission
        {
            Id = Guid.CreateVersion7(),
            Value = IdentityPermissions.Sessions.Read
        };
        yield return new IdentityPermission
        {
            Id = Guid.CreateVersion7(),
            Value = IdentityPermissions.Sessions.Manage
        };


        yield return new IdentityPermission
        {
            Id = Guid.CreateVersion7(),
            Value = IdentityPermissions.Mfas.Manage
        };


        yield return new IdentityPermission
        {
            Id = Guid.CreateVersion7(),
            Value = IdentityPermissions.Contacts.Read
        };
        yield return new IdentityPermission
        {
            Id = Guid.CreateVersion7(),
            Value = IdentityPermissions.Contacts.Manage
        };


        yield return new IdentityPermission
        {
            Id = Guid.CreateVersion7(),
            Value = IdentityPermissions.Devices.Read
        };
        yield return new IdentityPermission
        {
            Id = Guid.CreateVersion7(),
            Value = IdentityPermissions.Devices.Manage
        };


        yield return new IdentityPermission
        {
            Id = Guid.CreateVersion7(),
            Value = IdentityPermissions.Clients.Read
        };
        yield return new IdentityPermission
        {
            Id = Guid.CreateVersion7(),
            Value = IdentityPermissions.Clients.Manage
        };


        yield return new IdentityPermission
        {
            Id = Guid.CreateVersion7(),
            Value = IdentityPermissions.Roles.Read
        };
        yield return new IdentityPermission
        {
            Id = Guid.CreateVersion7(),
            Value = IdentityPermissions.Roles.Manage
        };

        yield return new IdentityPermission
        {
            Id = Guid.CreateVersion7(),
            Value = IdentityPermissions.Permissions.Read
        };
        yield return new IdentityPermission
        {
            Id = Guid.CreateVersion7(),
            Value = IdentityPermissions.Permissions.Manage
        };


        yield return new IdentityPermission
        {
            Id = Guid.CreateVersion7(),
            Value = IdentityPermissions.Qrs.Read
        };
        yield return new IdentityPermission
        {
            Id = Guid.CreateVersion7(),
            Value = IdentityPermissions.Qrs.Manage
        };
    }

    public static IEnumerable<IdentityRole> GetRoles()
    {
        yield return new IdentityRole
        {
            Id = Guid.CreateVersion7(),
            Name = DefaultsRoles.Admin,
            NameNormalized = DefaultsRoles.Admin.ToUpper(),
            IsDefault = false
        };
        yield return new IdentityRole
        {
            Id = Guid.CreateVersion7(),
            Name = DefaultsRoles.User,
            NameNormalized = DefaultsRoles.User.ToUpper(),
            IsDefault = true
        };
        yield return new IdentityRole
        {
            Id = Guid.CreateVersion7(),
            Name = DefaultsRoles.Developer,
            NameNormalized = DefaultsRoles.Developer.ToUpper(),
            IsDefault = false
        };
    }

    public static IEnumerable<IdentityClient> GetClients(IHasher hasher, string audience = "IdentityPrvd")
    {
        var clientId = Guid.CreateVersion7();
        yield return new IdentityClient
        {
            Id = clientId,
            Name = audience,
            ShortName = "IdentityPrvd",
            ClientId = "1jjd-Pt0B-QFdk-x3Vw",
            ActiveFrom = DateTime.MinValue,
            ActiveTo = DateTime.MaxValue,
            Description = "IdentityPrvd Client",
            IsActive = true,
            Image = "/assets/images/logo.png",
            ClientSecretRequired = false,
            //ClientSecrets = [new IdentityClientSecret {
            //    Value = hasher.GetHash("lTiv0Fn0PcWqAsjQGmHrBfsrEZuSfvMjDlST6311QjEfEolUl8qjOPCEUX0JJhXMCaJFJr"),
            //    ClientId = clientId
            //}]
        };
    }
}
