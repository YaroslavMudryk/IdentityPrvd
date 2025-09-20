using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;

namespace IdentityPrvd.Features.Personal.Devices.Services;

public class UnverifyDeviceOrchestrator(
    IUserContext userContext,
    IDeviceStore deviceStore,
    TimeProvider timeProvider)
{
    public async Task UnverifyDeviceAsync(Ulid deviceId, bool deleteDevice = false)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissions(IdentityClaims.Types.Identity, IdentityClaims.Values.All);

        var userId = currentUser.UserId;

        var deviceToUnverify = await deviceStore.GetAsync(deviceId) ?? throw new NotFoundException($"Device id:{deviceId} not found");
        if (deviceToUnverify.UserId != userId.GetIdAsUlid())
            throw new BadRequestException("Not your device");

        deviceToUnverify.Verified = false;
        deviceToUnverify.VerifiedAt = null;
        deviceToUnverify.UnverifiedBySessionId = currentUser.SessionId;
        deviceToUnverify.UnverifiedAt = timeProvider.GetUtcNow().UtcDateTime;

        if (deleteDevice)
            await deviceStore.DeleteAsync(deviceToUnverify);
        else
            await deviceStore.UpdateAsync(deviceToUnverify);
    }
}
