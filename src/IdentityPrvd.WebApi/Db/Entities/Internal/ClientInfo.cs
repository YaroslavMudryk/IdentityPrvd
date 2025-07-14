namespace IdentityPrvd.WebApi.Db.Entities.Internal;

public class ClientInfo
{
    public DeviceInfo Device { get; set; }
    public OsInfo Os { get; set; }
    public BrowserInfo Browser { get; set; }
}
