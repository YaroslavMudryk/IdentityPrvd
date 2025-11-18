using FluentValidation;
using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Authorization.Claims.Dtos;

namespace IdentityPrvd.Features.Authorization.Claims.Services;

public class CreateClaimOrchestrator(
    IIdentityContext identityContext,
    IValidator<CreateClaimDto> validator,
    IClaimsQuery query,
    IClaimStore repo)
{
    public async Task<ClaimDto> CreateClaimAsync(CreateClaimDto dto)
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Claims, IdentityClaims.Values.Create,
            [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        await ValidationHelper.ValidateAndThrowAsync(validator, dto);

        var newClaim = new IdentityClaim
        {
            Type = dto.Type,
            Value = dto.Value,
            Issuer = dto.Issuer,
            DisplayName = dto.DisplayName
        };
        await repo.AddAsync(newClaim);

        return await query.GetClaimAsync(newClaim.Id);
    }
}
