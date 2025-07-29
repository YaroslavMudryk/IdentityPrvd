namespace IdentityPrvd.Infrastructure.Database.Audits;

public interface ISoftDeletable
{
    public DateTime? DeletedAt { get; set; }
    public string DeletedBy { get; set; }


    public bool HardDelete { get; set; }
}
