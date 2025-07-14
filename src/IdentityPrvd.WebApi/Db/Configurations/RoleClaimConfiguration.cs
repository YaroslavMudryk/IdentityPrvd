using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.WebApi.Db.Configurations;

public class RoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim>
{
    public void Configure(EntityTypeBuilder<IdentityRoleClaim> builder)
    {
        builder.HasKey(rc => rc.Id);
        builder.HasQueryFilter(d => d.DeletedAt == null);
    }
}
