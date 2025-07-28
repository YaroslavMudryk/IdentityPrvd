using IdentityPrvd.Domain.ValueObjects;
using IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;

namespace IdentityPrvd.Features.Authentication.ExternalSignin.Services;

public static class MapToClientInfoExtensions
{
    public static ClientInfo MapToClientInfo(this ExternalSigninDto signinDto)
    {
        return new ClientInfo
        {
            Browser = CreateBrowserInfoIfNotEmpty(signinDto),
            Os = CreateOsInfoIfNotEmpty(signinDto),
            Device = CreateDeviceInfoIfNotEmpty(signinDto)
        };
    }

    private static BrowserInfo CreateBrowserInfoIfNotEmpty(ExternalSigninDto signinDto)
    {
        if (string.IsNullOrWhiteSpace(signinDto.BrowserName) &&
            string.IsNullOrWhiteSpace(signinDto.BrowserVersion) &&
            string.IsNullOrWhiteSpace(signinDto.BrowserEngine) &&
            string.IsNullOrWhiteSpace(signinDto.BrowserEngineVersion) &&
            string.IsNullOrWhiteSpace(signinDto.BrowserType))
        {
            return null;
        }

        return new BrowserInfo
        {
            Name = signinDto.BrowserName,
            Version = signinDto.BrowserVersion,
            Engine = signinDto.BrowserEngine,
            EngineVersion = signinDto.BrowserEngineVersion,
            Type = signinDto.BrowserType,
        };
    }

    private static OsInfo CreateOsInfoIfNotEmpty(ExternalSigninDto signinDto)
    {
        if (string.IsNullOrWhiteSpace(signinDto.OsName) &&
            string.IsNullOrWhiteSpace(signinDto.OsPlatform) &&
            string.IsNullOrWhiteSpace(signinDto.OsVersion))
        {
            return null;
        }

        return new OsInfo
        {
            Name = signinDto.OsName,
            Platform = signinDto.OsPlatform,
            Version = signinDto.OsVersion
        };
    }

    private static DeviceInfo CreateDeviceInfoIfNotEmpty(ExternalSigninDto signinDto)
    {
        if (string.IsNullOrWhiteSpace(signinDto.DeviceBrand) &&
            string.IsNullOrWhiteSpace(signinDto.DeviceId) &&
            string.IsNullOrWhiteSpace(signinDto.DeviceModel) &&
            string.IsNullOrWhiteSpace(signinDto.DeviceType) &&
            string.IsNullOrWhiteSpace(signinDto.DeviceVendorModel))
        {
            return null;
        }

        return new DeviceInfo
        {
            Brand = signinDto.DeviceBrand,
            DeviceId = signinDto.DeviceId,
            Model = signinDto.DeviceModel,
            Type = signinDto.DeviceType,
            VendorModel = signinDto.DeviceVendorModel
        };
    }
}
