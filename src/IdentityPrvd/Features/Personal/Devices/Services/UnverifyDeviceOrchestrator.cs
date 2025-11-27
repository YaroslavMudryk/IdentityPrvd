using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;

namespace IdentityPrvd.Features.Personal.Devices.Services;

public class UnverifyDeviceOrchestrator(
    IIdentityContext identityContext,
    IDeviceStore deviceStore,
    TimeProvider timeProvider)
{
    public async Task UnverifyDeviceAsync(Guid deviceId, bool deleteDevice = false)
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermission(IdentityPermissions.Contacts.Manage);

        var userId = currentUser.UserId;

        var deviceToUnverify = await deviceStore.GetAsync(deviceId) ?? throw new NotFoundException($"Device id:{deviceId} not found");
        if (deviceToUnverify.UserId != userId.GetIdAsGuid())
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
