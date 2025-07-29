using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Personal.Devices.Dtos;
using Riok.Mapperly.Abstractions;

namespace IdentityPrvd.Mappers;

[Mapper]
public static partial class DevicesMapper
{
    public static partial IQueryable<DeviceDto> ProjectToDto(this IQueryable<IdentityDevice> devices);
    public static partial DeviceDto MapToDto(this IdentityDevice device);
}

public static class DeviceMapperExtensions
{
    public static IdentityDevice MapToEntity(this VerifyDeviceDto deviceDto)
    {
        return new IdentityDevice
        {
            Identifier = deviceDto.Identifier,
            Brand = deviceDto.Brand,
            Model = deviceDto.Model,
            VendorModel = deviceDto.VendorModel,
            Type = deviceDto.Type,
            Os = deviceDto.Os,
            OsVersion = deviceDto.OsVersion,
            OsShortName = deviceDto.OsShortName,
            OsUI = deviceDto.OsUI,
            OsPlatform = deviceDto.OsPlatform,
            Browser = deviceDto.Browser,
            BrowserVersion = deviceDto.BrowserVersion,
            BrowserType = deviceDto.BrowserType,
            BrowserEngine = deviceDto.BrowserEngine,
            BrowserEngineVersion = deviceDto.BrowserEngineVersion,
        };
    }
}
