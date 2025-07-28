using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Authorization.Claims.Dtos;
using Riok.Mapperly.Abstractions;

namespace IdentityPrvd.Mappers;

[Mapper]
public static partial class ClaimsMapper
{
    public static partial IQueryable<ClaimDto> ProjectToDto(this IQueryable<IdentityClaim> claims);
}
