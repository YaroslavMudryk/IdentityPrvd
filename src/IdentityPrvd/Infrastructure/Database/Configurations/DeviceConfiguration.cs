using IdentityPrvd.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.Infrastructure.Database.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<IdentityDevice>
{
    public void Configure(EntityTypeBuilder<IdentityDevice> builder)
    {
        builder.HasKey(b => b.Id);
        builder.HasQueryFilter(d => d.DeletedAt == null);
    }
}
