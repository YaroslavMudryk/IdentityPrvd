using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Authorization.Permissions.Dtos;
using Riok.Mapperly.Abstractions;

namespace IdentityPrvd.Mappers;

[Mapper]
public static partial class PermissionsMapper
{
    public static partial IQueryable<PermissionDto> ProjectToDto(this IQueryable<IdentityPermission> permissions);
}
