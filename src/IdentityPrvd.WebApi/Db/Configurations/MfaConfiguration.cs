using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.WebApi.Db.Configurations;

public class MfaConfiguration : IEntityTypeConfiguration<IdentityMfa>
{
    public void Configure(EntityTypeBuilder<IdentityMfa> builder)
    {
        builder.HasKey(m => m.Id);
        builder.HasQueryFilter(d => d.DeletedAt == null);
    }
}
