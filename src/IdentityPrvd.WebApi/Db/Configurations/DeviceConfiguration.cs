using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.WebApi.Db.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<IdentityDevice>
{
    public void Configure(EntityTypeBuilder<IdentityDevice> builder)
    {
        builder.HasKey(b => b.Id);
        builder.HasQueryFilter(d => d.DeletedAt == null);
    }
}
