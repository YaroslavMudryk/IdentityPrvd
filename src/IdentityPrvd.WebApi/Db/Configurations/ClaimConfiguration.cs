using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.WebApi.Db.Configurations;

public class ClaimConfiguration : IEntityTypeConfiguration<IdentityClaim>
{
    public void Configure(EntityTypeBuilder<IdentityClaim> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasQueryFilter(d => d.DeletedAt == null);
    }
}
