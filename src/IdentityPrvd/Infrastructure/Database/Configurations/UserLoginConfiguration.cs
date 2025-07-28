using IdentityPrvd.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.Infrastructure.Database.Configurations;

public class UserLoginConfiguration : IEntityTypeConfiguration<IdentityUserLogin>
{
    public void Configure(EntityTypeBuilder<IdentityUserLogin> builder)
    {
        builder.HasKey(et => et.Id);
        builder.HasQueryFilter(d => d.DeletedAt == null);
    }
}
