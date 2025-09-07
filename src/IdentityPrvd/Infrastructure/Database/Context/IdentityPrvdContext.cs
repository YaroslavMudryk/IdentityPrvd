using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.ValueObjects;
using IdentityPrvd.Infrastructure.Database.Audits;
using IdentityPrvd.Infrastructure.Database.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace IdentityPrvd.Infrastructure.Database.Context;

public class IdentityPrvdContext(DbContextOptions<IdentityPrvdContext> options)
        : DbContext(options)
{
    public DbSet<Audit> Audits { get; set; } = null!;
    public DbSet<IdentityBan> Bans { get; set; } = null!;
    public DbSet<IdentityClaim> Claims { get; set; } = null!;
    public DbSet<IdentityClient> Clients { get; set; } = null!;
    public DbSet<IdentityClientSecret> ClientSecrets { get; set; } = null!;
    public DbSet<IdentityClientClaim> ClientClaims { get; set; } = null!;
    public DbSet<IdentityCode> Confirms { get; set; } = null!;
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
        // Configure Audit
        modelBuilder.Entity<Audit>(builder =>
        {
            builder.HasKey(a => a.Id);
            builder.OwnsMany(s => s.Changes, builder =>
            {
                builder.ToJson();
            });
        });

        // Configure IdentityBan
        modelBuilder.Entity<IdentityBan>(builder =>
        {
            builder.HasKey(b => b.Id);
            builder.HasQueryFilter(d => d.DeletedAt == null);
        });

        // Configure IdentityClaim
        modelBuilder.Entity<IdentityClaim>(builder =>
        {
            builder.HasKey(c => c.Id);
            builder.HasQueryFilter(d => d.DeletedAt == null);
        });

        // Configure IdentityClient
        modelBuilder.Entity<IdentityClient>(builder =>
        {
            builder.HasKey(c => c.Id);
            builder.HasQueryFilter(d => d.DeletedAt == null);
        });

        // Configure IdentityClientClaim
        modelBuilder.Entity<IdentityClientClaim>(builder =>
        {
            builder.HasKey(cc => cc.Id);
            builder.HasQueryFilter(d => d.DeletedAt == null);
        });

        // Configure IdentityClientSecret
        modelBuilder.Entity<IdentityClientSecret>(builder =>
        {
            builder.HasKey(c => c.Id);
            builder.HasQueryFilter(d => d.DeletedAt == null);
        });

        // Configure IdentityConfirm
        modelBuilder.Entity<IdentityCode>(builder =>
        {
            builder.HasKey(c => c.Id);
            builder.HasQueryFilter(d => d.DeletedAt == null);
        });

        // Configure IdentityContact
        modelBuilder.Entity<IdentityContact>(builder =>
        {
            builder.HasKey(c => c.Id);
            builder.HasQueryFilter(d => d.DeletedAt == null);
        });

        // Configure IdentityDevice
        modelBuilder.Entity<IdentityDevice>(builder =>
        {
            builder.HasKey(b => b.Id);
            builder.HasQueryFilter(d => d.DeletedAt == null);
        });

        // Configure IdentityFailedLoginAttempt
        modelBuilder.Entity<IdentityFailedLoginAttempt>(builder =>
        {
            builder.HasKey(fla => fla.Id);
            builder.HasQueryFilter(d => d.DeletedAt == null);
            builder.Property(s => s.Location).HasConversion(
                v => v.ToJson(),
                v => v.FromJson<LocationInfo>());

            builder.Property(s => s.Client).HasConversion(
                v => v.ToJson(),
                v => v.FromJson<AppInfo>());
        });

        // Configure IdentityMfa
        modelBuilder.Entity<IdentityMfa>(builder =>
        {
            builder.HasKey(m => m.Id);
            builder.HasQueryFilter(d => d.DeletedAt == null);
        });

        // Configure IdentityMfaRecoveryCode
        modelBuilder.Entity<IdentityMfaRecoveryCode>(builder =>
        {
            builder.HasKey(m => m.Id);
            builder.HasQueryFilter(d => d.DeletedAt == null);
        });

        // Configure IdentityPassword
        modelBuilder.Entity<IdentityPassword>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.HasQueryFilter(d => d.DeletedAt == null);
        });

        // Configure IdentityQr
        modelBuilder.Entity<IdentityQr>(builder =>
        {
            builder.HasKey(q => q.Id);
            builder.HasQueryFilter(d => d.DeletedAt == null);
            builder.HasOne(s => s.Session).WithOne(s => s.Qr)
                    .HasForeignKey<IdentityQr>(s => s.SessionId);
        });

        // Configure IdentityRefreshToken
        modelBuilder.Entity<IdentityRefreshToken>(builder =>
        {
            builder.HasKey(rt => rt.Id);
            builder.HasQueryFilter(d => d.DeletedAt == null);
        });

        // Configure IdentityRole
        modelBuilder.Entity<IdentityRole>(builder =>
        {
            builder.HasKey(r => r.Id);
            builder.HasQueryFilter(d => d.DeletedAt == null);
        });

        // Configure IdentityRoleClaim
        modelBuilder.Entity<IdentityRoleClaim>(builder =>
        {
            builder.HasKey(rc => rc.Id);
            builder.HasQueryFilter(d => d.DeletedAt == null);
        });

        // Configure IdentitySession
        modelBuilder.Entity<IdentitySession>(builder =>
        {
            builder.HasKey(s => s.Id);
            builder.HasQueryFilter(d => d.DeletedAt == null);

            builder.Property(s => s.App).HasConversion(
                v => v.ToJson(),
                v => v.FromJson<AppInfo>());

            builder.Property(s => s.Location).HasConversion(
                v => v.ToJson(),
                v => v.FromJson<LocationInfo>());

            builder.Property(s => s.Client).HasConversion(
                v => v.ToJson(),
                v => v.FromJson<ClientInfo>());

            builder.Property(s => s.Data).HasConversion(
                v => v.ToJson(),
                v => v.FromJson<Dictionary<string, string>>());
        });

        // Configure IdentityUser
        modelBuilder.Entity<IdentityUser>(builder =>
        {
            builder.HasKey(u => u.Id);
            builder.HasQueryFilter(d => d.DeletedAt == null);
        });

        // Configure IdentityUserLogin
        modelBuilder.Entity<IdentityUserLogin>(builder =>
        {
            builder.HasKey(et => et.Id);
            builder.HasQueryFilter(d => d.DeletedAt == null);
        });

        // Configure IdentityUserRole
        modelBuilder.Entity<IdentityUserRole>(builder =>
        {
            builder.HasKey(ur => ur.Id);
            builder.HasQueryFilter(d => d.DeletedAt == null);
        });
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
                s.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
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
