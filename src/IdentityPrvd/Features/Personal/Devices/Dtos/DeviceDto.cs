namespace IdentityPrvd.Features.Personal.Devices.Dtos;

public class DeviceDto
{
    public Ulid Id { get; set; }
    public DateTime CreatedAt { get; set; }

    public string Identifier { get; set; }
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
    public bool Verified { get; set; }
    public DateTime? VerifiedAt { get; set; }


    public DateTime UpdatedAt { get; set; }
}
