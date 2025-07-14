using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.WebApi.Db.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<IdentityRefreshToken>
{
    public void Configure(EntityTypeBuilder<IdentityRefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);
        builder.HasQueryFilter(d => d.DeletedAt == null);
    }
}
