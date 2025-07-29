using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.Infrastructure.Database.Configurations;

public class FailedLoginAttemptConfiguration : IEntityTypeConfiguration<IdentityFailedLoginAttempt>
{
    public void Configure(EntityTypeBuilder<IdentityFailedLoginAttempt> builder)
    {
        builder.HasKey(fla => fla.Id);
        builder.HasQueryFilter(d => d.DeletedAt == null);
        builder.Property(s => s.Location).HasConversion(
            v => v.ToJson(),
            v => v.FromJson<LocationInfo>());

        builder.Property(s => s.Client).HasConversion(
            v => v.ToJson(),
            v => v.FromJson<AppInfo>());
    }
}
