using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.WebApi.Db.Configurations;

public class MfaRecoveryCodeConfiguration : IEntityTypeConfiguration<IdentityMfaRecoveryCode>
{
    public void Configure(EntityTypeBuilder<IdentityMfaRecoveryCode> builder)
    {
        builder.HasKey(m => m.Id);
        builder.HasQueryFilter(d => d.DeletedAt == null);
    }
}
