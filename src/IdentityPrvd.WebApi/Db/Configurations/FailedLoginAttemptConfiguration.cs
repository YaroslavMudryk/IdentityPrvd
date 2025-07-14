using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Db.Entities.Internal;
using IdentityPrvd.WebApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.WebApi.Db.Configurations;

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
            v => v.FromJson<ClientAppInfo>());
    }
}
