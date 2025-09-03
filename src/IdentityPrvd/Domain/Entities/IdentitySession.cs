using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Domain.ValueObjects;

namespace IdentityPrvd.Domain.Entities;

public class IdentitySession : BaseModel
{
    public AppInfo App { get; set; }
    public LocationInfo Location { set; get; }
    public ClientInfo Client { set; get; }
    public SessionType Type { get; set; }
    public bool ViaMfa { get; set; }
    public SessionStatus Status { set; get; }
    public string VerificationId { get; set; }
    public DateTime? VerificationExpire { get; set; }
    public Ulid? AuthorizedBy { get; set; }
    public string Language { set; get; }
    public Ulid? DeactivatedBySessionId { set; get; }
    public DateTime? DeactivatedAt { set; get; }
    public DateTime ExpireAt { get; set; }
    public Ulid UserId { get; set; }
    public IdentityUser User { get; set; }
    public Ulid? DeviceId { get; set; }
    public IdentityDevice Device { get; set; }
    public Ulid? QrId { get; set; }
    public IdentityQr Qr { get; set; }
    public Dictionary<string, string> Data { get; set; }
    public List<IdentityRefreshToken> Tokens { get; set; } = [];
}
