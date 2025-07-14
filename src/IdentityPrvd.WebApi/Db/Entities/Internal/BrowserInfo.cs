namespace IdentityPrvd.WebApi.Db.Entities.Internal;

public class BrowserInfo : BaseInfo
{
    public string Type { get; set; }
    public string Engine { get; set; }
    public string EngineVersion { get; set; }
}
