using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Db.Entities.Internal;
using IdentityPrvd.WebApi.Features.Claims.Dtos;
using IdentityPrvd.WebApi.Features.Roles.Dtos;
using IdentityPrvd.WebApi.Features.Sessions.GetSessions.Dtos;
using Riok.Mapperly.Abstractions;
using Ext = Extensions.DeviceDetector.Models.ClientInfo;

namespace IdentityPrvd.WebApi.Mappers;

[Mapper]
public static partial class DefaultMapper
{
    public static partial IQueryable<ClaimDto> ProjectToDto(this IQueryable<IdentityClaim> claims);

    public static partial IQueryable<RoleDto> ProjectToDto(this IQueryable<IdentityRole> roles);

    public static partial IQueryable<SessionDto> ProjectToDto(this IQueryable<IdentitySession> sessions);

    [MapProperty(nameof(Ext.OS), nameof(ClientInfo.Os))]
    public static partial ClientInfo MapToClientInfo(this Ext clientInfo);
}

public static class IdentityMapperExtensions
{
    public static ClientAppInfo MapToAppInfo(this IdentityClient client, string appVersion)
    {
        return new ClientAppInfo
        {
            Id = client.Id,
            Name = client.Name,
            Version = appVersion,
            Description = client.Description,
            Image = client.Image,
            ShortName = client.ShortName
        };
    }
}
