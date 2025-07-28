using Microsoft.AspNetCore.Mvc;

namespace IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;

public class ExternalSigninDto
{
    [FromQuery(Name = "provider")]
    public string Provider { get; set; }
    [FromQuery(Name = "returnUrl")]
    public string ReturnUrl { get; set; }

    [FromQuery(Name = "language")]
    public string Language { get; set; }
    [FromQuery(Name = "clientId")]
    public string ClientId { get; set; }
    [FromQuery(Name = "clientSecret")]
    public string ClientSecret { get; set; }
    [FromQuery(Name = "appVersion")]
    public string AppVersion { get; set; }

    
    [FromQuery(Name = "deviceId")]
    public string DeviceId { get; set; }
    [FromQuery(Name = "deviceBrand")]
    public string DeviceBrand { get; set; }
    [FromQuery(Name = "deviceModel")]
    public string DeviceModel { get; set; }
    [FromQuery(Name = "deviceType")]
    public string DeviceType { get; set; }
    [FromQuery(Name = "deviceVendorModel")]
    public string DeviceVendorModel { get; set; }


    [FromQuery(Name = "osName")]
    public string OsName { get; set; }
    [FromQuery(Name = "osVersion")]
    public string OsVersion { get; set; }
    [FromQuery(Name = "osPlatform")]
    public string OsPlatform { get; set; }


    [FromQuery(Name = "browserName")]
    public string BrowserName { get; set; }
    [FromQuery(Name = "browserVersion")]
    public string BrowserVersion { get; set; }
    [FromQuery(Name = "browserType")]
    public string BrowserType { get; set; }
    [FromQuery(Name = "browserEngine")]
    public string BrowserEngine { get; set; }
    [FromQuery(Name = "browserEngineVersion")]
    public string BrowserEngineVersion { get; set; }
}
