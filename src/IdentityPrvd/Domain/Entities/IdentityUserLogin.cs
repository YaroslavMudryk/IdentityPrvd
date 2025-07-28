namespace IdentityPrvd.Domain.Entities;

public class IdentityUserLogin : BaseModel
{
    public string Token { get; set; }
    public string Provider { get; set; }
    public string ProviderUserId { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Ulid UserId { get; set; }
    public IdentityUser User { get; set; }
}
