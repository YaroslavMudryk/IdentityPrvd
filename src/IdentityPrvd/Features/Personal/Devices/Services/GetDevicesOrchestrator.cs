using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Features.Personal.Devices.Dtos;

namespace IdentityPrvd.Features.Personal.Devices.Services;

public class GetDevicesOrchestrator(
    IIdentityContext identityContext,
    IDevicesQuery devicesQuery)
{
    public async Task<List<DeviceDto>> GetDevicesAsync()
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissions(IdentityClaims.Types.Identity, IdentityClaims.Values.All);

        return await devicesQuery.GetUserDevicesAsync(currentUser.UserId.GetIdAsUlid());
    }
}
