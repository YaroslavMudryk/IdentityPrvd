using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Services.Security;

namespace IdentityPrvd.Features.Security.Mfa.DisableMfa.Services;

public class DisableMfaOrchestrator(
    IMfaStore mfaStore,
    IMfaService mfaService,
    IIdentityContext identityContext)
{
    public async Task DisableMfaAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new BadRequestException("Code must be a value");

        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissions(
            IdentityClaims.Types.Identity, IdentityClaims.Values.All);

        var userId = currentUser.UserId.GetIdAsUlid();

        var userMfa = await mfaStore.GetUserActiveMfaNullableAsync(userId)
            ?? throw new BadRequestException("Mfa already diactivated");

        if (!await mfaService.VerifyMfaAsync(code, userMfa.Secret))
            throw new BadRequestException("Your otp code is invalid");

        await mfaStore.DeleteAsync(userMfa);
        var recoveryCodes = await mfaStore.GetMfaRecoveryCodesAsync(userMfa.Id);
        await mfaStore.DeleteRecoveryCodesAsync(recoveryCodes);
    }
}
