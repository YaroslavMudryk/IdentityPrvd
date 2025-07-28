using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Authorization.Roles.Dtos;
using Riok.Mapperly.Abstractions;

namespace IdentityPrvd.Mappers;

[Mapper]
public static partial class RolesMapper
{
    public static partial IQueryable<RoleDto> ProjectToDto(this IQueryable<IdentityRole> roles);
}
