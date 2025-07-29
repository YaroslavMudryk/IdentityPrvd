using IdentityPrvd.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.Infrastructure.Database.Configurations;

public class ClientClaimConfiguration : IEntityTypeConfiguration<IdentityClientClaim>
{
    public void Configure(EntityTypeBuilder<IdentityClientClaim> builder)
    {
        builder.HasKey(cc => cc.Id);
        builder.HasQueryFilter(d => d.DeletedAt == null);
    }
}
