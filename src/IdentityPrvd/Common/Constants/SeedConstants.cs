using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Services.Security;

namespace IdentityPrvd.Common.Constants;

public class SeedConstants
{
    public static IEnumerable<IdentityClaim> GetClaims()
    {
        yield return new IdentityClaim
        {
            Id = Ulid.NewUlid(),
            Type = IdentityClaims.Types.Identity,
            Value = IdentityClaims.Values.All
        };
    }

    public static IEnumerable<IdentityRole> GetRoles()
    {
        yield return new IdentityRole
        {
            Id = Ulid.NewUlid(),
            Name = DefaultsRoles.SuperAdmin,
            NameNormalized = DefaultsRoles.SuperAdmin.ToUpper(),
            IsDefault = true
        };
        yield return new IdentityRole
        {
            Id = Ulid.NewUlid(),
            Name = DefaultsRoles.Admin,
            NameNormalized = DefaultsRoles.Admin.ToUpper(),
            IsDefault = false
        };
        yield return new IdentityRole
        {
            Id = Ulid.NewUlid(),
            Name = DefaultsRoles.User,
            NameNormalized = DefaultsRoles.User.ToUpper(),
            IsDefault = false
        };
    }

    public static IEnumerable<IdentityClient> GetClients(IHasher hasher)
    {
        var clientId = Ulid.NewUlid();
        yield return new IdentityClient
        {
            Id = clientId,
            Name = "IdentityPrvd",
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
