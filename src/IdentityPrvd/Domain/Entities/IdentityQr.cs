namespace IdentityPrvd.Domain.Entities;

public class IdentityQr : BaseModel
{
    public DateTime ActiveFrom { get; set; }
    public DateTime ActiveTo { get; set; }
    public DateTime ActivatedAt { get; set; }
    public string QrCodeVerify { get; set; }
    public string Ip { get; set; }
    public Ulid SessionId { get; set; }
    public IdentitySession Session { get; set; }
    public Ulid UserId { get; set; }
    public IdentityUser User { get; set; }
}
