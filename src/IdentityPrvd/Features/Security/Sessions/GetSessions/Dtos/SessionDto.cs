using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Domain.ValueObjects;

namespace IdentityPrvd.Features.Security.Sessions.GetSessions.Dtos;

public class SessionDto
{
    public Ulid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public AppInfo App { get; set; }
    public LocationInfo Location { set; get; }
    public ClientInfo Client { set; get; }
    public SessionType Type { get; set; }
    public bool ViaMFA { get; set; }
    public SessionStatus Status { set; get; }
    public string Language { set; get; }
    public Ulid? DeactivatedBySessionId { set; get; }
    public DateTime? DeactivatedAt { set; get; }
    public DateTime? LastActivityAt { set; get; }
    public DateTime UpdatedAt { get; set; }
    public Ulid? DeviceId { get; set; }
}
