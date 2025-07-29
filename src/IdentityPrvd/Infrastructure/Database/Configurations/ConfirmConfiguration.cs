using IdentityPrvd.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.Infrastructure.Database.Configurations;

public class ConfirmConfiguration : IEntityTypeConfiguration<IdentityConfirm>
{
    public void Configure(EntityTypeBuilder<IdentityConfirm> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasQueryFilter(d => d.DeletedAt == null);
    }
}
