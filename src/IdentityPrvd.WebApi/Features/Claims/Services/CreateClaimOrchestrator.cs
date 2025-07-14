using FluentValidation;
using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Features.Claims.DataAccess;
using IdentityPrvd.WebApi.Features.Claims.Dtos;
using IdentityPrvd.WebApi.Helpers;
using IdentityPrvd.WebApi.UserContext;

namespace IdentityPrvd.WebApi.Features.Claims.Services;

public class CreateClaimOrchestrator(
    IUserContext userContext,
    IValidator<CreateClaimDto> validator,
    ClaimsQuery query,
    ClaimRepo repo)
{
    public async Task<ClaimDto> CreateClaimAsync(CreateClaimDto dto)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Claims, IdentityClaims.Values.Create,
            [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        await validator.ValidateAndThrowAsync(dto);

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
