using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.WebApi.Db.Configurations;

public class PasswordConfiguration : IEntityTypeConfiguration<IdentityPassword>
{
    public void Configure(EntityTypeBuilder<IdentityPassword> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasQueryFilter(d => d.DeletedAt == null);
    }
}
