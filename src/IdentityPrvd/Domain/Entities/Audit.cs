using IdentityPrvd.Infrastructure.Database.Audits;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityPrvd.Domain.Entities;

public class Audit
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string By { get; set; }
    public string Event { get; set; }
    public string ItemId { get; set; }
    public string ItemType { get; set; }
    public string TransactionId { get; set; } //same as CorrelationId
    public List<PropertyInfo> Changes { get; set; } = [];
}
