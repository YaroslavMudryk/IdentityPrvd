using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.WebApi.Db.Configurations;

public class BanConfiguration : IEntityTypeConfiguration<IdentityBan>
{
    public void Configure(EntityTypeBuilder<IdentityBan> builder)
    {
        builder.HasKey(b => b.Id);
        builder.HasQueryFilter(d => d.DeletedAt == null);
    }
}
