using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.WebApi.Db.Configurations;

public class ClientSecretConfiguration : IEntityTypeConfiguration<IdentityClientSecret>
{
    public void Configure(EntityTypeBuilder<IdentityClientSecret> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasQueryFilter(d => d.DeletedAt == null);
    }
}
