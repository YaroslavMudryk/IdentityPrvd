using IdentityPrvd.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.Infrastructure.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<IdentityUser>
{
    public void Configure(EntityTypeBuilder<IdentityUser> builder)
    {
        builder.HasKey(u => u.Id);
        builder.HasQueryFilter(d => d.DeletedAt == null);
    }
}
