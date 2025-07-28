namespace IdentityPrvd.Domain.ValueObjects;

public class ClientInfo
{
    public DeviceInfo Device { get; set; }
    public OsInfo Os { get; set; }
    public BrowserInfo Browser { get; set; }
}
