using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Db.Entities.Internal;
using IdentityPrvd.WebApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.WebApi.Db.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<IdentitySession>
{
    public void Configure(EntityTypeBuilder<IdentitySession> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasQueryFilter(d => d.DeletedAt == null);

        builder.Property(s => s.App).HasConversion(
            v => v.ToJson(),
            v => v.FromJson<ClientAppInfo>());

        builder.Property(s => s.Location).HasConversion(
            v => v.ToJson(),
            v => v.FromJson<LocationInfo>());

        builder.Property(s => s.Client).HasConversion(
            v => v.ToJson(),
            v => v.FromJson<ClientInfo>());
    }
}
