namespace IdentityPrvd.Domain.ValueObjects;

public class DeviceInfo
{
    public string DeviceId { get; set; } // it can be like AndroidId or IDFV or IMEI or SerialNumber
    public string Brand { get; set; }
    public string Model { get; set; }
    public string Type { get; set; }
    public string VendorModel { get; set; } // fullName of device (Brand+Model)
}
