using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Infrastructure.Caching;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Services.Security;
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
        var identityContext = scope.ServiceProvider.GetRequiredService<IIdentityContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IHasher>();
        ((IdentityContext)identityContext).CurrentUser = new ServiceUser("Seed");
        ((IdentityContext)identityContext).CorrelationId = Guid.NewGuid().ToString("N");

        await dbContext.Database.EnsureCreatedAsync();

        await using var transaction = await transactionManager.BeginTransactionAsync();
        await IdentityPrvdSeedHelper.SeedDefaultsAsync(dbContext, hasher, sessionStore);
        await transaction.CommitAsync();
    }
}
