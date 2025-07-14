using IdentityPrvd.WebApi.Db.Audits;
using IdentityPrvd.WebApi.Db.Converters;
using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.UserContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace IdentityPrvd.WebApi.Db;

public class IdentityPrvdContext(DbContextOptions<IdentityPrvdContext> options) : DbContext(options)
{
    public DbSet<Audit> Audits { get; set; } = null!;
    public DbSet<IdentityBan> Bans { get; set; } = null!;
    public DbSet<IdentityClaim> Claims { get; set; } = null!;
    public DbSet<IdentityClient> Clients { get; set; } = null!;
    public DbSet<IdentityClientSecret> ClientSecrets { get; set; } = null!;
    public DbSet<IdentityClientClaim> ClientClaims { get; set; } = null!;
    public DbSet<IdentityConfirm> Confirms { get; set; } = null!;
    public DbSet<IdentityContact> Contacts { get; set; } = null!;
    public DbSet<IdentityDevice> Devices { get; set; } = null!;
    public DbSet<IdentityFailedLoginAttempt> FailedLoginAttempts { get; set; } = null!;
    public DbSet<IdentityMfa> Mfas { get; set; } = null!;
    public DbSet<IdentityMfaRecoveryCode> MfaRecoveryCodes { get; set; } = null!;
    public DbSet<IdentityPassword> Passwords { get; set; } = null!;
    public DbSet<IdentityQr> Qrs { get; set; } = null!;
    public DbSet<IdentityRefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<IdentityRole> Roles { get; set; } = null!;
    public DbSet<IdentityRoleClaim> RoleClaims { get; set; } = null!;
    public DbSet<IdentitySession> Sessions { get; set; } = null!;
    public DbSet<IdentityUser> Users { get; set; } = null!;
    public DbSet<IdentityUserLogin> UserLogins { get; set; } = null!;
    public DbSet<IdentityUserRole> UserRoles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityPrvdContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Ulid>()
            .HaveConversion<UlidToStringConverter>();

        configurationBuilder
            .Properties<Enum>()
            .HaveConversion<EnumToStringConverter>();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        OnBeforeSaveChanges();
        SaveAuditsItems();
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected virtual void OnBeforeSaveChanges()
    {
        var utcNow = this.GetService<TimeProvider>().GetUtcNow().UtcDateTime;
        var by = GetCurrentUser();

        foreach (var entry in ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Deleted:
                    HandleDeletedEntry(entry, utcNow, by);
                    break;
                case EntityState.Added:
                    HandleAddedEntry(entry, utcNow, by);
                    break;
                case EntityState.Modified:
                    HandleModifiedEntry(entry, utcNow, by);
                    break;
                default:
                    break;
            }
        }
    }

    protected virtual void SaveAuditsItems()
    {
        var utcNow = this.GetService<TimeProvider>().GetUtcNow().UtcDateTime;
        var by = GetCurrentUser();

        var auditItems = ChangeTracker.Entries()
            .Where(s => s.Entity is IVersionable &&
                (s.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
            .Select(entry =>
            {
                return AuditBuilder.NewDefaultAudit()
                .SoftDeleted(entry.Entity is ISoftDeletable sd && sd.DeletedAt != null)
                .On(entry.State)
                .With(s =>
                {
                    s.ItemId = entry.Properties.Single(s => s.Metadata.IsPrimaryKey()).CurrentValue!.ToString()!;
                    s.ItemType = entry.Entity.GetType().Name;
                    s.TransactionId = ContextId.InstanceId.ToString();
                    s.CreatedAt = utcNow;
                    s.By = by;
                })
                .WithChanges(entry)
                .Build();
            }).ToList();
        Audits.AddRange(auditItems);
    }

    private static void HandleDeletedEntry(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, DateTime utcNow, string by)
    {
        if (entry.Entity is ISoftDeletable deleteEntity)
        {
            if (!deleteEntity.HardDelete)
            {
                deleteEntity.DeletedAt = utcNow;
                deleteEntity.DeletedBy = by;
                entry.State = EntityState.Modified;
            }
        }
    }

    private static void HandleAddedEntry(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, DateTime utcNow, string by)
    {
        if (entry.Entity is BaseModel baseModel && baseModel.Id == Ulid.Empty)
        {
            baseModel.Id = Ulid.NewUlid();
        }

        if (entry.Entity is IAuditable auditEntity)
        {
            auditEntity.CreatedAt = utcNow;
            auditEntity.CreatedBy = by;
            auditEntity.UpdatedAt = utcNow;
            auditEntity.UpdatedBy = by;
        }

        if (entry.Entity is IVersionable versionEntity)
        {
            versionEntity.Version = 1;
        }
    }

    private static void HandleModifiedEntry(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, DateTime utcNow, string by)
    {
        if (entry.Entity is IAuditable auditEntity)
        {
            auditEntity.UpdatedAt = utcNow;
            auditEntity.UpdatedBy = by;
        }

        if (entry.Entity is IVersionable versionEntity)
        {
            versionEntity.Version++;
        }
    }

    private string GetCurrentUser()
    {
        return this.GetService<IUserContext>().GetBy<BasicAuthenticatedUser>();
    }
}
