namespace IdentityPrvd.Domain.ValueObjects;

public class BrowserInfo : BaseInfo
{
    public string Type { get; set; }
    public string Engine { get; set; }
    public string EngineVersion { get; set; }
}
