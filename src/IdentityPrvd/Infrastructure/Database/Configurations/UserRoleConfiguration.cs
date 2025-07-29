using IdentityPrvd.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.Infrastructure.Database.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole> builder)
    {
        builder.HasKey(ur => ur.Id);
        builder.HasQueryFilter(d => d.DeletedAt == null);
    }
}
