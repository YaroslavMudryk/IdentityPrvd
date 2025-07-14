using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.WebApi.Db.Configurations;

public class ContactConfiguration : IEntityTypeConfiguration<IdentityContact>
{
    public void Configure(EntityTypeBuilder<IdentityContact> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasQueryFilter(d => d.DeletedAt == null);
    }
}
