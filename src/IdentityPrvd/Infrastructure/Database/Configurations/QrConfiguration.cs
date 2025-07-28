using IdentityPrvd.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.Infrastructure.Database.Configurations;

public class QrConfiguration : IEntityTypeConfiguration<IdentityQr>
{
    public void Configure(EntityTypeBuilder<IdentityQr> builder)
    {
        builder.HasKey(q => q.Id);
        builder.HasQueryFilter(d => d.DeletedAt == null);
        builder.HasOne(s => s.Session).WithOne(s => s.Qr)
                .HasForeignKey<IdentityQr>(s => s.SessionId);
    }
}
