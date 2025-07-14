using IdentityPrvd.WebApi.Db.Entities.Enums;
using IdentityPrvd.WebApi.Db.Entities.Internal;

namespace IdentityPrvd.WebApi.Db.Entities;

public class IdentitySession : BaseModel
{
    public ClientAppInfo App { get; set; }
    public LocationInfo Location { set; get; }
    public ClientInfo Client { set; get; }
    public SessionType Type { get; set; }
    public bool ViaMFA { get; set; }
    public SessionStatus Status { set; get; }
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
    public List<IdentityRefreshToken> Tokens { get; set; } = [];
}
