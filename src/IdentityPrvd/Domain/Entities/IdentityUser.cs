namespace IdentityPrvd.Domain.Entities;

public class IdentityUser : BaseModel
{
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }
    public string Image { get; set; }

    public string Login { get; set; }
    public string PasswordHash { get; set; }

    public bool CanBeBlocked { get; set; }
    public DateTime? BlockedUntil { get; set; }
    public int FailedLoginAttemptsCount { get; set; }

    public bool IsConfirmed { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public string ConfirmedBy { get; set; }

    public List<IdentityContact> Contacts { get; set; } = [];

    public List<IdentityPassword> Passwords { get; set; } = [];
    public List<IdentityQr> Qrs { get; set; } = [];
    public List<IdentityBan> Bans { get; set; } = [];
    public List<IdentityMfa> Mfas { get; set; } = [];
    public List<IdentityCode> Confirms { get; set; } = [];
    public List<IdentityUserRole> UserRoles { get; set; } = [];
    public List<IdentityFailedLoginAttempt> FailedLoginAttempts { get; set; } = [];
    public List<IdentitySession> Sessions { get; set; } = [];
    public List<IdentityUserLogin> UserLogins { get; set; } = [];
    public List<IdentityDevice> Devices { get; set; } = [];
}
