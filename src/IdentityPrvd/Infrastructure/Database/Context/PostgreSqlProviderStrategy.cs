using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Infrastructure.Database.Context;

/// <summary>
/// PostgreSQL-specific database provider strategy
/// </summary>
public class PostgreSqlProviderStrategy : IDatabaseProviderStrategy
{
    public string ProviderName => "PostgreSQL";

    public void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, string connectionString)
    {
        optionsBuilder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
    }

    public void ConfigureModel(ModelBuilder modelBuilder)
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
        modelBuilder.Entity<IdentityConfirm>(builder =>
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

        // PostgreSQL-specific optimizations
        // Add PostgreSQL-specific optimizations here if needed
        // For example: indexes, constraints, etc.
    }
}