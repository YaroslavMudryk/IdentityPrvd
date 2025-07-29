using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Services.Security;

namespace IdentityPrvd.Features.Security.Mfa.DisableMfa.Services;

public class DisableMfaOrchestrator(
    IMfaStore mfaStore,
    IMfaService mfaService,
    IUserContext userContext)
{
    public async Task DisableMfaAsync(string code)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Identity, IdentityClaims.Values.All,
            [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        var userId = Ulid.Parse(currentUser.UserId);

        var userMfa = await mfaStore.GetUserActiveMfaNullableAsync(userId)
            ?? throw new BadRequestException("Mfa already diactivated");

        if (!await mfaService.VerifyMfaAsync(code, userMfa.Secret))
            throw new BadRequestException("Your otp code is invalid");

        await mfaStore.DeleteAsync(userMfa);
        var recoveryCodes = await mfaStore.GetMfaRecoveryCodesAsync(userMfa.Id);
        await mfaStore.DeleteRecoveryCodesAsync(recoveryCodes);
    }
}
