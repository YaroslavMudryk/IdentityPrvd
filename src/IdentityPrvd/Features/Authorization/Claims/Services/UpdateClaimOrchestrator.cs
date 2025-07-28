using FluentValidation;
using IdentityPrvd.Common.Constants;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Features.Authorization.Claims.Dtos;

namespace IdentityPrvd.Features.Authorization.Claims.Services;

public class UpdateClaimOrchestrator(
    IUserContext userContext,
    IValidator<UpdateClaimDto> validator,
    IClaimsQuery query,
    IClaimStore store)
{
    public async Task<ClaimDto> UpdateClaimAsync(Ulid claimId, UpdateClaimDto dto)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
             IdentityClaims.Types.Claims, IdentityClaims.Values.Update,
             [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        dto.Id = claimId;
        await validator.ValidateAndThrowAsync(dto);

        var claim = await store.GetAsync(claimId);
        claim.Type = dto.Type;
        claim.Value = dto.Value;
        claim.Issuer = dto.Issuer;
        claim.DisplayName = dto.DisplayName;

        await store.UpdateAsync(claim);

        return await query.GetClaimAsync(claimId);
    }
}
