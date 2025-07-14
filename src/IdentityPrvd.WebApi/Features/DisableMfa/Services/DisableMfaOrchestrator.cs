using IdentityPrvd.WebApi.Exceptions;
using IdentityPrvd.WebApi.Features.EnableMfa.DataAccess;
using IdentityPrvd.WebApi.Helpers;
using IdentityPrvd.WebApi.Protections;
using IdentityPrvd.WebApi.UserContext;

namespace IdentityPrvd.WebApi.Features.DisableMfa.Services;

public class DisableMfaOrchestrator(
    MfaRepo mfaRepo,
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

        var userMfa = await mfaRepo.GetUserActiveMfaNullableAsync(userId)
            ?? throw new BadRequestException("Mfa already diactivated");

        if (!await mfaService.VerifyMfaAsync(code, userMfa.Secret))
            throw new BadRequestException("Your otp code is invalid");

        await mfaRepo.DeleteAsync(userMfa);
        var recoveryCodes = await mfaRepo.GetMfaRecoveryCodesAsync(userMfa.Id);
        await mfaRepo.DeleteRecoveryCodesAsync(recoveryCodes);
    }
}
