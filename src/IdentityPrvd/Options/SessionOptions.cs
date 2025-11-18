namespace IdentityPrvd.Options;

public class SessionOptions
{
    public bool TrackActivity { get; set; } = true;
    public bool SingleSessionPerUser { get; set; } = false;
}
