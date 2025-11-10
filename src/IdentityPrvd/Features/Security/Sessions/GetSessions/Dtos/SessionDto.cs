using IdentityPrvd.Domain.Enums;

namespace IdentityPrvd.Features.Security.Sessions.GetSessions.Dtos;

public class SessionDto
{
    public Ulid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string App { get; set; }
    public string Location { set; get; }
    public string Client { set; get; }
    public SessionStatus Status { set; get; }
    public DateTime? LastActivityAt { set; get; }
    public string Image { get; set; }
}
