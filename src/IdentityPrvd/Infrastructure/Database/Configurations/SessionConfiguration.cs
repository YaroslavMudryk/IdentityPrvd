using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.Infrastructure.Database.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<IdentitySession>
{
    public void Configure(EntityTypeBuilder<IdentitySession> builder)
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
    }
}
