using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Features.Personal.Devices.Dtos;
using IdentityPrvd.Mappers;

namespace IdentityPrvd.Features.Personal.Devices.Services;

public class VerifyDeviceOrchestrator(
    IUserContext userContext,
    IDeviceStore deviceStore,
    IDevicesQuery devicesQuery,
    TimeProvider timeProvider)
{
    public async Task<DeviceDto> VerifyDeviceAsync(VerifyDeviceDto dto)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissions(IdentityClaims.Types.Identity, IdentityClaims.Values.All);

        var userId = currentUser.UserId;

        var existDevice = await devicesQuery.GetDeviceByIdentifierAsync(dto.Identifier, userId.GetIdAsUlid());
        if (existDevice != null && existDevice.Verified)
            throw new BadRequestException("This device already verified");

        var newDevice = dto.MapToEntity();
        newDevice.Verified = true;
        newDevice.VerifiedAt = timeProvider.GetUtcNow().UtcDateTime;
        newDevice.VerifiedBySessionId = currentUser.SessionId;
        newDevice.UserId = userId.GetIdAsUlid();

        var addedDevice = await deviceStore.AddAsync(newDevice);

        return addedDevice.MapToDto();
    }
}
