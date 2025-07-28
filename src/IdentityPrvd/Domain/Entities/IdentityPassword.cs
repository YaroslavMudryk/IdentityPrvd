namespace IdentityPrvd.Domain.Entities;

public class IdentityPassword : BaseModel
{
    public string PasswordHash { set; get; }
    public string Hint { get; set; }
    public bool IsActive { get; set; }
    public DateTime ActivatedAt { get; set; }
    public DateTime? DeactivatedAt { get; set; }
    public Ulid UserId { get; set; }
    public IdentityUser User { get; set; }
}
