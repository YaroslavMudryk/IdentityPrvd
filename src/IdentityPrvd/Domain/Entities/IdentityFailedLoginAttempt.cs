using IdentityPrvd.Domain.ValueObjects;

namespace IdentityPrvd.Domain.Entities;

public class IdentityFailedLoginAttempt : BaseModel
{
    public string Login { get; set; }
    public string Password { get; set; }
    public AppInfo Client { get; set; }
    public LocationInfo Location { get; set; }
    public Ulid UserId { get; set; }
    public IdentityUser User { get; set; }
}
