using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;

namespace IdentityPrvd.Features.Personal.Devices.Services;

public class DeleteDeviceOrchestrator(
    IUserContext userContext,
    IDeviceStore deviceStore)
{
    public async Task DeleteDeviceAsync(Ulid deviceId)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissions(IdentityClaims.Types.Identity, IdentityClaims.Values.All);

        var userId = currentUser.UserId;

        var deviceToDelete = await deviceStore.GetAsync(deviceId) ?? throw new NotFoundException($"Device id:{deviceId} not found");

        if (deviceToDelete.UserId != userId.GetIdAsUlid())
            throw new UnauthorizedException("Not your device");

        await deviceStore.DeleteAsync(deviceToDelete);
    }
}
