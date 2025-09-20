using IdentityPrvd.Common.Constants;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Caching;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Infrastructure.Database.Seeding;

public static class IdentityPrvdSeedLoader
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var utcNow = scope.ServiceProvider.GetRequiredService<TimeProvider>().GetUtcNow().UtcDateTime;
        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityPrvdContext>();
        var sessionStore = scope.ServiceProvider.GetRequiredService<ISessionManagerStore>();
        var userContext = scope.ServiceProvider.GetRequiredService<IUserContext>();
        var currentContext = scope.ServiceProvider.GetRequiredService<ICurrentContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IHasher>();
        ((UserContext)userContext).CurrentUser = new ServiceUser("Seed");
        ((CurrentContext)currentContext).CorrelationId = Guid.NewGuid().ToString("N");

        await dbContext.Database.EnsureCreatedAsync();

        await using var transaction = await transactionManager.BeginTransactionAsync();

        var itemsCountAdded = 0;
        if (!await dbContext.Roles.AnyAsync())
        {
            await dbContext.Roles.AddRangeAsync(SeedConstants.GetRoles());
            itemsCountAdded++;
        }

        if (!await dbContext.Claims.AnyAsync())
        {
            await dbContext.Claims.AddRangeAsync(SeedConstants.GetClaims());
            itemsCountAdded++;
        }

        if (!await dbContext.Clients.AnyAsync())
        {
            await dbContext.Clients.AddRangeAsync(SeedConstants.GetClients(hasher));
            itemsCountAdded++;
        }

        if (itemsCountAdded > 0)
        {
            await dbContext.SaveChangesAsync();
            await MapRolesAndClaimsAsync(dbContext);
        }

        await sessionStore.InitializeAsync();
        await transaction.CommitAsync();
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
}
