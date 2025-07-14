using IdentityPrvd.WebApi.Features.ExternalSignin.Dtos;

namespace IdentityPrvd.WebApi.Features.ExternalSignin.Services;

public static class ExternalSigninDtoHelper
{
    // Dictionary key constants
    private const string ProviderKey = "provider";
    private const string ReturnUrlKey = "returnUrl";
    private const string LanguageKey = "language";
    private const string ClientIdKey = "clientId";
    private const string ClientSecretKey = "clientSecret";
    private const string AppVersionKey = "appVersion";
    private const string DeviceIdKey = "deviceId";
    private const string DeviceBrandKey = "deviceBrand";
    private const string DeviceModelKey = "deviceModel";
    private const string DeviceTypeKey = "deviceType";
    private const string DeviceVendorModelKey = "deviceVendorModel";
    private const string OsNameKey = "osName";
    private const string OsVersionKey = "osVersion";
    private const string OsPlatformKey = "osPlatform";
    private const string BrowserNameKey = "browserName";
    private const string BrowserVersionKey = "browserVersion";
    private const string BrowserTypeKey = "browserType";
    private const string BrowserEngineKey = "browserEngine";
    private const string BrowserEngineVersionKey = "browserEngineVersion";

    public static void SetupItemsFromDto(this IDictionary<string, string> items, ExternalSigninDto dto)
    {
        if (dto.Provider != null)
            items.Add(ProviderKey, dto.Provider);
        
        if (dto.ReturnUrl != null)
            items.Add(ReturnUrlKey, dto.ReturnUrl);
        
        if (dto.Language != null)
            items.Add(LanguageKey, dto.Language);
        
        if (dto.ClientId != null)
            items.Add(ClientIdKey, dto.ClientId);
        
        if (dto.ClientSecret != null)
            items.Add(ClientSecretKey, dto.ClientSecret);
        
        if (dto.AppVersion != null)
            items.Add(AppVersionKey, dto.AppVersion);
        
        if (dto.DeviceId != null)
            items.Add(DeviceIdKey, dto.DeviceId);
        
        if (dto.DeviceBrand != null)
            items.Add(DeviceBrandKey, dto.DeviceBrand);
        
        if (dto.DeviceModel != null)
            items.Add(DeviceModelKey, dto.DeviceModel);
        
        if (dto.DeviceType != null)
            items.Add(DeviceTypeKey, dto.DeviceType);
        
        if (dto.DeviceVendorModel != null)
            items.Add(DeviceVendorModelKey, dto.DeviceVendorModel);
        
        if (dto.OsName != null)
            items.Add(OsNameKey, dto.OsName);
        
        if (dto.OsVersion != null)
            items.Add(OsVersionKey, dto.OsVersion);
        
        if (dto.OsPlatform != null)
            items.Add(OsPlatformKey, dto.OsPlatform);
        
        if (dto.BrowserName != null)
            items.Add(BrowserNameKey, dto.BrowserName);
        
        if (dto.BrowserVersion != null)
            items.Add(BrowserVersionKey, dto.BrowserVersion);
        
        if (dto.BrowserType != null)
            items.Add(BrowserTypeKey, dto.BrowserType);
        
        if (dto.BrowserEngine != null)
            items.Add(BrowserEngineKey, dto.BrowserEngine);
        
        if (dto.BrowserEngineVersion != null)
            items.Add(BrowserEngineVersionKey, dto.BrowserEngineVersion);
    }

    public static ExternalSigninDto GetDtoFromItems(this IDictionary<string, string> items)
    {
        string unknown = string.Empty;
        return new ExternalSigninDto
        {
            Provider = items.TryGetValue(ProviderKey, out var provider) ? provider : unknown,
            ReturnUrl = items.TryGetValue(ReturnUrlKey, out var returnUrl) ? returnUrl : unknown,
            Language = items.TryGetValue(LanguageKey, out var language) ? language : unknown,
            ClientId = items.TryGetValue(ClientIdKey, out var clientId) ? clientId : unknown,
            ClientSecret = items.TryGetValue(ClientSecretKey, out var clientSecret) ? clientSecret : unknown,
            AppVersion = items.TryGetValue(AppVersionKey, out var appVersion) ? appVersion : unknown,
            DeviceId = items.TryGetValue(DeviceIdKey, out var deviceId) ? deviceId : unknown,
            DeviceBrand = items.TryGetValue(DeviceBrandKey, out var deviceBrand) ? deviceBrand : unknown,
            DeviceModel = items.TryGetValue(DeviceModelKey, out var deviceModel) ? deviceModel : unknown,
            DeviceType = items.TryGetValue(DeviceTypeKey, out var deviceType) ? deviceType : unknown,
            DeviceVendorModel = items.TryGetValue(DeviceVendorModelKey, out var deviceVendorModel) ? deviceVendorModel : unknown,
            OsName = items.TryGetValue(OsNameKey, out var osName) ? osName : unknown,
            OsVersion = items.TryGetValue(OsVersionKey, out var osVersion) ? osVersion : unknown,
            OsPlatform = items.TryGetValue(OsPlatformKey, out var osPlatform) ? osPlatform : unknown,
            BrowserName = items.TryGetValue(BrowserNameKey, out var browserName) ? browserName : unknown,
            BrowserVersion = items.TryGetValue(BrowserVersionKey, out var browserVersion) ? browserVersion : unknown,
            BrowserType = items.TryGetValue(BrowserTypeKey, out var browserType) ? browserType : unknown,
            BrowserEngine = items.TryGetValue(BrowserEngineKey, out var browserEngine) ? browserEngine : unknown,
            BrowserEngineVersion = items.TryGetValue(BrowserEngineVersionKey, out var browserEngineVersion) ? browserEngineVersion : unknown,
        };
    }
}
