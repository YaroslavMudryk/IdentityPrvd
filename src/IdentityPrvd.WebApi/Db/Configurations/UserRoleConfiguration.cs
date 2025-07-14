using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.WebApi.Db.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole> builder)
    {
        builder.HasKey(ur => ur.Id);
        builder.HasQueryFilter(d => d.DeletedAt == null);
    }
}
