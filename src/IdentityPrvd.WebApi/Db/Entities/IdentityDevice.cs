namespace IdentityPrvd.WebApi.Db.Entities;

public class IdentityDevice : BaseModel
{
    public string DeviceIdentifier { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public string VendorModel { get; set; }
    public string Type { get; set; }
    public string Os { get; set; }
    public string OsVersion { get; set; }
    public string OsShortName { get; set; }
    public string OsUI { get; set; }
    public string OsPlatform { get; set; }
    public string Browser { get; set; }
    public string BrowserVersion { get; set; }
    public string BrowserType { get; set; }
    public string BrowserEngine { get; set; }
    public string BrowserEngineVersion { get; set; }

    public DateTime? TrustedAt { get; set; }
    public Ulid? TrustedBySessionId { get; set; }

    public List<IdentitySession> Sessions { get; set; } = [];
}
