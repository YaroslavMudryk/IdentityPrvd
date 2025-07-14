using IdentityPrvd.WebApi.Db.Entities.Internal;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IdentityPrvd.WebApi.Features.Signin.Services;

public class IpApiLocationService(HttpClient httpClient) : ILocationService
{
    public async Task<LocationInfo> GetIpInfoAsync(string ip)
    {
        if (ip == "127.0.1" || ip == "127.0.0.1")
            ip = "localhost";
        var location = new LocationInfo
        {
            IP = ip
        };
        try
        {
            var urlRequest = "/json";
            if (string.IsNullOrEmpty(ip))
            {
                urlRequest = urlRequest + "?fields=63700991";
            }
            else
            {
                if (ip.Contains("::1") || ip.Contains("localhost"))
                    urlRequest += "?fields=63700991";
                else
                    urlRequest += $"/{ip}?fields=63700991";
            }
            var resultFromApi = await httpClient.GetAsync(urlRequest);
            if (!resultFromApi.IsSuccessStatusCode)
                return location!;
            var content = await resultFromApi.Content.ReadAsStringAsync();
            var res = JsonSerializer.Deserialize<IPGeo>(content);
            location.Provider = res!.Isp;
            location.Country = res.Country;
            location.City = res.City;
            location.Region = res.RegionName;
            location.Lat = res.Latitude;
            location.Lon = res.Longitude;
            location.IP = res.Query;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        return location;
    }
}

public class IPGeo
{
    [JsonPropertyName("query")]
    public string Query { get; set; }
    [JsonPropertyName("status")]
    public string Status { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; }
    [JsonPropertyName("continent")]
    public string Continent { get; set; }
    [JsonPropertyName("continentCode")]
    public string ContinentCode { get; set; }
    [JsonPropertyName("country")]
    public string Country { get; set; }
    [JsonPropertyName("countryCode")]
    public string CountryCode { get; set; }
    [JsonPropertyName("region")]
    public string Region { get; set; }
    [JsonPropertyName("regionName")]
    public string RegionName { get; set; }
    [JsonPropertyName("city")]
    public string City { get; set; }
    [JsonPropertyName("district")]
    public string District { get; set; }
    [JsonPropertyName("zip")]
    public string Zip { get; set; }
    [JsonPropertyName("lat")]
    public double Latitude { get; set; }
    [JsonPropertyName("lon")]
    public double Longitude { get; set; }
    [JsonPropertyName("timezone")]
    public string TimeZone { get; set; }
    [JsonPropertyName("offset")]
    public long Offset { get; set; }
    [JsonPropertyName("currency")]
    public string Currency { get; set; }
    [JsonPropertyName("isp")]
    public string Isp { get; set; }
    [JsonPropertyName("org")]
    public string Org { get; set; }
    [JsonPropertyName("as")]
    public string As { get; set; }
    [JsonPropertyName("asname")]
    public string AsName { get; set; }
    [JsonPropertyName("reverse")]
    public string Reverse { get; set; }
    [JsonPropertyName("mobile")]
    public bool Mobile { get; set; }
    [JsonPropertyName("proxy")]
    public bool Proxy { get; set; }
    [JsonPropertyName("hosting")]
    public bool Hosting { get; set; }
}
