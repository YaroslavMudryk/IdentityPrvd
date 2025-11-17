namespace IdentityPrvd.Options;

public class RateLimitOptions
{
    public int MaxAttempts { get; set; } = 5;    
    public int TimeWindowInMinutes { get; set; } = 60;    
    public bool Enabled { get; set; } = true;
}
