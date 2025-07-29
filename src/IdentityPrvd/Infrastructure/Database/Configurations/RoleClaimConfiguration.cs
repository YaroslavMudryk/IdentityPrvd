using IdentityPrvd.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.Infrastructure.Database.Configurations;

public class RoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim>
{
    public void Configure(EntityTypeBuilder<IdentityRoleClaim> builder)
    {
        builder.HasKey(rc => rc.Id);
        builder.HasQueryFilter(d => d.DeletedAt == null);
    }
}
