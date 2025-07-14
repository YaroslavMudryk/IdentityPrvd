using IdentityPrvd.WebApi.CurrentContext;
using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Helpers;
using IdentityPrvd.WebApi.Protections;
using IdentityPrvd.WebApi.ServerSideSessions.Stores;
using IdentityPrvd.WebApi.UserContext;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Seeding;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var utcNow = scope.ServiceProvider.GetRequiredService<TimeProvider>().GetUtcNow().UtcDateTime;
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityPrvdContext>();
        var sessionStore = scope.ServiceProvider.GetRequiredService<ISessionStore>();
        var userContext = scope.ServiceProvider.GetRequiredService<IUserContext>();
        var currentContext = scope.ServiceProvider.GetRequiredService<ICurrentContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IHasher>();
        ((UserContext.UserContext)userContext).CurrentUser = new ServiceUser("Seed");
        ((CurrentContext.CurrentContext)currentContext).CorrelationId = Guid.NewGuid().ToString("N");

        await dbContext.Database.EnsureCreatedAsync();

        var itemsCountAdded = 0;
        if (!await dbContext.Roles.AnyAsync())
        {
            await dbContext.Roles.AddRangeAsync(GetRoles());
            itemsCountAdded++;
        }

        if (!await dbContext.Claims.AnyAsync())
        {
            await dbContext.Claims.AddRangeAsync(GetClaims());
            itemsCountAdded++;
        }

        if (!await dbContext.Clients.AnyAsync())
        {
            await dbContext.Clients.AddRangeAsync(GetClients(hasher));
            itemsCountAdded++;
        }

        if (itemsCountAdded > 0)
        {
            await dbContext.SaveChangesAsync();
            await MapRolesAndClaimsAsync(dbContext);
        }

        await sessionStore.InitializeAsync();
    }

    private static async Task MapRolesAndClaimsAsync(IdentityPrvdContext dbContext)
    {
        var defaultRole = dbContext.Roles.Local.Where(r => r.IsDefault).FirstOrDefault();
        var identityClaim = dbContext.Claims.Local.FirstOrDefault(c => c.Type == IdentityClaims.Types.Identity && c.Value == IdentityClaims.Values.All);

        await dbContext.RoleClaims.AddAsync(new IdentityRoleClaim
        {
            Id = Ulid.NewUlid(),
            RoleId = defaultRole!.Id,
            ClaimId = identityClaim!.Id,
            ActiveFrom = DateTime.MinValue,
            ActiveTo = DateTime.MaxValue,
            IsActive = true
        });
        await dbContext.SaveChangesAsync();
    }

    private static IEnumerable<IdentityClient> GetClients(IHasher hasher)
    {
        var clientId = Ulid.NewUlid();
        yield return new IdentityClient
        {
            Id = clientId,
            Name = "IdentityPrvd.WebApi",
            ShortName = "IdentityPrvd.WebApi",
            ClientId = "1jjd-Pt0B-QFdk-x3Vw",
            ActiveFrom = DateTime.MinValue,
            ActiveTo = DateTime.MaxValue,
            Description = "IdentityPrvd.WebApi Client",
            IsActive = true,
            Image = "/assets/images/logo.png",
            ClientSecrets = [new IdentityClientSecret {
                Value = hasher.GetHash("lTiv0Fn0PcWqAsjQGmHrBfsrEZuSfvMjDlST6311QjEfEolUl8qjOPCEUX0JJhXMCaJFJr"),
                ClientId = clientId
            }]
        };
    }

    private static IEnumerable<IdentityClaim> GetClaims()
    {
        yield return new IdentityClaim
        {
            Id = Ulid.NewUlid(),
            Type = IdentityClaims.Types.Identity,
            Value = IdentityClaims.Values.All
        };
    }

    private static IEnumerable<IdentityRole> GetRoles()
    {
        yield return new IdentityRole
        {
            Id = Ulid.NewUlid(),
            Name = DefaultsRoles.SuperAdmin,
            NameNormalized = DefaultsRoles.SuperAdmin.ToUpper(),
            IsDefault = false
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
            IsDefault = true
        };
    }
}
