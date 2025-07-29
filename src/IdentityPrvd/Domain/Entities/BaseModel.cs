using IdentityPrvd.Infrastructure.Database.Audits;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityPrvd.Domain.Entities;

public class BaseModel : IAuditable, IVersionable, ISoftDeletable
{
    public Ulid Id { get; set; } = Ulid.Empty;

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public string UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
    public string DeletedBy { get; set; }

    public int Version { get; set; }

    [NotMapped]
    public bool HardDelete { get; set; }
}
