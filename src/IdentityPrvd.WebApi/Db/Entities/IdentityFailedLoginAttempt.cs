using IdentityPrvd.WebApi.Db.Entities.Internal;

namespace IdentityPrvd.WebApi.Db.Entities;

public class IdentityFailedLoginAttempt : BaseModel
{
    public string Login { get; set; }
    public string Password { get; set; }
    public ClientAppInfo Client { get; set; }
    public LocationInfo Location { get; set; }
    public Ulid UserId { get; set; }
    public IdentityUser User { get; set; }
}
