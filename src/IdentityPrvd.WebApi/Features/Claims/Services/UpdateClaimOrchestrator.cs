using FluentValidation;
using IdentityPrvd.WebApi.Features.Claims.DataAccess;
using IdentityPrvd.WebApi.Features.Claims.Dtos;
using IdentityPrvd.WebApi.Helpers;
using IdentityPrvd.WebApi.UserContext;

namespace IdentityPrvd.WebApi.Features.Claims.Services;

public class UpdateClaimOrchestrator(
    IUserContext userContext,
    IValidator<UpdateClaimDto> validator,
    ClaimsQuery query,
    ClaimRepo repo)
{
    public async Task<ClaimDto> UpdateClaimAsync(Ulid claimId, UpdateClaimDto dto)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
             IdentityClaims.Types.Claims, IdentityClaims.Values.Update,
             [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        dto.Id = claimId;
        await validator.ValidateAndThrowAsync(dto);

        var claim = await repo.GetAsync(claimId);
        claim.Type = dto.Type;
        claim.Value = dto.Value;
        claim.Issuer = dto.Issuer;
        claim.DisplayName = dto.DisplayName;

        await repo.UpdateAsync(claim);

        return await query.GetClaimAsync(claimId);
    }
}
